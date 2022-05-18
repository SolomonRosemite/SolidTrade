import 'package:solidtrade/data/enums/name_for_large_number.dart';
import 'package:solidtrade/data/common/shared/tr/tr_stock_details.dart';
import 'package:solidtrade/data/enums/lang_ticker.dart';
import 'package:solidtrade/services/util/tr_util.dart';

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
  IProductViewTranslation get productView => DeProductViewTranslation();

  @override
  IChartTranslation get chart => DeChartTranslation();

  @override
  IWelcomeTranslation get welcome => DeWelcomeTranslation();
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
}

class DePortfolioTranslation implements IPortfolioTranslation {}

class DeProductViewTranslation implements IProductViewTranslation {
  @override
  String whatAnalystsSayContent(TrStockDetails details) {
    return "Die durchschnittliche Aktienkursschätzung liegt bei ${details.analystRating.targetPrice.average}. Die höchste Schätzung liegt bei ${details.analystRating.targetPrice.high.toStringAsFixed(2)} € und die niedrigste Schätzung bei ${details.analystRating.targetPrice.low.toStringAsFixed(2)} €.\n\nDiese Aktie wird von ${TrUtil.productViewGetAnalystsCount(details.analystRating.recommendations)} Analysten bewertet.";
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
