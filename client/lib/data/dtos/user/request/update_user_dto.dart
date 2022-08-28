import 'dart:typed_data';

import 'package:solidtrade/data/entities/user.dart';

class UpdateUserDto {
  String bio;
  String displayName;
  String email;
  Uint8List? profilePictureFile;
  bool publicPortfolio;
  String username;

  UpdateUserDto({
    required this.bio,
    required this.displayName,
    required this.email,
    required this.publicPortfolio,
    required this.username,
    this.profilePictureFile,
  });

  factory UpdateUserDto.copyWith(UpdateUserDto dto) {
    return UpdateUserDto(
      bio: dto.bio,
      displayName: dto.displayName,
      email: dto.email,
      publicPortfolio: dto.publicPortfolio,
      username: dto.username,
      profilePictureFile: dto.profilePictureFile,
    );
  }

  Map<String, String> toMapWithOnlyChangedProperties(User user) {
    Map<String, String> map = {};

    void addIf(bool shouldAdd, MapEntry<String, String> content) {
      if (shouldAdd) {
        map.addEntries([
          content
        ]);
      }
    }

    addIf(user.bio != bio, MapEntry("Bio", bio));
    addIf(user.displayName != displayName, MapEntry("DisplayName", displayName));
    addIf(user.email?.toLowerCase() != email.toLowerCase(), MapEntry("Email", email));
    addIf(user.username.toLowerCase() != username.toLowerCase(), MapEntry("Username", username));
    addIf(user.hasPublicPortfolio != publicPortfolio, MapEntry("PublicPortfolio", publicPortfolio.toString()));

    return map;
  }

  bool hasUpdatedProfileByUser(User user) {
    if (bio != user.bio) return true;
    if (email != user.email) return true;
    if (publicPortfolio != user.hasPublicPortfolio) return true;
    if (displayName != user.displayName) return true;
    if (username != user.username) return true;
    if (profilePictureFile != null) return true;

    return false;
  }

  bool hasUpdatedProfileByDto(UpdateUserDto user) {
    if (bio != user.bio) return true;
    if (email != user.email) return true;
    if (publicPortfolio != user.publicPortfolio) return true;
    if (displayName != user.displayName) return true;
    if (username != user.username) return true;
    if (profilePictureFile != null) return true;

    return false;
  }
}
