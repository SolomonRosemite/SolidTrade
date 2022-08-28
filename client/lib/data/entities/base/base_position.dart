abstract class IPosition {
  final String isin;
  final double buyInPrice;
  final double numberOfShares;

  IPosition({required this.isin, required this.buyInPrice, required this.numberOfShares});
}
