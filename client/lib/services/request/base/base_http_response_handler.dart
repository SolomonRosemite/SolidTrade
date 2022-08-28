import 'dart:convert';

import 'package:http/http.dart' as http;
import 'package:solidtrade/data/models/request_response/request_response.dart';

class HttpResponseHandler<T> {
  Future<RequestResponse<http.Response>> requestResponseFuture;

  HttpResponseHandler(this.requestResponseFuture);

  Future<RequestResponse<T>> create(
    T Function(Map<String, dynamic> data) createExpression, {
    RequestResponse<T> Function(http.Response response)? onError,
  }) async {
    var requestResponse = await requestResponseFuture;

    if (!requestResponse.isSuccessful) {
      return RequestResponse.inheritErrorResponse(requestResponse);
    }

    http.Response response = requestResponse.result!;
    var decodedJson = jsonDecode(response.body);

    if (response.statusCode == 200) {
      return RequestResponse.successful(createExpression.call(decodedJson));
    }

    return onError?.call(response) ?? RequestResponse.failed(decodedJson);
  }

  Future<RequestResponse<T>> createCustom(
    T Function(http.Response data) createExpression, {
    RequestResponse<T> Function(http.Response response)? onError,
  }) async {
    var requestResponse = await requestResponseFuture;

    if (!requestResponse.isSuccessful) {
      return RequestResponse.inheritErrorResponse(requestResponse);
    }

    http.Response response = requestResponse.result!;

    if (response.statusCode == 200) {
      return RequestResponse.successful(createExpression.call(response));
    }

    return onError?.call(response) ?? RequestResponse.failed(jsonDecode(response.body));
  }
}
