import 'package:solidtrade/services/stream/base/base_service.dart';
import 'package:firebase_messaging/firebase_messaging.dart';
import 'package:rxdart/subjects.dart';
import 'package:solidtrade/services/util/debug/log.dart';

class MessagingService extends IService<Map<String, dynamic>?> {
  MessagingService() : super(BehaviorSubject.seeded(null)) {
    FirebaseMessaging.onMessage.listen(_onMessageReceived);
  }

  void _onMessageReceived(RemoteMessage message) {
    Log.d('Received new message:\n${message.data}');
    behaviorSubject.add(message.data);
  }
}
