import 'dart:io';
import 'dart:typed_data';

import 'package:crop/crop.dart';
import 'package:firebase_auth/firebase_auth.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter/material.dart';
import 'package:image_cropper/image_cropper.dart';
import 'package:image_picker/image_picker.dart';
import 'package:solidtrade/components/base/st_widget.dart';
import 'package:solidtrade/components/login/login_screen.dart';
import 'package:solidtrade/pages/login/signup/continue_signup_page.dart';
import 'package:solidtrade/pages/settings/crop_image.dart';
import 'package:solidtrade/services/util/debug/log.dart';
import 'package:solidtrade/services/util/user_util.dart';
import 'package:solidtrade/services/util/util.dart';
import 'dart:ui' as ui;

class LoginSignUp extends StatefulWidget {
  const LoginSignUp({Key? key}) : super(key: key);

  @override
  State<LoginSignUp> createState() => _LoginSignUpState();
}

class _LoginSignUpState extends State<LoginSignUp> with STWidget {
  Uint8List? imageAsBytes;
  bool showSeedInputField = true;

  final cropController = CropController(aspectRatio: 1 / 1);
  String _dicebearSeed = "your-custom-seed";
  late String _tempCurrentSeed;

  Future<void> _handleChangeSeed(String seed) async {
    if (seed.length > 100) {
      return;
    }

    _tempCurrentSeed = seed;

    await Future.delayed(const Duration(milliseconds: 400));

    if (_tempCurrentSeed != seed) {
      return;
    }

    setState(() {
      _dicebearSeed = seed;
    });
  }

  Future<void> _handleClickUploadImage() async {
    final ImagePicker _picker = ImagePicker();
    final XFile? image = await _picker.pickImage(source: ImageSource.gallery);
    if (image == null) return;

    if (kIsWeb) {
      ui.Image? croppedImage = await Util.pushToRoute<ui.Image>(context, CropImageScreen(bytes: await image.readAsBytes()));

      if (croppedImage == null) return;

      var buffer = (await croppedImage.toByteData())!.buffer;

      setState(() {
        imageAsBytes = Uint8List.view(buffer);
      });

      return;
    }

    Log.d(image.name);
    Log.d(image.mimeType);
    Log.d(image.path);

    File? cropped;
    var isGifFile = image.name.endsWith(".gif");

    // For gif's cropping can not be applied
    cropped = isGifFile
        ? File(image.path)
        : await ImageCropper().cropImage(
            sourcePath: image.path,
            aspectRatio: const CropAspectRatio(ratioX: 1.0, ratioY: 1.0),
            aspectRatioPresets: [
              CropAspectRatioPreset.square
            ],
            androidUiSettings: const AndroidUiSettings(
              toolbarTitle: 'Cropper',
              toolbarColor: Colors.deepOrange,
              toolbarWidgetColor: Colors.white,
              initAspectRatio: CropAspectRatioPreset.square,
            ),
          );

    if (cropped == null) return;

    setState(() {
      showSeedInputField = false;
      imageAsBytes = cropped!.readAsBytesSync();
    });
  }

  Future<void> _handleClickContinueSignUp() async {
    var user = FirebaseAuth.instance.currentUser ?? await UtilUserService.signInWithGoogle();

    if (user == null) {
      Util.openDialog(context, "Login failed", message: "Something went wrong with the login. Please try again.");
      return;
    }

    Util.pushToRoute(context, ContinueSignupScreen(user: user, dicebearSeed: _dicebearSeed, profilePictureBytes: imageAsBytes));
  }

  @override
  Widget build(BuildContext context) {
    return LoginScreen(
      imageUrl: "https://avatars.dicebear.com/api/micah/$_dicebearSeed.svg",
      imageAsBytes: imageAsBytes,
      title: "Welcome to Solidtrade!",
      subTitle: "Ready to create your solidtrade profile? Let's start with your profile picture!\nType a custom seed to generate a picture or upload your own custom image.",
      additionalWidgets: [
        showSeedInputField
            ? SizedBox(
                child: TextFormField(
                  decoration: const InputDecoration(
                    isDense: true,
                    contentPadding: EdgeInsets.all(10),
                    border: OutlineInputBorder(),
                    hintText: 'Why not enter your name 😉',
                  ),
                  style: const TextStyle(fontSize: 16),
                  textAlign: TextAlign.center,
                  initialValue: _dicebearSeed,
                  onChanged: _handleChangeSeed,
                ),
              )
            : const SizedBox.shrink(),
        const SizedBox(height: 10),
        Util.roundedButton(
          [
            const SizedBox(width: 2),
            const Text(
              "Upload own picture. GIFs are also supported!",
            ),
            const SizedBox(width: 2),
          ],
          colors: colors,
          onPressed: _handleClickUploadImage,
        ),
        const SizedBox(height: 10),
        Util.roundedButton(
          [
            const Spacer(flex: 3),
            const Text("Looks good? Continue here"),
            const Spacer(),
            const Icon(Icons.chevron_right),
            const Spacer(),
          ],
          colors: colors,
          onPressed: _handleClickContinueSignUp,
        ),
      ],
    );
  }
}
