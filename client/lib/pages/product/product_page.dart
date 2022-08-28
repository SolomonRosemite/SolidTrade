import 'dart:async';

import 'package:flutter/material.dart';
import 'package:get_it/get_it.dart';
import 'package:solidtrade/components/base/st_page.dart';
import 'package:solidtrade/components/base/st_widget.dart';
import 'package:solidtrade/components/base/st_stream_builder.dart';
import 'package:solidtrade/components/common/chart.dart';
import 'package:solidtrade/data/entities/portfolio.dart';
import 'package:solidtrade/data/models/enums/client_enums/chart_date_range_view.dart';
import 'package:solidtrade/data/models/enums/shared_enums/buy_or_sell.dart';
import 'package:solidtrade/data/models/enums/shared_enums/position_type.dart';
import 'package:solidtrade/data/models/trade_republic/tr_product_info.dart';
import 'package:solidtrade/data/models/trade_republic/tr_product_price.dart';
import 'package:solidtrade/pages/create_order/create_order_page.dart';
import 'package:solidtrade/pages/product/components/product_metrics.dart';
import 'package:solidtrade/pages/product/components/product_analysts_recommendations.dart';
import 'package:solidtrade/pages/product/components/product_app_bar.dart';
import 'package:solidtrade/pages/product/components/product_chart_date_range_selection.dart';
import 'package:solidtrade/pages/product/components/product_derivatives_selection.dart';
import 'package:solidtrade/pages/product/components/product_details.dart';
import 'package:solidtrade/pages/product/components/product_information.dart';
import 'package:solidtrade/pages/product/components/position_metrics.dart';
import 'package:solidtrade/services/stream/abstract/tr_continuous_product_prices_service.dart';
import 'package:solidtrade/services/stream/chart_date_range_service.dart';
import 'package:solidtrade/services/stream/portfolio_service.dart';
import 'package:solidtrade/services/stream/tr_stock_details_service.dart';
import 'package:solidtrade/services/util/tr_util.dart';
import 'package:solidtrade/services/util/util.dart';
import 'package:syncfusion_flutter_charts/charts.dart';
import 'package:visibility_detector/visibility_detector.dart';

class ProductPage extends StatefulWidget {
  const ProductPage({
    Key? key,
    required this.trProductPriceStream,
    required this.productInfo,
    required this.positionType,
    required this.productDetails,
  }) : super(key: key);

  final Stream<TrProductPrice?> trProductPriceStream;
  final TrUiProductDetails productDetails;
  final TrProductInfo productInfo;
  final PositionType positionType;

  @override
  State<ProductPage> createState() => _ProductPageState();
}

class _ProductPageState extends State<ProductPage> with STWidget {
  final TrStockDetailsService stockDetailsService = GetIt.instance.get<TrStockDetailsService>();
  final PortfolioService portfolioService = GetIt.instance.get<PortfolioService>();
  final chartDateRangeStream = ChartDateRangeService();
  final Completer<bool> _trContinuousPricesServiceIsInitialized = Completer();

  late ChartAxis primaryXChartAxis;
  late StreamSubscription<ChartDateRangeView> chartDateRangeStreamSubscription;
  late TrContinuousProductPricesService trContinuousPricesService;

  bool showProductInAppbar = false;
  bool widgetWasDisposed = false;

  @override
  void initState() {
    super.initState();
    trContinuousPricesService = TrContinuousProductPricesService(
      chartDateRangeStream.stream$,
      widget.trProductPriceStream,
      "${widget.productInfo.isin}.${widget.productInfo.exchangeIds.first}",
    );

    chartDateRangeStreamSubscription = chartDateRangeStream.stream$.listen(onChartRangeChange);
    onChartRangeChange(chartDateRangeStream.current);
    _trContinuousPricesServiceIsInitialized.complete(true);
  }

  void onChartRangeChange(ChartDateRangeView range) {
    setState(() {
      primaryXChartAxis = range == ChartDateRangeView.oneDay
          ? DateTimeAxis(
              majorGridLines: const MajorGridLines(width: 0),
              axisLine: const AxisLine(width: 0),
            )
          : DateTimeCategoryAxis(
              majorGridLines: const MajorGridLines(width: 0),
              axisLine: const AxisLine(width: 0),
            );
    });
  }

