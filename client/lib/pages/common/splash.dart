import 'package:firebase_core/firebase_core.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter/material.dart';
import 'package:get_it/get_it.dart';
import 'package:solidtrade/components/base/st_widget.dart';
import 'package:solidtrade/app/main_common.dart';
import 'package:solidtrade/components/common/st_logo.dart';
import 'package:solidtrade/providers/language/language_provider.dart';
import 'package:solidtrade/services/stream/historicalpositions_service.dart';
import 'package:solidtrade/services/stream/portfolio_service.dart';
import 'package:solidtrade/services/stream/user_service.dart';
import 'package:solidtrade/services/util/util.dart';

class Splash extends StatefulWidget {
  const Splash({Key? key}) : super(key: key);

  @override
  _SplashState createState() => _SplashState();
}

class _SplashState extends State<Splash> with STWidget {
  final historicalPositionService = GetIt.instance.get<HistoricalPositionService>();
  final portfolioService = GetIt.instance.get<PortfolioService>();
  final userService = GetIt.instance.get<UserService>();

  late Future _fadeAnimationFuture;
  bool _visible = false;

  @override
  void initState() {
    super.initState();

    _fadeContent();
    _preFetchUser();
  }

  void _fadeContent() {
    _fadeAnimationFuture = Future.delayed(const Duration(milliseconds: 600), () {
      setState(() {
        _visible = !_visible;
      });
    });
  }

  Future<void> _preFetchUser() async {
    var delay = Future.delayed(const Duration(milliseconds: 2500));

    await Firebase.initializeApp();

    if (kIsWeb) {
      // We have to wait a second after the firebase initialization because even if the user has already signed in, it will return null for the current user
      // (even though it should not). After a while firebase realizes that there is a current user and then does not return null anymore (has it should).
      // This is very odd and this only happens on web. This solves the problem for the time being.
      // See here for more info about this issue: https://github.com/firebase/flutterfire/issues/5964
      await Future.delayed(const Duration(seconds: 1));
    }

    var userRequest = await userService.fetchUserCurrentUser();
    if (userRequest.isSuccessful) {
      await historicalPositionService.fetchHistoricalPositions(userRequest.result!.id);
      await portfolioService.fetchPortfolioByUserId(userRequest.result!.id);

      Log.d("Fetched user info successfully");

      await _fadeAnimationFuture;
      await delay;

      Navigator.pop(context, true);
      return;
    }

    delay.ignore();
    _fadeAnimationFuture.ignore();
    Log.w("User login failed. Proceeding to login");

    Navigator.pop(context, false);
  }

  @override
  Widget build(BuildContext context) {
    if (Startup.colorThemeHasToBeInitialized) {
      final theme = Util.currentDeviceColorTheme(context);
      configurationProvider.themeProvider.updateTheme(theme, savePermanently: false);

      Startup.colorThemeHasToBeInitialized = false;
    }

    if (Startup.languageHasToBeInitialized) {
      final ticker = Util.currentDeviceLanguage(context);
      configurationProvider.languageProvider.updateLanguage(LanguageProvider.tickerToTranslation(ticker));

      Startup.languageHasToBeInitialized = false;
    }

    return Scaffold(
      backgroundColor: colors.splashScreenColor,
      body: Center(
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.center,
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            const SizedBox(height: 50),
            const Spacer(),
            STLogo(colors.logoAsGif, key: UniqueKey()),
            const Spacer(),
            AnimatedOpacity(
              opacity: _visible ? 1.0 : 0.0,
              duration: const Duration(milliseconds: 800),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.center,
                mainAxisAlignment: MainAxisAlignment.center,
                mainAxisSize: MainAxisSize.max,
                children: [
                  Text(translations.splash.loading),
                  SizedBox(
                    width: 220,
                    child: Divider(thickness: 2, color: colors.softForeground),
                  ),
                  const Text("Solidtrade"),
                ],
              ),
            ),
            const SizedBox(height: 50),
          ],
        ),
      ),
    );
  }
}
