import 'package:solidtrade/providers/app/app_update_stream_provider.dart';
import 'package:solidtrade/providers/language/language_provider.dart';
import 'package:solidtrade/providers/theme/app_theme.dart';

class ConfigurationProvider {
  LanguageProvider get languageProvider => _languageProvider;
  final LanguageProvider _languageProvider;

  ThemeProvider get themeProvider => _themeProvider;
  final ThemeProvider _themeProvider;

  UIUpdateStreamProvider get updateProvider => _updateProvider;
  final UIUpdateStreamProvider _updateProvider;

  ConfigurationProvider(this._languageProvider, this._themeProvider, this._updateProvider);
}
