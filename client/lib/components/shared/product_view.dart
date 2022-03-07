import 'package:flutter/material.dart';
import 'package:get_it/get_it.dart';
import 'package:solidtrade/components/base/st_widget.dart';
import 'package:solidtrade/components/chart/chart.dart';
import 'package:solidtrade/components/shared/analysts_recommendations.dart';
import 'package:solidtrade/components/shared/derivatives_selection.dart';
import 'package:solidtrade/components/shared/product_app_bar.dart';
import 'package:solidtrade/components/shared/product_chart_date_range_selection.dart';
import 'package:solidtrade/components/shared/product_details.dart';
import 'package:solidtrade/components/shared/product_information.dart';
import 'package:solidtrade/components/shared/product_metrics.dart';
import 'package:solidtrade/data/common/error/request_response.dart';
import 'package:solidtrade/data/common/shared/position_type.dart';
import 'package:solidtrade/data/common/shared/st_stream_builder.dart';
import 'package:solidtrade/data/common/shared/tr/tr_product_info.dart';
import 'package:solidtrade/data/common/shared/tr/tr_product_price.dart';
import 'package:solidtrade/data/models/portfolio.dart';
import 'package:solidtrade/services/stream/chart_date_range_service.dart';
import 'package:solidtrade/services/stream/portfolio_service.dart';
import 'package:solidtrade/services/stream/tr_stock_details_service.dart';
import 'package:solidtrade/services/util/tr_util.dart';
import 'package:visibility_detector/visibility_detector.dart';

class ProductView extends StatefulWidget {
  const ProductView({
    Key? key,
    required this.trProductPriceStream,
    required this.productInfo,
    required this.positionType,
  }) : super(key: key);

  final Stream<RequestResponse<TrProductPrice>?> trProductPriceStream;
  final TrProductInfo productInfo;
  final PositionType positionType;

  @override
  State<ProductView> createState() => _ProductViewState();
}

class _ProductViewState extends State<ProductView> with STWidget {
  final TrStockDetailsService stockDetailsService = GetIt.instance.get<TrStockDetailsService>();

  final PortfolioService portfolioService = GetIt.instance.get<PortfolioService>();

  final chartDateRangeStream = ChartDateRangeService();

  bool showProductInAppbar = false;
  bool widgetWasDisposed = false;

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

  @override
  void dispose() {
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
    );

    return Scaffold(
      appBar: AppBar(
        backgroundColor: colors.background,
        foregroundColor: colors.foreground,
        elevation: 0,
        titleTextStyle: Theme.of(context).textTheme.bodyText2,
        leadingWidth: 30,
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
                        child: productAppBar,
                      ),
                      SizedBox(width: double.infinity, height: chartHeight, child: Chart(chartDateRangeStream: chartDateRangeStream)),
                      const SizedBox(height: 5),
                      Container(
                        margin: const EdgeInsets.symmetric(horizontal: 10),
                        height: 30,
                        child: ProductChartDateRangeSelection(
                          chartDateRangeStream: chartDateRangeStream,
                        ),
                      ),
                      const SizedBox(height: 15),
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
                    final bool ownsPosition = TrUtil.userOwnsPosition(portfolio, widget.productInfo.isin);
                    final buttonWidth = (ownsPosition ? constraints.maxWidth / 2 : constraints.maxWidth) - 20;

                    return Row(
                      mainAxisAlignment: MainAxisAlignment.spaceEvenly,
                      children: [
                        ownsPosition
                            ? Container(
                                width: buttonWidth,
                                margin: const EdgeInsets.all(5),
                                child: TextButton(
                                  onPressed: () {},
                                  child: Text("Sell", style: TextStyle(color: Colors.white)),
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
                            onPressed: () {},
                            child: Text("Buy", style: TextStyle(color: Colors.white)),
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
    );
  }
}
