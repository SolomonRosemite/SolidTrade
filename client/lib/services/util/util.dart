import 'dart:typed_data';

import 'package:cached_network_image/cached_network_image.dart';
import 'package:enum_to_string/enum_to_string.dart';
import 'package:firebase_messaging/firebase_messaging.dart';
import 'package:flutter/cupertino.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter/material.dart';
import 'package:flutter/scheduler.dart';
import 'package:flutter_svg/flutter_svg.dart';
import 'package:skeletons/skeletons.dart';
import 'package:solidtrade/data/models/enums/client_enums/chart_date_range_view.dart';
import 'package:solidtrade/data/models/enums/client_enums/lang_ticker.dart';
import 'package:solidtrade/providers/language/language_provider.dart';
import 'package:solidtrade/providers/language/translation.dart';
import 'package:solidtrade/providers/theme/app_theme.dart';

class Util {
  static String chartDateRangeToTranslatedString(ITranslation translation, ChartDateRangeView range) {
    switch (range) {
      case ChartDateRangeView.oneDay:
        return translation.chart.chartDateRangeView.oneDay;
      case ChartDateRangeView.oneWeek:
        return translation.chart.chartDateRangeView.oneWeek;
      case ChartDateRangeView.oneMonth:
        return translation.chart.chartDateRangeView.oneMonth;
      case ChartDateRangeView.sixMonth:
        return translation.chart.chartDateRangeView.sixMonth;
      case ChartDateRangeView.oneYear:
        return translation.chart.chartDateRangeView.oneYear;
      case ChartDateRangeView.fiveYear:
        return translation.chart.chartDateRangeView.fiveYear;
    }
  }

  static Future<void> googleLoginFailedDialog(BuildContext context) {
    return Util.openDialog(context, "Google login failed", message: "Something went wrong with the login. Please try again.");
  }

  static Future<void> openDialog(
    BuildContext context,
    String title, {
    String closeText = "Okay",
    String? message,
    Iterable<String>? messages,
    List<Widget>? widgets,
    List<Widget>? actionWidgets,
  }) {
    if (widgets == null) {
      if (messages == null) {
        widgets = [
          Text(message!)
        ];
      } else {
        widgets = messages.map((text) => Text(text)).toList();
      }
    }

    return showDialog<void>(
      context: context,
      builder: (BuildContext context) {
        return AlertDialog(
          backgroundColor: Theme.of(context).backgroundColor,
          title: Text(title),
          content: SingleChildScrollView(
            child: ListBody(
              children: [
                ...widgets!
              ],
            ),
          ),
          actions: actionWidgets ??
              [
                TextButton(
                  child: Text(closeText),
                  onPressed: () => Navigator.of(context).pop(),
                ),
              ],
        );
      },
    );
  }

  static Future<bool> requestNotificationPermissionsWithUserFriendlyPopup(BuildContext context) async {
    var currentSettings = await FirebaseMessaging.instance.getNotificationSettings();

    if (currentSettings.authorizationStatus == AuthorizationStatus.authorized) {
      return true;
    }

    // TODO: If we plan to add notifications that will appear to the user. This message must be changed.
    // Because saying "notifications will NOT appear to the user." doesn't apply then anymore and therefor the message must be changed.
    await openDialog(
      context,
      "Allow notifications",
      message: "A popup will appear and ask permissions to send notifications. This is necessary for the server communication. Don't worry notifications will NOT appear to the user.",
    );

    var settings = await FirebaseMessaging.instance.requestPermission();
    final isGranted = settings.authorizationStatus == AuthorizationStatus.authorized;

    if (!isGranted) {
      await openDialog(context, "Oh snap", message: "Seems like notifications has been denied for solidtrade. Please open your browser notifications settings and allow notifications for solidtrade.");
    }

    return isGranted;
  }

