import 'dart:async';

import 'package:flutter/material.dart';
import 'package:get_it/get_it.dart';
import 'package:solidtrade/components/base/st_page.dart';
import 'package:solidtrade/components/base/st_widget.dart';
import 'package:solidtrade/data/models/trade_republic/tr_product_price.dart';
import 'package:solidtrade/data/models/trade_republic/tr_product_search.dart';
import 'package:solidtrade/pages/product/product_page.dart';
import 'package:solidtrade/pages/search/components/search_input_field.dart';
import 'package:solidtrade/services/stream/tr_product_price_service.dart';
import 'package:solidtrade/services/stream/tr_product_search_service.dart';
import 'package:solidtrade/services/util/extensions/build_context_extensions.dart';
import 'package:solidtrade/services/util/tr_util.dart';
import 'package:solidtrade/services/util/util.dart';

class SearchView extends StatefulWidget {
  const SearchView({
    Key? key,
    required this.child,
    required this.inputFieldHeroTag,
  }) : super(key: key);
  final String inputFieldHeroTag;
  final Widget child;

  @override
  State<SearchView> createState() => _SearchViewState();
}

class _SearchViewState extends State<SearchView> with STWidget {
  final TextEditingController _textEditingController = TextEditingController();

  @override
  Widget build(BuildContext context) {
    return STPage(
      page: () => Scaffold(
        body: SafeArea(
          child: Column(
            children: [
              Hero(
                tag: widget.inputFieldHeroTag,
                child: Container(
                  height: 70,
                  width: double.infinity,
                  color: colors.softBackground,
                  child: Material(
                    color: Colors.transparent,
                    child: SizedBox(
                      width: context.screenWidth * 0.5,
                      child: SearchInputField(
                        autofocus: true,
                        leftPadding: const SizedBox(width: 0),
                        textEditingController: _textEditingController,
                        customLeadingWidget: _CustomIconButton(
                          onPressed: () => Navigator.pop(context),
                          width: 50,
                          icon: Icon(
                            Icons.arrow_back,
                            size: 20,
                            color: colors.foreground,
                          ),
                        ),
                        customActionWidget: _CustomIconButton(
                          onPressed: _onClickClearInput,
                          width: 50,
                          icon: Icon(
                            Icons.close,
                            size: 20,
                            color: colors.foreground,
                          ),
                        ),
                      ),
                    ),
                  ),
                ),
              ),
              Expanded(
                child: _SearchResults(
                  textEditingController: _textEditingController,
                ),
              )
            ],
          ),
        ),
      ),
    );
  }

  void _onClickClearInput() {
    _textEditingController.clear();
  }
}

class _SearchResults extends StatefulWidget {
  const _SearchResults({Key? key, required this.textEditingController}) : super(key: key);
  final TextEditingController textEditingController;

  @override
  State<_SearchResults> createState() => _SearchResultsState();
}

class _SearchResultsState extends State<_SearchResults> with STWidget {
  final _trProductSearchService = GetIt.instance.get<TrProductSearchService>();
  final List<TrProductSearchResult> _productSearchResults = [];
  final _animatedListKey = GlobalKey<AnimatedListState>();
  int _totalNumberOfResults = 0;

  String _previousSearch = "_";

  @override
  void initState() {
    super.initState();
    widget.textEditingController.addListener(_onSearchChanged);
  }

