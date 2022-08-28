import 'package:solidtrade/data/entities/historicalposition.dart';
import 'package:solidtrade/data/entities/portfolio.dart';

import '../entities/base/base_entity.dart';

class User implements IBaseEntity {
  @override
  final int id;
  @override
  final DateTime createdAt;
  @override
  final DateTime updatedAt;

  final String bio;
  final String? email;
  final String displayName;
  final String profilePictureUrl;
  final bool hasPublicPortfolio;
  final String uid;
  final String username;

  final List<HistoricalPosition> historicalPositions;
  final Portfolio? portfolio;

  const User({
    required this.id,
    required this.createdAt,
    required this.updatedAt,
    required this.bio,
    required this.email,
    required this.displayName,
    required this.profilePictureUrl,
    required this.hasPublicPortfolio,
    required this.uid,
    required this.username,
    required this.historicalPositions,
    required this.portfolio,
  });

  factory User.fromJson(Map<String, dynamic> json) {
    return User(
      id: json["id"],
      createdAt: DateTime.parse(json["createdAt"]),
      updatedAt: DateTime.parse(json["updatedAt"]),
      bio: json["bio"],
      displayName: json["displayName"],
      email: json["email"],
      hasPublicPortfolio: json["hasPublicPortfolio"],
      profilePictureUrl: json["profilePictureUrl"],
      uid: json["uid"],
      username: json["username"],
      historicalPositions: (json["historicalPositions"] as List<dynamic>).map((e) => HistoricalPosition.fromJson(e)).toList(),
      portfolio: json["portfolio"] == null ? null : Portfolio.fromJson(json["portfolio"]),
    );
  }
}
