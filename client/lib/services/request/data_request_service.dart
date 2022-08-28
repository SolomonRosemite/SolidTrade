import 'package:solidtrade/services/request/historicalpositions_data_request_service.dart';
import 'package:solidtrade/services/request/knockout_data_request_service.dart';
import 'package:solidtrade/services/request/ongoing_knockout_data_request_service.dart';
import 'package:solidtrade/services/request/ongoing_warrant_data_request_service.dart';
import 'package:solidtrade/services/request/portfolio_data_request_service.dart';
import 'package:solidtrade/services/request/stock_data_request_service.dart';
import 'package:solidtrade/services/request/tr_api_data_request_service.dart';
import 'package:solidtrade/services/request/user_data_request_service.dart';
import 'package:solidtrade/services/request/warrant_data_request_service.dart';

class DataRequestService {
  static late HistoricalPositionsDataRequestService historicalPositionsDataRequestService;
  static late KnockoutDataRequestService knockoutDataRequestService;
  static late OngoingKnockoutDataRequestService ongoingKnockoutDataRequestService;
  static late OngoingWarrantDataRequestService ongoingWarrantDataRequestService;
  static late PortfolioDataRequestService portfolioDataRequestService;
  static late StockDataRequestService stockDataRequestService;
  static late UserDataRequestService userDataRequestService;
  static late WarrantDataRequestService warrantDataRequestService;
  static late TrApiDataRequestService trApiDataRequestService;

  static initialize() {
    historicalPositionsDataRequestService = HistoricalPositionsDataRequestService();
    knockoutDataRequestService = KnockoutDataRequestService();
    ongoingKnockoutDataRequestService = OngoingKnockoutDataRequestService();
    ongoingWarrantDataRequestService = OngoingWarrantDataRequestService();
    portfolioDataRequestService = PortfolioDataRequestService();
    stockDataRequestService = StockDataRequestService();
    userDataRequestService = UserDataRequestService();
    warrantDataRequestService = WarrantDataRequestService();
    trApiDataRequestService = TrApiDataRequestService();
  }
}
