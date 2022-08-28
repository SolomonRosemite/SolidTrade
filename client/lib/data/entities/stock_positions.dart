import 'package:solidtrade/data/entities/base/base_entity.dart';
import 'package:solidtrade/data/entities/base/base_position.dart';

class StockPosition implements IBaseEntity, IPosition {
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

  const StockPosition({
    required this.id,
    required this.createdAt,
    required this.updatedAt,
    required this.isin,
    required this.buyInPrice,
    required this.numberOfShares,
  });

  factory StockPosition.fromJson(Map<String, dynamic> json) {
    return StockPosition(
      id: json["id"],
      createdAt: DateTime.parse(json["createdAt"]),
      updatedAt: DateTime.parse(json["updatedAt"]),
      isin: json["isin"],
      buyInPrice: json["buyInPrice"],
      numberOfShares: json["numberOfShares"],
    );
  }
}
