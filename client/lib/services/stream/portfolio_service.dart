import 'package:rxdart/subjects.dart';
import 'package:solidtrade/data/entities/portfolio.dart';
import 'package:solidtrade/data/models/request_response/request_response.dart';
import 'package:solidtrade/services/request/data_request_service.dart';
import 'package:solidtrade/services/stream/base/base_service.dart';

class PortfolioService extends IService<Portfolio?> {
  PortfolioService() : super(BehaviorSubject.seeded(null));

  Future<RequestResponse<Portfolio>> fetchPortfolioByUserId(int id) async {
    var result = await DataRequestService.portfolioDataRequestService.getPortfolioByUserId(id);

    if (result.isSuccessful) {
      behaviorSubject.add(result.result);
    }

    return result;
  }
}
