import 'package:solidtrade/data/entities/base/base_entity.dart';
import 'package:solidtrade/data/entities/knockout_position.dart';
import 'package:solidtrade/data/entities/ongoing_knockout_position.dart';
import 'package:solidtrade/data/entities/ongoing_warrant_position.dart';
import 'package:solidtrade/data/entities/stock_positions.dart';
import 'package:solidtrade/data/entities/warrant_position.dart';

class Portfolio implements IBaseEntity {
  @override
  final int id;
  @override
  final DateTime createdAt;
  @override
  final DateTime updatedAt;

  final int userId;
  final double cash;
  final double initialCash;

  final List<StockPosition> stockPositions;
  final List<WarrantPosition> warrantPositions;
  final List<KnockoutPosition> knockOutPositions;
  final List<OngoingKnockoutPosition> ongoingKnockOutPositions;
  final List<OngoingWarrantPosition> ongoingWarrantPositions;

  const Portfolio({
    required this.id,
    required this.createdAt,
    required this.updatedAt,
    required this.cash,
    required this.initialCash,
    required this.userId,
    required this.knockOutPositions,
    required this.ongoingKnockOutPositions,
    required this.ongoingWarrantPositions,
    required this.stockPositions,
    required this.warrantPositions,
  });

  factory Portfolio.fromJson(Map<String, dynamic> json) {
    return Portfolio(
      id: json["id"],
      createdAt: DateTime.parse(json["createdAt"]),
      updatedAt: DateTime.parse(json["updatedAt"]),
      cash: json["cash"],
      initialCash: json["initialCash"],
      userId: json["userId"],
      knockOutPositions: (json["knockOutPositions"] as List<dynamic>).map((e) => KnockoutPosition.fromJson(e)).toList(),
      ongoingKnockOutPositions: (json["ongoingKnockOutPositions"] as List<dynamic>).map((e) => OngoingKnockoutPosition.fromJson(e)).toList(),
      ongoingWarrantPositions: (json["ongoingWarrantPositions"] as List<dynamic>).map((e) => OngoingWarrantPosition.fromJson(e)).toList(),
      stockPositions: (json["stockPositions"] as List<dynamic>).map((e) => StockPosition.fromJson(e)).toList(),
      warrantPositions: (json["warrantPositions"] as List<dynamic>).map((e) => WarrantPosition.fromJson(e)).toList(),
    );
  }
}
