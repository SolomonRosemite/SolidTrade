import 'package:solidtrade/data/entities/historicalposition.dart';
import 'package:rxdart/rxdart.dart';
import 'package:solidtrade/data/models/request_response/request_response.dart';
import 'package:solidtrade/services/request/data_request_service.dart';
import 'package:solidtrade/services/stream/base/base_service.dart';

class HistoricalPositionService extends IService<List<HistoricalPosition>?> {
  HistoricalPositionService() : super(BehaviorSubject.seeded(null));

  Future<RequestResponse<List<HistoricalPosition>>> fetchHistoricalPositions(int userId) async {
    var future = DataRequestService.historicalPositionsDataRequestService.fetchHistoricalPosition(userId);

    var result = await future;
    if (result.isSuccessful) {
      behaviorSubject.add(result.result);
    }

    return future;
  }
}
