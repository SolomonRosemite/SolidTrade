import 'dart:async';

import 'package:flutter/material.dart';
import 'package:solidtrade/components/base/st_widget.dart';
import 'package:solidtrade/components/common/prevent_render_flex_overflow_wrapper.dart';
import 'package:solidtrade/pages/search/components/learn_the_basics.dart';
import 'package:solidtrade/pages/search/components/search_categories.dart';
import 'package:solidtrade/pages/search/components/search_input_field.dart';
import 'package:solidtrade/pages/search/search_view.dart';
import 'package:solidtrade/services/util/extensions/build_context_extensions.dart';
import 'package:solidtrade/services/util/util.dart';

class SearchPage extends StatefulWidget {
  const SearchPage({Key? key}) : super(key: key);

  @override
  State<SearchPage> createState() => _SearchPageState();
}

class _SearchPageState extends State<SearchPage> with STWidget {
  final String _inputFieldHeroTag = "HeroTag:SearchInputField";
  Text _constructTitle(String title) => Text(
        title,
        style: const TextStyle(
          fontWeight: FontWeight.bold,
          fontSize: 20,
        ),
      );

  @override
  Widget build(BuildContext context) {
    return StreamBuilder(
      stream: uiUpdate.stream$,
      builder: (context, _) => Container(
        margin: EdgeInsets.symmetric(horizontal: context.screenWidth * 0.05),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            const SizedBox(height: 25),
            Text(
              "Browse",
              style: TextStyle(
                fontSize: 30,
                fontWeight: FontWeight.bold,
                color: colors.foreground,
              ),
            ),
            Hero(
              tag: _inputFieldHeroTag,
              child: Material(
                color: Colors.transparent,
                child: Container(
                  alignment: Alignment.center,
                  margin: const EdgeInsets.symmetric(vertical: 15),
                  decoration: BoxDecoration(
                    color: colors.softBackground,
                  ),
                  child: LayoutBuilder(
                    builder: (context, constraints) => SearchInputField(
                      enableField: false,
                      onGestureTap: _onSearchFieldTap,
                    ),
                  ),
                ),
              ),
            ),
            Expanded(
              child: PreventColumnRenderFlexOverflowWrapper(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    _constructTitle("Popular categories"),
                    const SizedBox(height: 10),
                    SearchCategories(onPressCategory: _handlePressCategory),
                    const SizedBox(height: 20),
                    _constructTitle("Learn the basics"),
                    const SizedBox(height: 10),
                    const LearnTheBasics(),
                  ],
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }

  bool _hasNavigatedToSearch = false;

  void _handlePressCategory(String category) {
    // TODO: Implement...
    print(category);
  }

  void _onSearchFieldTap() {
    if (!_hasNavigatedToSearch) {
      Util.pushToRoute(
        context,
        SearchView(
          inputFieldHeroTag: _inputFieldHeroTag,
          child: SearchInputField(
            autofocus: true,
            customLeadingWidget: SizedBox(
              width: 20,
              height: 20,
              child: IconButton(
                onPressed: () => Navigator.pop(context),
                icon: Icon(
                  Icons.arrow_back,
                  size: 20,
                  color: colors.foreground,
                ),
              ),
            ),
          ),
        ),
      ).then((_) {
        Future.delayed(const Duration(milliseconds: 100)).then((_) => FocusScope.of(context).unfocus());
        Future.delayed(const Duration(milliseconds: 200)).then((_) => _hasNavigatedToSearch = false);
      });
    }
  }
}
