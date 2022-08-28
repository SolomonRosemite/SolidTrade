import 'dart:io';
import 'dart:typed_data';

import 'package:flutter/foundation.dart';
import 'package:flutter/material.dart';
import 'package:get_it/get_it.dart';
import 'package:image_cropper/image_cropper.dart';
import 'package:image_picker/image_picker.dart';
import 'package:solidtrade/app/main_common.dart';
import 'package:solidtrade/components/base/st_page.dart';
import 'package:solidtrade/components/base/st_stream_builder.dart';
import 'package:solidtrade/components/base/st_widget.dart';
import 'package:solidtrade/components/custom/timer_button.dart';
import 'package:solidtrade/data/dtos/user/request/update_user_dto.dart';
import 'package:solidtrade/data/entities/user.dart';
import 'package:solidtrade/data/models/common/constants.dart';
import 'package:solidtrade/data/models/enums/client_enums/lang_ticker.dart';
import 'package:solidtrade/pages/settings/app_preferences_profile_settings_page.dart';
import 'package:solidtrade/pages/settings/crop_image_page.dart';
import 'package:solidtrade/providers/language/language_provider.dart';
import 'package:solidtrade/providers/theme/app_theme.dart';
import 'package:solidtrade/services/stream/user_service.dart';
import 'package:solidtrade/services/util/local_auth_util.dart';

import 'package:solidtrade/services/util/user_util.dart';
import 'package:solidtrade/services/util/util.dart';

class AppPreferences extends StatefulWidget {
  const AppPreferences({Key? key, required this.user}) : super(key: key);
  final User user;

  @override
  State<AppPreferences> createState() => _AppPreferencesState();
}

class _AppPreferencesState extends State<AppPreferences> with STWidget {
  final _userService = GetIt.instance.get<UserService>();
  late int _initialColorTheme;
  late int _initialLanguage;

  late UpdateUserDto _updateUserDto;
  Uint8List? _imageAsBytes;

  @override
  void initState() {
    super.initState();
    _initialColorTheme = colors.themeColorType.index;
    _initialLanguage = translations.langTicker.index;

    _updateUserDto = UpdateUserDto(
      bio: widget.user.bio,
      email: widget.user.email!,
      displayName: widget.user.displayName,
      publicPortfolio: widget.user.hasPublicPortfolio,
      username: widget.user.username,
    );
  }

  final ButtonStyle _roundedButtonStyle = ButtonStyle(
    shape: MaterialStateProperty.all<RoundedRectangleBorder>(
      RoundedRectangleBorder(
        borderRadius: BorderRadius.circular(10.0),
      ),
    ),
  );

  bool get _hasUpdatedPreferences => _hasUpdatedAppSettings || _hasUpdateProfile;

  bool get _hasUpdatedAppSettings {
    return _initialColorTheme != colors.themeColorType.index || _initialLanguage != translations.langTicker.index;
  }

  bool get _hasUpdateProfile {
    return _updateUserDto.hasUpdatedProfileByUser(widget.user) || _imageAsBytes != null;
  }

  void revertAppSettingsChanges() {
    configurationProvider.languageProvider.updateLanguage(LanguageProvider.byTicker(LanguageTicker.values[_initialLanguage]).language);
    configurationProvider.themeProvider.updateTheme(ColorThemeType.values[_initialColorTheme]);
  }

  Future<void> _handleUpdateProfile() async {
    _updateUserDto.profilePictureFile = _imageAsBytes;

    if (!_hasUpdateProfile) {
      Navigator.pop(context);
      return;
    }

    var closeLoadingDialog = Util.showLoadingDialog(context);

    var response = await _userService.updateUser(_updateUserDto);
    closeLoadingDialog();

    if (response.isSuccessful) {
      Navigator.pop(context);
      return;
    }

    Util.openDialog(
      context,
      "Updating user failed",
      message: response.error!.userFriendlyMessage!,
    );
  }

  Future<void> _handleDiscardProfileUpdate() async {
    if (!_hasUpdatedPreferences) {
      Navigator.pop(context);
      return;
    }

    if (await Util.showUnsavedChangesWarningDialog(context)) {
      revertAppSettingsChanges();
      Navigator.of(context).pop();
    }
  }

