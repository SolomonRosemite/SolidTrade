import 'package:flutter/material.dart';

class PortfolioOverviewTitle extends StatelessWidget {
  const PortfolioOverviewTitle({Key? key, required this.title, this.textStyle}) : super(key: key);
  final String title;
  final TextStyle? textStyle;

  @override
  Widget build(BuildContext context) {
    return Align(
      alignment: Alignment.centerLeft,
      child: Text(title, style: textStyle ?? Theme.of(context).textTheme.headline6),
    );
  }
}
