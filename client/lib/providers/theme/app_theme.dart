import 'package:flutter/material.dart';
import 'package:get_it/get_it.dart';
import 'package:shared_preferences/shared_preferences.dart';
import 'package:solidtrade/data/enums/shared_preferences_keys.dart';
import 'package:solidtrade/providers/app/app_configuration_provider.dart';

abstract class IColorTheme {
  // Info
  ColorThemeType get themeColorType;

  // Common colors
  Color get profilePictureBorder;
  Color get lessSoftForeground;
  Color get softForeground;
  Color get foreground;
  Color get background;
  Color get softBackground;
  String get logoAsGif;

  // Common stock colors
  Color get lightGreen;
  Color get midGreen;
  Color get darkGreen;

  // More stock colors
  Color get stockGreenLight;
  Color get stockGreen;

  Color get stockRedLight;
  Color get stockRed;

  Color get blueBackground;
  Color get blueText;

  // Navigation colors
  Color get selectedItem;
  Color get unselectedItem;
  Color get navigationBackground;
}

class SharedColorThemes {
  static const Color lightGreen = Color.fromRGBO(239, 251, 246, 1);
  static const Color midGreen = Color.fromRGBO(188, 236, 214, 1);
  static const Color darkGreen = Color.fromRGBO(154, 216, 187, 1);

  static const Color stockGreenLight = Color.fromRGBO(226, 250, 236, 1);
  static const Color stockGreen = Color.fromRGBO(54, 223, 120, 1);

  static const Color stockRedLight = Color.fromRGBO(255, 232, 226, 1);
  static const Color stockRed = Color.fromRGBO(252, 99, 55, 1);

  static const Color unselectedItem = Colors.grey;
}

class LightColorTheme implements IColorTheme {
  @override
  ColorThemeType get themeColorType => ColorThemeType.light;

  @override
  Color get background => Colors.white;

  @override
  Color get foreground => Colors.black;

  @override
  Color get lessSoftForeground => Colors.black54;

  @override
  Color get softForeground => Colors.black26;

  @override
  Color get softBackground => Colors.grey[300]!;

  @override
  String get logoAsGif => "assets/images/light-logo.gif";

  @override
  Color get selectedItem => Colors.black;

  @override
  Color get unselectedItem => SharedColorThemes.unselectedItem;

  @override
  Color get navigationBackground => Colors.white;

  @override
  Color get darkGreen => SharedColorThemes.darkGreen;

  @override
  Color get midGreen => SharedColorThemes.midGreen;

  @override
  Color get lightGreen => SharedColorThemes.lightGreen;

  @override
  Color get profilePictureBorder => Colors.black;

  @override
  Color get stockGreen => SharedColorThemes.stockGreen;

  @override
  Color get stockGreenLight => SharedColorThemes.stockGreenLight;

  @override
  Color get stockRed => SharedColorThemes.stockRed;

  @override
  Color get stockRedLight => SharedColorThemes.stockRedLight;

  @override
  Color get blueBackground => const Color.fromRGBO(223, 244, 255, 1);

  @override
  Color get blueText => const Color.fromRGBO(6, 155, 248, 1);
}

class DarkColorTheme implements IColorTheme {
  @override
  ColorThemeType get themeColorType => ColorThemeType.dark;

  @override
  Color get background => Colors.black;

  @override
  Color get foreground => Colors.white;

  @override
  Color get lessSoftForeground => Colors.white70;

  @override
  Color get softForeground => Colors.white24;

  @override
  Color get softBackground => Colors.grey[900]!;

  @override
  String get logoAsGif => "assets/images/dark-logo.gif";

  @override
  Color get selectedItem => Colors.white;

  @override
  Color get unselectedItem => SharedColorThemes.unselectedItem;

  @override
  Color get navigationBackground => Colors.grey[900]!;

  @override
  Color get darkGreen => SharedColorThemes.darkGreen;

  @override
  Color get midGreen => SharedColorThemes.midGreen;

  @override
  Color get lightGreen => SharedColorThemes.lightGreen;

  @override
  Color get profilePictureBorder => Colors.grey;

  @override
  Color get stockGreen => SharedColorThemes.stockGreen;

  @override
  Color get stockGreenLight => SharedColorThemes.stockGreenLight;

  @override
  Color get stockRed => SharedColorThemes.stockRed;

  @override
  Color get stockRedLight => SharedColorThemes.stockRedLight;

  @override
  Color get blueBackground => const Color.fromRGBO(0, 33, 52, 1);

  @override
  Color get blueText => const Color.fromRGBO(16, 160, 238, 1);
}

class ThemeProvider {
  ConfigurationProvider? configurationProvider;

  IColorTheme get theme => _currentTheme;
  IColorTheme _currentTheme;

  ThemeProvider(this._currentTheme);

  factory ThemeProvider.byThemeType(ColorThemeType theme) {
    return ThemeProvider(colorTypeToColorTheme(theme));
  }

  void updateTheme(ColorThemeType theme, {bool savePermanently = true}) {
    _currentTheme = colorTypeToColorTheme(theme);

    configurationProvider ??= GetIt.instance.get<ConfigurationProvider>();
    configurationProvider?.uiUpdateProvider.invokeUpdate();

    if (savePermanently) {
      final prefs = GetIt.instance.get<SharedPreferences>();
      prefs.setInt(SharedPreferencesKeys.colorTheme.toString(), theme.index);
    }
  }

  static IColorTheme colorTypeToColorTheme(ColorThemeType type) {
    switch (type) {
      case ColorThemeType.light:
        return LightColorTheme();
      case ColorThemeType.dark:
        return DarkColorTheme();
    }
  }
}

enum ColorThemeType {
  light,
  dark,
}
