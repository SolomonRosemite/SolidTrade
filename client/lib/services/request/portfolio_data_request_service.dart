import 'package:solidtrade/data/entities/portfolio.dart';
import 'package:solidtrade/data/models/common/constants.dart';
import 'package:solidtrade/data/models/request_response/request_response.dart';
import 'package:solidtrade/services/request/base/base_request_service.dart';

class PortfolioDataRequestService extends IBaseRequestService {
  Future<RequestResponse<Portfolio>> getPortfolioByUserId(int userId) async {
    final queryParameters = {
      'UserId': userId.toString(),
    };

    return await makeRequest<Portfolio>(
      HttpMethod.get,
      Constants.endpointPortfolio,
      queryParameters: queryParameters,
    ).create((data) => Portfolio.fromJson(data));
  }
}
