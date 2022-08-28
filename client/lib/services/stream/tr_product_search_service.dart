import 'package:rxdart/subjects.dart';
import 'package:solidtrade/data/models/common/constants.dart';
import 'package:solidtrade/data/models/request_response/request_response.dart';
import 'package:solidtrade/data/models/trade_republic/tr_product_search.dart';
import 'package:solidtrade/services/request/data_request_service.dart';
import 'package:solidtrade/services/stream/base/base_service.dart';

class TrProductSearchService extends IService<TrProductSearch?> {
  TrProductSearchService() : super(BehaviorSubject.seeded(null));
  final Map<String, TrProductSearch> _cache = {};

  Future<RequestResponse<TrProductSearch>> requestTrProductSearch(String search) async {
    RequestResponse<TrProductSearch>? result;
    if (_cache.containsKey(search)) {
      result = RequestResponse.successful(_cache[search]!);
    } else {
      result = await DataRequestService.trApiDataRequestService.makeRequest<TrProductSearch>(Constants.getTrProductSearchRequestString(
        search,
        // TODO: Currently only stocks will be shown. Fix...
        _SearchType.stock.name,
      ));

      if (result.isSuccessful) {
        _cache[search] = result.result!;
      }
    }

    if (result.isSuccessful) {
      behaviorSubject.add(result.result);
    }

    return result;
  }
}

enum _SearchType { stock, crypto, fund, derivatives }
