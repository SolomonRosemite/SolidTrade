import 'package:simple_json_mapper/simple_json_mapper.dart';

@JsonObject()
class TrProductSearch {
  final List<TrProductSearchResult> results;
  final int resultCount;

  TrProductSearch({
    required this.results,
    required this.resultCount,
  });
}

class TrProductSearchResult {
  final String isin;
  final String name;
  final String type;
  final List<String> derivativeProductCategories;
  final List<TrProductSearchTags> tags;

  TrProductSearchResult({
    required this.isin,
    required this.name,
    required this.type,
    required this.derivativeProductCategories,
    required this.tags,
  });
}

class TrProductSearchTags {
  final String id;
  final String name;
  final String type;

  const TrProductSearchTags({
    required this.id,
    required this.name,
    required this.type,
  });
}
