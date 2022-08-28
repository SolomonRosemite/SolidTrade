import 'package:flutter/material.dart';
import 'package:solidtrade/components/base/st_widget.dart';
import 'package:solidtrade/components/base/st_stream_builder.dart';
import 'package:solidtrade/data/models/trade_republic/tr_stock_details.dart';
import 'package:solidtrade/services/util/tr_util.dart';

enum BuyHoldSell { buy, hold, sell }

class AnalystsRecommendations extends StatelessWidget with STWidget {
  AnalystsRecommendations({Key? key, required this.trStockDetailsStream}) : super(key: key);
  final Stream<TrStockDetails?> trStockDetailsStream;

  Widget _analystsResultWidget(BuildContext context, BuyHoldSell buyHoldSell, double percent) {
    Color color;
    String buyHoldSellText;
    Color? textColor = percent == 0 ? colors.lessSoftForeground : null;

    switch (buyHoldSell) {
      case BuyHoldSell.buy:
        color = colors.stockGreen;
        buyHoldSellText = "Buy";
        break;
      case BuyHoldSell.hold:
        color = colors.lessSoftForeground;
        buyHoldSellText = "Hold";
        break;
      case BuyHoldSell.sell:
        color = colors.stockRed;
        buyHoldSellText = "Sell";
        break;
    }

    return Column(
      mainAxisAlignment: MainAxisAlignment.start,
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(
          buyHoldSellText,
          style: Theme.of(context).textTheme.subtitle1!.copyWith(color: color),
        ),
        Text(
          percent.toStringAsFixed(1) + "%",
          style: Theme.of(context).textTheme.headline6!.copyWith(color: textColor),
        )
      ],
    );
  }

  @override
  Widget build(BuildContext context) {
    return STStreamBuilder<TrStockDetails>(
      stream: trStockDetailsStream,
      builder: (context, details) {
        Recommendations recommendations = details.analystRating.recommendations;
        double buyRecommendation = 100 * (recommendations.buy + recommendations.outperform) / TrUtil.productPageGetAnalystsCount(recommendations);
        double holdRecommendation = 100 * recommendations.hold / TrUtil.productPageGetAnalystsCount(recommendations);
        double sellRecommendation = 100 * (recommendations.sell + recommendations.underperform) / TrUtil.productPageGetAnalystsCount(recommendations);

        return Column(
          mainAxisAlignment: MainAxisAlignment.start,
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              translations.productPage.whatAnalystsSayContent(details),
            ),
            const SizedBox(height: 10),
            Row(
              children: [
                Expanded(flex: buyRecommendation.toInt(), child: Container(color: colors.stockGreen, height: 10)),
                const SizedBox(width: 2.5),
                Expanded(flex: holdRecommendation.toInt(), child: Container(color: colors.softForeground, height: 10)),
                const SizedBox(width: 2.5),
                Expanded(flex: sellRecommendation.toInt(), child: Container(color: colors.stockRed, height: 10)),
              ],
            ),
            const SizedBox(height: 5),
            Row(children: [
              _analystsResultWidget(context, BuyHoldSell.buy, buyRecommendation),
              const SizedBox(width: 15),
              _analystsResultWidget(context, BuyHoldSell.hold, holdRecommendation),
              const SizedBox(width: 15),
              _analystsResultWidget(context, BuyHoldSell.sell, sellRecommendation),
            ]),
            const SizedBox(height: 10),
            Text(
              "This data is updated every banking day, but analysts only update their estimates periodically.",
              style: Theme.of(context).textTheme.bodyText2!.copyWith(color: colors.lessSoftForeground, fontSize: 12),
            )
          ],
        );
      },
    );
  }
}
