import 'package:firebase_auth/firebase_auth.dart';
import 'package:google_sign_in/google_sign_in.dart';
import 'package:solidtrade/data/dtos/user/response/delete_user_response.dart';
import 'package:solidtrade/data/models/request_response/request_response.dart';
import 'package:solidtrade/services/stream/user_service.dart';

class UtilUserService {
  static Future<User?> signInWithGoogle({bool disconnectFirst = true}) async {
    if (disconnectFirst) {
      await _tryGoogleSigninDisconnect();
    }

    // Trigger the authentication flow
    final googleUser = await GoogleSignIn().signIn();

    if (googleUser == null) {
      return null;
    }

    // Obtain the auth details from the request
    final GoogleSignInAuthentication googleAuth = await googleUser.authentication;

    // Create a new credential
    final credential = GoogleAuthProvider.credential(
      accessToken: googleAuth.accessToken,
      idToken: googleAuth.idToken,
    );

    await FirebaseAuth.instance.signInWithCredential(credential);

    return FirebaseAuth.instance.currentUser;
  }

  static Future<void> signOut() async {
    await _tryGoogleSigninDisconnect();
    FirebaseAuth.instance.signOut();
    GoogleSignIn().signOut();
  }

  static Future<void> _tryGoogleSigninDisconnect() async {
    try {
      await GoogleSignIn().disconnect();
      // ignore: empty_catches
    } catch (e) {}
  }

  static Future<RequestResponse<DeleteUserResponse>> deleteAccount(UserService userService) async {
    var response = await userService.deleteUser();

    if (!response.isSuccessful) {
      return response;
    }

    await signOut();
    return response;
  }
}
