import 'package:shared_preferences/shared_preferences.dart';
import 'package:solidtrade/config/config_reader.dart';
import 'package:solidtrade/data/models/enums/client_enums/environment.dart';
import 'package:solidtrade/data/models/enums/client_enums/lang_ticker.dart';
import 'package:solidtrade/data/models/enums/client_enums/shared_preferences_keys.dart';
import 'package:solidtrade/providers/app/app_configuration_provider.dart';
import 'package:solidtrade/providers/app/app_update_stream_provider.dart';
import 'package:solidtrade/providers/language/language_provider.dart';
import 'package:solidtrade/providers/theme/app_theme.dart';
import 'package:solidtrade/services/request/data_request_service.dart';
import 'package:solidtrade/services/stream/aggregate_history_service.dart';
import 'package:solidtrade/services/stream/floating_action_button_update_service.dart';
import 'package:solidtrade/services/stream/historicalpositions_service.dart';
import 'package:get_it/get_it.dart';
import 'package:solidtrade/app.dart';
import 'package:flutter/material.dart';
import 'package:solidtrade/services/stream/knockout_service.dart';
import 'package:solidtrade/services/stream/messaging_service.dart';
import 'package:solidtrade/services/stream/ongoing_knockout_service.dart';
import 'package:solidtrade/services/stream/ongoing_warrant_service.dart';
import 'package:solidtrade/services/stream/portfolio_service.dart';
import 'package:solidtrade/services/stream/stock_service.dart';
import 'package:solidtrade/services/stream/tr_derivatives_search_service.dart';
import 'package:solidtrade/services/stream/tr_product_info_service.dart';
import 'package:solidtrade/services/stream/tr_product_price_service.dart';
import 'package:solidtrade/services/stream/tr_product_search_service.dart';
import 'package:solidtrade/services/stream/tr_stock_details_service.dart';
import 'package:solidtrade/services/stream/user_service.dart';
import 'package:solidtrade/services/stream/warrant_service.dart';

import '../mapper.g.dart' as mapper;

var navigatorKey = GlobalKey<NavigatorState>();
late Environment environment;
late MyAppState myAppState;

Future<void> commonMain(Environment env) async {
  await Startup.initializeApp(env);
  registerFlutterErrorHandler(env);

  runApp(MyApp(navigatorKey: navigatorKey));
}

void registerFlutterErrorHandler(Environment environment) {
  var isShowingErrorSnackBar = false;

  FlutterError.onError = (details) {
    // We can exclude image errors, because these are already being handled.
    if (details.exception is Exception && (details.exception as Exception).toString().contains("Invalid image data")) {
      return;
    }

    if (environment != Environment.production) {
      // ignore: avoid_print
      print(details);
    }

    WidgetsBinding.instance?.addPostFrameCallback((_) {
      // We dont want to stack error snack bars. There for we only create one, if there currently is not other.
      if (isShowingErrorSnackBar) {
        return;
      }

      isShowingErrorSnackBar = true;
      Future.delayed(const Duration(seconds: 1), () {
        final context = navigatorKey.currentState!.context;
        final snackBar = SnackBar(
          duration: const Duration(seconds: 15),
          content: Row(children: [
            const Flexible(
              child: Text('We are sorry. Something went wrong. If you are facing any issues reload the app.'),
            ),
            SnackBarAction(
              label: 'Reload',
              onPressed: () async => await myAppState.restart(),
              textColor: Colors.red,
            ),
            SnackBarAction(
              label: 'Dismiss',
              onPressed: () => ScaffoldMessenger.of(context).hideCurrentSnackBar(),
              textColor: Colors.blue,
            ),
          ]),
        );

        ScaffoldMessenger.of(context).showSnackBar(snackBar).closed.then((_) => isShowingErrorSnackBar = false);
      });
    });
  };
}

class Startup {
  static bool languageHasToBeInitialized = false;
  static bool colorThemeHasToBeInitialized = false;

  static Future<void> initializeApp(Environment env) async {
    environment = env;
    colorThemeHasToBeInitialized = false;
    languageHasToBeInitialized = false;

    WidgetsFlutterBinding.ensureInitialized();

    mapper.init();

    final SharedPreferences prefs = await SharedPreferences.getInstance();
    await ConfigReader.initialize();

    final languageTickerIndex = prefs.getInt(SharedPreferencesKeys.langTicker.toString()) ?? _initialLanguage().index;
    final languageProvider = LanguageProvider.byTicker(LanguageTicker.values[languageTickerIndex]);

    final colorThemeIndex = prefs.getInt(SharedPreferencesKeys.colorTheme.toString()) ?? _initialColorTheme().index;
    final themeProvider = ThemeProvider.byThemeType(ColorThemeType.values[colorThemeIndex]);

    final updateProvider = UIUpdateStreamProvider();

    GetIt getItService = GetIt.instance;
    getItService.allowReassignment = true;
    getItService.registerSingleton<SharedPreferences>(prefs);
    getItService.registerSingleton<UserService>(UserService());
    getItService.registerSingleton<StockService>(StockService());
    getItService.registerSingleton<WarrantService>(WarrantService());
    getItService.registerSingleton<KnockoutService>(KnockoutService());
    getItService.registerSingleton<OngoingWarrantService>(OngoingWarrantService());
    getItService.registerSingleton<OngoingKnockoutService>(OngoingKnockoutService());
    getItService.registerSingleton<PortfolioService>(PortfolioService());
    getItService.registerSingleton<MessagingService>(MessagingService());
    getItService.registerSingleton<TrStockDetailsService>(TrStockDetailsService());
    getItService.registerSingleton<TrProductSearchService>(TrProductSearchService());
    getItService.registerSingleton<TrDerivativesSearchService>(TrDerivativesSearchService());
    getItService.registerSingleton<AggregateHistoryService>(AggregateHistoryService());
    getItService.registerSingleton<HistoricalPositionService>(HistoricalPositionService());

    getItService.registerFactory<TrProductInfoService>(() => TrProductInfoService());
    getItService.registerFactory<TrProductPriceService>(() => TrProductPriceService());

    // Component services.
    getItService.registerSingleton<FloatingActionButtonUpdateService>(FloatingActionButtonUpdateService());

    getItService.registerSingleton<ConfigurationProvider>(ConfigurationProvider(languageProvider, themeProvider, updateProvider));

    DataRequestService.initialize();
  }

  static LanguageTicker _initialLanguage() {
    const defaultTicker = LanguageTicker.en;
    Startup.languageHasToBeInitialized = true;

    return defaultTicker;
  }

  static ColorThemeType _initialColorTheme() {
    const defaultTheme = ColorThemeType.light;
    Startup.colorThemeHasToBeInitialized = true;

    return defaultTheme;
  }
}
