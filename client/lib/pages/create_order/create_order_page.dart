import 'dart:async';

import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:get_it/get_it.dart';
import 'package:intl/intl.dart';
import 'package:modal_bottom_sheet/modal_bottom_sheet.dart';
import 'package:solidtrade/components/base/st_page.dart';
import 'package:solidtrade/components/base/st_stream_builder.dart';
import 'package:solidtrade/components/base/st_widget.dart';
import 'package:solidtrade/components/common/prevent_render_flex_overflow_wrapper.dart';
import 'package:solidtrade/data/entities/portfolio.dart';
import 'package:solidtrade/data/models/enums/client_enums/order_type.dart';
import 'package:solidtrade/data/models/enums/shared_enums/buy_or_sell.dart';
import 'package:solidtrade/data/models/enums/shared_enums/position_type.dart';
import 'package:solidtrade/data/models/request_response/request_response.dart';
import 'package:solidtrade/data/models/trade_republic/tr_product_info.dart';
import 'package:solidtrade/data/models/trade_republic/tr_product_price.dart';
import 'package:solidtrade/pages/create_order/components/order_type_selection.dart';
import 'package:solidtrade/pages/create_order/components/order_validation_hint.dart';
import 'package:solidtrade/pages/create_order/order_settings_view.dart';
import 'package:solidtrade/pages/create_order/order_type_description_view.dart';
import 'package:solidtrade/services/stream/knockout_service.dart';
import 'package:solidtrade/services/stream/ongoing_knockout_service.dart';
import 'package:solidtrade/services/stream/ongoing_warrant_service.dart';
import 'package:solidtrade/services/stream/portfolio_service.dart';
import 'package:solidtrade/services/stream/stock_service.dart';
import 'package:solidtrade/services/stream/warrant_service.dart';
import 'package:solidtrade/services/util/extensions/double_extensions.dart';
import 'package:solidtrade/services/util/local_auth_util.dart';
import 'package:solidtrade/services/util/tr_util.dart';
import 'package:solidtrade/services/util/util.dart';
import 'package:flutter_datetime_picker/flutter_datetime_picker.dart';

class CreateOrderPage extends StatefulWidget {
  const CreateOrderPage({
    Key? key,
    required this.productInfo,
    required this.buyOrSell,
    required this.trProductPriceStream,
    required this.productDetails,
  }) : super(key: key);
  final Stream<TrProductPrice?> trProductPriceStream;
  final TrUiProductDetails productDetails;
  final TrProductInfo productInfo;
  final BuyOrSell buyOrSell;

  @override
  State<CreateOrderPage> createState() => _CreateOrderPageState();
}

class _CreateOrderPageState extends State<CreateOrderPage> with STWidget {
  final _ongoingKnockoutService = GetIt.instance.get<OngoingKnockoutService>();
  final _ongoingWarrantService = GetIt.instance.get<OngoingWarrantService>();
  final _knockoutService = GetIt.instance.get<KnockoutService>();
  final _warrantService = GetIt.instance.get<WarrantService>();
  final _stockService = GetIt.instance.get<StockService>();
  final _portfolioService = GetIt.instance.get<PortfolioService>();

  late TrProductPrice _currentPrice;
  late DateTime _goodUntil = _defaultGoodUntil;

  UserInputValidationResult _inputValidation = UserInputValidationResult.validInput();
  OrderType _orderType = OrderType.market;
  double _definedStopLimitPrice = 0;
  double _numberOfShares = 0;

  bool get _enableCreateOrderButton => !_isMarketOrder && _totalPrice != 0 || _totalPrice != 0 && _inputValidation.isValid;
  bool get _isMarketOrder => _orderType == OrderType.market;
  DateTime get _defaultGoodUntil {
    DateTime tomorrow = DateTime.now().add(const Duration(days: 1));
    return DateTime(tomorrow.year, tomorrow.month, tomorrow.day);
  }

