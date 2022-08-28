import 'package:rxdart/subjects.dart';
import 'package:solidtrade/data/dtos/shared/request/buy_or_sell_position.dart';
import 'package:solidtrade/data/entities/stock_positions.dart';
import 'package:solidtrade/data/models/enums/shared_enums/buy_or_sell.dart';
import 'package:solidtrade/data/models/request_response/request_response.dart';
import 'package:solidtrade/services/request/data_request_service.dart';
import 'package:solidtrade/services/stream/base/base_service.dart';

class StockService extends IService<StockPosition?> {
  StockService() : super(BehaviorSubject.seeded(null));

  Future<RequestResponse<StockPosition>> buyOrSellAtMarketPrice(
    BuyOrSell buyOrSell,
    String isinWithExchangeExtension,
    double numberOfShares,
  ) async {
    var result = await DataRequestService.stockDataRequestService.buyOrSellAtMarketPrice(
      buyOrSell,
      BuyOrSellRequestDto(
        isin: isinWithExchangeExtension,
        numberOfShares: numberOfShares,
      ),
    );

    if (result.isSuccessful) {
      behaviorSubject.add(result.result);
    }

    return result;
  }
}
