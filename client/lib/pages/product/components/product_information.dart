import 'package:flutter/material.dart';
import 'package:solidtrade/components/base/st_stream_builder.dart';
import 'package:solidtrade/data/models/trade_republic/tr_stock_details.dart';

class ProductInformation extends StatelessWidget {
  const ProductInformation({Key? key, required this.trStockDetailsStream}) : super(key: key);
  final Stream<TrStockDetails?> trStockDetailsStream;

  @override
  Widget build(BuildContext context) {
    return STStreamBuilder<TrStockDetails>(
      stream: trStockDetailsStream,
      builder: (context, details) {
        return Container(
          alignment: Alignment.centerLeft,
          margin: const EdgeInsets.only(bottom: 2.5),
          child: Text(details.company.description ?? "No Information about this stock yet. 😔", textAlign: TextAlign.start),
        );
      },
    );
  }
}
