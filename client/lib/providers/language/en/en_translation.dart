import 'package:solidtrade/data/models/enums/client_enums/lang_ticker.dart';
import 'package:solidtrade/data/models/enums/client_enums/name_for_large_number.dart';
import 'package:solidtrade/data/models/enums/client_enums/order_type.dart';
import 'package:solidtrade/data/models/enums/shared_enums/buy_or_sell.dart';
import 'package:solidtrade/data/models/trade_republic/tr_stock_details.dart';
import 'package:solidtrade/services/util/extensions/double_extensions.dart';
import 'package:solidtrade/services/util/tr_util.dart';

import '../translation.dart';

class EnTranslation implements ITranslation {
  @override
  LanguageTicker get langTicker => LanguageTicker.en;

  @override
  IPortfolioTranslation get portfolio => EnPortfolioTranslation();

  @override
  ISettingsTranslation get settings => EnSettingsTranslation();

  @override
  INavigationBarTranslation get navigationBar => EnNavigationBarTranslation();

  @override
  ISplashTranslation get splash => SplashTranslation();

  @override
  IUserAppBarTranslation get userAppBar => EnUserAppBarTranslation();

  @override
  ICommonTranslation get common => EnCommonTranslation();

  @override
  IProductPageTranslation get productPage => EnProductPageTranslation();

  @override
  IChartTranslation get chart => EnChartTranslation();

  @override
  IWelcomeTranslation get welcome => EnWelcomeTranslation();

  @override
  ICreateOrderPageTranslation get createOrderPage => EnCreateOrderPageTranslation();

  @override
  IEditOrderSettingsTranslation get editOrderSettingsView => EnEditOrderSettingsTranslation();
}

class EnEditOrderSettingsTranslation implements IEditOrderSettingsTranslation {
  @override
  String buyLimitOrderDescription(String tickerOrName, double currentPrice) => "Trigger a limit buy if $tickerOrName€ decreases from ${currentPrice.toDefaultPrice()} to:";

  @override
  String buyStopOrderDescription(String tickerOrName, double currentPrice) => "Buy at market price if $tickerOrName increases from ${currentPrice.toDefaultPrice()} to:";

  @override
  String sellLimitOrderDescription(String tickerOrName, double currentPrice) => "Trigger a limit sell if $tickerOrName increases from ${currentPrice.toDefaultPrice()} to:";

  @override
  String sellStopOrderDescription(String tickerOrName, double currentPrice) => "Sell at market price if $tickerOrName decreases from ${currentPrice.toDefaultPrice()} to:";

  @override
  String get errorMessagePriceMustBeHigher => "The specified price must be more than the market price";

  @override
  String get errorMessagePriceMustBeLower => "The specified price must be less than the market price";

  @override
  String get errorMessagePriceCannotBeEmptyOrZero => "The price given can not be empty or zero";

  @override
  String get errorMessageInsufficientFunds => "You dont have sufficient funds for this transaction";

  @override
  String get errorMessageNumberOfSharesCannotBeZero => "The specified number of shares can not be zero";
}

class EnCreateOrderPageTranslation implements ICreateOrderPageTranslation {
  @override
  String buyLimitOrderDescription(String tickerOrName) => "Set the limit price, or the maximum price at which you're willing to buy $tickerOrName. Your order will only be fulfilled at your limit price or lower.";

  @override
  String buyStopOrderDescription(String tickerOrName) => "Set a stop price above the current price of $tickerOrName. When the stop price is reached, your Stop Order becomes a Market Order and then executed at the best price available.";

  @override
  String sellLimitOrderDescription(String tickerOrName) => "Set the limit price, or the minimum price at which you're willing to sell $tickerOrName. Your order will only be fulfilled at your limit price or higher.";

  @override
  String sellStopOrderDescription(String tickerOrName) => "Set a stop price below the current price of $tickerOrName. When the stop price is reached, your Stop Order becomes a Market Order and then executed at the best price available.";

  @override
  String buySellProduct(BuyOrSell buyOrSell, String tickerOrName) => "${buyOrSell.name} $tickerOrName";

  @override
  String cashAvailable(double cash) => "Cash available: ${cash.toDefaultPrice()}";

  @override
  String get createOrderAsTextLiteral => "Create order";

  @override
  String sharesOwned(double numberOfShares) => "Shares owned: $numberOfShares";

  @override
  String totalPrice(double totalPrice) => "Total price: ${totalPrice.toDefaultPrice()}";

  @override
  String stopLimitText(OrderType orderType) => "${orderType.name} price";
}

class EnChartTranslation implements IChartTranslation {
  @override
  IChartDateRangeViewTranslation get chartDateRangeView => EnChartDateRangeViewTranslation();
}

class EnWelcomeTranslation implements IWelcomeTranslation {
  @override
  String get getStarted => "Get Started";
}

class EnChartDateRangeViewTranslation implements IChartDateRangeViewTranslation {
  @override
  String get fiveYear => "5Y";

  @override
  String get oneDay => "1D";

  @override
  String get oneMonth => "1M";

  @override
  String get oneWeek => "1W";

  @override
  String get oneYear => "1Y";

  @override
  String get sixMonth => "6M";
}

class EnCommonTranslation implements ICommonTranslation {
  @override
  String get httpFriendlyErrorResponse => "Something went wrong. Please make sure your input is valid.";

  @override
  String get buyAsTextLiteral => "Buy";

  @override
  String get sellAsTextLiteral => "Sell";

  @override
  String get changeAsTextLiteral => "Change";
}

class EnPortfolioTranslation implements IPortfolioTranslation {}

class EnProductPageTranslation implements IProductPageTranslation {
  @override
  String whatAnalystsSayContent(TrStockDetails details) {
    return "The average share price estimate lays by ${details.analystRating.targetPrice.average.toStringAsFixed(2)}. The highest estimate is ${details.analystRating.targetPrice.high.toStringAsFixed(2)}€ and the lowest estimate ${details.analystRating.targetPrice.low.toStringAsFixed(2)}€.\n\nThis stock is evaluated by ${TrUtil.productPageGetAnalystsCount(details.analystRating.recommendations)} analysts.";
  }

  @override
  String nameOfNumberPrefix(NameForLargeNumber nameForLargeNumber) {
    switch (nameForLargeNumber) {
      case NameForLargeNumber.billion:
        return "B";
      case NameForLargeNumber.million:
        return "M";
      case NameForLargeNumber.thousand:
      case NameForLargeNumber.trillion:
        return "T";
    }
  }

  @override
  String get marketCap => "Market cap";

  @override
  String get derivativesRiskDisclaimer => "Please be aware that these types of investments come with high risk.";
}

class EnUserAppBarTranslation implements IUserAppBarTranslation {
  @override
  String get invite => "Invite";
}

class EnSettingsTranslation implements ISettingsTranslation {
  @override
  String get changeLanguage => "Change language.";

  @override
  String get changeTheme => "Change theme.";

  @override
  String get settings => "Settings";
}

class EnNavigationBarTranslation implements INavigationBarTranslation {
  @override
  String get leaderboard => SharedTranslations.navigationBarChat;

  @override
  String get portfolio => SharedTranslations.navigationBarPortfolio;

  @override
  String get profile => "Profile";

  @override
  String get search => "Search";
}

class SplashTranslation implements ISplashTranslation {
  @override
  String get loading => "Loading";
}