  static Widget loadImage(
    String url,
    double size, {
    BorderRadius? borderRadius,
    BoxFit? boxFit,
    BoxShape loadingBoxShape = BoxShape.circle,
    Color backgroundColor = Colors.transparent,
  }) {
    borderRadius ??= BorderRadius.circular(90);

    return SizedBox(
      width: size,
      height: size,
      child: ClipRRect(
        borderRadius: borderRadius,
        child: Container(
          color: backgroundColor,
          child: CachedNetworkImage(
            fit: boxFit,
            imageUrl: url,
            height: size,
            width: size,
            placeholder: (context, url) => SkeletonAvatar(
              style: SkeletonAvatarStyle(shape: loadingBoxShape),
            ),
            errorWidget: (context, url, error) => loadSvgImage(url, size, size),
          ),
        ),
      ),
    );
  }

  static Widget loadImageFromMemory(Uint8List bytes, double size, {BorderRadius? borderRadius, BoxFit? boxFit, BoxShape loadingBoxShape = BoxShape.circle}) {
    borderRadius ??= BorderRadius.circular(90);

    return SizedBox(
      width: size,
      height: size,
      child: ClipRRect(
        borderRadius: borderRadius,
        child: Image.memory(
          bytes,
          fit: boxFit,
          height: size,
          width: size,
        ),
      ),
    );
  }

  static Widget loadImageFromAssets(String assetName, double size, {BorderRadius? borderRadius, BoxFit? boxFit, BoxShape loadingBoxShape = BoxShape.circle}) {
    borderRadius ??= BorderRadius.circular(90);

    return SizedBox(
      width: size,
      height: size,
      child: ClipRRect(
        borderRadius: borderRadius,
        child: Image.asset(
          assetName,
          fit: boxFit,
          height: size,
          width: size,
        ),
      ),
    );
  }

  static Widget loadSvgImage(String url, double width, double height) {
    return SvgPicture.network(
      url,
      width: width,
      height: height,
      placeholderBuilder: (BuildContext _) => Container(
        padding: EdgeInsets.symmetric(horizontal: width, vertical: height),
        width: width,
        height: height,
        child: SizedBox(
          width: width,
          height: height,
          child: const SkeletonAvatar(
            style: SkeletonAvatarStyle(shape: BoxShape.circle),
          ),
        ),
      ),
    );
  }