  Future<void> _handleClickDeleteAccount() async {
    bool deleteAccount = false;

    await Util.openDialog(
      context,
      "Delete Account",
      message: "Are you sure you want to delete your account?",
      actionWidgets: [
        TextButton(
          child: const Text("Dont delete account"),
          onPressed: () => Navigator.of(context).pop(),
        ),
        TimerButton(
          onPressed: () {
            deleteAccount = true;
            Navigator.of(context).pop();
          },
          text: "Delete account",
          initialSecondsLeft: 5,
          enabledButtonStyle: _roundedButtonStyle,
          disabledButtonStyle: _roundedButtonStyle,
        ),
      ],
    );

    if (!deleteAccount || !(await UtilLocalAuth.authenticate())) {
      return;
    }

    var response = await UtilUserService.deleteAccount(_userService);

    var title = response.isSuccessful ? "Account deleted" : "Account deletion failed";

    await Util.openDialog(
      context,
      title,
      message: response.isSuccessful ? "Account deleted successfully.\nPress okay to continue." : response.error!.userFriendlyMessage,
      closeText: "Okay",
    );

    if (response.isSuccessful) {
      myAppState.restart();
    }
  }

  Future<void> _handleClickSignOut() async {
    bool signOut = false;

    await Util.openDialog(
      context,
      "Sign out",
      message: "Are you sure you want to sign out?",
      actionWidgets: [
        TextButton(
          child: const Text("Dont sign out"),
          onPressed: () => Navigator.of(context).pop(),
        ),
        TextButton(
          child: const Text("Sign out"),
          style: _roundedButtonStyle.copyWith(
            backgroundColor: MaterialStateProperty.all(Colors.red),
            foregroundColor: MaterialStateProperty.all(Colors.white),
          ),
          onPressed: () {
            signOut = true;
            Navigator.of(context).pop();
          },
        ),
      ],
    );

    if (!signOut || !(await UtilLocalAuth.authenticate())) {
      return;
    }

    await UtilUserService.signOut();

    await Util.openDialog(
      context,
      "Sign out was successful",
      message: "Sign out was successful.\nPress okay to continue.",
      closeText: "Okay",
    );

    myAppState.restart();
  }

  Future<void> _handleEditProfileClick() async {
    var userDto = await Util.pushToRoute<UpdateUserDto>(
      context,
      ProfileSettingsPage(updateUserDto: _updateUserDto),
    );

    if (userDto == null) {
      return;
    }

    setState(() {
      _updateUserDto = userDto;
    });
  }

