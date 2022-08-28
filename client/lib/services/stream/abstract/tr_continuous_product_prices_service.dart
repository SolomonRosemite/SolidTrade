import 'dart:async';

import 'package:get_it/get_it.dart';
import 'package:rxdart/streams.dart';
import 'package:rxdart/subjects.dart';
import 'package:solidtrade/data/models/enums/client_enums/chart_date_range_view.dart';
import 'package:solidtrade/data/models/trade_republic/tr_continuous_product_prices_event.dart';
import 'package:solidtrade/data/models/trade_republic/tr_product_price.dart';
import 'package:solidtrade/services/stream/aggregate_history_service.dart';
import 'package:solidtrade/services/util/debug/log.dart';

class TrContinuousProductPricesService implements Disposable {
  final BehaviorSubject<TrContinuousProductPricesEvent> _secondaryBehaviorSubject = BehaviorSubject.seeded(TrContinuousProductPricesEvent.empty());
  final BehaviorSubject<TrContinuousProductPricesEvent> _behaviorSubject = BehaviorSubject.seeded(TrContinuousProductPricesEvent.empty());
  ValueStream<TrContinuousProductPricesEvent> get primaryProductPricesStream$ => _behaviorSubject.stream;
  ValueStream<TrContinuousProductPricesEvent> get secondaryStream$ => _secondaryBehaviorSubject.stream;

  final AggregateHistoryService _aggregateHistoryService = GetIt.instance.get<AggregateHistoryService>();
  final List<MapEntry<DateTime, double>> _currentAggregateHistory = [];
  final Stream<TrProductPrice?> _priceStream;
  final Stream<ChartDateRangeView> _chartDateRangeStream;
  final String _isinWithExtension;

  late ChartDateRangeView _currentView;
  late Duration _aggregatePriceInterval;
  late int _expectedClosingTime;
  double? _preOpen;

  StreamSubscription? _priceStreamSubscription;

  TrContinuousProductPricesService(this._chartDateRangeStream, this._priceStream, this._isinWithExtension) {
    _chartDateRangeStream.listen(onChartDateRangeChanged);
  }

  Future<void> onChartDateRangeChanged(ChartDateRangeView view) async {
    var result = await fetchAndAddTrAggregateHistory(view);

    _currentView = view;
    _priceStreamSubscription ??= _priceStream.listen(onNewTrProductPrice);

    loadClosePriceToStream();
    if (result != null) {
      emitNewTrContinuousProductPrices(result);
    }
  }

  Future<TrContinuousProductPricesEvent?> fetchAndAddTrAggregateHistory(ChartDateRangeView range) async {
    var response = await _aggregateHistoryService.getTrAggregateHistory(_isinWithExtension, range);

    if (!response.isSuccessful) {
      // TODO: Handle...
      Log.f("Failed to fetch aggregateHistory successfully");
      return null;
    }

    _currentAggregateHistory.clear();
    _expectedClosingTime = response.result!.expectedClosingTime;
    var data = response.result!.aggregates.map((e) => MapEntry(DateTime.fromMillisecondsSinceEpoch(e.time), e.close));
    _currentAggregateHistory.addAll(data);

    _aggregatePriceInterval = _currentAggregateHistory[1].key.difference(_currentAggregateHistory[0].key);
    return TrContinuousProductPricesEvent(data: _currentAggregateHistory, type: TrContinuousProductPricesEventType.fullUpdate);
  }

  Future<void> onNewTrProductPrice(TrProductPrice? price, {int numberOfRetriesMade = 0}) async {
    Future<TrContinuousProductPricesEvent?> processNewTrProductPrice() async {
      if (price == null) {
        return null;
      }

      TrContinuousProductPricesEvent result;

      if (currentTimeExceedsAggregatePriceInterval()) {
        result = TrContinuousProductPricesEvent(
          data: [
            MapEntry(DateTime.fromMillisecondsSinceEpoch(price.bid.time), price.bid.price)
          ],
          type: TrContinuousProductPricesEventType.additionUpdate,
        );
      } else {
        result = TrContinuousProductPricesEvent(
          data: [
            MapEntry(_behaviorSubject.value.data.last.key, price.bid.price)
          ],
          type: TrContinuousProductPricesEventType.lastValueUpdate,
        );
      }

      _preOpen = price.pre.price;
      loadClosePriceToStream();

      return result;
    }

    if (!_behaviorSubject.hasListener) {
      if (numberOfRetriesMade++ > 5) {
        return;
      }
      await Future.delayed(const Duration(milliseconds: 100));
      return onNewTrProductPrice(price, numberOfRetriesMade: numberOfRetriesMade);
    }

    var result = await processNewTrProductPrice();

    if (result != null) {
      emitNewTrContinuousProductPrices(result);
    }
  }

  void loadClosePriceToStream() {
    if (!oneDayClosePriceCanBeDisplayed()) {
      return;
    }

    var preOpenPrice = _currentView == ChartDateRangeView.oneDay ? _preOpen! : _currentAggregateHistory.first.value;

    var dataPoints = _currentAggregateHistory.map((e) => MapEntry(e.key, preOpenPrice)).toList();
    dataPoints.add(MapEntry(DateTime.fromMillisecondsSinceEpoch(_expectedClosingTime), preOpenPrice));

    _secondaryBehaviorSubject.add(TrContinuousProductPricesEvent(
      data: dataPoints,
      type: TrContinuousProductPricesEventType.fullUpdate,
    ));
  }

  bool oneDayClosePriceCanBeDisplayed() {
    return !(_currentView == ChartDateRangeView.oneDay && _preOpen == null);
  }

  void emitNewTrContinuousProductPrices(TrContinuousProductPricesEvent event) {
    _behaviorSubject.add(event);
  }

  bool currentTimeExceedsAggregatePriceInterval() {
    return DateTime.now().difference(_behaviorSubject.value.data.last.key) > _aggregatePriceInterval;
  }

  @override
  FutureOr onDispose() async {
    await _priceStreamSubscription?.cancel();
  }
}
