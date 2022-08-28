import 'package:simple_json_mapper/simple_json_mapper.dart';

@JsonObject()
class BuyOrSellRequestDto {
  final String isin;
  final num numberOfShares;

  BuyOrSellRequestDto({required this.isin, required this.numberOfShares});
}
