enum PositionType {
  warrant,
  knockout,
  stock,
}

extension PositionTypeExtension on PositionType {
  String get trName {
    switch (this) {
      case PositionType.warrant:
        return "vanillaWarrant";
      case PositionType.knockout:
        return "knockOutProduct";
      case PositionType.stock:
        return "stock";
    }
  }
}
