import 'package:flutter/cupertino.dart';
import 'package:solidtrade/services/util/util.dart';

class RequestResponseResult<T> {
  Widget? loadingOrErrorWidget;
  T? value;
  final bool isProcessing;

  RequestResponseResult({required this.isProcessing, this.value, this.loadingOrErrorWidget});

  factory RequestResponseResult.errorWidget(Widget widget) => RequestResponseResult(loadingOrErrorWidget: widget, isProcessing: true);
  factory RequestResponseResult.loading() => RequestResponseResult(loadingOrErrorWidget: showLoadingSkeleton(BoxShape.rectangle), isProcessing: true);
  factory RequestResponseResult.result(T value) => RequestResponseResult(value: value, isProcessing: false);
}
