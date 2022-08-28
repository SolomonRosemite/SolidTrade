abstract class IBaseEntity {
  final int id;
  final DateTime createdAt;
  final DateTime updatedAt;

  IBaseEntity({required this.id, required this.createdAt, required this.updatedAt});
}
