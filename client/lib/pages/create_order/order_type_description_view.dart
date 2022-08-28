import 'package:flutter/material.dart';
import 'package:solidtrade/components/base/st_page.dart';
import 'package:solidtrade/components/base/st_widget.dart';
import 'package:solidtrade/components/common/prevent_render_flex_overflow_wrapper.dart';
import 'package:solidtrade/data/models/common/constants.dart';
import 'package:solidtrade/data/models/enums/client_enums/order_type.dart';
import 'package:solidtrade/data/models/enums/shared_enums/buy_or_sell.dart';
import 'package:solidtrade/services/util/util.dart';
import 'package:url_launcher/url_launcher.dart';

class OrderTypeDescriptionView extends StatelessWidget with STWidget {
  OrderTypeDescriptionView({
    Key? key,
    required this.name,
    required this.orderType,
    required this.buyOrSell,
    required this.nextPage,
  }) : super(key: key);
  final String name;
  final BuyOrSell buyOrSell;
  final OrderType orderType;
  final Widget nextPage;

  @override
  Widget build(BuildContext context) {
    return STPage(
      page: () => Scaffold(
        backgroundColor: colors.orderDescriptionColor,
        appBar: AppBar(elevation: 0, backgroundColor: colors.orderDescriptionColor, foregroundColor: colors.foreground),
        body: Container(
          margin: const EdgeInsets.only(bottom: 15, left: 20, right: 20),
          child: PreventColumnRenderFlexOverflowWrapper(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  orderType.fullName,
                  style: Theme.of(context).textTheme.headline4!.copyWith(fontWeight: FontWeight.w600),
                  textAlign: TextAlign.left,
                ),
                const SizedBox(height: 10),
                Text(_orderDescription, style: TextStyle(fontSize: 16.5, color: colors.lessSoftForeground)),
                const Spacer(),
                Image.asset(_orderTypeImage),
                const Spacer(flex: 3),
                TextButton(
                  onPressed: _onClickLearnAboutOrderTypes,
                  child: Row(
                    mainAxisAlignment: MainAxisAlignment.center,
                    children: [
                      const Icon(Icons.info, size: 25),
                      Container(
                        margin: const EdgeInsets.symmetric(horizontal: 10),
                        child: Text(
                          "Tip: Want to learn more about order types?",
                          overflow: TextOverflow.ellipsis,
                          style: TextStyle(color: colors.foreground),
                        ),
                      ),
                    ],
                  ),
                ),
                const SizedBox(height: 10),
                ClipRRect(
                  borderRadius: BorderRadius.circular(10),
                  child: ElevatedButton(
                    style: ElevatedButton.styleFrom(
                      minimumSize: const Size.fromHeight(50),
                    ),
                    onPressed: () => _pushToNextPage(context),
                    child: const Text(
                      "Continue",
                      style: TextStyle(color: Colors.white),
                    ),
                  ),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }

  String get _orderTypeImage {
    switch (buyOrSell) {
      case BuyOrSell.buy:
        switch (orderType) {
          case OrderType.market:
            throw ("Did not expect market order");
          case OrderType.stop:
            return colors.stopOrderBuy;
          case OrderType.limit:
            return colors.limitOrderBuy;
        }
      case BuyOrSell.sell:
        switch (orderType) {
          case OrderType.market:
            throw ("Did not expect market order");
          case OrderType.stop:
            return colors.stopOrderSell;
          case OrderType.limit:
            return colors.limitOrderSell;
        }
    }
  }

  String get _orderDescription {
    switch (buyOrSell) {
      case BuyOrSell.buy:
        switch (orderType) {
          case OrderType.market:
            throw ("Did not expect market order");
          case OrderType.stop:
            return translations.createOrderPage.buyStopOrderDescription(name);
          case OrderType.limit:
            return translations.createOrderPage.buyLimitOrderDescription(name);
        }
      case BuyOrSell.sell:
        switch (orderType) {
          case OrderType.market:
            throw ("Did not expect market order");
          case OrderType.stop:
            return translations.createOrderPage.sellStopOrderDescription(name);
          case OrderType.limit:
            return translations.createOrderPage.sellLimitOrderDescription(name);
        }
    }
  }

  Future<void> _pushToNextPage(BuildContext context) async {
    var result = await Util.pushToRoute(context, nextPage);
    Navigator.pop(context, result);
  }

  void _onClickLearnAboutOrderTypes() => launch(Constants.learnMoreAboutOrderTypesLink);
}
