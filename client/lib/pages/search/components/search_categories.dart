import 'package:flutter/material.dart';
import 'package:solidtrade/components/base/st_widget.dart';
import 'package:solidtrade/services/util/extensions/build_context_extensions.dart';

class _CategoryItem {
  final String name;
  final String basePath;
  String get assetPath => "$basePath/$name.png";

  _CategoryItem(this.name, {this.basePath = "assets/images/categories"});
}

var _matrixOfCategories = [
  [
    _CategoryItem("Technology"),
    // _CategoryItem("Electronics"),
    _CategoryItem("Computer & Network"),
    _CategoryItem("Crypto"),
  ],
  [
    _CategoryItem("Health Care"),
    _CategoryItem("Cosmetics & Pharmaceuticals"),
    _CategoryItem("Water & Energy"),
    _CategoryItem("Biotech & Nanotech"),
  ],
  [
    _CategoryItem("Hotel & Tourism"),
    _CategoryItem("Food"),
    _CategoryItem("Retail & Stores"),
  ],
  [
    _CategoryItem("Baking & Finance"),
    _CategoryItem("Insurance"),
    _CategoryItem("Oil & Gas"),
    _CategoryItem("Unique"),
  ],
];

class SearchCategories extends StatelessWidget {
  const SearchCategories({Key? key, required this.onPressCategory}) : super(key: key);
  final void Function(String category) onPressCategory;

  Iterable<_CategoryItemWidget> get _categories => _matrixOfCategories.expand(
        (categories) => categories.map(
          (category) => _CategoryItemWidget(
            key: Key(category.name),
            categoryItem: category,
            onPressCategory: onPressCategory,
          ),
        ),
      );

  @override
  Widget build(BuildContext context) {
    return SingleChildScrollView(
      scrollDirection: Axis.horizontal,
      child: SizedBox(
        width: context.screenWidth * 2,
        child: Wrap(
          spacing: 8,
          runSpacing: 10,
          children: [
            ..._categories
          ],
        ),
      ),
    );
  }
}

class _CategoryItemWidget extends StatelessWidget with STWidget {
  _CategoryItemWidget({Key? key, required this.categoryItem, required this.onPressCategory}) : super(key: key);
  final void Function(String category) onPressCategory;
  final _CategoryItem categoryItem;

  @override
  Widget build(BuildContext context) {
    return GestureDetector(
      onTap: () => onPressCategory.call(categoryItem.name),
      child: Container(
        decoration: BoxDecoration(
          color: colors.softBackground,
          border: Border.all(color: colors.lessSoftForeground, width: 1.75),
          borderRadius: BorderRadius.circular(25),
        ),
        child: Container(
          margin: const EdgeInsets.only(bottom: 1, top: 1, left: 5, right: 0),
          child: Row(
            mainAxisSize: MainAxisSize.min,
            children: [
              Container(
                width: 30,
                height: 30,
                margin: const EdgeInsets.only(left: 2.5),
                child: Image.asset(
                  categoryItem.assetPath,
                ),
              ),
              Container(
                margin: const EdgeInsets.symmetric(horizontal: 10),
                child: Text(categoryItem.name),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
