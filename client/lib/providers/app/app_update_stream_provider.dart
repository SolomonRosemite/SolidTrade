import 'package:rxdart/streams.dart';
import 'package:rxdart/subjects.dart';

class UIUpdateStreamProvider {
  final BehaviorSubject<int> _behaviorSubject = BehaviorSubject.seeded(0);
  ValueStream<int> get stream$ => _behaviorSubject.stream;

  bool get currentHasValue => _behaviorSubject.hasValue;
  int get current => _behaviorSubject.value;

  void invokeUpdate() {
    _behaviorSubject.add(current + 1);
  }
}
