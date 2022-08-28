import 'package:flutter/material.dart';
import 'package:solidtrade/components/base/st_widget.dart';
import 'package:solidtrade/services/util/extensions/build_context_extensions.dart';

class STPage extends StatelessWidget with STWidget {
  STPage({Key? key, required this.page}) : super(key: key);
  final Widget Function() page;

  @override
  Widget build(BuildContext context) => StreamBuilder(
        stream: uiUpdate.stream$,
        builder: (context, snapshot) => Container(
          color: colors.navigationBackground,
          child: SafeArea(
            child: Container(
              margin: context.adjustedWidthMargin,
              child: page.call(),
            ),
          ),
        ),
      );
}
