enum TrContinuousProductPricesEventType { fullUpdate, lastValueUpdate, additionUpdate }

class TrContinuousProductPricesEvent {
  final List<MapEntry<DateTime, double>> data;
  final TrContinuousProductPricesEventType type;

  TrContinuousProductPricesEvent({required this.data, required this.type});

  factory TrContinuousProductPricesEvent.empty() {
    return TrContinuousProductPricesEvent(data: [], type: TrContinuousProductPricesEventType.fullUpdate);
  }
}
