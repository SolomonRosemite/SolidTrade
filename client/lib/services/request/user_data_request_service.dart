import 'dart:async';
import 'dart:typed_data';

import 'package:solidtrade/data/dtos/user/request/update_user_dto.dart';
import 'package:solidtrade/data/dtos/user/response/delete_user_response.dart';
import 'package:solidtrade/data/entities/user.dart';
import 'package:solidtrade/data/models/common/constants.dart';
import 'package:solidtrade/data/models/request_response/request_response.dart';
import 'package:solidtrade/services/request/base/base_request_service.dart';

class UserDataRequestService extends IBaseRequestService {
  Future<RequestResponse<User>> createUser(
    String displayName,
    String username,
    int initialBalance,
    String email, {
    String? profilePictureSeed,
    Uint8List? profilePictureFile,
  }) async {
    Map<String, String> body = {
      "DisplayName": displayName,
      "Username": username,
      "Email": email,
      "ProfilePictureSeed": profilePictureSeed.toString(),
      "InitialBalance": initialBalance.toString(),
    };

    Map<String, List<int>> files = profilePictureFile != null
        ? {
            "ProfilePictureFile": profilePictureFile
          }
        : {};

    return await makeRequestWithMultipartFile<User>(
      HttpMethod.post,
      Constants.endpointUser,
      fields: body,
      files: files,
    ).create((data) => User.fromJson(data));
  }

  Future<RequestResponse<User>> updateUser(UpdateUserDto dto, User currentUser) async {
    var body = dto.toMapWithOnlyChangedProperties(currentUser);

    Map<String, List<int>> files = dto.profilePictureFile != null
        ? {
            "ProfilePictureFile": dto.profilePictureFile!
          }
        : {};

    return await makeRequestWithMultipartFile<User>(
      HttpMethod.patch,
      Constants.endpointUser,
      fields: body,
      files: files,
    ).create((data) => User.fromJson(data));
  }

  Future<RequestResponse<User>> fetchUserByUid(String uid) async {
    return await makeRequest<User>(HttpMethod.get, Constants.endpointUser, queryParameters: {
      "Uid": uid,
    }).create((data) => User.fromJson(data));
  }

  Future<RequestResponse<DeleteUserResponse>> deleteUser() async {
    return await makeRequest<DeleteUserResponse>(
      HttpMethod.delete,
      Constants.endpointUser,
    ).create((data) => DeleteUserResponse.fromJson(data));
  }
}
