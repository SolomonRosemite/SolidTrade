import 'package:solidtrade/data/models/enums/entity_enums/enter_or_exit_position_type.dart';
import 'package:simple_json_mapper/simple_json_mapper.dart';

@JsonObject()
class OngoingPositionRequestDto {
  final String isin;
  final DateTime goodUntil;
  final double priceThreshold;
  final double numberOfShares;
  final EnterOrExitPositionType type;

  OngoingPositionRequestDto({
    required this.isin,
    required this.goodUntil,
    required this.priceThreshold,
    required this.numberOfShares,
    required this.type,
  });
}
