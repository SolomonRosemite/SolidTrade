import 'package:flutter/material.dart';
import 'package:solidtrade/components/base/st_widget.dart';
import 'package:solidtrade/components/base/st_stream_builder.dart';
import 'package:solidtrade/data/entities/base/base_position.dart';
import 'package:solidtrade/data/models/trade_republic/tr_product_price.dart';
import 'package:solidtrade/data/models/trade_republic/tr_stock_details.dart';
import 'package:solidtrade/providers/theme/app_theme.dart';
import 'package:solidtrade/services/util/extensions/double_extensions.dart';

class PositionMetrics extends StatefulWidget {
  const PositionMetrics({
    required this.trProductPriceStream,
    required this.trStockDetailsStream,
    required this.position,
    Key? key,
  }) : super(key: key);
  final Stream<TrProductPrice?> trProductPriceStream;
  final Stream<TrStockDetails?> trStockDetailsStream;
  final IPosition position;

  @override
  State<PositionMetrics> createState() => _PositionMetricsState();
}

class _PositionMetricsState extends State<PositionMetrics> with STWidget {
  bool _showPerformanceInPercent = true;

  double _getTotal(TrProductPrice prices) => prices.bid.price * widget.position.numberOfShares;

  String _getPerformance(TrProductPrice prices, bool inPercent) {
    if (!inPercent) {
      return (_getTotal(prices) - widget.position.buyInPrice * widget.position.numberOfShares).toDefaultPrice();
    }

    return (_getTotal(prices) / widget.position.buyInPrice * widget.position.numberOfShares).toDefaultNumber(suffix: "%", maxFractionDigits: 3);
  }

  void _onClickOnPerformance(_) {
    setState(() {
      _showPerformanceInPercent = !_showPerformanceInPercent;
    });
  }

  @override
  Widget build(BuildContext context) {
    return Card(
      elevation: 0,
      borderOnForeground: false,
      shape: RoundedRectangleBorder(
        side: BorderSide(
          color: colors.themeColorType == ColorThemeType.dark ? colors.softForeground : colors.background,
          width: 3.5,
        ),
        borderRadius: BorderRadius.circular(4.0),
      ),
      color: colors.themeColorType == ColorThemeType.dark ? colors.softBackground : colors.background,
      child: STStreamBuilder<TrProductPrice>(
        stream: widget.trProductPriceStream,
        builder: (context, prices) => Container(
          margin: const EdgeInsets.symmetric(vertical: 5),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.center,
            children: [
              Row(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  _Metric(name: "Total", value: _getTotal(prices).toDefaultPrice()),
                  _Metric(name: "Shares", value: widget.position.numberOfShares.toDefaultNumber()),
                ],
              ),
              Row(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  _Metric(name: "Buy in", value: widget.position.buyInPrice.toDefaultPrice()),
                  _Metric(
                    name: "Performance",
                    value: _getPerformance(prices, _showPerformanceInPercent),
                    onTapDown: _onClickOnPerformance,
                  ),
                ],
              ),
            ],
          ),
        ),
      ),
    );
  }
}

class _Metric extends StatelessWidget {
  const _Metric({
    Key? key,
    required this.name,
    required this.value,
    this.onTapDown,
    this.fallbackValue = "--",
  }) : super(key: key);
  final String fallbackValue;
  final String? value;
  final String name;
  final void Function(TapDownDetails)? onTapDown;

  @override
  Widget build(BuildContext context) {
    return Expanded(
      child: GestureDetector(
        onTapDown: onTapDown,
        child: Container(
          margin: const EdgeInsets.symmetric(vertical: 8, horizontal: 10),
          child: Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              Text(name),
              Text(value ?? fallbackValue),
            ],
          ),
        ),
      ),
    );
  }
}
