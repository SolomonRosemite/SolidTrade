class ErrorModel {
  final String? userFriendlyMessage;

  const ErrorModel({
    required this.userFriendlyMessage,
  });

  factory ErrorModel.fromJson(Map<String, dynamic> json) {
    return ErrorModel(
      userFriendlyMessage: json['userFriendlyMessage'],
    );
  }
}
