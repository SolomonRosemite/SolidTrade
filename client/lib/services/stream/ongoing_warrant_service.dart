import 'package:rxdart/subjects.dart';
import 'package:solidtrade/data/dtos/shared/request/ongoing_position_request_dto.dart';
import 'package:solidtrade/data/entities/ongoing_warrant_position.dart';
import 'package:solidtrade/data/models/enums/client_enums/order_type.dart';
import 'package:solidtrade/data/models/enums/shared_enums/buy_or_sell.dart';
import 'package:solidtrade/data/models/request_response/request_response.dart';
import 'package:solidtrade/services/request/data_request_service.dart';
import 'package:solidtrade/services/stream/base/base_service.dart';

class OngoingWarrantService extends IService<OngoingWarrantPosition?> {
  OngoingWarrantService() : super(BehaviorSubject.seeded(null));

  Future<RequestResponse<OngoingWarrantPosition>> enterOrExitOngoingOrder(
    BuyOrSell buyOrSell,
    OrderType orderType,
    String isinWithExchangeExtension,
    double numberOfShares,
    DateTime goodUntil,
    double price,
  ) async {
    var result = await DataRequestService.ongoingWarrantDataRequestService.enterOrExitOngoingOrder(
      buyOrSell,
      OngoingPositionRequestDto(
        type: orderType.toEnterOrExitPosition(buyOrSell),
        goodUntil: goodUntil,
        isin: isinWithExchangeExtension,
        numberOfShares: numberOfShares,
        priceThreshold: price,
      ),
    );

    if (result.isSuccessful) {
      behaviorSubject.add(result.result);
    }

    return result;
  }
}
