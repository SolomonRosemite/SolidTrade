import 'dart:async';

import 'package:flutter/material.dart';
import 'package:get_it/get_it.dart';
import 'package:solidtrade/components/base/st_widget.dart';
import 'package:solidtrade/components/base/st_stream_builder.dart';
import 'package:solidtrade/data/models/common/tuple.dart';
import 'package:solidtrade/data/models/enums/client_enums/chart_date_range_view.dart';
import 'package:solidtrade/data/models/enums/shared_enums/position_type.dart';
import 'package:solidtrade/data/models/trade_republic/tr_aggregate_history.dart';
import 'package:solidtrade/data/models/trade_republic/tr_product_info.dart';
import 'package:solidtrade/data/models/trade_republic/tr_product_price.dart';
import 'package:solidtrade/services/stream/aggregate_history_service.dart';
import 'package:solidtrade/services/util/extensions/double_extensions.dart';
import 'package:solidtrade/services/util/tr_util.dart';
import 'package:solidtrade/services/util/util.dart';

class ProductAppBar extends StatefulWidget {
  const ProductAppBar({
    Key? key,
    required this.trProductPriceStream,
    required this.productInfo,
    required this.positionType,
    required this.chartDateRangeViewStream,
  }) : super(key: key);

  final Stream<ChartDateRangeView> chartDateRangeViewStream;
  final Stream<TrProductPrice?> trProductPriceStream;
  final TrProductInfo productInfo;
  final PositionType positionType;

  @override
  State<ProductAppBar> createState() => _ProductAppBarState();
}

class _ProductAppBarState extends State<ProductAppBar> with STWidget, SingleTickerProviderStateMixin {
  final _aggregateHistoryService = GetIt.instance.get<AggregateHistoryService>();

  Completer<Tuple<TrAggregateHistory?, bool>> requestTrAggregateHistory(ChartDateRangeView range) {
    if (range == ChartDateRangeView.oneDay) {
      return Completer()..complete(Tuple(t1: null, t2: false));
    }

    Completer<Tuple<TrAggregateHistory?, bool>> completer = Completer();

    _aggregateHistoryService.getTrAggregateHistory(widget.productInfo.isinWithExchangeExtension, range).then((response) {
      if (!response.isSuccessful) {
        // TODO: Handle...
        return;
      }

      completer.complete(Tuple(t1: response.result!, t2: true));
    });

    return completer;
  }

  final TextStyle _subtitleTextStyle = const TextStyle(
    fontWeight: FontWeight.w500,
    fontSize: 12,
    color: Colors.grey,
  );

  late final AnimationController _controller = AnimationController(
    duration: const Duration(milliseconds: 900),
    vsync: this,
  )..forward();

  late final _offsetAnimation = Tween<Offset>(
    begin: const Offset(0, -1),
    end: Offset.zero,
  ).animate(
    CurvedAnimation(
      parent: _controller,
      curve: Curves.fastLinearToSlowEaseIn,
    ),
  );

  // TODO: Use "productInfo.positionType" instead?
  String _getProductTicker() {
    switch (widget.productInfo.typeId) {
      case "stock":
        return widget.productInfo.name;
      case "crypto":
        return widget.productInfo.homeSymbol!;
      case "fund":
      case "derivative":
      default:
        return widget.productInfo.issuerDisplayName!;
    }
  }

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return STStreamBuilder<ChartDateRangeView>(
        stream: widget.chartDateRangeViewStream,
        builder: (_, range) {
          var completer = requestTrAggregateHistory(range);
          return FutureBuilder<Tuple<TrAggregateHistory?, bool>>(
              future: completer.future,
              builder: (_, snapshot) {
                if (!snapshot.hasData) {
                  return const SizedBox.shrink();
                }

                return STStreamBuilder<TrProductPrice>(
                  stream: widget.trProductPriceStream,
                  builder: (context, priceInfo) {
                    TrUiProductDetails details = TrUtil.getTrUiProductDetails(
                      priceInfo,
                      widget.productInfo,
                      widget.positionType,
                      entires: snapshot.data!.t2 ? snapshot.data?.t1?.aggregates : null,
                    );

                    final color = details.isUp ? colors.stockGreen : colors.stockRed;

                    return LayoutBuilder(
                      builder: (context, constraints) {
                        return SlideTransition(
                          position: _offsetAnimation,
                          child: Row(
                            children: [
                              Util.loadImage(
                                details.imageUrl,
                                50,
                              ),
                              SizedBox(
                                width: constraints.maxWidth * 0.6,
                                child: Column(
                                  crossAxisAlignment: CrossAxisAlignment.start,
                                  mainAxisAlignment: MainAxisAlignment.center,
                                  children: [
                                    Text(widget.productInfo.shortName, style: Theme.of(context).textTheme.bodyText1, overflow: TextOverflow.ellipsis),
                                    Text(widget.productInfo.intlSymbol ?? _getProductTicker(), style: _subtitleTextStyle),
                                  ],
                                ),
                              ),
                              const Spacer(),
                              Column(
                                crossAxisAlignment: CrossAxisAlignment.end,
                                mainAxisAlignment: MainAxisAlignment.center,
                                children: [
                                  Text(priceInfo.bid.price.toDefaultPrice(maxFractionDigits: 2)),
                                  Row(
                                    children: [
                                      Icon(
                                        details.isUp ? Icons.arrow_drop_up : Icons.arrow_drop_down,
                                        color: color,
                                        size: 20,
                                      ),
                                      Text(
                                        details.plusMinusProductNamePrefix + ((details.percentageChange - 1) * 100).toStringAsFixed(2) + "%",
                                        style: TextStyle(
                                          color: color,
                                        ),
                                      ),
                                    ],
                                  )
                                ],
                              ),
                              const SizedBox(width: 10)
                            ],
                          ),
                        );
                      },
                    );
                  },
                );
              });
        });
  }
}
