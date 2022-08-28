enum ChartDateRangeView {
  oneDay,
  oneWeek,
  oneMonth,
  sixMonth,
  oneYear,
  fiveYear,
}

extension ChartDateRangeViewExtension on ChartDateRangeView {
  String get name {
    switch (this) {
      case ChartDateRangeView.oneDay:
        return "1d";
      case ChartDateRangeView.oneWeek:
        return "5d";
      case ChartDateRangeView.oneMonth:
        return "1m";
      case ChartDateRangeView.sixMonth:
        return "6m";
      case ChartDateRangeView.oneYear:
        return "1y";
      case ChartDateRangeView.fiveYear:
        return "5y";
    }
  }
}