  static Widget roundedButton(
    List<Widget> content, {
    required void Function() onPressed,
    required IColorTheme colors,
    BorderRadius? borderRadius,
    Color? backgroundColor,
    Color? foregroundColor,
    double height = 50,
  }) {
    foregroundColor ??= colors.foreground;
    backgroundColor ??= colors.background;
    borderRadius ??= BorderRadius.circular(45);

    return ClipRRect(
      borderRadius: borderRadius,
      child: SizedBox(
        height: height,
        child: TextButton(
          style: ButtonStyle(
            foregroundColor: MaterialStateProperty.all(foregroundColor),
            backgroundColor: MaterialStateProperty.all(backgroundColor),
          ),
          onPressed: onPressed,
          child: Row(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              ...content
            ],
          ),
        ),
      ),
    );
  }

  static Future<T?> pushToRoute<T>(BuildContext context, Widget route) {
    return Navigator.push<T>(context, MaterialPageRoute(builder: (context) => route));
  }

  static ColorThemeType currentDeviceColorTheme(BuildContext context) {
    if (kIsWeb) {
      var brightness = SchedulerBinding.instance!.window.platformBrightness;
      bool isDarkMode = brightness == Brightness.dark;

      return isDarkMode ? ColorThemeType.dark : ColorThemeType.light;
    }

    var brightness = MediaQuery.of(context).platformBrightness;
    bool isDarkMode = brightness == Brightness.dark;

    return isDarkMode ? ColorThemeType.dark : ColorThemeType.light;
  }

  static LanguageTicker currentDeviceLanguage(BuildContext context) {
    final Locale appLocale = Localizations.localeOf(context);

    var tickers = LanguageTicker.values.map((e) {
      var s = e.toString();
      return s.substring(s.indexOf(".") + 1);
    });

    if (!tickers.any((ticker) => ticker == appLocale.languageCode)) {
      return LanguageTicker.en;
    }

    return EnumToString.fromString(LanguageTicker.values, tickers.firstWhere((ticker) => ticker == appLocale.languageCode))!;
  }

  static void Function() showLoadingDialog(BuildContext context, {String waitingText = "Loading...", bool showIndicator = true}) {
    showDialog(
      context: context,
      barrierDismissible: false,
      builder: (BuildContext context) {
        return SimpleDialog(
          backgroundColor: Theme.of(context).backgroundColor,
          children: [
            Center(
              child: Row(
                children: [
                  const SizedBox(width: 20),
                  showIndicator
                      ? CircularProgressIndicator(
                          backgroundColor: Colors.blue[100],
                          valueColor: AlwaysStoppedAnimation(Colors.blue[800]),
                          strokeWidth: 4,
                        )
                      : const SizedBox.shrink(),
                  const SizedBox(
                    height: 10,
                    width: 20,
                  ),
                  Text(waitingText),
                ],
              ),
            ),
          ],
        );
      },
    );

    return () => Navigator.of(context, rootNavigator: true).pop();
  }

  static Future<bool> showUnsavedChangesWarningDialog(
    BuildContext context, {
    String title = 'Unsaved Settings',
    String content = 'Are you sure you dont want to Save?',
    String confirmText = 'Don\'t Save changes',
  }) async {
    bool discardChanges = false;

    await openDialog(context, title, message: content, actionWidgets: [
      TextButton(
        child: const Text('Go Back'),
        onPressed: () => Navigator.pop(context),
      ),
      TextButton(
        style: ButtonStyle(
          backgroundColor: MaterialStateProperty.all(Colors.red),
          foregroundColor: MaterialStateProperty.all(Colors.white),
          shape: MaterialStateProperty.all<RoundedRectangleBorder>(
            RoundedRectangleBorder(
              borderRadius: BorderRadius.circular(10.0),
            ),
          ),
        ),
        child: Text(confirmText),
        onPressed: () {
          discardChanges = true;
          Navigator.of(context).pop();
        },
      ),
    ]);

    return discardChanges;
  }
}

class UtilCupertino {
  static Future<void> showCupertinoDialog(
    BuildContext context, {
    required String title,
    required String message,
    required List<Widget> widgets,
    CupertinoActionSheetAction? cancelButton,
  }) {
    return showCupertinoModalPopup(
      context: context,
      builder: (BuildContext context) => CupertinoActionSheet(
        title: Text(title),
        message: Text(message),
        actions: widgets,
        cancelButton: cancelButton ??
            CupertinoActionSheetAction(
              onPressed: () => Navigator.pop(context),
              child: Text('Cancel', style: TextStyle(color: Colors.red[300])),
            ),
      ),
    );
  }

  static List<CupertinoActionSheetAction> languageActionSheets(BuildContext context, LanguageProvider languageProvider) {
    void onChangeLanguage(LanguageTicker langTicker) {
      languageProvider.updateLanguage(LanguageProvider.byTicker(langTicker).language);
      Navigator.pop(context);
    }

    return LanguageTicker.values
        .map((e) => CupertinoActionSheetAction(
              onPressed: () => onChangeLanguage(e),
              child: Text(e.name),
            ))
        .toList();
  }

  static List<CupertinoActionSheetAction> colorThemeActionSheets(BuildContext context, ThemeProvider themeProvider) {
    void onChangeTheme(ColorThemeType theme) {
      themeProvider.updateTheme(theme);
      Navigator.pop(context);
    }

    return ColorThemeType.values
        .map((e) => CupertinoActionSheetAction(
              onPressed: () => onChangeTheme(e),
              child: Text(e.name),
            ))
        .toList();
  }
}

Widget showLoadingWhileWaiting({required bool isLoading, required BoxShape loadingBoxShape, required Widget child}) {
  return isLoading ? showLoadingSkeleton(loadingBoxShape) : child;
}

Widget showLoadingSkeleton(BoxShape shape) {
  return SkeletonAvatar(
    style: SkeletonAvatarStyle(shape: shape),
  );
}