  double get _totalPrice {
    double _getPriceOfSingleShare() {
      switch (_orderType) {
        case OrderType.market:
          return _currentPrice.getPriceDependingOfBuyOrSell(widget.buyOrSell);
        case OrderType.stop:
        case OrderType.limit:
          return _definedStopLimitPrice;
      }
    }

    return (_getPriceOfSingleShare() * _numberOfShares);
  }

  double _getNumberOfSharesOwned(Portfolio portfolio) {
    double numberOfShares = 0;
    var stocks = portfolio.stockPositions.where((p) => p.isin == widget.productInfo.isin);
    var knockOuts = portfolio.knockOutPositions.where((p) => p.isin == widget.productInfo.isin);
    var warrants = portfolio.warrantPositions.where((p) => p.isin == widget.productInfo.isin);
    if (stocks.isNotEmpty) {
      numberOfShares = stocks.first.numberOfShares;
    } else if (knockOuts.isNotEmpty) {
      numberOfShares = knockOuts.first.numberOfShares;
    } else if (warrants.isNotEmpty) {
      numberOfShares = warrants.first.numberOfShares;
    }

    return numberOfShares;
  }

  @override
  Widget build(BuildContext context) {
    return STStreamBuilder<Portfolio>(
      stream: _portfolioService.stream$,
      builder: (_, portfolio) => STPage(
        page: () => Scaffold(
          appBar: AppBar(
            title: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(widget.productInfo.shortName, style: Theme.of(context).textTheme.bodyText1),
                const SizedBox(height: 1),
                Text(translations.createOrderPage.cashAvailable(portfolio.cash), style: Theme.of(context).textTheme.bodyText1!.copyWith(fontWeight: FontWeight.w400)),
              ],
            ),
            actions: [
              TextButton(
                style: TextButton.styleFrom(
                  padding: const EdgeInsets.symmetric(horizontal: 10),
                ),
                onPressed: _selectOrderType,
                child: Row(
                  children: [
                    const Icon(Icons.keyboard_arrow_down),
                    Text(_orderType.fullName),
                  ],
                ),
              ),
            ],
            elevation: 0,
            backgroundColor: colors.background,
            foregroundColor: colors.foreground,
          ),
          body: Container(
            margin: const EdgeInsets.only(bottom: 15, left: 20, right: 20),
            child: STStreamBuilder<TrProductPrice>(
              stream: widget.trProductPriceStream,
              builder: (_, price) {
                _currentPrice = price;
                return PreventColumnRenderFlexOverflowWrapper(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Row(
                        children: [
                          Text(
                            translations.createOrderPage.buySellProduct(
                              widget.buyOrSell,
                              widget.productInfo.tickerOrShortName,
                            ),
                            style: Theme.of(context).textTheme.headline4!.copyWith(fontWeight: FontWeight.w600),
                            textAlign: TextAlign.left,
                          ),
                          const Spacer(),
                          Util.loadImage(
                            widget.productDetails.imageUrl,
                            60,
                            backgroundColor: colors.softBackground,
                          ),
                          const SizedBox(width: 10),
                        ],
                      ),
                      widget.productInfo.isDerivative
                          ? Container(
                              margin: const EdgeInsets.only(bottom: 20),
                              child: Text(
                                widget.productInfo.shortName,
                                style: Theme.of(context).textTheme.bodyText2!.copyWith(fontSize: 16),
                              ),
                            )
                          : const SizedBox.shrink(),
                      Container(
                        margin: const EdgeInsets.only(bottom: 10),
                        child: Text(
                          "➚ 1 ${widget.productInfo.tickerOrShortName} = ${price.getPriceDependingOfBuyOrSell(widget.buyOrSell).toDefaultPrice()}",
                          style: Theme.of(context).textTheme.bodyText1!.copyWith(color: colors.blueText, fontSize: 17),
                        ),
                      ),
                      _SharesInput(
                        name: widget.productInfo.tickerOrShortName,
                        isBuy: widget.buyOrSell.isBuy,
                        onChanged: _onNumberOfSharesChanged,
                        totalPrice: _totalPrice,
                        numberOfSharesOwned: _getNumberOfSharesOwned(portfolio),
                      ),
                      const SizedBox(height: 10),
                      !_isMarketOrder
                          ? _EditStopLimitPrice(
                              name: widget.productInfo.tickerOrShortName,
                              orderType: _orderType,
                              price: _definedStopLimitPrice,
                              onPressed: () => _openOrderSettingsAndSet(withOrderDescription: false),
                            )
                          : const SizedBox.shrink(),
                      const SizedBox(height: 10),
                      !_isMarketOrder
                          ? _SelectGoodUntilDate(
                              currentGoodUntilDate: _goodUntil,
                              onGoodUntilDateChanged: _onGoodUntilDateChanged,
                            )
                          : const SizedBox.shrink(),
                      Container(
                        margin: const EdgeInsets.symmetric(vertical: 10),
                        height: 20,
                        child: OrderValidationHint(inputValidation: _inputValidation),
                      ),
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
                          onPressed: _enableCreateOrderButton ? _createOrder : null,
                          child: Text(
                            _isMarketOrder
                                ? translations.createOrderPage.buySellProduct(
                                    widget.buyOrSell,
                                    widget.productInfo.tickerOrShortName,
                                  )
                                : translations.createOrderPage.createOrderAsTextLiteral,
                            style: const TextStyle(color: Colors.white),
                          ),
                        ),
                      ),
                    ],
                  ),
                );
              },
            ),
          ),
        ),
      ),
    );
  }

  Future<void> _createOrder() async {
    if (!(await UtilLocalAuth.authenticate())) {
      return;
    }

    var closeDialog = Util.showLoadingDialog(context);
    var response = await _makeAndGetRequestResponse();

    closeDialog();

    if (!response.isSuccessful) {
      Util.openDialog(context, "Something didn't go right", message: response.error!.userFriendlyMessage);
      return;
    }

    Navigator.pop(context, _orderType);
  }

  Future<RequestResponse> _makeAndGetRequestResponse() {
    switch (widget.productInfo.positionType) {
      case PositionType.warrant:
        if (_isMarketOrder) {
          return _warrantService.buyOrSellAtMarketPrice(
            widget.buyOrSell,
            widget.productInfo.isinWithExchangeExtension,
            _numberOfShares,
          );
        }
        return _ongoingWarrantService.enterOrExitOngoingOrder(
          widget.buyOrSell,
          _orderType,
          widget.productInfo.isinWithExchangeExtension,
          _numberOfShares,
          _goodUntil,
          _definedStopLimitPrice,
        );
      case PositionType.knockout:
        if (_isMarketOrder) {
          return _knockoutService.buyOrSellAtMarketPrice(
            widget.buyOrSell,
            widget.productInfo.isinWithExchangeExtension,
            _numberOfShares,
          );
        }
        return _ongoingKnockoutService.enterOrExitOngoingOrder(
          widget.buyOrSell,
          _orderType,
          widget.productInfo.isinWithExchangeExtension,
          _numberOfShares,
          _goodUntil,
          _definedStopLimitPrice,
        );
      case PositionType.stock:
        return _stockService.buyOrSellAtMarketPrice(
          widget.buyOrSell,
          widget.productInfo.isinWithExchangeExtension,
          _numberOfShares,
        );
    }
  }

  OrderSettingsView get _getOrderSettingsView => OrderSettingsView(
        buyOrSell: widget.buyOrSell,
        name: widget.productInfo.shortName,
        orderType: _orderType,
        trProductPrice: widget.trProductPriceStream,
      );

  Future<double?> _openOrderDescription() {
    return Util.pushToRoute<double>(
      context,
      OrderTypeDescriptionView(
        orderType: _orderType,
        buyOrSell: widget.buyOrSell,
        name: widget.productInfo.shortName,
        nextPage: _getOrderSettingsView,
      ),
    );
  }

  Future<void> _openOrderSettingsAndSet({bool withOrderDescription = true}) async {
    double? price;
    if (withOrderDescription) {
      price = await _openOrderDescription();
    } else {
      price = await Util.pushToRoute<double>(
        context,
        _getOrderSettingsView,
      );
    }

    price = price ?? _definedStopLimitPrice;

    setState(() {
      _definedStopLimitPrice = price!;
    });
  }

  void _onNumberOfSharesChanged(String numberOfSharesAsString) {
    _updateNumberOfShares(numberOfSharesAsString);
    _validateInput();
  }

  void _updateNumberOfShares(String numberOfSharesAsString) {
    setState(() {
      if (numberOfSharesAsString.isEmpty) {
        _numberOfShares = 0;
      }

      _numberOfShares = double.tryParse(numberOfSharesAsString) ?? _numberOfShares;
    });

    _validateInput();
  }

  void _validateInput() {
    if (_totalPrice > _portfolioService.current!.cash && _isMarketOrder) {
      setState(() {
        _inputValidation = UserInputValidationResult.insufficientFunds();
      });
      return;
    }

    setState(() {
      _inputValidation = UserInputValidationResult.validInput();
    });
  }

  void _onGoodUntilDateChanged(DateTime date) {
    setState(() {
      _goodUntil = date;
    });
  }

  Future<void> _selectOrderType() async {
    var selectedOrderType = await showMaterialModalBottomSheet<OrderType>(
      expand: false,
      context: context,
      backgroundColor: Colors.transparent,
      builder: (context) => Material(
        color: colors.background,
        child: SafeArea(
          top: false,
          child: OrderTypeSelection(buyOrSell: widget.buyOrSell, orderType: _orderType, positionType: widget.productInfo.positionType),
        ),
      ),
    );

    if (selectedOrderType == null || selectedOrderType == _orderType) {
      return;
    }

    setState(() {
      _orderType = selectedOrderType;
    });

    if (selectedOrderType != OrderType.market) {
      await _openOrderSettingsAndSet();
    }
  }
}

