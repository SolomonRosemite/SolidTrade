import 'package:solidtrade/data/models/enums/client_enums/name_for_large_number.dart';
import 'package:solidtrade/data/models/enums/client_enums/order_type.dart';
import 'package:solidtrade/data/models/enums/shared_enums/buy_or_sell.dart';
import 'package:solidtrade/data/models/trade_republic/tr_stock_details.dart';
import 'package:solidtrade/services/util/extensions/double_extensions.dart';
import 'package:solidtrade/services/util/tr_util.dart';

import '../../../data/models/enums/client_enums/lang_ticker.dart';
import '../translation.dart';

class DeTranslation implements ITranslation {
  @override
  LanguageTicker get langTicker => LanguageTicker.de;

  @override
  IPortfolioTranslation get portfolio => DePortfolioTranslation();

  @override
  ISettingsTranslation get settings => DeSettingsTranslation();

  @override
  INavigationBarTranslation get navigationBar => DeNavigationBarTranslation();

  @override
  ISplashTranslation get splash => DeSplashTranslation();

  @override
  IUserAppBarTranslation get userAppBar => DeUserAppBarTranslation();

  @override
  ICommonTranslation get common => DeCommonTranslation();

  @override
  IProductPageTranslation get productPage => DeProductPageTranslation();

  @override
  IChartTranslation get chart => DeChartTranslation();

  @override
  IWelcomeTranslation get welcome => DeWelcomeTranslation();

  @override
  ICreateOrderPageTranslation get createOrderPage => DeCreateOrderPageTranslation();

  @override
  IEditOrderSettingsTranslation get editOrderSettingsView => DeEditOrderSettingsTranslation();
}

class DeEditOrderSettingsTranslation implements IEditOrderSettingsTranslation {
  String _defaultSuffixText(String tickerOrName, double currentPrice) => "Jetziger $tickerOrName kurs liegt bei ${currentPrice.toDefaultPrice()}.";

  @override
  String buyLimitOrderDescription(String tickerOrName, double currentPrice) => "Löse einen Limitkauf aus, wenn der $tickerOrName kurs unter den gewünschten preis fällt. ${_defaultSuffixText(tickerOrName, currentPrice)}";

  @override
  String buyStopOrderDescription(String tickerOrName, double currentPrice) => "Kaufen Sie zum Marktpreis, wenn der $tickerOrName kurs über den gewünschten preis steigt. ${_defaultSuffixText(tickerOrName, currentPrice)}";

  @override
  String sellLimitOrderDescription(String tickerOrName, double currentPrice) => "Löse einen Limitverkauf aus, wenn der $tickerOrName kurs über den gewünschten preis steigt. ${_defaultSuffixText(tickerOrName, currentPrice)}";

  @override
  String sellStopOrderDescription(String tickerOrName, double currentPrice) => "Verkaufen Sie zum Marktpreis, wenn der $tickerOrName kurs unter den gewünschten preis fällt. ${_defaultSuffixText(tickerOrName, currentPrice)}";

  @override
  String get errorMessagePriceMustBeHigher => "Der angegebene Preis muss höher sein als der Marktpreis";

  @override
  String get errorMessagePriceMustBeLower => "Der angegebene Preis muss niedriger sein als der Marktpreis";

  @override
  String get errorMessagePriceCannotBeEmptyOrZero => "Der angegebene Preis kann nicht null sein";

  @override
  String get errorMessageInsufficientFunds => "Ihr Kapital ist nicht ausreichend für diese Transaktion";

  @override
  String get errorMessageNumberOfSharesCannotBeZero => "Die Anzahl der Anteile muss über null sein";
}

class DeCreateOrderPageTranslation implements ICreateOrderPageTranslation {
  @override
  String buyLimitOrderDescription(String tickerOrName) => "Legen Sie den Limitpreis fest, d.h. den maximalen Preis, zu dem Sie bereit sind, $tickerOrName zu kaufen. Ihr Auftrag wird nur zu Ihrem Limitpreis oder niedriger ausgeführt.";

  @override
  String buyStopOrderDescription(String tickerOrName) => "Legen Sie einen Stoppkurs über dem aktuellen $tickerOrName-Kurs fest. Wenn der Stop-Kurs erreicht ist, wird Ihre Stop-Order zu einer Market-Order und wird dann zum besten verfügbaren Kurs ausgeführt.";

