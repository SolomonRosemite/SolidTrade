class DeleteUserResponse {
  final bool successful;

  DeleteUserResponse({required this.successful});

  factory DeleteUserResponse.fromJson(Map<String, dynamic> json) {
    return DeleteUserResponse(
      successful: json["successful"],
    );
  }
}