  List<Widget> section(
    BuildContext context,
    String title,
    Widget childWidget, {
    bool onlyShowIfProductIsStock = false,
    bool isStock = false,
    EdgeInsetsGeometry? margin,
    bool shouldShow = true,
  }) {
    if ((onlyShowIfProductIsStock && !isStock) || !shouldShow) {
      return [
        const SizedBox.shrink()
      ];
    }

    return [
      Container(
        margin: const EdgeInsets.only(left: 25, top: 5),
        child: Align(
          alignment: Alignment.centerLeft,
          child: Text(title, style: Theme.of(context).textTheme.headline6),
        ),
      ),
      Container(
        margin: margin ?? const EdgeInsets.only(left: 25, right: 25, top: 10),
        child: childWidget,
      ),
      divider(),
    ];
  }

  Widget divider() {
    return Container(margin: const EdgeInsets.symmetric(horizontal: 20), child: Divider(color: colors.softForeground, thickness: 3));
  }

  void _pushToCreateOrder(BuyOrSell buyOrSell) {
    Util.pushToRoute(
      context,
      CreateOrderPage(
        productInfo: widget.productInfo,
        buyOrSell: buyOrSell,
        trProductPriceStream: widget.trProductPriceStream,
        productDetails: widget.productDetails,
      ),
    );
  }

  void _onClickLeadingButton() => Navigator.pop(context);

