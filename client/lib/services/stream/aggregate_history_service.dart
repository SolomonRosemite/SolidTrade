import 'package:solidtrade/data/models/common/constants.dart';
import 'package:solidtrade/data/models/enums/client_enums/chart_date_range_view.dart';
import 'package:solidtrade/data/models/request_response/request_response.dart';
import 'package:solidtrade/data/models/trade_republic/tr_aggregate_history.dart';
import 'package:solidtrade/services/request/data_request_service.dart';

// We are not using the IService because we won't be needing the BehaviorSubject nor any streams.
class AggregateHistoryService {
  final Map<String, TrAggregateHistory> _histories = {};

  Future<RequestResponse<TrAggregateHistory>> getTrAggregateHistory(String isinWithExtension, ChartDateRangeView range) async {
    final key = "$isinWithExtension-$range";

    if (_histories.containsKey(key)) {
      return Future.value(RequestResponse.successful(_histories[key]!));
    }

    var requestString = Constants.getTrAggregateHistoryRequestString(isinWithExtension, range.name);
    var result = await DataRequestService.trApiDataRequestService.makeRequest<TrAggregateHistory>(requestString);

    if (result.isSuccessful) {
      _histories["$isinWithExtension-$range"] = result.result!;
    }

    return result;
  }
}
