import 'dart:async';

extension StreamExtension<T> on Stream<T> {
  Future<T> get currentValue async {
    var valueCompleter = Completer<T>();
    var subscriptionCompleter = Completer();
    var subscription = listen((value) {
      valueCompleter.complete(value);
      subscriptionCompleter.complete();
    });

    await subscriptionCompleter.future;
    subscription.cancel();

    return valueCompleter.future;
  }
}
