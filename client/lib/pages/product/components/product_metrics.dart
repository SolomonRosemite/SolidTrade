import 'package:flutter/material.dart';
import 'package:solidtrade/components/base/st_widget.dart';
import 'package:solidtrade/components/base/st_stream_builder.dart';
import 'package:solidtrade/data/models/enums/shared_enums/position_type.dart';
import 'package:solidtrade/data/models/trade_republic/tr_product_info.dart';
import 'package:solidtrade/data/models/trade_republic/tr_product_price.dart';
import 'package:solidtrade/data/models/trade_republic/tr_stock_details.dart';
import 'package:solidtrade/providers/theme/app_theme.dart';
import 'package:solidtrade/services/util/extensions/double_extensions.dart';
import 'package:solidtrade/services/util/tr_util.dart';

class ProductMetrics extends StatelessWidget with STWidget {
  ProductMetrics({
    required this.trProductPriceStream,
    required this.trStockDetailsStream,
    required this.isStock,
    required this.productInfo,
    Key? key,
  }) : super(key: key);
  final Stream<TrProductPrice?> trProductPriceStream;
  final Stream<TrStockDetails?> trStockDetailsStream;
  final TrProductInfo productInfo;
  final bool isStock;

  Widget _additionalStockInfoIfExistent() {
    if (!isStock) {
      return const SizedBox.shrink();
    }

    return STStreamBuilder<TrStockDetails>(
      stream: trStockDetailsStream,
      builder: (context, result) {
        final hasMarketCapValue = result.company.marketCapSnapshot != null;

        final tupleNameForNumber = hasMarketCapValue ? TrUtil.getNameForNumber(result.company.marketCapSnapshot!) : null;
        return Row(
          children: [
            _Metric(
              name: translations.productPage.marketCap,
              value: tupleNameForNumber == null ? "--" : tupleNameForNumber.t2.toStringAsFixed(3) + " " + translations.productPage.nameOfNumberPrefix(tupleNameForNumber.t1),
            ),
            _Metric(name: "P/E", value: result.company.peRatioSnapshot?.toStringAsFixed(2)),
          ],
        );
      },
    );
  }

  List<Widget> _additionalDerivativesInfoIfExistent() {
    List<Widget> rows = [];

    if (productInfo.derivativeInfo == null) {
      return rows;
    }

    DerivativeInfo di = productInfo.derivativeInfo!;

    rows.add(
      Row(
        children: [
          _Metric(name: "Strike", value: di.properties.strike.toDefaultPrice(currencyCode: di.properties.currency)),
          productInfo.positionType == PositionType.warrant
              ? _Metric(
                  name: "Delta",
                  value: di.properties.delta!.toStringAsFixed(2),
                )
              : _Metric(
                  name: "Leverage",
                  value: di.properties.leverage?.toStringAsFixed(2),
                  fallbackValue: "Not specified",
                ),
        ],
      ),
    );

    if (productInfo.positionType == PositionType.knockout) {
      rows.add(
        Row(
          children: [
            _Metric(
              name: "Barrier",
              value: di.properties.barrier?.toDefaultPrice(currencyCode: di.properties.currency),
            ),
            _Metric.empty(),
          ],
        ),
      );
    }

    return rows;
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
        stream: trProductPriceStream,
        builder: (context, prices) => Container(
          margin: const EdgeInsets.symmetric(vertical: 5),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.center,
            children: [
              Row(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  _Metric(name: "Bid", value: prices.bid.price.toDefaultPrice()),
                  _Metric(name: "Ask", value: prices.ask?.price.toDefaultPrice()),
                ],
              ),
              !productInfo.isDerivative
                  ? Row(
                      mainAxisAlignment: MainAxisAlignment.center,
                      children: [
                        _Metric(name: "Open", value: prices.open.price.toDefaultPrice()),
                        _Metric(name: "Close", value: prices.pre.price.toDefaultPrice()),
                      ],
                    )
                  : const SizedBox.shrink(),
              _additionalStockInfoIfExistent(),
              ..._additionalDerivativesInfoIfExistent(),
            ],
          ),
        ),
      ),
    );
  }
}

class _Metric extends StatelessWidget {
  const _Metric({Key? key, required this.name, required this.value, this.fallbackValue = "--"}) : super(key: key);
  final String fallbackValue;
  final String? value;
  final String name;

  @override
  Widget build(BuildContext context) {
    return Expanded(
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
    );
  }

  factory _Metric.empty() => const _Metric(name: "", value: "");
}
