import 'package:solidtrade/data/entities/historicalposition.dart';
import 'package:solidtrade/data/models/common/constants.dart';
import 'package:solidtrade/data/models/request_response/request_response.dart';
import 'package:solidtrade/services/request/base/base_request_service.dart';
import 'dart:convert';

class HistoricalPositionsDataRequestService extends IBaseRequestService {
  Future<RequestResponse<List<HistoricalPosition>>> fetchHistoricalPosition(int userId) async {
    return await makeRequest<List<HistoricalPosition>>(
      HttpMethod.get,
      Constants.endpointHistoricalPosition + userId.toString(),
    ).createCustom((input) {
      var data = jsonDecode(input.body) as List<dynamic>;
      var result = data.map((e) => HistoricalPosition.fromJson(e));
      return result.toList();
    });
  }
}
