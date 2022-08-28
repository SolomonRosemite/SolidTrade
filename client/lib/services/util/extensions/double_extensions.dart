import 'package:intl/intl.dart';

const Map<String, String> _currencyTranslations = {
  "USD": "\$",
  "CAD": "\$",
  "EUR": "€",
  "GBP": "£",
  "JPY": "¥",
};

String _currencyCodeToSymbol(String currencyCode) {
  currencyCode = currencyCode.toUpperCase();
  final hasSymbol = _currencyTranslations.containsKey(currencyCode);

  return hasSymbol ? _currencyTranslations[currencyCode]! : currencyCode.toUpperCase();
}

extension StringExtension on double {
  String toDefaultPrice({int maxFractionDigits = 5, String currencyCode = "EUR"}) {
    final _format = NumberFormat("###,##0.00", "en_US");
    _format.maximumFractionDigits = maxFractionDigits;

    return _format.format(this) + _currencyCodeToSymbol(currencyCode);
  }

  String toDefaultNumber({int maxFractionDigits = 5, String? suffix}) {
    final _format = NumberFormat("###,##0.00", "en_US");
    _format.maximumFractionDigits = maxFractionDigits;

    final formattedNumber = _format.format(this);
    return suffix == null ? formattedNumber : formattedNumber + suffix;
  }
}
