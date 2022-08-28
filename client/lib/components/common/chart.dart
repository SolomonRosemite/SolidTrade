import 'dart:async';
import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import 'package:solidtrade/components/base/st_widget.dart';
import 'package:solidtrade/data/models/trade_republic/tr_continuous_product_prices_event.dart';
import 'package:syncfusion_flutter_charts/charts.dart';

class Chart extends StatefulWidget {
  const Chart({
    Key? key,
    this.secondaryStreamData,
    required this.dateTimeXAxis,
    required this.primaryStreamData,
  }) : super(key: key);

  final Stream<TrContinuousProductPricesEvent>? secondaryStreamData;
  final Stream<TrContinuousProductPricesEvent> primaryStreamData;
  final ChartAxis dateTimeXAxis;

  @override
  State<Chart> createState() => _ChartState();
}

class _ChartState extends State<Chart> with STWidget {
  final List<MapEntry<DateTime, double>> _secondaryData = [];
  final List<MapEntry<DateTime, double>> _data = [];

  StreamSubscription? _dataSecondarySubscription;
  StreamSubscription? _dataSubscription;
  bool isHoldingDownOnChart = false;

  @override
  void initState() {
    super.initState();
    _dataSubscription = widget.primaryStreamData.listen(onDataStreamUpdate);
    _dataSecondarySubscription = widget.secondaryStreamData?.listen(onSecondaryDataStreamUpdate);
  }

  void onDataStreamUpdate(TrContinuousProductPricesEvent event) {
    switch (event.type) {
      case TrContinuousProductPricesEventType.fullUpdate:
        _data.clear();
        _data.addAll(event.data);
        break;
      case TrContinuousProductPricesEventType.lastValueUpdate:
        if (_data.isNotEmpty) {
          _data.last = event.data.first;
        }
        break;
      case TrContinuousProductPricesEventType.additionUpdate:
        _data.addAll(event.data);
        break;
    }

    if (isHoldingDownOnChart) {
      return;
    }

    setState(() {
      _data;
    });
  }

  void onSecondaryDataStreamUpdate(TrContinuousProductPricesEvent event) {
    switch (event.type) {
      case TrContinuousProductPricesEventType.fullUpdate:
        _secondaryData.clear();
        _secondaryData.addAll(event.data);
        break;
      case TrContinuousProductPricesEventType.lastValueUpdate:
        if (_secondaryData.isNotEmpty) {
          _secondaryData.last = event.data.first;
        }
        break;
      case TrContinuousProductPricesEventType.additionUpdate:
        _secondaryData.addAll(event.data);
        break;
    }

    if (isHoldingDownOnChart) {
      return;
    }

    setState(() {
      _secondaryData;
    });
  }

  void onTrackballPositionChanging(TrackballArgs args) {
    var data = args.chartPointInfo.series!.dataSource as List<MapEntry<DateTime, double>>;

    String str;
    if (data.last.key.difference(data.first.key) < const Duration(days: 1)) {
      str = DateFormat("HH:mm").format(args.chartPointInfo.chartDataPoint!.x).trim();
    } else {
      str = DateFormat("dd.MM.yyyy HH:mm").format(args.chartPointInfo.chartDataPoint!.x).trim();
    }

    /// FYI: The character U+0589 "։" could be confused with the character U+003a "։", which is more common in source code.
    // We do this because when the character U+003a ":" is used the text is not centered for some reason.
    // To bypass this issue we use the character U+0589 "։" which looks the same and also keep the text centered.
    args.chartPointInfo.header = str.replaceFirst(":", "։");

    if (args.chartPointInfo.series?.name != translations.common.changeAsTextLiteral) {
      args.chartPointInfo.label = "${num.parse(args.chartPointInfo.label!).toStringAsFixed(2)}€";
      return;
    }

    var selectedPoint = args.chartPointInfo.chartDataPoint!.yValue as double;
    var percent = (selectedPoint / data.first.value - 1) * 100;
    var changePrefix = percent.isNegative ? "" : "+";

    args.chartPointInfo.label = "$changePrefix${percent.toStringAsFixed(2)}%";
    args.chartPointInfo.chartDataPoint!.pointColorMapper = percent.isNegative ? colors.stockRed : colors.stockGreen;
  }

  Color get _displayLineColor {
    final bool isNegative = !(_data.isEmpty || _secondaryData.isEmpty || (_data.last.value >= _secondaryData.first.value));
    return isNegative ? colors.stockRed : colors.stockGreen;
  }

  double get _yAxisStartingPoint {
    if (_data.isEmpty) {
      return 0;
    }
    var values = _data.map((e) => e.value).toList();
    values.sort();

    var result = (values.first - (values.last - values.first) * 0.5).roundToDouble();
    return result.isNegative ? 0 : result;
  }

  @override
  void dispose() {
    super.dispose();
    _dataSubscription?.cancel();
    _dataSecondarySubscription?.cancel();
  }

  @override
  Widget build(BuildContext context) {
    return SfCartesianChart(
      primaryXAxis: widget.dateTimeXAxis,
      primaryYAxis: NumericAxis(
        visibleMinimum: _yAxisStartingPoint,
        majorGridLines: const MajorGridLines(width: 0),
        axisLine: const AxisLine(width: 0),
      ),
      trackballBehavior: TrackballBehavior(
        enable: true,
        tooltipDisplayMode: TrackballDisplayMode.groupAllPoints,
        lineColor: colors.lessSoftForeground,
      ),
      onTrackballPositionChanging: onTrackballPositionChanging,
      onChartTouchInteractionDown: (_) => isHoldingDownOnChart = true,
      onChartTouchInteractionUp: (_) => isHoldingDownOnChart = false,
      series: <ChartSeries<MapEntry<DateTime, double>, DateTime>>[
        LineSeries<MapEntry<DateTime, double>, DateTime>(
          name: translations.common.changeAsTextLiteral,
          dataSource: _data,
          animationDuration: 500,
          enableTooltip: true,
          xValueMapper: (MapEntry<DateTime, double> x, _) => x.key,
          yValueMapper: (MapEntry<DateTime, double> y, _) => y.value,
          color: _displayLineColor,
        ),
        LineSeries<MapEntry<DateTime, double>, DateTime>(
          name: "Latest",
          dataSource: _data,
          animationDuration: 500,
          enableTooltip: true,
          xValueMapper: (MapEntry<DateTime, double> x, _) => x.key,
          yValueMapper: (MapEntry<DateTime, double> y, _) => y.value,
          color: _displayLineColor,
          // isVisible: false,
        ),
        FastLineSeries<MapEntry<DateTime, double>, DateTime>(
          name: "Close",
          dataSource: _secondaryData,
          animationDuration: 500,
          isVisible: _secondaryData.isNotEmpty,
          color: Colors.grey,
          dashArray: [
            3,
            6
          ],
          xValueMapper: (MapEntry<DateTime, double> x, _) => x.key,
          yValueMapper: (MapEntry<DateTime, double> y, _) => y.value,
        ),
      ],
    );
  }
}