  @override
  void dispose() {
    chartDateRangeStreamSubscription.cancel();
    trContinuousPricesService.onDispose();
    widgetWasDisposed = true;
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    // We only fetch details if the product is a stock and not crypto because anything else does not have kpis
    final isStock = widget.positionType == PositionType.stock && !widget.productInfo.isin.startsWith("XF");
    if (isStock) {
      stockDetailsService.requestTrProductInfo(widget.productInfo.isin);
    }

    final chartHeight = MediaQuery.of(context).size.height * .5;
    const double bottomBarHeight = 60;

    final productAppBar = ProductAppBar(
      positionType: widget.positionType,
      productInfo: widget.productInfo,
      trProductPriceStream: widget.trProductPriceStream,
      chartDateRangeViewStream: chartDateRangeStream.stream$,
    );

    return STPage(
      page: () => Scaffold(
        appBar: AppBar(
          backgroundColor: colors.background,
          foregroundColor: colors.foreground,
          elevation: 0,
          titleSpacing: 0,
          titleTextStyle: Theme.of(context).textTheme.bodyText2,
          leading: IconButton(
            onPressed: _onClickLeadingButton,
            icon: const Icon(Icons.arrow_back),
          ),
          leadingWidth: 40,
          title: LayoutBuilder(
            builder: (context, constraints) {
              return showProductInAppbar
                  ? SizedBox(
                      width: constraints.maxWidth,
                      child: productAppBar,
                    )
                  : const SizedBox.shrink();
            },
          ),
        ),
        body: LayoutBuilder(
          builder: (BuildContext context, BoxConstraints constraints) => SizedBox(
            height: constraints.maxHeight,
            child: Column(
              children: [
                SizedBox(
                  height: constraints.maxHeight - bottomBarHeight,
                  child: SingleChildScrollView(
                    child: Column(
                      children: [
                        VisibilityDetector(
                          key: const Key("VisibilityDetectorKey"),
                          onVisibilityChanged: (VisibilityInfo info) {
                            if (widgetWasDisposed) {
                              return;
                            }

                            if (info.visibleFraction == 0 && showProductInAppbar == false) {
                              setState(() {
                                showProductInAppbar = true;
                              });
                            } else if (showProductInAppbar) {
                              setState(() {
                                showProductInAppbar = false;
                              });
                            }
                          },
                          child: SizedBox(
                            width: constraints.maxWidth,
                            height: 50,
                            child: !showProductInAppbar ? productAppBar : const SizedBox.shrink(),
                          ),
                        ),
                        SizedBox(
                          width: double.infinity,
                          height: chartHeight,
                          child: FutureBuilder<bool>(
                            future: _trContinuousPricesServiceIsInitialized.future,
                            builder: (context, snap) {
                              if (!snap.hasData || !snap.data!) {
                                return showLoadingSkeleton(BoxShape.rectangle);
                              }
                              return Chart(
                                dateTimeXAxis: primaryXChartAxis,
                                primaryStreamData: trContinuousPricesService.primaryProductPricesStream$,
                                secondaryStreamData: trContinuousPricesService.secondaryStream$,
                              );
                            },
                          ),
                        ),
                        const SizedBox(height: 5),
                        Container(
                          margin: const EdgeInsets.symmetric(horizontal: 10),
                          height: 30,
                          child: ProductChartDateRangeSelection(
                            chartDateRangeStream: chartDateRangeStream,
                          ),
                        ),
                        const SizedBox(height: 15),
                        STStreamBuilder<Portfolio>(
                          stream: portfolioService.stream$,
                          builder: (context, portfolio) {
                            var position = TrUtil.getPositionOrDefault(portfolio, widget.productInfo.isin);
                            if (position == null) {
                              return const SizedBox.shrink();
                            }
                            return Column(
                              children: [
                                ...section(
                                  context,
                                  "📈 Position",
                                  PositionMetrics(
                                    trProductPriceStream: widget.trProductPriceStream,
                                    trStockDetailsStream: stockDetailsService.stream$,
                                    position: position,
                                  ),
                                  margin: const EdgeInsets.symmetric(horizontal: 15, vertical: 5),
                                ),
                              ],
                            );
                          },
                        ),
                        ...section(
                          context,
                          "📈 Statistics",
                          ProductMetrics(
                            trProductPriceStream: widget.trProductPriceStream,
                            trStockDetailsStream: stockDetailsService.stream$,
                            isStock: isStock,
                            productInfo: widget.productInfo,
                          ),
                          margin: const EdgeInsets.symmetric(horizontal: 15, vertical: 5),
                        ),
                        ...section(
                          context,
                          "🤔 What Analysts Say",
                          AnalystsRecommendations(
                            trStockDetailsStream: stockDetailsService.stream$,
                          ),
                          isStock: isStock,
                          onlyShowIfProductIsStock: true,
                        ),
                        ...section(
                          context,
                          "💎 Derivatives",
                          DerivativesSelection(
                            productInfo: widget.productInfo,
                          ),
                          isStock: isStock,
                          onlyShowIfProductIsStock: true,
                          margin: const EdgeInsets.symmetric(horizontal: 15),
                          shouldShow: widget.productInfo.derivativeProductCount.knockOutProduct != null || widget.productInfo.derivativeProductCount.vanillaWarrant != null,
                        ),
                        ...section(
                          context,
                          "ℹ️ About ${widget.productInfo.shortName}",
                          ProductInformation(
                            trStockDetailsStream: stockDetailsService.stream$,
                          ),
                          isStock: isStock,
                          onlyShowIfProductIsStock: true,
                        ),
                        ...section(
                          context,
                          "ℹ️ Details",
                          ProductDetails(
                            trStockDetailsStream: stockDetailsService.stream$,
                            productInfo: widget.productInfo,
                            isStock: isStock,
                          ),
                          // margin: const EdgeInsets.only(left: 25, right: 25, top: 10),
                          isStock: isStock,
                          onlyShowIfProductIsStock: false,
                        ),
                        const SizedBox(height: 5),
                        Text(
                          "In the event of disruptions, outdated data may occur. When transactions are made, it is ensured that these disturbances are taken into account.",
                          textAlign: TextAlign.center,
                          style: TextStyle(color: colors.lessSoftForeground, fontSize: 13),
                        ),
                        const SizedBox(height: 10),
                      ],
                    ),
                  ),
                ),
                Container(
                  height: bottomBarHeight,
                  width: constraints.maxWidth,
                  color: colors.navigationBackground,
                  child: STStreamBuilder<Portfolio>(
                    stream: portfolioService.stream$,
                    builder: (context, portfolio) {
                      final bool ownsPosition = TrUtil.getPositionOrDefault(portfolio, widget.productInfo.isin) != null;
                      final buttonWidth = (ownsPosition ? constraints.maxWidth / 2 : constraints.maxWidth) - 20;

                      return Row(
                        mainAxisAlignment: MainAxisAlignment.spaceEvenly,
                        children: [
                          ownsPosition
                              ? Container(
                                  width: buttonWidth,
                                  margin: const EdgeInsets.all(5),
                                  child: TextButton(
                                    onPressed: () => _pushToCreateOrder(BuyOrSell.sell),
                                    child: const Text("Sell", style: TextStyle(color: Colors.white)),
                                    style: ButtonStyle(
                                      backgroundColor: MaterialStateProperty.all(colors.stockRed),
                                      foregroundColor: MaterialStateProperty.all(colors.foreground),
                                    ),
                                  ),
                                )
                              : const SizedBox.shrink(),
                          Container(
                            width: buttonWidth,
                            margin: const EdgeInsets.all(5),
                            child: TextButton(
                              onPressed: () => _pushToCreateOrder(BuyOrSell.buy),
                              child: const Text("Buy", style: TextStyle(color: Colors.white)),
                              style: ButtonStyle(
                                backgroundColor: MaterialStateProperty.all(colors.stockGreen),
                                foregroundColor: MaterialStateProperty.all(colors.foreground),
                              ),
                            ),
                          ),
                        ],
                      );
                    },
                  ),
                )
              ],
            ),
          ),
        ),
      ),
    );
  }
}
