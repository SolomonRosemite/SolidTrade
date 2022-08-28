import 'package:rxdart/subjects.dart';
import 'package:solidtrade/data/models/enums/client_enums/chart_date_range_view.dart';
import 'package:solidtrade/services/stream/base/base_service.dart';

class ChartDateRangeService extends IService<ChartDateRangeView> {
  // TODO: Maybe make this a setting that the user can change the default ChartDateRangeView.
  ChartDateRangeService() : super(BehaviorSubject.seeded(ChartDateRangeView.oneWeek));

  void changeChartDateRange(ChartDateRangeView view) async {
    if (view != current) {
      behaviorSubject.add(view);
    }
  }
}
