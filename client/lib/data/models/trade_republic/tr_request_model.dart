class TrRequestModel {
  final int id;
  final void Function(String) onResponseCallback;

  TrRequestModel({required this.id, required this.onResponseCallback});
}
