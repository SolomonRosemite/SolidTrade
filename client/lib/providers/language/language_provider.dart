import 'package:get_it/get_it.dart';
import 'package:shared_preferences/shared_preferences.dart';
import 'package:solidtrade/data/models/enums/client_enums/lang_ticker.dart';
import 'package:solidtrade/data/models/enums/client_enums/shared_preferences_keys.dart';
import 'package:solidtrade/providers/app/app_configuration_provider.dart';
import 'package:solidtrade/providers/language/translation.dart';

import 'de/de_translation.dart';
import 'en/en_translation.dart';

class LanguageProvider {
  ConfigurationProvider? configurationProvider;

  ITranslation get language => _currentTranslation;
  late ITranslation _currentTranslation;

  static ITranslation tickerToTranslation(LanguageTicker ticker) {
    switch (ticker) {
      case LanguageTicker.en:
        return EnTranslation();
      case LanguageTicker.de:
        return DeTranslation();
    }
  }

  factory LanguageProvider.byTicker(LanguageTicker ticker) {
    return LanguageProvider(tickerToTranslation(ticker));
  }

  LanguageProvider(this._currentTranslation);

  void updateLanguage(ITranslation lang, {bool savePermanently = true}) {
    _currentTranslation = lang;

    configurationProvider ??= GetIt.instance.get<ConfigurationProvider>();
    configurationProvider?.uiUpdateProvider.invokeUpdate();

    if (savePermanently) {
      final prefs = GetIt.instance.get<SharedPreferences>();
      prefs.setInt(SharedPreferencesKeys.langTicker.toString(), lang.langTicker.index);
    }
  }
}
