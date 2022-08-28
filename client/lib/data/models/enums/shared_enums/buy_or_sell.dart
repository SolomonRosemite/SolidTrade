import 'package:get_it/get_it.dart';
import 'package:solidtrade/providers/app/app_configuration_provider.dart';
import 'package:solidtrade/providers/language/translation.dart';

enum BuyOrSell {
  buy,
  sell,
}

extension BuyOrSellExtension on BuyOrSell {
  bool get isBuy => this == BuyOrSell.buy;
  bool get isSell => this == BuyOrSell.sell;

  ITranslation get translation => GetIt.instance.get<ConfigurationProvider>().languageProvider.language;

  String get name {
    switch (this) {
      case BuyOrSell.buy:
        return translation.common.buyAsTextLiteral;
      case BuyOrSell.sell:
        return translation.common.sellAsTextLiteral;
    }
  }
}
