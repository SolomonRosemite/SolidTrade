import 'package:rxdart/subjects.dart';
import 'package:solidtrade/data/dtos/shared/request/buy_or_sell_position.dart';
import 'package:solidtrade/data/entities/knockout_position.dart';
import 'package:solidtrade/data/models/enums/shared_enums/buy_or_sell.dart';
import 'package:solidtrade/data/models/request_response/request_response.dart';
import 'package:solidtrade/services/request/data_request_service.dart';
import 'package:solidtrade/services/stream/base/base_service.dart';

class KnockoutService extends IService<KnockoutPosition?> {
  KnockoutService() : super(BehaviorSubject.seeded(null));

  Future<RequestResponse<KnockoutPosition>> buyOrSellAtMarketPrice(
    BuyOrSell buyOrSell,
    String isinWithExchangeExtension,
    double numberOfShares,
  ) async {
    var result = await DataRequestService.knockoutDataRequestService.buyOrSellAtMarketPrice(
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
