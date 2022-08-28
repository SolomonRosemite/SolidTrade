import 'dart:async';

import 'package:rxdart/subjects.dart';
import 'package:solidtrade/data/models/trade_republic/tr_request_response.dart';
import 'package:solidtrade/services/request/data_request_service.dart';
import 'package:solidtrade/services/stream/base/base_service.dart';
import 'package:solidtrade/services/util/debug/log.dart';

abstract class ITransientService<T> extends IService<T?> {
  ITransientService(BehaviorSubject<T?> behaviorSubject) : super(behaviorSubject);

  bool hadInitialListener = false;

  void onEvent(TrRequestResponse<T>? event, StreamSubscription<TrRequestResponse<T>?>? subscription) {
    if (event == null) {
      return;
    }

    if (!behaviorSubject.hasListener && hadInitialListener) {
      subscription!.cancel();
      DataRequestService.trApiDataRequestService.unsub(event.id);
      behaviorSubject.close();
      return;
    }

    if (event.requestResponse.isSuccessful) {
      behaviorSubject.add(event.requestResponse.result);
    } else {
      Log.f("If you see this error, then this issue has to be looked in to and future errors must be handled.");
      Log.f("Received unsuccessful request response, which wont get handled. Error Message: ${event.requestResponse.error!.userFriendlyMessage}");
    }

    hadInitialListener = true;
  }
}
