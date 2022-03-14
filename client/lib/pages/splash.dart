import 'package:firebase_core/firebase_core.dart';
import 'package:flutter/material.dart';
import 'package:get_it/get_it.dart';
import 'package:solidtrade/components/base/st_widget.dart';
import 'package:solidtrade/components/shared/st_logo.dart';
import 'package:solidtrade/main/main_common.dart';
import 'package:solidtrade/pages/welcome_page.dart';
import 'package:solidtrade/providers/language/language_provider.dart';
import 'package:solidtrade/services/stream/historicalpositions_service.dart';
import 'package:solidtrade/services/stream/portfolio_service.dart';
import 'package:solidtrade/services/stream/user_service.dart';
import 'package:solidtrade/services/util/debug/log.dart';
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
    _navigateToHome();
  }

  void _fadeContent() {
    _fadeAnimationFuture = Future.delayed(const Duration(milliseconds: 100), () {
      setState(() {
        _visible = !_visible;
      });
    });
  }

  // TODO: Resolve PR conflicts: https://github.com/SolomonRosemite/SolidTrade/pull/13
  Future<void> _navigateToHome() async {
    var delay = Future.delayed(const Duration(seconds: 1));

    await Firebase.initializeApp();
    Log.w(Firebase.app().options.appId);

    await _fadeAnimationFuture;
    // var userRequest = await userService.fetchUser();
    // if (userRequest.isSuccessful) {
    //   await historicalPositionService.fetchHistoricalPositions(userRequest.result!.id);
    //   await portfolioService.fetchPortfolioByUserId();

    //   Log.d("fetched user info successfully");

    //   await delay;

    //   Navigator.pushReplacement(context, MaterialPageRoute(builder: (context) => const HomePage()));
    //   return;
    // }

    delay.ignore();
    Log.w("User request failed.");
    // Log.w(userRequest.error!.userFriendlyMessage);

    Navigator.pushReplacement(context, MaterialPageRoute(builder: (context) => const WelcomePage()));
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
      backgroundColor: colors.splasScreenColor,
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
