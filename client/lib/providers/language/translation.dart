import 'package:solidtrade/data/models/enums/client_enums/lang_ticker.dart';
import 'package:solidtrade/data/models/enums/client_enums/name_for_large_number.dart';
import 'package:solidtrade/data/models/enums/client_enums/order_type.dart';
import 'package:solidtrade/data/models/enums/shared_enums/buy_or_sell.dart';
import 'package:solidtrade/data/models/trade_republic/tr_stock_details.dart';

abstract class ITranslation {
  LanguageTicker get langTicker;

  IProductPageTranslation get productPage;
  ICreateOrderPageTranslation get createOrderPage;
  IEditOrderSettingsTranslation get editOrderSettingsView;
  IPortfolioTranslation get portfolio;
  ISettingsTranslation get settings;
  INavigationBarTranslation get navigationBar;
  ISplashTranslation get splash;
  IUserAppBarTranslation get userAppBar;
  IChartTranslation get chart;
  ICommonTranslation get common;
  IWelcomeTranslation get welcome;
}

class SharedTranslations {
  static const String navigationBarPortfolio = "Portfolio";
  static const String navigationBarChat = "Leaderboard";

  static const List<String> welcomeMessages = [
    "Welcome to",
    "Willkommen zu",
    "Velkommen til",
    "Bienvenue à",
    "ようこそ",
    "Bienvenido a",
    "欢迎来到",
  ];
}

abstract class IChartTranslation {
  IChartDateRangeViewTranslation get chartDateRangeView;
}

abstract class IWelcomeTranslation {
  String get getStarted;
}

abstract class IChartDateRangeViewTranslation {
  String get oneDay;
  String get oneWeek;
  String get oneMonth;
  String get sixMonth;
  String get oneYear;
  String get fiveYear;
}

abstract class ICommonTranslation {
  String get httpFriendlyErrorResponse;
  String get buyAsTextLiteral;
  String get sellAsTextLiteral;
  String get changeAsTextLiteral;
}

abstract class ISplashTranslation {
  String get loading;
}

abstract class IPortfolioTranslation {}

abstract class IProductPageTranslation {
  String whatAnalystsSayContent(TrStockDetails details);
  String nameOfNumberPrefix(NameForLargeNumber nameForLargeNumber);
  String get marketCap;
  String get derivativesRiskDisclaimer;
}

abstract class ICreateOrderPageTranslation {
  String buyLimitOrderDescription(String tickerOrName);
  String buyStopOrderDescription(String tickerOrName);
  String sellLimitOrderDescription(String tickerOrName);
  String sellStopOrderDescription(String tickerOrName);

  String cashAvailable(double cash);
  String sharesOwned(double numberOfShares);
  String buySellProduct(BuyOrSell buyOrSell, String tickerOrName);
  String stopLimitText(OrderType orderType);

  String get createOrderAsTextLiteral;
  String totalPrice(double totalPrice);
}

abstract class IEditOrderSettingsTranslation {
  String buyLimitOrderDescription(String tickerOrName, double currentPrice);
  String buyStopOrderDescription(String tickerOrName, double currentPrice);
  String sellLimitOrderDescription(String tickerOrName, double currentPrice);
  String sellStopOrderDescription(String tickerOrName, double currentPrice);

  String get errorMessagePriceMustBeHigher;
  String get errorMessagePriceMustBeLower;
  String get errorMessageNumberOfSharesCannotBeZero;
  String get errorMessagePriceCannotBeEmptyOrZero;
  String get errorMessageInsufficientFunds;
}

abstract class IUserAppBarTranslation {
  String get invite;
}

abstract class INavigationBarTranslation {
  String get portfolio;
  String get search;
  String get leaderboard;
  String get profile;
}

abstract class ISettingsTranslation {
  String get settings;
  String get changeTheme;
  String get changeLanguage;
}
