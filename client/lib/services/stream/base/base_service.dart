import 'package:rxdart/streams.dart';
import 'package:rxdart/subjects.dart';

abstract class IService<T> {
  final BehaviorSubject<T> behaviorSubject;

  IService(this.behaviorSubject);

  ValueStream<T> get stream$ => behaviorSubject.stream;

  bool get currentHasValue => behaviorSubject.hasValue;
  T get current => behaviorSubject.value;
}
