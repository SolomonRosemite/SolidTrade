import 'package:flutter/foundation.dart';
import 'package:flutter/material.dart';

import 'dart:async' show StreamSubscription;

import 'package:solidtrade/data/models/request_response/request_response_stream_result.dart';
import 'package:solidtrade/services/util/util.dart';

typedef AsyncWidgetBuilderRequestResponse<T> = Widget Function(BuildContext context, T value);

class STStreamBuilder<T> extends StreamBuilderBase<T, AsyncSnapshotRequestResponse<T>> {
  const STStreamBuilder({
    Key? key,
    required Stream<T?> stream,
    required this.builder,
  }) : super(key: key, stream: stream);

  final AsyncWidgetBuilderRequestResponse<T> builder;

  @override
  RequestResponseResult<AsyncSnapshotRequestResponse<T>> initial() => RequestResponseResult.result(AsyncSnapshotRequestResponse<T>.nothing());

  @override
  Widget build(BuildContext context, AsyncSnapshotRequestResponse<T> currentSummary) {
    return currentSummary.data == null ? showLoadingSkeleton(BoxShape.rectangle) : builder(context, currentSummary.data!);
  }

  @override
  RequestResponseResult<AsyncSnapshotRequestResponse<T>> afterData(RequestResponseResult<AsyncSnapshotRequestResponse<T>> current, T? data) {
    return RequestResponseResult.result(AsyncSnapshotRequestResponse.withData(ConnectionState.done, data));
  }
}

@immutable
class AsyncSnapshotRequestResponse<T> {
  const AsyncSnapshotRequestResponse._(this.connectionState, this.data, this.error, this.stackTrace)
      : assert(!(data != null && error != null)),
        assert(stackTrace == null || error != null);

  const AsyncSnapshotRequestResponse.nothing() : this._(ConnectionState.none, null, null, null);

  const AsyncSnapshotRequestResponse.waiting() : this._(ConnectionState.waiting, null, null, null);

  const AsyncSnapshotRequestResponse.withData(ConnectionState state, T? data) : this._(state, data, null, null);

  const AsyncSnapshotRequestResponse.withError(
    ConnectionState state,
    Object error, [
    StackTrace stackTrace = StackTrace.empty,
  ]) : this._(state, null, error, stackTrace);

  final ConnectionState connectionState;

  final T? data;

  T get requireData {
    if (hasData) return data!;
    if (hasError) throw error!;
    throw StateError('Snapshot has neither data nor error');
  }

  final Object? error;

  final StackTrace? stackTrace;

  AsyncSnapshotRequestResponse<T> inState(ConnectionState state) => AsyncSnapshotRequestResponse<T>._(state, data, error, stackTrace);

  bool get hasData => data != null;

  bool get hasError => error != null;

  @override
  String toString() => '${objectRuntimeType(this, 'AsyncSnapshotRequestResponse')}($connectionState, $data, $error, $stackTrace)';

  @override
  bool operator ==(Object other) {
    if (identical(this, other)) return true;
    return other is AsyncSnapshotRequestResponse<RequestResponseResult<T>> && other.connectionState == connectionState && other.data == data && other.error == error && other.stackTrace == stackTrace;
  }

  @override
  int get hashCode => hashValues(connectionState, data, error);
}

abstract class StreamBuilderBase<T, S> extends StatefulWidget {
  const StreamBuilderBase({Key? key, required this.stream}) : super(key: key);

  final Stream<T?> stream;

  RequestResponseResult<S> initial();

  RequestResponseResult<S> afterConnected(RequestResponseResult<S> current) => current;

  RequestResponseResult<S> afterData(RequestResponseResult<S> current, T? data);

  RequestResponseResult<S> afterError(RequestResponseResult<S> current, Object error, StackTrace stackTrace) => current;

  RequestResponseResult<S> afterDone(RequestResponseResult<S> current) => current;

  RequestResponseResult<S> afterDisconnected(RequestResponseResult<S> current) => current;

  Widget build(BuildContext context, S currentSummary);

  @override
  State<StreamBuilderBase<T, S>> createState() => _StreamBuilderBaseState<T, S>();
}

class _StreamBuilderBaseState<T, S> extends State<StreamBuilderBase<T, S>> {
  StreamSubscription<T?>? _subscription;
  late RequestResponseResult<S> _summary;

  @override
  void initState() {
    super.initState();
    _summary = widget.initial();
    _subscribe();
  }

  @override
  void didUpdateWidget(StreamBuilderBase<T, S> oldWidget) {
    super.didUpdateWidget(oldWidget);
    if (oldWidget.stream != widget.stream) {
      if (_subscription != null) {
        _unsubscribe();
        _summary = widget.afterDisconnected(_summary);
      }
      _subscribe();
    }
  }

  @override
  Widget build(BuildContext context) => _summary.isProcessing ? _summary.loadingOrErrorWidget! : widget.build(context, _summary.value!);

  @override
  void dispose() {
    _unsubscribe();
    super.dispose();
  }

  void _subscribe() {
    _subscription = widget.stream.listen((data) {
      setState(() {
        _summary = widget.afterData(_summary, data);
      });
    }, onError: (Object error, StackTrace stackTrace) {
      setState(() {
        _summary = widget.afterError(_summary, error, stackTrace);
      });
    }, onDone: () {
      setState(() {
        _summary = widget.afterDone(_summary);
      });
    });
    _summary = widget.afterConnected(_summary);
  }

  void _unsubscribe() {
    if (_subscription != null) {
      _subscription!.cancel();
      _subscription = null;
    }
  }
}
