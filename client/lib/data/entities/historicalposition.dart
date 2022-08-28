import 'package:enum_to_string/enum_to_string.dart';
import 'package:solidtrade/data/entities/base/base_entity.dart';
import 'package:solidtrade/data/models/enums/shared_enums/buy_or_sell.dart';
import 'package:solidtrade/data/models/enums/shared_enums/position_type.dart';

class HistoricalPosition implements IBaseEntity {
  @override
  final int id;
  @override
  final DateTime createdAt;
  @override
  final DateTime updatedAt;

  final String isin;
  final double buyInPrice;
  final double performance;
  final double numberOfShares;
  final BuyOrSell buyOrSell;
  final PositionType positionType;

  const HistoricalPosition({
    required this.id,
    required this.createdAt,
    required this.updatedAt,
    required this.positionType,
    required this.buyOrSell,
    required this.buyInPrice,
    required this.isin,
    required this.numberOfShares,
    required this.performance,
  });

  factory HistoricalPosition.fromJson(Map<String, dynamic> json) {
    return HistoricalPosition(
      id: json["id"],
      createdAt: DateTime.parse(json["createdAt"]),
      updatedAt: DateTime.parse(json["updatedAt"]),
      positionType: EnumToString.fromString(PositionType.values, json["positionType"])!,
      buyOrSell: EnumToString.fromString(BuyOrSell.values, json["buyOrSell"])!,
      buyInPrice: json["buyInPrice"],
      performance: json["performance"],
      numberOfShares: json["numberOfShares"],
      isin: json["isin"],
    );
  }

  factory HistoricalPosition.copy(HistoricalPosition hp, double numberOfShares) {
    return HistoricalPosition(
      id: hp.id,
      buyInPrice: hp.buyInPrice,
      buyOrSell: hp.buyOrSell,
      createdAt: hp.createdAt,
      isin: hp.isin,
      numberOfShares: numberOfShares,
      performance: hp.performance,
      positionType: hp.positionType,
      updatedAt: hp.updatedAt,
    );
  }
}
