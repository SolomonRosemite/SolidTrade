import 'package:flutter/cupertino.dart';

const double _adjustedScreenWidthMagnitude = 0.15;

extension MediaQueryExtension on BuildContext {
  double get screenWidth => MediaQuery.of(this).size.width;
  double get screenHeight => MediaQuery.of(this).size.height;

  double get adjustedScreenWidth => MediaQuery.of(this).size.width * (1 - _adjustedScreenWidthMagnitude);

  EdgeInsets get adjustedWidthMargin => _shouldAdjustWidth ? EdgeInsets.symmetric(horizontal: screenWidth * _adjustedScreenWidthMagnitude) : const EdgeInsets.all(0);
  bool get _shouldAdjustWidth => MediaQuery.of(this).size.width * 0.70 > screenHeight;
}