  Widget _buildItem(TrProductSearchResult result, Animation<double> animation, {required bool isFirstItem}) {
    var tween = Tween(begin: 0.0, end: 1.0).chain(
      CurveTween(curve: Curves.ease),
    );

    return FadeTransition(
      opacity: animation.drive(tween),
      child: SingleSearchResult(result: result, isFirstItem: isFirstItem),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Column(
      children: [
        Container(
          height: 30,
          margin: const EdgeInsets.symmetric(horizontal: 10, vertical: 5),
          child: Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              Text(
                "$_totalNumberOfResults results",
                style: TextStyle(color: colors.lessSoftForeground),
              ),
              IconButton(onPressed: () {}, icon: Icon(Icons.auto_awesome_mosaic_rounded, color: colors.foreground)),
            ],
          ),
        ),
        Expanded(
          child: AnimatedList(
            key: _animatedListKey,
            initialItemCount: _productSearchResults.length,
            itemBuilder: (context, index, animation) => _buildItem(_productSearchResults[index], animation, isFirstItem: index == 0),
          ),
        ),
      ],
    );
  }

  Future<void> _onSearchChanged() async {
    var search = widget.textEditingController.value.text;
    if (search == _previousSearch) return;

    _previousSearch = search;

    var searchResult = await _fetchSearchResults(search);
    _updateResults(searchResult);
  }

  Future<TrProductSearch> _fetchSearchResults(String search) async {
    var result = await _trProductSearchService.requestTrProductSearch(search);
    if (result.isSuccessful) {
      return result.result!;
    }

    throw UnimplementedError();
  }

  void _updateResults(TrProductSearch searchResult) {
    void insertItem(int destIndex, TrProductSearchResult item) {
      _productSearchResults.insert(destIndex, item);
      _animatedListKey.currentState?.insertItem(destIndex);
    }

    void removeItem(int targetIndex) {
      _productSearchResults.removeAt(targetIndex);
      _animatedListKey.currentState?.removeItem(
        targetIndex,
        (context, animation) => const SizedBox.shrink(),
        duration: Duration.zero,
      );
    }

    void replace(int targetIndex, TrProductSearchResult item) {
      removeItem(targetIndex);
      insertItem(targetIndex, item);
    }

    setState(() {
      _totalNumberOfResults = searchResult.resultCount;
    });

    for (int index = 0; index < searchResult.results.length; index++) {
      if (_productSearchResults.length != index && searchResult.results[index].name != _productSearchResults[index].name) {
        replace(index, searchResult.results[index]);
        continue;
      }

      insertItem(index, searchResult.results[index]);
    }

    for (var i = searchResult.results.length; i < _productSearchResults.length; i++) {
      removeItem(i--);
    }
  }
}

class SingleSearchResult extends StatelessWidget with STWidget {
  SingleSearchResult({Key? key, required this.result, required this.isFirstItem}) : super(key: key);
  final TrProductSearchResult result;
  final bool isFirstItem;

  @override
  Widget build(BuildContext context) {
    return Column(
      children: [
        !isFirstItem
            ? Divider(
                color: colors.softForeground,
                thickness: 1,
                height: 1,
              )
            : const SizedBox.shrink(),
        SizedBox(
          width: double.infinity,
          child: ElevatedButton(
            onPressed: () => _onClickProduct(context, result.isin),
            style: ButtonStyle(
              backgroundColor: MaterialStateProperty.all(colors.background),
              foregroundColor: MaterialStateProperty.all(colors.foreground),
              overlayColor: MaterialStateProperty.all(colors.softBackground),
              padding: MaterialStateProperty.all(EdgeInsets.zero),
              elevation: MaterialStateProperty.all(0),
              shadowColor: MaterialStateProperty.all(Colors.transparent),
              minimumSize: MaterialStateProperty.all(const Size(50, 50)),
            ),
            child: Text(result.name),
          ),
        )
      ],
    );
  }

  Future<void> _onClickProduct(BuildContext context, String isin) async {
    final trProductPriceService = GetIt.instance.get<TrProductPriceService>();
    var trProductInfo = await trProductPriceService.requestTrProductPriceByIsinWithoutExtension(isin);

    if (!trProductInfo.isSuccessful) {
      // TODO: Handle.
      throw UnimplementedError();
    }

    var info = trProductInfo.result!;

    var priceInfoFuture = Completer<TrProductPrice>();
    var sub = trProductPriceService.stream$.listen((event) {
      if (event != null) {
        priceInfoFuture.complete(event);
      }
    });

    var priceInfo = await priceInfoFuture.future;
    sub.cancel();

    var details = TrUtil.getTrUiProductDetails(
      priceInfo,
      info,
      info.positionType,
    );

    Util.pushToRoute(
      context,
      ProductPage(
        positionType: info.positionType,
        productInfo: info,
        trProductPriceStream: trProductPriceService.stream$,
        productDetails: details,
      ),
    );
  }
}

class _CustomIconButton extends StatelessWidget {
  const _CustomIconButton({
    Key? key,
    required this.onPressed,
    required this.icon,
    required this.width,
  }) : super(key: key);
  final void Function()? onPressed;
  final double width;
  final Icon icon;

  @override
  Widget build(BuildContext context) {
    return Container(
      height: double.infinity,
      alignment: Alignment.center,
      child: TextButton(
        onPressed: onPressed,
        style: TextButton.styleFrom(
          padding: EdgeInsets.zero,
          minimumSize: Size(width, double.infinity),
          tapTargetSize: MaterialTapTargetSize.shrinkWrap,
          alignment: Alignment.centerLeft,
        ),
        child: SizedBox(
          width: width,
          height: double.infinity,
          child: icon,
        ),
      ),
    );
  }
}
