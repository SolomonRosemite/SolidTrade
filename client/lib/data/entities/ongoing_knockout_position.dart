import 'package:enum_to_string/enum_to_string.dart';
import 'package:solidtrade/data/entities/base/base_entity.dart';
import 'package:solidtrade/data/entities/base/base_position.dart';
import 'package:solidtrade/data/models/enums/entity_enums/enter_or_exit_position_type.dart';

class OngoingKnockoutPosition implements IBaseEntity, IPosition {
  @override
  final int id;
  @override
  final DateTime createdAt;
  @override
  final DateTime updatedAt;
  @override
  final String isin;
  @override
  final double buyInPrice;
  @override
  final double numberOfShares;

  final DateTime goodUntil;
  final EnterOrExitPositionType type;

  const OngoingKnockoutPosition({
    required this.id,
    required this.createdAt,
    required this.updatedAt,
    required this.isin,
    required this.buyInPrice,
    required this.numberOfShares,
    required this.goodUntil,
    required this.type,
  });

  factory OngoingKnockoutPosition.fromJson(Map<String, dynamic> json) {
    return OngoingKnockoutPosition(
      id: json['id'],
      createdAt: DateTime.parse(json['createdAt']),
      updatedAt: DateTime.parse(json['updatedAt']),
      goodUntil: DateTime.parse(json['goodUntil']),
      type: EnumToString.fromString(EnterOrExitPositionType.values, json['type'])!,
      isin: json['isin'],
      buyInPrice: json['price'],
      numberOfShares: json['numberOfShares'],
    );
  }
}
