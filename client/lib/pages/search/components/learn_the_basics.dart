import 'package:flutter/material.dart';
import 'package:solidtrade/components/common/st_logo.dart';
import 'package:solidtrade/providers/theme/app_theme.dart';

class LearnTheBasics extends StatelessWidget {
  const LearnTheBasics({Key? key}) : super(key: key);

  @override
  Widget build(BuildContext context) {
    return Column(
      children: const [
        _BlogItem(
          title: "Why Invest?",
          description: "Lorem ipsum dolor sit amet consectetur, alias, velit corporis eius eveniet consectetur aperiam! Deleniti architecto veritatis ad vel impedit.",
          image: "",
        ),
        SizedBox(height: 10),
        _BlogItem(
          title: "What is the stock market?",
          description: "Lorem ipsum dolor sit amet consectetur, alias, velit corporis eius eveniet consectetur aperiam! Deleniti architecto veritatis ad vel impedit.",
          image: "",
        ),
        SizedBox(height: 10),
        _BlogItem(
          title: "What are your goals?",
          description: "Lorem ipsum dolor sit amet consectetur, alias, velit corporis eius eveniet consectetur aperiam! Deleniti architecto veritatis ad vel impedit.",
          image: "",
        ),
      ],
    );
  }
}

class _BlogItem extends StatelessWidget {
  const _BlogItem({
    Key? key,
    required this.title,
    required this.description,
    required this.image,
  }) : super(key: key);
  final String title;
  final String description;
  final String image;

  @override
  Widget build(BuildContext context) {
    return Row(
      children: [
        ClipRRect(
          child: STLogo(DarkColorTheme().logoAsGif, key: UniqueKey()),
          borderRadius: BorderRadius.circular(10),
        ),
        const SizedBox(width: 12.5),
        Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              title,
              style: const TextStyle(fontWeight: FontWeight.bold),
            ),
            SizedBox(
              width: 240,
              child: Text(description),
            ),
          ],
        )
      ],
    );
  }
}
