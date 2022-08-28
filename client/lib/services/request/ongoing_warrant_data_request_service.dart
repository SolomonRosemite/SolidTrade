import 'package:simple_json_mapper/simple_json_mapper.dart';
import 'package:solidtrade/data/dtos/shared/request/ongoing_position_request_dto.dart';
import 'package:solidtrade/data/entities/ongoing_warrant_position.dart';
import 'package:solidtrade/data/models/common/constants.dart';
import 'package:solidtrade/data/models/enums/shared_enums/buy_or_sell.dart';
import 'package:solidtrade/data/models/request_response/request_response.dart';
import 'package:solidtrade/services/request/base/base_request_service.dart';

class OngoingWarrantDataRequestService extends IBaseRequestService {
  Future<RequestResponse<OngoingWarrantPosition>> enterOrExitOngoingOrder(BuyOrSell buyOrSell, OngoingPositionRequestDto dto) async {
    final body = JsonMapper.serialize(dto);
    return await makeRequest<OngoingWarrantPosition>(
      buyOrSell == BuyOrSell.buy ? HttpMethod.post : HttpMethod.delete,
      Constants.endpointOngoingWarrant,
      body: body,
    ).create((data) => OngoingWarrantPosition.fromJson(data));
  }
}
