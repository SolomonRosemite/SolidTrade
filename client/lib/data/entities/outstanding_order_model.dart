import 'package:solidtrade/data/entities/ongoing_knockout_position.dart';
import 'package:solidtrade/data/entities/ongoing_warrant_position.dart';
import 'package:solidtrade/data/models/enums/entity_enums/enter_or_exit_position_type.dart';

import '../entities/base/base_entity.dart';

class OutstandingOrderModel implements IBaseEntity {
  @override
  final int id;
  @override
  final DateTime createdAt;
  @override
  final DateTime updatedAt;

  final String isin;
  final double price;
  final double numberOfShares;
  final DateTime goodUntil;
  final EnterOrExitPositionType type;

  const OutstandingOrderModel({
    required this.id,
    required this.createdAt,
    required this.updatedAt,
    required this.isin,
    required this.price,
    required this.numberOfShares,
    required this.goodUntil,
    required this.type,
  });

  static OutstandingOrderModel ongoingKnockoutPositionToOutstandingModel(OngoingKnockoutPosition position) {
    return OutstandingOrderModel(
      id: position.id,
      createdAt: position.createdAt,
      updatedAt: position.updatedAt,
      goodUntil: position.goodUntil,
      type: position.type,
      isin: position.isin,
      price: position.price,
      numberOfShares: position.numberOfShares,
    );
  }

  static OutstandingOrderModel ongoingWarrantPositionToOutstandingModel(OngoingWarrantPosition position) {
    return OutstandingOrderModel(
      id: position.id,
      createdAt: position.createdAt,
      updatedAt: position.updatedAt,
      goodUntil: position.goodUntil,
      type: position.type,
      isin: position.isin,
      price: position.price,
      numberOfShares: position.numberOfShares,
    );
  }
}
