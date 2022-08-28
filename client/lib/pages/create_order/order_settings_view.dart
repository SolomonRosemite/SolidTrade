import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:solidtrade/components/base/st_page.dart';
import 'package:solidtrade/components/base/st_stream_builder.dart';
import 'package:solidtrade/components/base/st_widget.dart';
import 'package:solidtrade/data/models/enums/client_enums/order_type.dart';
import 'package:solidtrade/data/models/enums/shared_enums/buy_or_sell.dart';
import 'package:solidtrade/data/models/trade_republic/tr_product_price.dart';
import 'package:solidtrade/pages/create_order/components/order_validation_hint.dart';
import 'package:solidtrade/services/util/extensions/stream_extensions.dart';

class OrderSettingsView extends StatefulWidget {
  const OrderSettingsView({
    Key? key,
    required this.orderType,
    required this.trProductPrice,
    required this.buyOrSell,
    required this.name,
  }) : super(key: key);
  final Stream<TrProductPrice?> trProductPrice;
  final String name;
  final BuyOrSell buyOrSell;
  final OrderType orderType;

  @override
  State<OrderSettingsView> createState() => _OrderSettingsViewState();
}

class _OrderSettingsViewState extends State<OrderSettingsView> with STWidget {
  UserInputValidationResult _inputValidation = UserInputValidationResult.validInput();
  double? _definedStopLimitPrice;

  String _orderSettingsDescription(double currentPrice) {
    switch (widget.buyOrSell) {
      case BuyOrSell.buy:
        switch (widget.orderType) {
          case OrderType.market:
            throw ("Did not expect market order");
          case OrderType.stop:
            return translations.editOrderSettingsView.buyStopOrderDescription(widget.name, currentPrice);
          case OrderType.limit:
            return translations.editOrderSettingsView.buyLimitOrderDescription(widget.name, currentPrice);
        }
      case BuyOrSell.sell:
        switch (widget.orderType) {
          case OrderType.market:
            throw ("Did not expect market order");
          case OrderType.stop:
            return translations.editOrderSettingsView.sellStopOrderDescription(widget.name, currentPrice);
          case OrderType.limit:
            return translations.editOrderSettingsView.sellLimitOrderDescription(widget.name, currentPrice);
        }
    }
  }

  @override
  Widget build(BuildContext context) {
    return STPage(
      page: () => Scaffold(
        backgroundColor: colors.orderDescriptionColor,
        appBar: AppBar(elevation: 0, backgroundColor: colors.orderDescriptionColor, foregroundColor: colors.foreground),
        body: Container(
          margin: const EdgeInsets.only(bottom: 15, left: 20, right: 20),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                "${widget.orderType.fullName} settings",
                style: Theme.of(context).textTheme.headline4!.copyWith(fontWeight: FontWeight.w600),
                textAlign: TextAlign.left,
              ),
              const SizedBox(height: 10),
              STStreamBuilder<TrProductPrice>(
                stream: widget.trProductPrice,
                builder: (_, price) => Text(
                  _orderSettingsDescription(price.getPriceDependingOfBuyOrSell(widget.buyOrSell)),
                  style: TextStyle(fontSize: 16.5, color: colors.lessSoftForeground),
                ),
              ),
              const SizedBox(height: 15),
              ClipRRect(
                borderRadius: BorderRadius.circular(10),
                child: Container(
                  color: !_inputValidation.isValid ? colors.redErrorBackground : colors.softBackground,
                  child: TextField(
                    autofocus: true,
                    onChanged: _onPriceInputChanged,
                    textAlign: TextAlign.left,
                    inputFormatters: [
                      FilteringTextInputFormatter.allow(RegExp(r'^\d+\.?\d{0,5}')),
                    ],
                    keyboardType: const TextInputType.numberWithOptions(decimal: true),
                    decoration: InputDecoration(
                      isDense: true,
                      contentPadding: const EdgeInsets.only(left: 10, right: 10, bottom: 15, top: 5),
                      border: InputBorder.none,
                      labelText: "${widget.orderType.name} price",
                      labelStyle: TextStyle(fontSize: 16, color: colors.foreground),
                      suffixText: "€",
                      hintText: "0",
                      suffixStyle: TextStyle(color: colors.foreground),
                      hintStyle: TextStyle(color: colors.softForeground),
                    ),
                    style: TextStyle(color: colors.foreground),
                  ),
                ),
              ),
              const SizedBox(height: 10),
              OrderValidationHint(inputValidation: _inputValidation),
              const Spacer(),
              ClipRRect(
                borderRadius: BorderRadius.circular(10),
                child: ElevatedButton(
                  style: ElevatedButton.styleFrom(
                    minimumSize: const Size.fromHeight(50),
                  ).copyWith(
                    backgroundColor: MaterialStateProperty.resolveWith<Color>(
                      (Set<MaterialState> states) {
                        if (states.contains(MaterialState.disabled)) {
                          return colors.softBackground;
                        }

                        return Theme.of(context).colorScheme.primary;
                      },
                    ),
                  ),
                  onPressed: !_inputValidation.isValid || _definedStopLimitPrice == null ? null : () => _onContinue(context),
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
    );
  }

  void _onPriceInputChanged(String input) async {
    var parsedInput = double.tryParse(input);
    _definedStopLimitPrice = parsedInput;

    var validationResult = await _validateInput(parsedInput);
    setState(() {
      _inputValidation = validationResult;
    });
  }

  void _onContinue(BuildContext context) => Navigator.pop(context, _definedStopLimitPrice);

  Future<UserInputValidationResult> _validateInput(double? input) async {
    if (input == null || input == 0) {
      return UserInputValidationResult.errorPriceCanNotBeZero();
    }

    var price = await widget.trProductPrice.currentValue;

    var inputPriceIsHigherThanCurrentMarket = input > price!.getPriceDependingOfBuyOrSell(widget.buyOrSell);

    switch (widget.buyOrSell) {
      case BuyOrSell.buy:
        switch (widget.orderType) {
          case OrderType.market:
            throw ("Did not expect market order");
          case OrderType.stop:
            return inputPriceIsHigherThanCurrentMarket ? UserInputValidationResult.validInput() : UserInputValidationResult.errorPriceMustBeHigher();
          case OrderType.limit:
            return inputPriceIsHigherThanCurrentMarket ? UserInputValidationResult.errorPriceMustBeLower() : UserInputValidationResult.validInput();
        }
      case BuyOrSell.sell:
        switch (widget.orderType) {
          case OrderType.market:
            throw ("Did not expect market order");
          case OrderType.stop:
            return inputPriceIsHigherThanCurrentMarket ? UserInputValidationResult.errorPriceMustBeLower() : UserInputValidationResult.validInput();
          case OrderType.limit:
            return inputPriceIsHigherThanCurrentMarket ? UserInputValidationResult.validInput() : UserInputValidationResult.errorPriceMustBeHigher();
        }
    }
  }
}
