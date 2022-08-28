import 'package:simple_json_mapper/simple_json_mapper.dart';
import 'package:solidtrade/data/models/enums/shared_enums/position_type.dart';
import 'package:solidtrade/data/models/trade_republic/tr_derivative_search_result.dart';

@JsonObject()
class TrDerivativeSearchResultMapper {
  final Map<String, int> issuerCount;
  final List<TrSingleDerivativeSearchResultMapper> results;
  final int resultCount;

  TrDerivativeSearchResultMapper({
    required this.issuerCount,
    required this.resultCount,
    required this.results,
  });

  Iterable<TrDerivativeSearchResult> convertToTrDerivativeSearchResults(PositionType positionType) {
    switch (positionType) {
      case PositionType.warrant:
        return results.map((e) => TrDerivativeSearchResult.fromWarrantSearchResult(e));
      case PositionType.knockout:
        return results.map((e) => TrDerivativeSearchResult.fromKnockoutSearchResult(e));
      default:
        throw Exception("Expected ${(PositionType).toString()} to be knockout or warrant. But provided argument was $positionType.");
    }
  }
}

class TrSingleDerivativeSearchResultMapper {
  final String isin;
  final String productCategoryName;
  final num? barrier;
  final num? leverage;
  final num strike;
  final num size;
  final num? delta;
  final String currency;
  final String? expiry;
  final String issuerDisplayName;

  TrSingleDerivativeSearchResultMapper({
    required this.isin,
    required this.productCategoryName,
    this.barrier,
    this.leverage,
    required this.strike,
    required this.size,
    this.delta,
    required this.currency,
    this.expiry,
    required this.issuerDisplayName,
  });
}
