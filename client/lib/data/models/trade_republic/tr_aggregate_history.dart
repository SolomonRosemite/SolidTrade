import 'package:simple_json_mapper/simple_json_mapper.dart';

@JsonObject()
class TrAggregateHistory {
  final List<TrAggregateHistoryEntry> aggregates;
  final int expectedClosingTime;
  final int lastAggregateEndTime;
  final int resolution;

  TrAggregateHistory({required this.aggregates, required this.expectedClosingTime, required this.lastAggregateEndTime, required this.resolution});
}

class TrAggregateHistoryEntry {
  final int time;
  final double open;
  final double close;

  TrAggregateHistoryEntry({required this.time, required this.open, required this.close});
}
