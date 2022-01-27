import 'package:shared_preferences/shared_preferences.dart';
import 'package:solidtrade/data/enums/lang_ticker.dart';
import 'package:solidtrade/data/enums/shared_preferences_keys.dart';
import 'package:solidtrade/providers/app/app_configuration_provider.dart';
import 'package:solidtrade/providers/app/app_update_stream_provider.dart';
import 'package:solidtrade/providers/language/language_provider.dart';
import 'package:solidtrade/providers/theme/app_theme.dart';
import 'package:solidtrade/services/stream/historicalpositions_service.dart';
import 'package:get_it/get_it.dart';
import 'package:solidtrade/config/config_reader.dart';
import 'package:solidtrade/data/enums/environment.dart';
import 'package:solidtrade/app.dart';
import 'package:flutter/cupertino.dart';
import 'package:flutter/material.dart';

Future<void> commonMain(Environment environment) async {
  WidgetsFlutterBinding.ensureInitialized();

  final SharedPreferences prefs = await SharedPreferences.getInstance();
  await ConfigReader.initialize();

  final languageTickerIndex = prefs.getInt(SharedPreferencesKeys.langTicker.toString()) ?? LanguageTicker.en.index;
  final languageProvider = LanguageProvider.byTicker(LanguageTicker.values[languageTickerIndex]);

  final colorThemeIndex = prefs.getInt(SharedPreferencesKeys.colorTheme.toString()) ?? ColorThemeType.light.index;
  final themeProvider = ThemeProvider.byThemeType(ColorThemeType.values[colorThemeIndex]);

  final updateProvider = UIUpdateStreamProvider();

  GetIt getItService = GetIt.instance;
  getItService.registerSingleton<SharedPreferences>(prefs);
  getItService.registerSingleton<HistoricalPositionService>(HistoricalPositionService());

  getItService.registerSingleton<ConfigurationProvider>(ConfigurationProvider(languageProvider, themeProvider, updateProvider));

  runApp(const MyApp());
}