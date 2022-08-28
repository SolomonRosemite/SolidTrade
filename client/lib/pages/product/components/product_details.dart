import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import 'package:solidtrade/components/base/st_widget.dart';
import 'package:solidtrade/data/models/trade_republic/tr_product_info.dart';
import 'package:solidtrade/data/models/trade_republic/tr_stock_details.dart';
import 'package:solidtrade/services/util/extensions/string_extensions.dart';

class ProductDetails extends StatelessWidget with STWidget {
  ProductDetails({Key? key, required this.trStockDetailsStream, required this.productInfo, required this.isStock}) : super(key: key);
  final Stream<TrStockDetails?> trStockDetailsStream;
  final TrProductInfo productInfo;
  final bool isStock;

  DateTime fromUnixToDate(int value) {
    return DateTime.fromMillisecondsSinceEpoch(value, isUtc: true);
  }

  Widget _detailEntry(BuildContext context, String name, String value) {
    const double fontSize = 15.25;
    final style = Theme.of(context).textTheme.bodyText2!.copyWith(fontSize: fontSize);

    return Container(
      margin: const EdgeInsets.symmetric(vertical: 3),
      child: Row(mainAxisAlignment: MainAxisAlignment.spaceBetween, children: [
        Text(
          name + ":",
          style: style.copyWith(
            color: Colors.grey[400],
          ),
        ),
        Text(
          value,
          style: style,
        )
      ]),
    );
  }

  List<Widget> _loadDetailEntries(BuildContext context) {
    final dateFormatter = DateFormat("dd.MM.yyyy");
    final tradingHoursFormatter = DateFormat("HH:mm");

    final isCrypto = productInfo.typeId == "crypto";
    final ticker = isCrypto ? productInfo.homeSymbol! : productInfo.intlSymbol ?? "--";

    if (isCrypto || isStock) {
      final name = productInfo.name.length > 30 ? productInfo.shortName : productInfo.name;

      final details = [
        _detailEntry(context, "Name", name),
        _detailEntry(context, "Isin", productInfo.isin),
      ];
      if (productInfo.company.ipoDate != null) {
        details.addAll([
          _detailEntry(
            context,
            "IPO Date*",
            dateFormatter.format(fromUnixToDate(productInfo.company.ipoDate!)),
          ),
          _detailEntry(context, "Ticker", ticker)
        ]);
      }
      return details;
    }

    final derivativeInfo = productInfo.derivativeInfo!;
    final firstTradingDay = DateTime.parse(derivativeInfo.properties.firstTradingDay);
    final lastTradingDay = derivativeInfo.properties.lastTradingDay != null ? DateTime.parse(derivativeInfo.properties.lastTradingDay!) : null;

    final tradingTimes = productInfo.exchanges.first.tradingTimes;

    final details = [
      _detailEntry(context, "Product", derivativeInfo.productCategoryName),
      _detailEntry(context, "Type", derivativeInfo.properties.optionType.capitalize()),
      _detailEntry(context, "Issuer", productInfo.issuerDisplayName ?? "--"),
      _detailEntry(context, "Isin", productInfo.isin),
      _detailEntry(context, "Wkn", productInfo.wkn),
      _detailEntry(context, "Underlying", derivativeInfo.underlying.name),
      _detailEntry(context, "Underlying Currency", derivativeInfo.properties.currency),
      _detailEntry(context, "Settlement", derivativeInfo.properties.settlementType.capitalize()),
      _detailEntry(context, "Ratio", derivativeInfo.properties.size.toString()),
      _detailEntry(context, "First Trading Day", dateFormatter.format(firstTradingDay)),
    ];

    if (lastTradingDay != null) {
      details.add(_detailEntry(context, "Last Trading Day", dateFormatter.format(lastTradingDay)));
    }

    if (derivativeInfo.properties.expiry != null) {
      details.add(_detailEntry(context, "Expiry", dateFormatter.format(DateTime.parse(derivativeInfo.properties.expiry!))));
    }

    if (tradingTimes != null) {
      details.add(_detailEntry(
        context,
        "Trading Hours",
        tradingHoursFormatter.format(fromUnixToDate(tradingTimes.start)) + " - " + tradingHoursFormatter.format(fromUnixToDate(tradingTimes.end)),
      ));
    }

    return [
      ...details
    ];
  }

  @override
  Widget build(BuildContext context) {
    return Column(
      children: [
        ..._loadDetailEntries(context),
      ],
    );
  }
}
