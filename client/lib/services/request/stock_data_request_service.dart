import 'package:simple_json_mapper/simple_json_mapper.dart';
import 'package:solidtrade/data/dtos/shared/request/buy_or_sell_position.dart';
import 'package:solidtrade/data/entities/stock_positions.dart';
import 'package:solidtrade/data/models/common/constants.dart';
import 'package:solidtrade/data/models/enums/shared_enums/buy_or_sell.dart';
import 'package:solidtrade/data/models/request_response/request_response.dart';
import 'package:solidtrade/services/request/base/base_request_service.dart';

class StockDataRequestService extends IBaseRequestService {
  Future<RequestResponse<StockPosition>> buyOrSellAtMarketPrice(BuyOrSell buyOrSell, BuyOrSellRequestDto dto) async {
    final body = JsonMapper.serialize(dto);
    return await makeRequest<StockPosition>(
      buyOrSell == BuyOrSell.buy ? HttpMethod.post : HttpMethod.delete,
      Constants.endpointStock,
      body: body,
    ).create((data) => StockPosition.fromJson(data));
  }
}
