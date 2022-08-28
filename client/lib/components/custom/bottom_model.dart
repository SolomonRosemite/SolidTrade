import 'package:flutter/material.dart';
import 'package:solidtrade/components/base/st_widget.dart';

class BottomModel extends StatelessWidget with STWidget {
  BottomModel({
    Key? key,
    required this.title,
    required this.subtitle,
    required this.content,
    this.contentBackgroundColor,
  }) : super(key: key);
  final String title;
  final String subtitle;
  final Iterable<Widget> content;
  final Color? contentBackgroundColor;

  @override
  Widget build(BuildContext context) {
    return Container(
      margin: const EdgeInsets.symmetric(horizontal: 20),
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          Container(
            margin: const EdgeInsets.symmetric(vertical: 10),
            width: MediaQuery.of(context).size.width * 0.2,
            child: ClipRRect(
              borderRadius: BorderRadius.circular(25),
              child: Divider(
                color: colors.softForeground,
                thickness: 6,
              ),
            ),
          ),
          Container(
            margin: const EdgeInsets.only(
              bottom: 20,
            ),
            alignment: Alignment.centerLeft,
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(title, style: Theme.of(context).textTheme.headline5!.copyWith(fontWeight: FontWeight.w600)),
                Text(subtitle)
              ],
            ),
          ),
          ClipRRect(
            borderRadius: BorderRadius.circular(10),
            child: Container(
              color: contentBackgroundColor ?? colors.softBackground,
              child: Column(
                children: [
                  ...content
                ],
              ),
            ),
          ),
          const SizedBox(height: 20),
        ],
      ),
    );
  }
}
