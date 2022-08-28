import 'package:solidtrade/data/models/enums/client_enums/lang_ticker.dart';

class Quote {
  final LanguageTicker lang;
  final String quote;

  Quote({required this.lang, required this.quote});
}
