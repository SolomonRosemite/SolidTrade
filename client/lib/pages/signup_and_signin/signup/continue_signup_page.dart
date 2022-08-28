import 'dart:typed_data';

import 'package:firebase_auth/firebase_auth.dart';
import 'package:flutter/material.dart';
import 'package:get_it/get_it.dart';
import 'package:solidtrade/components/base/st_page.dart';
import 'package:solidtrade/components/base/st_widget.dart';
import 'package:solidtrade/components/common/st_logo.dart';
import 'package:solidtrade/pages/home/home_page.dart';
import 'package:solidtrade/providers/theme/app_theme.dart';
import 'package:solidtrade/services/stream/historicalpositions_service.dart';
import 'package:solidtrade/services/stream/portfolio_service.dart';
import 'package:solidtrade/services/stream/user_service.dart';
import 'package:solidtrade/services/util/user_util.dart';
import 'package:solidtrade/services/util/util.dart';

import 'package:flutter/services.dart';

// ignore: must_be_immutable
class ContinueSignupScreen extends StatefulWidget {
  ContinueSignupScreen({
    Key? key,
    required this.user,
    required this.dicebearSeed,
    required this.profilePictureBytes,
  }) : super(key: key);
  final Uint8List? profilePictureBytes;
  final String? dicebearSeed;
  User user;

  @override
  State<ContinueSignupScreen> createState() => _ContinueSignupScreenState();
}

class _ContinueSignupScreenState extends State<ContinueSignupScreen> with STWidget {
  final TextEditingController _initialBalanceController = TextEditingController(text: 10000.toString());
  final TextEditingController _usernameController = TextEditingController();
  final TextEditingController _nameController = TextEditingController();

  final historicalPositionService = GetIt.instance.get<HistoricalPositionService>();
  final portfolioService = GetIt.instance.get<PortfolioService>();
  final userService = GetIt.instance.get<UserService>();

  InputBorder getInputBorderDecoration() {
    return UnderlineInputBorder(
      borderSide: BorderSide(color: colors.softBackground, width: 2),
    );
  }

  InputDecoration getInputDecoration(String labelText) {
    return InputDecoration(
      focusedBorder: getInputBorderDecoration(),
      enabledBorder: getInputBorderDecoration(),
      border: getInputBorderDecoration(),
      labelText: labelText,
      labelStyle: TextStyle(color: colors.foreground),
    );
  }

  Future<void> _handleClickChangeGoogleAccount() async {
    var user = await UtilUserService.signInWithGoogle();

    if (user == null) {
      Navigator.pop(context);
      return;
    }

    setState(() {
      widget.user = user;
    });
  }

  Future<void> _handleClickCreateAccount() async {
    var successful = await Util.requestNotificationPermissionsWithUserFriendlyPopup(context);

    if (!successful) {
      return;
    }

    final closeDialog = Util.showLoadingDialog(context);

    String name = _nameController.text;
    String username = _usernameController.text;
    int initialBalance = int.parse(_initialBalanceController.text);
    String email = FirebaseAuth.instance.currentUser!.email!;

    var response = await userService.createUser(
      name,
      username,
      email,
      initialBalance,
      profilePictureSeed: widget.dicebearSeed,
      profilePictureFile: widget.profilePictureBytes,
    );

    if (response.isSuccessful) {
      final userId = response.result!.id;
      await historicalPositionService.fetchHistoricalPositions(userId);
      await portfolioService.fetchPortfolioByUserId(userId);

      Navigator.of(context).popUntil((route) => route.isFirst);
      Navigator.pushReplacement(context, MaterialPageRoute(builder: (context) => const HomePage()));
      return;
    }

    closeDialog();

    Util.openDialog(context, "Could not create user", message: response.error!.userFriendlyMessage);
  }

  @override
  Widget build(BuildContext context) {
    return STPage(
      page: () => Scaffold(
        resizeToAvoidBottomInset: false,
        backgroundColor: colors.splashScreenColor,
        body: Column(
          children: [
            Expanded(
              child: Container(
                margin: const EdgeInsets.symmetric(horizontal: 20),
                child: Column(
                  children: [
                    const SizedBox(height: 20),
                    STLogo(colors.logoAsGif, key: UniqueKey(), animationDuration: const Duration(seconds: 0)),
                    const SizedBox(height: 25),
                    const Text(
                      "Hey friend👋\nOnly a few steps remaining\n\nChoose a name and define how much cash you want to start with!",
                      textAlign: TextAlign.center,
                      style: TextStyle(fontSize: 20),
                    ),
                    const Spacer(),
                    Container(
                      margin: const EdgeInsets.symmetric(vertical: 10),
                      child: TextFormField(
                        controller: _nameController,
                        cursorColor: colors.foreground,
                        decoration: getInputDecoration("Name"),
                      ),
                    ),
                    Container(
                      margin: const EdgeInsets.symmetric(vertical: 10),
                      child: TextFormField(
                        controller: _usernameController,
                        decoration: getInputDecoration("Username"),
                      ),
                    ),
                    Container(
                      margin: const EdgeInsets.symmetric(vertical: 10),
                      child: TextFormField(
                        controller: _initialBalanceController,
                        decoration: getInputDecoration("Starting account balance"),
                        inputFormatters: [
                          FilteringTextInputFormatter.digitsOnly
                        ],
                      ),
                    ),
                    Container(
                      margin: const EdgeInsets.symmetric(vertical: 10, horizontal: 0),
                      child: Util.roundedButton(
                        [
                          const Text("Create account!")
                        ],
                        onPressed: _handleClickCreateAccount,
                        colors: colors,
                        backgroundColor: DarkColorTheme().navigationBackground,
                        foregroundColor: DarkColorTheme().foreground,
                      ),
                    ),
                    Container(
                      margin: const EdgeInsets.symmetric(vertical: 10),
                      child: Divider(thickness: 4, color: colors.softForeground),
                    ),
                    Container(
                      margin: const EdgeInsets.symmetric(vertical: 10, horizontal: 0),
                      child: Util.roundedButton(
                        [
                          Util.loadImage(
                            "https://upload.wikimedia.org/wikipedia/commons/thumb/5/53/Google_%22G%22_Logo.svg/1200px-Google_%22G%22_Logo.svg.png",
                            20,
                          ),
                          const SizedBox(width: 10),
                          const Text(
                            "Change google account",
                            overflow: TextOverflow.fade,
                            maxLines: 1,
                            softWrap: false,
                          ),
                        ],
                        colors: colors,
                        backgroundColor: DarkColorTheme().navigationBackground,
                        foregroundColor: DarkColorTheme().foreground,
                        onPressed: _handleClickChangeGoogleAccount,
                      ),
                    ),
                    const SizedBox(height: 10),
                  ],
                ),
              ),
            ),
            Container(
              color: Colors.green,
              height: 40,
              child: Center(
                child: Text(
                  "Google account: ${widget.user.email}",
                  textAlign: TextAlign.center,
                  style: TextStyle(
                    color: DarkColorTheme().foreground,
                  ),
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }
}
