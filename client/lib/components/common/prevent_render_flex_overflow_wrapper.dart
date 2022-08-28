import 'package:flutter/material.dart';

class PreventColumnRenderFlexOverflowWrapper extends StatelessWidget {
  const PreventColumnRenderFlexOverflowWrapper({Key? key, required this.child}) : super(key: key);
  final Column child;

  @override
  Widget build(BuildContext context) {
    return CustomScrollView(
      controller: ScrollController(),
      slivers: [
        SliverFillRemaining(
          hasScrollBody: false,
          child: child,
        )
      ],
    );
  }
}