  @override
  String sellLimitOrderDescription(String tickerOrName) => "Legen Sie den Limitpreis fest, also den Mindestpreis, zu dem Sie bereit sind, $tickerOrName verkaufen. Ihr Auftrag wird nur zu Ihrem Limitpreis oder höher ausgeführt.";

  @override
  String sellStopOrderDescription(String tickerOrName) => "Legen Sie einen Stoppkurs unter dem aktuellen $tickerOrName-Kurs fest. Wenn der Stop-Kurs erreicht ist, wird Ihre Stop-Order zu einer Market-Order und wird dann zum besten verfügbaren Kurs ausgeführt.";

  @override
  String buySellProduct(BuyOrSell buyOrSell, String tickerOrName) => "${buyOrSell.name} $tickerOrName";

  @override
  String cashAvailable(double cash) => "Verfügbares Geld: ${cash.toDefaultPrice()}";

  @override
  String get createOrderAsTextLiteral => "Order ausführen";

  @override
  String sharesOwned(double numberOfShares) => "Anteile im Besitz: $numberOfShares";

  @override
  String totalPrice(double totalPrice) => "Gesamtpreis: ${totalPrice.toDefaultPrice()}";

  @override
  String stopLimitText(OrderType orderType) => "${orderType.name} preis";
}

class DeChartTranslation implements IChartTranslation {
  @override
  IChartDateRangeViewTranslation get chartDateRangeView => DeChartDateRangeViewTranslation();
}

class DeWelcomeTranslation implements IWelcomeTranslation {
  @override
  String get getStarted => "Loslegen";
}

class DeChartDateRangeViewTranslation implements IChartDateRangeViewTranslation {
  @override
  String get fiveYear => "5Y";

  @override
  String get oneDay => "1T";

  @override
  String get oneMonth => "1M";

  @override
  String get oneWeek => "1W";

  @override
  String get oneYear => "1Y";

  @override
  String get sixMonth => "6M";
}

class DeCommonTranslation implements ICommonTranslation {
  @override
  String get httpFriendlyErrorResponse => "Es ist ein Fehler aufgetreten. Bitte vergewissern Sie sich, dass Ihre Eingabe gültig ist.";

  @override
  String get buyAsTextLiteral => "Kaufe";

  @override
  String get sellAsTextLiteral => "Verkaufe";

  @override
  String get changeAsTextLiteral => "Ändern";
}

class DePortfolioTranslation implements IPortfolioTranslation {}

class DeProductPageTranslation implements IProductPageTranslation {
  @override
  String whatAnalystsSayContent(TrStockDetails details) {
    return "Die durchschnittliche Aktienkursschätzung liegt bei ${details.analystRating.targetPrice.average}. Die höchste Schätzung liegt bei ${details.analystRating.targetPrice.high.toStringAsFixed(2)} € und die niedrigste Schätzung bei ${details.analystRating.targetPrice.low.toStringAsFixed(2)} €.\n\nDiese Aktie wird von ${TrUtil.productPageGetAnalystsCount(details.analystRating.recommendations)} Analysten bewertet.";
  }

  @override
  String nameOfNumberPrefix(NameForLargeNumber nameForLargeNumber) {
    switch (nameForLargeNumber) {
      case NameForLargeNumber.thousand:
        return "T";
      case NameForLargeNumber.billion:
        return "Mrd";
      case NameForLargeNumber.million:
        return "Mio";
      case NameForLargeNumber.trillion:
        return "B";
    }
  }

  @override
  String get marketCap => "Marktkap.";

  @override
  String get derivativesRiskDisclaimer => "Seien Sie sich bewusst, dass diese Art von Investitionen mit einem hohen Risiko verbunden sind.";
}

class DeUserAppBarTranslation implements IUserAppBarTranslation {
  @override
  String get invite => "Einladen";
}

class DeSplashTranslation implements ISplashTranslation {
  @override
  String get loading => "Lädt";
}

class DeNavigationBarTranslation implements INavigationBarTranslation {
  @override
  String get leaderboard => SharedTranslations.navigationBarChat;

  @override
  String get portfolio => SharedTranslations.navigationBarPortfolio;

  @override
  String get profile => "Profil";

  @override
  String get search => "Suchen";
}

class DeSettingsTranslation implements ISettingsTranslation {
  @override
  String get changeLanguage => "Ändere die spache.";

  @override
  String get changeTheme => "Ändere denn farbmodus.";

  @override
  String get settings => "Einstellungen";
}
