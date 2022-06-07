import 'dart:convert';

import 'package:get_it/get_it.dart';
import 'package:http/http.dart' as http;
import 'package:mime/mime.dart';
import 'package:solidtrade/config/config_reader.dart';
import 'package:solidtrade/data/models/common/constants.dart';
import 'package:solidtrade/data/models/request_response/request_response.dart';
import 'package:solidtrade/services/stream/user_service.dart';
import 'package:solidtrade/services/util/debug/log.dart';

import 'package:http_parser/http_parser.dart' as parser;

abstract class IBaseRequestService {
  final UserService _userService = GetIt.instance.get<UserService>();
  static final String _baseUrl = ConfigReader.getBaseUrl();

  Future<RequestResponse<http.Response>> makeRequest(
    HttpMethod method,
    String endpoint, {
    Map<String, String> headers = const {
      "Content-Type": "application/json"
    },
    Object? body,
    Map<String, String>? queryParameters,
    bool selfHandleErrorCode = true,
  }) {
    return _makeRequest(
      method,
      endpoint,
      headers: headers,
      body: body,
      queryParameters: queryParameters,
      selfHandleErrorCode: selfHandleErrorCode,
    ).catchError((error) => _handleRequestError(error));
  }

  Future<RequestResponse<http.Response>> _makeRequest(
    HttpMethod method,
    String endpoint, {
    Map<String, String> headers = const {
      "Content-Type": "application/json"
    },
    Object? body,
    Map<String, String>? queryParameters,
    bool selfHandleErrorCode = true,
  }) async {
    final uri = Uri.https(_baseUrl, endpoint, queryParameters);

    Log.d(uri);

    var auth = await _userService.getUserAuthenticationHeader();

    if (!auth.isSuccessful && auth.result == null) {
      return RequestResponse.inheritErrorResponse(auth);
    }

    var deviceToken = await _userService.getUserDeviceHeader();

    if (!deviceToken.isSuccessful && deviceToken.result == null) {
      return RequestResponse.inheritErrorResponse(deviceToken);
    }

    http.Response response;
    Map<String, String> requestHeaders = {
      ...?deviceToken.result,
      ...?auth.result,
      ...headers,
    };

    switch (method) {
      case HttpMethod.get:
        response = await http.get(uri, headers: requestHeaders);
        break;
      case HttpMethod.post:
        response = await http.post(uri, headers: requestHeaders, body: body);
        break;
      case HttpMethod.patch:
        response = await http.patch(uri, headers: requestHeaders, body: body);
        break;
      case HttpMethod.delete:
        response = await http.delete(uri, headers: requestHeaders, body: body);
        break;
    }

    Log.d("Response status code: ${response.statusCode}");
    Log.d("Response body: ${response.body}");

    var responseBody = jsonDecode(response.body);

    if (selfHandleErrorCode && response.statusCode != 200) {
      if (response.statusCode == 400) {
        return RequestResponse.failedDueValidationError(responseBody);
      } else if (response.statusCode == 502) {
        RequestResponse.failedWithUserFriendlyMessage("The servers are currently offline. Please try again later.");
      } else {
        return RequestResponse.failed(responseBody);
      }
    }

    return RequestResponse.successful(response);
  }

  Future<RequestResponse<http.Response>> makeRequestWithMultipartFile(
    HttpMethod method,
    String endpoint, {
    Map<String, String> headers = const {
      "Content-Type": "application/json"
    },
    Map<String, String> fields = const {},
    Map<String, List<int>> files = const {},
    Map<String, String>? queryParameters,
    bool selfHandleErrorCode = true,
  }) {
    return _makeRequestWithMultipartFile(
      method,
      endpoint,
      headers: headers,
      fields: fields,
      files: files,
      queryParameters: queryParameters,
      selfHandleErrorCode: selfHandleErrorCode,
    ).catchError((error) => _handleRequestError(error));
  }

  Future<RequestResponse<http.Response>> _makeRequestWithMultipartFile(
    HttpMethod method,
    String endpoint, {
    Map<String, String> headers = const {
      "Content-Type": "application/json"
    },
    Map<String, String> fields = const {},
    Map<String, List<int>> files = const {},
    Map<String, String>? queryParameters,
    bool selfHandleErrorCode = true,
  }) async {
    final uri = Uri.https(_baseUrl, endpoint, queryParameters);

    Log.d(uri);

    var auth = await _userService.getUserAuthenticationHeader();

    if (!auth.isSuccessful && auth.result == null) {
      return RequestResponse.inheritErrorResponse(auth);
    }

    var deviceToken = await _userService.getUserDeviceHeader();

    if (!deviceToken.isSuccessful && deviceToken.result == null) {
      return RequestResponse.inheritErrorResponse(deviceToken);
    }

    http.Response response;
    Map<String, String> requestHeaders = {
      ...?deviceToken.result,
      ...?auth.result,
      ...headers,
    };

    var request = http.MultipartRequest(method.name, uri);
    request.fields.addAll(fields);

    for (var file in files.entries) {
      final mime = lookupMimeType('', headerBytes: file.value);
      final extension = mime!.split("/")[1];
      request.files.add(http.MultipartFile.fromBytes(
        file.key,
        file.value,
        filename: "file.$extension",
        contentType: parser.MediaType("image", extension[1]),
      ));
    }

    request.headers.addAll(requestHeaders);

    var responseStream = await request.send();

    response = await http.Response.fromStream(responseStream);

    Log.d("Response status code: ${response.statusCode}");
    Log.d("Response body: ${response.body}");

    var responseBody = jsonDecode(response.body);

    if (selfHandleErrorCode && response.statusCode != 200) {
      if (response.statusCode == 400) {
        try {
          return RequestResponse.failedDueValidationError(responseBody);
        } catch (_) {
          return RequestResponse.failedDueValidationError(responseBody);
        }
      } else if (response.statusCode == 502) {
        RequestResponse.failedWithUserFriendlyMessage("The servers are currently offline. Please try again later.");
      } else {
        return RequestResponse.failed(jsonDecode(response.body));
      }
    }

    return RequestResponse.successful(response);
  }

  Future<RequestResponse<http.Response>> _handleRequestError(dynamic error) {
    Log.f(error);
    return Future(() => RequestResponse<http.Response>.failedWithUserFriendlyMessage(Constants.genericErrorMessage));
  }

  RequestResponse<T> handleRequestResponse<T>(RequestResponse<http.Response> requestResponse, T Function(dynamic) createResponseObject) {
    if (!requestResponse.isSuccessful) {
      return RequestResponse.inheritErrorResponse(requestResponse);
    }

    var response = requestResponse.result!;
    var data = jsonDecode(response.body);
    return RequestResponse.successful(createResponseObject.call(data));
  }
}

enum HttpMethod {
  get,
  post,
  patch,
  delete,
}
