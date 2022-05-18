import 'dart:math';
import 'dart:typed_data';

import 'package:flutter/material.dart';
import 'package:solidtrade/components/base/st_widget.dart';
import 'package:solidtrade/services/util/util.dart';

class LoginScreen extends StatelessWidget with STWidget {
  LoginScreen({
    Key? key,
    required this.title,
    required this.subTitle,
    this.imageUrl,
    this.assetName,
    this.additionalWidgets,
    this.imageAsBytes,
  }) : super(key: key);

  final String title;
  final String subTitle;
  final String? imageUrl;
  final String? assetName;
  final Uint8List? imageAsBytes;
  final List<Widget>? additionalWidgets;

  List<Widget> getTitleContent(BoxConstraints constraints, BuildContext context) {
    return [
      Container(
        margin: const EdgeInsets.symmetric(horizontal: 40),
        child: Text(
          title,
          style: Theme.of(context).textTheme.headline4!.copyWith(fontWeight: FontWeight.w600, fontSize: 20),
          textAlign: TextAlign.center,
        ),
      ),
      const SizedBox(height: 10),
      Container(
        margin: const EdgeInsets.symmetric(horizontal: 20),
        child: Text(
          subTitle,
          textAlign: TextAlign.center,
          style: Theme.of(context).textTheme.bodyText2!.copyWith(fontSize: 16),
        ),
      )
    ];
  }

  double calculateImageSize(BoxConstraints constraints) => max(constraints.maxHeight, constraints.maxWidth) * 0.5;

  @override
  Widget build(BuildContext context) {
    return LayoutBuilder(
      builder: (BuildContext context, BoxConstraints constraints) => SingleChildScrollView(
        child: ConstrainedBox(
          constraints: BoxConstraints(
            maxHeight: MediaQuery.of(context).size.height * .88,
          ),
          child: Column(
            children: [
              imageAsBytes != null
                  ? Util.loadImageFromMemory(
                      imageAsBytes!,
                      calculateImageSize(constraints),
                      borderRadius: BorderRadius.circular(25),
                      boxFit: BoxFit.cover,
                      loadingBoxShape: BoxShape.rectangle,
                    )
                  : imageUrl != null
                      ? Util.loadImage(
                          imageUrl!,
                          calculateImageSize(constraints),
                          borderRadius: BorderRadius.circular(25),
                          boxFit: BoxFit.cover,
                          loadingBoxShape: BoxShape.rectangle,
                        )
                      : Util.loadImageFromAssets(
                          assetName!,
                          calculateImageSize(constraints),
                          borderRadius: BorderRadius.circular(25),
                          boxFit: BoxFit.cover,
                          loadingBoxShape: BoxShape.rectangle,
                        ),
              const SizedBox(height: 20),
              ...getTitleContent(constraints, context),
              const Spacer(),
              ...[
                ...additionalWidgets ??
                    [
                      const SizedBox.shrink()
                    ],
                const SizedBox(height: 15),
              ]
            ],
          ),
        ),
      ),
    );
  }
}
