import 'package:solidtrade/data/models/enums/client_enums/derivatives_query_options.dart';
import 'package:solidtrade/data/models/enums/client_enums/lang_ticker.dart';
import 'package:solidtrade/data/models/enums/shared_enums/position_type.dart';

class Constants {
  static const forgotOrLostAccountFormLink = "https://docs.google.com/forms/d/181J0K5WYEPrI0h4_flZBNtz3Io5VSEo07c9SflOUtGY/";
  static const learnMoreAboutOrderTypesLink = "https://www.investopedia.com/investing/basics-trading-stock-know-your-orders/";
  static const fileUploadLimitInBytes = 10000000; // 10MB

  static const endpointHistoricalPosition = "/historicalpositions/";
  static const endpointKnockout = "/knockouts/";
  static const endpointOngoingKnockout = "/knockouts/ongoing/";
  static const endpointOngoingWarrant = "/warrants/ongoing/";
  static const endpointPortfolio = "/portfolios/";
  static const endpointStock = "/stocks/";
  static const endpointUser = "/users/";
  static const endpointWarrant = "/warrants/";

  static const genericErrorMessage = "Something went wrong. Please try again later.";
  static const notLoggedInMessage = "User session expired.\nPlease login.";

  static const thousand = 1000;
  static const million = 1000000;
  static const billion = 1000000000;
  static const trillion = 1000000000000;

  static String getTrProductSearchRequestString(String search, String searchType) => "{\"type\":\"neonSearch\",\"data\":{\"q\":\"$search\",\"page\":1,\"pageSize\":10,\"filter\":[{\"key\":\"type\",\"value\":\"$searchType\"}]}}";
  static String getTrStockDetailsRequestString(String isin) => "{\"type\":\"stockDetails\",\"id\":\"$isin\"}";
  static String getTrProductInfoRequestString(String isin) => "{\"type\":\"instrument\",\"id\":\"$isin\"}";
  static String getTrProductPriceRequestString(String isin) => "{\"type\":\"ticker\",\"id\":\"$isin\"}";
  static String getTrAggregateHistoryRequestString(String isin, String range) => "{\"type\":\"aggregateHistoryLight\",\"range\":\"$range\",\"id\":\"$isin\"}";
  static String getTrDerivativesRequestString({
    required String isin,
    required PositionType derivativeType,
    required DerivativesSortOptions sortBy,
    required DerivativesOptionType filterByType,
    required int numberOfAvailableProducts,
    DerivativesSortDirectionOptions sortDirection = DerivativesSortDirectionOptions.asc,
  }) =>
      "{\"type\":\"derivatives\",\"lang\":\"en\",\"underlying\":\"$isin\",\"productCategory\":\"${derivativeType.trName}\",\"leverage\":0,\"sortBy\":\"${sortBy.name}\",\"sortDirection\":\"${sortDirection.name}\",\"optionType\":\"${filterByType.name}\",\"pageSize\":$numberOfAvailableProducts,\"after\":\"0\"}";
}

class Quotes {
  static List<QuoteInfo> splashScreenQuotes = [
    QuoteInfo(lang: LanguageTicker.en, quote: "Funding secured😎"),
    QuoteInfo(lang: LanguageTicker.en, quote: "Buy High Sell Low. Right?"),
    QuoteInfo(lang: LanguageTicker.en, quote: "GME to the moon🚀"),
    QuoteInfo(lang: LanguageTicker.en, quote: "TSLA to the moon🚀"),
    QuoteInfo(lang: LanguageTicker.en, quote: "A wise man once said with wisdom comes 100X leverage🚀"),
    QuoteInfo(lang: LanguageTicker.en, quote: "Why buy ETFs if you can buy Knockouts with 100X leverage🤷‍♂️"),
    QuoteInfo(lang: LanguageTicker.en, quote: "What is a Margin Call and why can't I buy more TSLA?!"),
    QuoteInfo(lang: LanguageTicker.en, quote: "The Big Long🚀"),
    QuoteInfo(lang: LanguageTicker.en, quote: "Some people are actually making money in the stock market?"),
    QuoteInfo(lang: LanguageTicker.en, quote: "My portfolio is doing better than usual today\n Meanwhile, Portfolio down 70%"),
    QuoteInfo(lang: LanguageTicker.en, quote: "Anyone knows a Trade Republic money glitch?"),
    QuoteInfo(lang: LanguageTicker.en, quote: "With great leverage comes greater profits. Technically..."),
    QuoteInfo(lang: LanguageTicker.en, quote: "Ever heard of technical analysis? It's pretty much astrology for men. Try it!"),
  ];

  // TODO: Add more quotes...
  // The max length is 42.s
  static List<QuoteInfo> knockoutsQuotes = [
    QuoteInfo(lang: LanguageTicker.en, quote: "10x to the moon 🚀🌑 or lose it all."),
  ];

  static List<QuoteInfo> warrantQuotes = [
    QuoteInfo(lang: LanguageTicker.en, quote: "🧐 Analysts recommend warrants with 5 DTE."),
  ];
}

class QuoteInfo {
  final LanguageTicker lang;
  final String quote;

  QuoteInfo({required this.lang, required this.quote});
}