  Future<void> _handleClickChangeProfilePicture() async {
    final ImagePicker _picker = ImagePicker();
    final XFile? image = await _picker.pickImage(source: ImageSource.gallery);
    if (image == null) return;

    if (await image.length() > Constants.fileUploadLimitInBytes) {
      Util.openDialog(context, "File too large", message: "Sorry, this image is too big.");
      return;
    }

    var isGifFile = image.name.endsWith(".gif");

    if (kIsWeb) {
      var bytes = await image.readAsBytes();

      if (!isGifFile) {
        final closeDialog = Util.showLoadingDialog(context, showIndicator: false, waitingText: "Loading. This might take a while...");

        await Future.delayed(const Duration(milliseconds: 400));

        var cropResult = await Navigator.push<Future<Uint8List>?>(
          context,
          MaterialPageRoute(
            builder: (ctx) => Cropper(
              image: bytes,
            ),
          ),
        );

        closeDialog();

        if (cropResult == null) {
          return;
        }

        var croppedImageResult = await cropResult;

        setState(() {
          _imageAsBytes = croppedImageResult;
        });

        return;
      }

      setState(() {
        _imageAsBytes = bytes;
      });
      return;
    }

    File? cropped;

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
      _imageAsBytes = cropped!.readAsBytesSync();
    });
  }

  Widget _settingsButton({
    required String text,
    required void Function() callback,
    double width = double.infinity,
    double fontSize = 15,
    double height = 50,
    FontWeight? fontWeight,
    Color? textColor,
  }) {
    return SizedBox(
      width: width,
      height: height,
      child: TextButton(
        onPressed: () => callback(),
        child: Text(
          text,
          style: TextStyle(color: textColor ?? colors.foreground, fontSize: fontSize, fontWeight: fontWeight),
        ),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return STPage(
      page: () => STStreamBuilder<User>(
        stream: _userService.stream$,
        builder: (context, user) => Scaffold(
          appBar: AppBar(
            backgroundColor: colors.background,
            foregroundColor: colors.foreground,
            leading: IconButton(icon: Icon(Icons.close, size: 25, color: Colors.red[300]), onPressed: _handleDiscardProfileUpdate),
            title: const Text(
              "Preferences",
            ),
            elevation: 5,
            centerTitle: true,
            actions: <Widget>[
              IconButton(icon: Icon(Icons.done, size: 25, color: Colors.blue[300]), onPressed: _handleUpdateProfile)
            ],
          ),
          body: Column(
            mainAxisSize: MainAxisSize.max,
            mainAxisAlignment: MainAxisAlignment.start,
            crossAxisAlignment: CrossAxisAlignment.center,
            children: <Widget>[
              const SizedBox(height: 20),
              Text(
                _updateUserDto.displayName,
                textAlign: TextAlign.center,
                style: const TextStyle(fontSize: 28, fontWeight: FontWeight.bold),
              ),
              const SizedBox(height: 40),
              ClipRRect(
                borderRadius: BorderRadius.circular(20),
                child: Material(
                  elevation: 5,
                  color: colors.background,
                  child: GestureDetector(
                    onTap: _handleClickChangeProfilePicture,
                    child: Container(
                      child: _imageAsBytes != null
                          ? Util.loadImageFromMemory(
                              _imageAsBytes!,
                              150.0,
                              borderRadius: BorderRadius.circular(20),
                              boxFit: BoxFit.cover,
                              loadingBoxShape: BoxShape.rectangle,
                            )
                          : Util.loadImage(
                              user.profilePictureUrl,
                              150.0,
                              borderRadius: BorderRadius.circular(20),
                              boxFit: BoxFit.cover,
                              loadingBoxShape: BoxShape.rectangle,
                            ),
                    ),
                  ),
                ),
              ),
              const SizedBox(height: 10),
              TextButton(
                onPressed: _handleClickChangeProfilePicture,
                child: const Text(
                  'Change Profile Picture',
                  style: TextStyle(fontWeight: FontWeight.normal),
                ),
              ),
              const Spacer(flex: 1000),
              _settingsButton(
                callback: _handleEditProfileClick,
                text: 'Edit profile',
              ),
              _settingsButton(
                callback: () => UtilCupertino.showCupertinoDialog(
                  context,
                  title: 'Language',
                  message: 'Choose a language',
                  widgets: UtilCupertino.languageActionSheets(context, configurationProvider.languageProvider),
                ),
                text: translations.settings.changeLanguage,
              ),
              _settingsButton(
                callback: () => UtilCupertino.showCupertinoDialog(
                  context,
                  title: 'Change Theme',
                  message: 'Choose a color theme',
                  widgets: UtilCupertino.colorThemeActionSheets(context, configurationProvider.themeProvider),
                ),
                text: translations.settings.changeTheme,
              ),
              const SizedBox(height: 10),
              Divider(thickness: 5, color: colors.softBackground),
              LayoutBuilder(
                builder: (context, constrains) => Row(
                  children: [
                    _settingsButton(
                      callback: _handleClickSignOut,
                      text: "Sign out",
                      textColor: Colors.red,
                      width: constrains.maxWidth / 2,
                    ),
                    _settingsButton(
                      callback: _handleClickDeleteAccount,
                      text: "Delete Account",
                      textColor: Colors.red,
                      width: constrains.maxWidth / 2,
                    ),
                  ],
                ),
              ),
              const SizedBox(height: 5),
              const Spacer(flex: 1),
            ],
          ),
        ),
      ),
    );
  }
}
