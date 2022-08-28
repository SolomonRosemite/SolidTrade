import 'package:rxdart/subjects.dart';
import 'package:solidtrade/data/models/common/constants.dart';
import 'package:solidtrade/data/models/request_response/request_response.dart';
import 'package:solidtrade/data/models/trade_republic/tr_stock_details.dart';
import 'package:solidtrade/services/request/data_request_service.dart';
import 'package:solidtrade/services/stream/base/base_service.dart';

class TrStockDetailsService extends IService<TrStockDetails?> {
  TrStockDetailsService() : super(BehaviorSubject.seeded(null));
  final Map<String, TrStockDetails> _cache = {};

  Future<RequestResponse<TrStockDetails>> requestTrProductInfo(String isinWithoutExtension) async {
    RequestResponse<TrStockDetails>? result;
    if (_cache.containsKey(isinWithoutExtension)) {
      result = RequestResponse.successful(_cache[isinWithoutExtension]!);
    } else {
      result = await DataRequestService.trApiDataRequestService.makeRequest<TrStockDetails>(Constants.getTrStockDetailsRequestString(isinWithoutExtension));

      if (result.isSuccessful) {
        _cache[isinWithoutExtension] = result.result!;
      }
    }

    if (result.isSuccessful) {
      behaviorSubject.add(result.result);
    }

    return result;
  }
}
