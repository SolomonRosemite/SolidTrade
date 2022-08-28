import 'package:flutter/material.dart';
import 'package:get_it/get_it.dart';
import 'package:solidtrade/components/base/st_widget.dart';
import 'package:solidtrade/data/models/common/constants.dart';
import 'package:solidtrade/pages/home/home_page.dart';
import 'package:solidtrade/pages/signup_and_signin/components/login_screen.dart';
import 'package:solidtrade/services/stream/historicalpositions_service.dart';
import 'package:solidtrade/services/stream/portfolio_service.dart';
import 'package:solidtrade/services/stream/user_service.dart';
import 'package:solidtrade/services/util/user_util.dart';
import 'package:solidtrade/services/util/util.dart';

import 'package:url_launcher/url_launcher.dart';

class LoginSignIn extends StatelessWidget with STWidget {
  LoginSignIn({Key? key}) : super(key: key);

  final historicalPositionService = GetIt.instance.get<HistoricalPositionService>();
  final portfolioService = GetIt.instance.get<PortfolioService>();
  final userService = GetIt.instance.get<UserService>();

  Future<void> _handleClickLoginWithGoogle(BuildContext context) async {
    var successful = await Util.requestNotificationPermissionsWithUserFriendlyPopup(context);

    if (!successful) {
      return;
    }

    var user = await UtilUserService.signInWithGoogle();

    if (user == null) {
      Util.googleLoginFailedDialog(context);
      return;
    }

    var closeDialog = Util.showLoadingDialog(context);

    var response = await userService.fetchUser(user.uid);

    if (response.isSuccessful) {
      final userId = response.result!.id;
      await historicalPositionService.fetchHistoricalPositions(userId);
      await portfolioService.fetchPortfolioByUserId(userId);

      Navigator.of(context).popUntil((route) => route.isFirst);
      Navigator.pushReplacement(context, MaterialPageRoute(builder: (context) => const HomePage()));
      return;
    }

    closeDialog();
    Util.openDialog(context, "Login failed", message: response.error!.userFriendlyMessage);
  }

  void _handleClickForgotOrLostGoogleAccount() {
    launch(Constants.forgotOrLostAccountFormLink);
  }

  @override
  Widget build(BuildContext context) {
    return LoginScreen(
      imageUrl: "https://c.tenor.com/wQ5IslyynbkAAAAC/elon-musk-smoke.gif",
      title: "Hello Again!",
      subTitle: "Welcome back you've been missed!",
      additionalWidgets: [
        Util.roundedButton(
          [
            Util.loadImage(
              "https://upload.wikimedia.org/wikipedia/commons/thumb/5/53/Google_%22G%22_Logo.svg/1200px-Google_%22G%22_Logo.svg.png",
              20,
            ),
            const SizedBox(width: 10),
            const Text("Login with Google"),
          ],
          colors: colors,
          onPressed: () => _handleClickLoginWithGoogle(context),
        ),
        const SizedBox(height: 10),
        Util.roundedButton(
          [
            const Text("Forgot or lost your Google Account? Click here!"),
          ],
          colors: colors,
          onPressed: _handleClickForgotOrLostGoogleAccount,
        ),
      ],
    );
  }
}
