import 'package:flutter/material.dart';
import 'package:solidtrade/components/base/st_widget.dart';
import 'package:solidtrade/data/models/enums/client_enums/chart_date_range_view.dart';
import 'package:solidtrade/services/stream/chart_date_range_service.dart';
import 'package:solidtrade/services/util/util.dart';

class ProductChartDateRangeSelection extends StatelessWidget with STWidget {
  ProductChartDateRangeSelection({Key? key, required this.chartDateRangeStream}) : super(key: key);
  final ChartDateRangeService chartDateRangeStream;

  void changeDateRange(ChartDateRangeView range) => chartDateRangeStream.changeChartDateRange(range);

  List<Widget> constructSelections(int currentIndex) {
    List<Widget> widgets = [];

    for (var index = 0; index < ChartDateRangeView.values.length; index++) {
      final isSelected = currentIndex == index;
      final color = isSelected ? colors.foreground : Colors.grey;

      widgets.add(SizedBox(
        width: 50,
        child: TextButton(
          style: ButtonStyle(
            foregroundColor: MaterialStateProperty.all<Color>(color),
            shape: MaterialStateProperty.all<RoundedRectangleBorder>(
              RoundedRectangleBorder(
                borderRadius: BorderRadius.circular(5.0),
                side: BorderSide(color: color),
              ),
            ),
          ),
          onPressed: () => changeDateRange(ChartDateRangeView.values[index]),
          child: Text(
            Util.chartDateRangeToTranslatedString(translations, ChartDateRangeView.values[index]),
          ),
        ),
      ));
    }

    return widgets;
  }

  @override
  Widget build(BuildContext context) {
    return StreamBuilder<ChartDateRangeView>(
      stream: chartDateRangeStream.stream$,
      builder: (context, snap) {
        if (!snap.hasData) {
          return const SizedBox.shrink();
        }

        return Row(
          mainAxisAlignment: MainAxisAlignment.spaceEvenly,
          children: [
            ...constructSelections(snap.data!.index),
          ],
        );
      },
    );
  }
}
