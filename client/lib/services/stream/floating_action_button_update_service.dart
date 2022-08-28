import 'package:rxdart/subjects.dart';
import 'package:solidtrade/services/stream/base/base_service.dart';

class FloatingActionButtonUpdateService extends IService<bool> {
  FloatingActionButtonUpdateService() : super(BehaviorSubject.seeded(false));

  void onClickFloatingActionButtonOrScrollUpFarEnough() async {
    behaviorSubject.add(false);
  }

  void onScrollDownFarEnough() async {
    behaviorSubject.add(true);
  }
}
