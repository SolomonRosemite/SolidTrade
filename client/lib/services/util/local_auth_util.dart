import 'package:flutter/foundation.dart';
import 'package:flutter/services.dart';
import 'package:local_auth/local_auth.dart';
import 'package:solidtrade/services/util/debug/log.dart';

class UtilLocalAuth {
  static final LocalAuthentication _auth = LocalAuthentication();

  static Future<bool> authenticate() async {
    if (kIsWeb || !await _auth.isDeviceSupported()) {
      return true;
    }

    try {
      return await _auth.authenticate(
        localizedReason: 'Scan your fingerprint (or face or whatever) to authenticate',
        options: const AuthenticationOptions(
          useErrorDialogs: true,
          stickyAuth: true,
        ),
      );
    } on PlatformException catch (e) {
      Log.w("Error occurred when trying to authenticate user. See following Exception for more info");
      Log.w(e);
      return false;
    }
  }
}
