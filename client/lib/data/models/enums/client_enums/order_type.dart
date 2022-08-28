import 'package:solidtrade/data/models/enums/entity_enums/enter_or_exit_position_type.dart';
import 'package:solidtrade/data/models/enums/shared_enums/buy_or_sell.dart';

enum OrderType {
  market,
  stop,
  limit,
}

extension OrderTypeExtension on OrderType {
  String get name {
    switch (this) {
      case OrderType.market:
        return "Market";
      case OrderType.stop:
        return "Stop";
      case OrderType.limit:
        return "Limit";
    }
  }

  String get fullName {
    switch (this) {
      case OrderType.market:
        return "Market order";
      case OrderType.stop:
        return "Stop order";
      case OrderType.limit:
        return "Limit order";
    }
  }

  EnterOrExitPositionType toEnterOrExitPosition(BuyOrSell buyOrSell) {
    switch (this) {
      case OrderType.stop:
        switch (buyOrSell) {
          case BuyOrSell.buy:
            return EnterOrExitPositionType.buyStopOrder;
          case BuyOrSell.sell:
            return EnterOrExitPositionType.sellStopOrder;
        }
      case OrderType.limit:
        switch (buyOrSell) {
          case BuyOrSell.buy:
            return EnterOrExitPositionType.buyLimitOrder;
          case BuyOrSell.sell:
            return EnterOrExitPositionType.sellLimitOrder;
        }
      case OrderType.market:
        throw "Did not expect market order type";
    }
  }
}
