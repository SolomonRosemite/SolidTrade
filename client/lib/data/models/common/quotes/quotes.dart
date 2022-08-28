import 'package:solidtrade/data/models/common/quotes/quote.dart';
import 'package:solidtrade/data/models/enums/client_enums/lang_ticker.dart';

class Quotes {
  static List<Quote> splashScreenQuotes = [
    Quote(lang: LanguageTicker.en, quote: "Funding secured😎"),
    Quote(lang: LanguageTicker.en, quote: "Buy High Sell Low. Right?"),
    Quote(lang: LanguageTicker.en, quote: "GME to the moon🚀"),
    Quote(lang: LanguageTicker.en, quote: "TSLA to the moon🚀"),
    Quote(lang: LanguageTicker.en, quote: "A wise man once said with wisdom comes 100X leverage🚀"),
    Quote(lang: LanguageTicker.en, quote: "Why buy ETFs if you can buy Knockouts with 100X leverage🤷‍♂️"),
    Quote(lang: LanguageTicker.en, quote: "What is a Margin Call and why can't I buy more TSLA?!"),
    Quote(lang: LanguageTicker.en, quote: "The Big Long🚀"),
    Quote(lang: LanguageTicker.en, quote: "Some people are actually making money in the stock market?"),
    Quote(lang: LanguageTicker.en, quote: "My portfolio is doing better than usual today\n Meanwhile, Portfolio down 70%"),
    Quote(lang: LanguageTicker.en, quote: "Anyone knows a Trade Republic money glitch?"),
    Quote(lang: LanguageTicker.en, quote: "With great leverage comes greater profits. Technically..."),
    Quote(lang: LanguageTicker.en, quote: "Ever heard of technical analysis? It's pretty much astrology for men. Try it!"),
  ];

  // TODO: Add more quotes...
  // The max length is 42.s
  static List<Quote> knockoutsQuotes = [
    Quote(lang: LanguageTicker.en, quote: "10x to the moon 🚀🌑 or lose it all."),
  ];

  static List<Quote> warrantQuotes = [
    Quote(lang: LanguageTicker.en, quote: "🧐 Analysts recommend warrants with 5 DTE."),
  ];
}
