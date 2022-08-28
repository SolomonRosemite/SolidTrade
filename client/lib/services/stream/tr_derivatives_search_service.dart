import 'package:solidtrade/data/models/common/constants.dart';
import 'package:solidtrade/data/models/enums/client_enums/derivatives_query_options.dart';
import 'package:solidtrade/data/models/enums/shared_enums/position_type.dart';
import 'package:solidtrade/data/models/request_response/request_response.dart';
import 'package:solidtrade/data/models/trade_republic/tr_derivative_search_result_mapper.dart';
import 'package:solidtrade/services/request/data_request_service.dart';

class TrDerivativesSearchService {
  final Map<String, TrDerivativeSearchResultMapper> _cache = {};

  Future<RequestResponse<TrDerivativeSearchResultMapper>> fetchDerivatives({
    required String isin,
    required PositionType derivativeType,
    required DerivativesSortOptions sortBy,
    required DerivativesOptionType filterByType,
    required int numberOfAvailableProducts,
    DerivativesSortDirectionOptions sortDirection = DerivativesSortDirectionOptions.asc,
  }) async {
    var cacheId = _createCacheIdentifier(
      isin: isin,
      derivativeType: derivativeType,
      sortBy: sortBy,
      filterByType: filterByType,
      numberOfAvailableProducts: numberOfAvailableProducts,
    );
    if (_cache.containsKey(cacheId)) {
      return RequestResponse.successful(_cache[cacheId]!);
    }

    var result = await DataRequestService.trApiDataRequestService.makeRequest<TrDerivativeSearchResultMapper>(
      Constants.getTrDerivativesRequestString(
        isin: isin,
        derivativeType: derivativeType,
        filterByType: filterByType,
        numberOfAvailableProducts: numberOfAvailableProducts,
        sortBy: sortBy,
        sortDirection: sortDirection,
      ),
    );

    if (result.isSuccessful) {
      _cache[cacheId] = result.result!;
    }

    return result;
  }

  String _createCacheIdentifier({
    required String isin,
    required PositionType derivativeType,
    required DerivativesSortOptions sortBy,
    required DerivativesOptionType filterByType,
    required int numberOfAvailableProducts,
    DerivativesSortDirectionOptions sortDirection = DerivativesSortDirectionOptions.asc,
  }) =>
      "$isin-$derivativeType-$sortBy-$filterByType-$numberOfAvailableProducts-$sortDirection";
}
