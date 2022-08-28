import 'package:intl/intl.dart';
import 'package:solidtrade/data/models/trade_republic/tr_derivative_search_result_mapper.dart';

class TrDerivativeSearchResult {
  final String name;
  final String isin;
  final String issuerDisplayName;
  final num leverageOrStrike;
  final num knockoutBarrierOrDelta;
  final num size;
  final String currency;
  final String expiryText;

  TrDerivativeSearchResult({
    required this.name,
    required this.isin,
    required this.issuerDisplayName,
    required this.leverageOrStrike,
    required this.knockoutBarrierOrDelta,
    required this.size,
    required this.currency,
    required this.expiryText,
  });

  static TrDerivativeSearchResult fromKnockoutSearchResult(TrSingleDerivativeSearchResultMapper result) {
    return TrDerivativeSearchResult(
      name: result.productCategoryName,
      isin: result.isin,
      issuerDisplayName: result.issuerDisplayName,
      leverageOrStrike: result.leverage!,
      knockoutBarrierOrDelta: result.barrier!,
      size: result.size,
      currency: result.currency,
      expiryText: result.productCategoryName,
    );
  }

  static TrDerivativeSearchResult fromWarrantSearchResult(TrSingleDerivativeSearchResultMapper result) {
    return TrDerivativeSearchResult(
      name: result.productCategoryName,
      isin: result.isin,
      issuerDisplayName: result.issuerDisplayName,
      leverageOrStrike: result.strike,
      knockoutBarrierOrDelta: result.delta!,
      size: result.size,
      currency: result.currency,
      expiryText: DateFormat("dd.MM.yyyy").format(DateTime.parse(result.expiry!)),
    );
  }
}