class _SharesInput extends StatelessWidget with STWidget {
  _SharesInput({
    Key? key,
    required this.name,
    required this.isBuy,
    required this.onChanged,
    required this.totalPrice,
    required this.numberOfSharesOwned,
  }) : super(key: key);
  final void Function(String) onChanged;
  final double numberOfSharesOwned;
  final double totalPrice;
  final String name;
  final bool isBuy;

  @override
  Widget build(BuildContext context) {
    return ClipRRect(
      borderRadius: BorderRadius.circular(10),
      child: Container(
        color: colors.softBackground,
        child: Padding(
          padding: const EdgeInsets.all(15),
          child: Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(name, style: const TextStyle(fontSize: 18, fontWeight: FontWeight.w700)),
                  const SizedBox(height: 2.5),
                  Text(translations.createOrderPage.sharesOwned(numberOfSharesOwned), style: TextStyle(fontSize: 16.5, color: colors.lessSoftForeground))
                ],
              ),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.end,
                  children: [
                    TextField(
                      autofocus: true,
                      textAlign: TextAlign.right,
                      onChanged: onChanged,
                      inputFormatters: [
                        FilteringTextInputFormatter.allow(RegExp(r'^\d+\.?\d{0,5}')),
                      ],
                      keyboardType: const TextInputType.numberWithOptions(decimal: true),
                      decoration: InputDecoration(
                        isDense: true,
                        hintText: isBuy ? "+0" : "-0",
                        contentPadding: const EdgeInsets.only(bottom: 2),
                        border: InputBorder.none,
                        hintStyle: TextStyle(color: colors.lessSoftForeground),
                      ),
                      style: TextStyle(color: colors.foreground),
                    ),
                    Text(translations.createOrderPage.totalPrice(totalPrice), style: TextStyle(fontSize: 16.5, color: colors.lessSoftForeground))
                  ],
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}

class _SelectGoodUntilDate extends StatelessWidget with STWidget {
  _SelectGoodUntilDate({
    Key? key,
    required this.currentGoodUntilDate,
    required this.onGoodUntilDateChanged,
  }) : super(key: key);
  final void Function(DateTime) onGoodUntilDateChanged;
  final DateTime currentGoodUntilDate;

  final dateFormat = DateFormat("dd.MM.yyy");
  String get _formattedDate => dateFormat.format(currentGoodUntilDate);

  @override
  Widget build(BuildContext context) {
    return _GeneralButtonWithDecoration(
      children: [
        const Text("Good until", style: TextStyle(fontSize: 17, fontWeight: FontWeight.w600)),
        Text("Order active until the date: $_formattedDate"),
      ],
      onPressed: () => _showDatePicker(context),
      buttonText: translations.common.changeAsTextLiteral,
    );
  }

  void _showDatePicker(BuildContext context) async {
    final style = TextStyle(
      color: colors.foreground,
      fontWeight: FontWeight.w600,
      fontSize: 18,
    );

    final earliestOrderStopDay = DateTime.now().add(const Duration(days: 1));
    final lastOrderStopDay = DateTime.now().add(const Duration(days: 365));

    DateTime? confirmedDate = await DatePicker.showDatePicker(
      context,
      showTitleActions: true,
      minTime: earliestOrderStopDay,
      maxTime: lastOrderStopDay,
      theme: DatePickerTheme(
        headerColor: colors.navigationBackground,
        backgroundColor: colors.softBackground,
        itemStyle: style,
        doneStyle: style,
        cancelStyle: style,
      ),
      currentTime: DateTime.now(),
      locale: LocaleType.en,
    );

    if (confirmedDate == null) {
      return;
    }

    onGoodUntilDateChanged.call(confirmedDate);
  }
}

class _EditStopLimitPrice extends StatelessWidget with STWidget {
  _EditStopLimitPrice({
    Key? key,
    required this.name,
    required this.orderType,
    required this.price,
    required this.onPressed,
  }) : super(key: key);
  final void Function() onPressed;
  final String name;
  final OrderType orderType;
  final double price;

  @override
  Widget build(BuildContext context) {
    return _GeneralButtonWithDecoration(
      children: [
        Text(translations.createOrderPage.stopLimitText(orderType), style: const TextStyle(fontSize: 17, fontWeight: FontWeight.w600)),
        const SizedBox(height: 2.5),
        Text("1 $name = ${price.toDefaultPrice()}", style: const TextStyle(fontWeight: FontWeight.w400)),
      ],
      onPressed: onPressed,
      buttonText: translations.common.changeAsTextLiteral,
    );
  }
}

class _GeneralButtonWithDecoration extends StatelessWidget with STWidget {
  _GeneralButtonWithDecoration({
    Key? key,
    required this.children,
    required this.buttonText,
    required this.onPressed,
  }) : super(key: key);
  final void Function() onPressed;
  final String buttonText;
  final List<Widget> children;

  @override
  Widget build(BuildContext context) {
    return ClipRRect(
      borderRadius: BorderRadius.circular(10),
      child: Container(
        color: colors.softBackground,
        child: Padding(
          padding: const EdgeInsets.all(15),
          child: Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                ...children
              ]),
              Util.roundedButton(
                [
                  Padding(
                    padding: const EdgeInsets.symmetric(horizontal: 10),
                    child: Text(buttonText),
                  )
                ],
                height: 40,
                colors: colors,
                onPressed: onPressed,
                borderRadius: BorderRadius.circular(10),
              )
            ],
          ),
        ),
      ),
    );
  }
}
