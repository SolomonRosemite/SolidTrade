import 'package:simple_json_mapper/simple_json_mapper.dart';
import 'package:solidtrade/data/dtos/shared/request/ongoing_position_request_dto.dart';
import 'package:solidtrade/data/entities/ongoing_knockout_position.dart';
import 'package:solidtrade/data/models/common/constants.dart';
import 'package:solidtrade/data/models/enums/shared_enums/buy_or_sell.dart';
import 'package:solidtrade/data/models/request_response/request_response.dart';
import 'package:solidtrade/services/request/base/base_request_service.dart';

class OngoingKnockoutDataRequestService extends IBaseRequestService {
  Future<RequestResponse<OngoingKnockoutPosition>> enterOrExitOngoingOrder(BuyOrSell buyOrSell, OngoingPositionRequestDto dto) async {
    final body = JsonMapper.serialize(dto);
    return await makeRequest<OngoingKnockoutPosition>(
      buyOrSell == BuyOrSell.buy ? HttpMethod.post : HttpMethod.delete,
      Constants.endpointOngoingKnockout,
      body: body,
    ).create((data) => OngoingKnockoutPosition.fromJson(data));
  }
}
