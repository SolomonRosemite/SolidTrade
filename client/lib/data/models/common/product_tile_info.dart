import 'package:solidtrade/data/models/enums/shared_enums/position_type.dart';

class ProductTileInfo {
  final PositionType positionType;
  final String isin;

  ProductTileInfo(this.positionType, this.isin);
}
