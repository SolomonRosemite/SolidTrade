import 'package:flutter/material.dart';
import 'package:solidtrade/components/base/st_widget.dart';
import 'package:solidtrade/pages/signup_and_signin/components/login_screen.dart';
import 'package:solidtrade/services/util/user_util.dart';
import 'package:solidtrade/services/util/util.dart';

class LoginHome extends StatelessWidget with STWidget {
  LoginHome({Key? key, required this.tabController}) : super(key: key);

  final TabController tabController;

  Future<void> _handleClickCreateAccount(BuildContext context) async {
    var user = await UtilUserService.signInWithGoogle();

    if (user != null) {
      tabController.animateTo(0);
      return;
    }

    Util.googleLoginFailedDialog(context);
  }

  void _handleClickLoginUser() => tabController.animateTo(2);

  @override
  Widget build(BuildContext context) {
    return LoginScreen(
      assetName: "assets/images/welcome_image.jpg",
      title: "Too broke to start investing?",
      subTitle: "Start paper trading with Stocks, ETFs and Derivatives. Try new investment strategies and compare your portfolio with friends and others!",
      additionalWidgets: [
        Util.roundedButton(
          [
            Util.loadImage(
              "https://upload.wikimedia.org/wikipedia/commons/thumb/5/53/Google_%22G%22_Logo.svg/1200px-Google_%22G%22_Logo.svg.png",
              20,
            ),
            const SizedBox(width: 10),
            const Text("Create new account with Google"),
          ],
          colors: colors,
          onPressed: () => _handleClickCreateAccount(context),
        ),
        const SizedBox(height: 10),
        Util.roundedButton([
          const Spacer(flex: 8),
          SizedBox(width: IconTheme.of(context).size),
          const Text("Already have an account? Sign In here"),
          const Spacer(flex: 7),
          const Icon(Icons.keyboard_arrow_right_rounded),
          const Spacer(flex: 1),
        ], colors: colors, onPressed: _handleClickLoginUser),
      ],
    );
  }
}
