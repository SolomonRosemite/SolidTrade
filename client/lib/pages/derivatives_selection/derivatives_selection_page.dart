import 'dart:async';

import 'package:flutter/material.dart';
import 'package:get_it/get_it.dart';
import 'package:solidtrade/components/base/st_page.dart';
import 'package:solidtrade/components/base/st_stream_builder.dart';
import 'package:solidtrade/components/base/st_widget.dart';
import 'package:solidtrade/data/models/enums/client_enums/derivatives_query_options.dart';
import 'package:solidtrade/data/models/enums/shared_enums/position_type.dart';
import 'package:solidtrade/data/models/trade_republic/tr_derivative_search_result.dart';
import 'package:solidtrade/data/models/trade_republic/tr_product_info.dart';
import 'package:solidtrade/data/models/trade_republic/tr_product_price.dart';
import 'package:solidtrade/pages/product/product_page.dart';
import 'package:solidtrade/services/stream/tr_derivatives_search_service.dart';
import 'package:solidtrade/services/stream/tr_product_price_service.dart';
import 'package:solidtrade/services/util/extensions/double_extensions.dart';
import 'package:solidtrade/services/util/extensions/string_extensions.dart';
import 'package:solidtrade/services/util/tr_util.dart';
import 'package:solidtrade/services/util/util.dart';

class DerivativesSelectionPage extends StatefulWidget {
  const DerivativesSelectionPage({
    Key? key,
    required this.derivativesPositionType,
    required this.productInfo,
    required this.priceStream,
  }) : super(key: key);
  final PositionType derivativesPositionType;
  final Stream<TrProductPrice?> priceStream;
  final TrProductInfo productInfo;

  @override
  State<DerivativesSelectionPage> createState() => _DerivativesSelectionPageState();
}

class _DerivativesSelectionPageState extends State<DerivativesSelectionPage> with SingleTickerProviderStateMixin {
  bool get _isKnockout => widget.derivativesPositionType == PositionType.knockout;

  String get _longOptionTypeName => _isKnockout ? DerivativesOptionType.long.name : DerivativesOptionType.call.name;
  String get _shortOptionTypeName => _isKnockout ? DerivativesOptionType.short.name : DerivativesOptionType.put.name;

  late DerivativesSortDirectionOptions _sortingDirection;
  late DerivativesSortOptions _sortByProperty;
  DerivativesOptionType get _filterByType {
    switch (_currentPage) {
      case _CurrentPageLongShort.long:
        return _isKnockout ? DerivativesOptionType.long : DerivativesOptionType.call;
      case _CurrentPageLongShort.short:
        return _isKnockout ? DerivativesOptionType.short : DerivativesOptionType.put;
    }
  }

  final List<TrDerivativeSearchResult> _longDerivativeListResults = [];
  final List<TrDerivativeSearchResult> _shortDerivativeListResults = [];

  final _trDerivativesSearchService = GetIt.instance.get<TrDerivativesSearchService>();
  late final TabController _tabController;

  _CurrentPageLongShort _currentPage = _CurrentPageLongShort.long;

  @override
  void initState() {
    _tabController = TabController(length: 2, vsync: this);
    _tabController.addListener(_onChangeTab);
    _initializeParameters();

    _loadDerivatives();
    super.initState();
  }

  void _initializeParameters() {
    if (!_isKnockout) {
      _setSortingDirection(DerivativesSortDirectionOptions.desc);
      _setSortingByProperty(_SortByPropertyOptions.knockoutOrDelta);
      return;
    }
    _setSortingDirection(DerivativesSortDirectionOptions.desc);
    _setSortingByProperty(_SortByPropertyOptions.leverageOrStrike);
  }

  @override
  Widget build(BuildContext context) {
    return STPage(
      page: () => Scaffold(
        appBar: AppBar(
          centerTitle: true,
          title: STStreamBuilder<TrProductPrice>(
            stream: widget.priceStream,
            builder: (context, prices) => Column(children: [
              Text(widget.productInfo.name),
              Text(prices.bid.price.toDefaultPrice()),
            ]),
          ),
          bottom: TabBar(
            controller: _tabController,
            tabs: [
              Tab(icon: Text(_longOptionTypeName.capitalize())),
              Tab(icon: Text(_shortOptionTypeName.capitalize())),
            ],
          ),
        ),
        body: Container(
          margin: const EdgeInsets.symmetric(horizontal: 20),
          child: Column(
            children: [
              Container(
                margin: const EdgeInsets.symmetric(vertical: 10),
                child: _UnifiedRow(children: [
                  _ParameterOption(
                    optionName: _isKnockout ? "Leverage" : "Strike",
                    isSelected: _sortByProperty.toSortByPropertyOptions == _SortByPropertyOptions.leverageOrStrike,
                    sortingProperty: _SortByPropertyOptions.leverageOrStrike,
                    sortDirectionOptions: _sortingDirection,
                    onClick: _handleClickParameterOption,
                  ),
                  _ParameterOption(
                    optionName: _isKnockout ? "Knockout" : "Delta",
                    isSelected: _sortByProperty.toSortByPropertyOptions == _SortByPropertyOptions.knockoutOrDelta,
                    sortingProperty: _SortByPropertyOptions.knockoutOrDelta,
                    sortDirectionOptions: _sortingDirection,
                    onClick: _handleClickParameterOption,
                  ),
                  _ParameterOption(
                    optionName: "Size",
                    isSelected: _sortByProperty.toSortByPropertyOptions == _SortByPropertyOptions.size,
                    sortingProperty: _SortByPropertyOptions.size,
                    sortDirectionOptions: _sortingDirection,
                    onClick: _handleClickParameterOption,
                  ),
                  _ParameterOption(
                    optionName: "Expiry",
                    isSelected: _sortByProperty.toSortByPropertyOptions == _SortByPropertyOptions.expiry,
                    sortingProperty: _SortByPropertyOptions.expiry,
                    sortDirectionOptions: _sortingDirection,
                    onClick: _handleClickParameterOption,
                  ),
                ]),
              ),
              Expanded(
                child: AnimatedCrossFade(
                  duration: const Duration(seconds: 3),
                  firstChild: _DerivativeSearchResults(derivativeListResults: _longDerivativeListResults),
                  secondChild: _DerivativeSearchResults(derivativeListResults: _shortDerivativeListResults),
                  crossFadeState: _currentPage == _CurrentPageLongShort.long ? CrossFadeState.showFirst : CrossFadeState.showSecond,
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }

  Future<void> _handleClickParameterOption(_SortByPropertyOptions sortOption) async {
    var convertedSortOption = _sortByProperty.toSortByPropertyOptions;

    var isAlreadySelected = convertedSortOption == sortOption;
    Log.i(isAlreadySelected);
    Log.i(convertedSortOption);
    Log.i(sortOption);
    if (isAlreadySelected) {
      _toggleSortingDirection();
    } else {
      // When changing the parameter type we always start with the descending order
      _setSortingDirection(DerivativesSortDirectionOptions.desc);
      _setSortingByProperty(sortOption);
    }

    await _loadDerivatives();
  }

  void _toggleSortingDirection() {
    setState(() {
      _sortingDirection = _sortingDirection == DerivativesSortDirectionOptions.desc ? DerivativesSortDirectionOptions.asc : DerivativesSortDirectionOptions.desc;
    });
  }

  void _setSortingDirection(DerivativesSortDirectionOptions option) {
    setState(() {
      _sortingDirection = option;
    });
  }

  void _setSortingByProperty(_SortByPropertyOptions option) {
    DerivativesSortOptions getSortingOption() {
      switch (option) {
        case _SortByPropertyOptions.leverageOrStrike:
          return _isKnockout ? DerivativesSortOptions.leverage : DerivativesSortOptions.strike;
        case _SortByPropertyOptions.knockoutOrDelta:
          return _isKnockout ? DerivativesSortOptions.knockout : DerivativesSortOptions.delta;
        case _SortByPropertyOptions.size:
          return DerivativesSortOptions.size;
        case _SortByPropertyOptions.expiry:
          return DerivativesSortOptions.expiry;
      }
    }

    setState(() {
      _sortByProperty = getSortingOption();
    });
  }

  Future<void> _onChangeTab() async {
    // https://stackoverflow.com/q/60252355/13024474
    if (!_tabController.indexIsChanging) {
      return;
    } else if (_currentPage == _CurrentPageLongShort.long) {
      _currentPage = _CurrentPageLongShort.short;
    } else {
      _currentPage = _CurrentPageLongShort.long;
    }

    await _loadDerivatives();
  }

  Future<void> _loadDerivatives() async {
    Log.w("starting");
    var results = await _fetchDerivatives();
    _setResults(results);
    _fadeToNewResults();
  }

  Future<Iterable<TrDerivativeSearchResult>> _fetchDerivatives() async {
    var trDerivativeSearchResultMapper = await _trDerivativesSearchService.fetchDerivatives(
      isin: widget.productInfo.isin,
      derivativeType: widget.derivativesPositionType,
      sortBy: _sortByProperty,
      sortDirection: _sortingDirection,
      filterByType: _filterByType,
      numberOfAvailableProducts: widget.productInfo.derivativeProductCount.knockOutProduct!,
    );

    if (!trDerivativeSearchResultMapper.isSuccessful) {
      // TODO: Handle...
      throw UnimplementedError();
    }

    return trDerivativeSearchResultMapper.result!.convertToTrDerivativeSearchResults(widget.derivativesPositionType);
  }

  void _setResults(Iterable<TrDerivativeSearchResult> results) {
    if (_currentPage == _CurrentPageLongShort.long) {
      _longDerivativeListResults.clear();
      setState(() {
        _longDerivativeListResults.addAll(results);
      });
      return;
    }

    _shortDerivativeListResults.clear();
    setState(() {
      _shortDerivativeListResults.addAll(results);
    });
  }

  void _fadeToNewResults() {
    // This looks weird, but all this does is updating the widget that uses "_currentPage".
    // We dont update the value here, because it has already updated when calling the "_onChangeTab" method.
    setState(() {
      _currentPage;
    });
  }
}

class _ParameterOption extends StatelessWidget with STWidget {
  _ParameterOption({
    Key? key,
    required this.isSelected,
    required this.optionName,
    required this.sortingProperty,
    required this.sortDirectionOptions,
    required this.onClick,
  }) : super(key: key);
  final void Function(_SortByPropertyOptions sortOption) onClick;
  final DerivativesSortDirectionOptions sortDirectionOptions;
  final _SortByPropertyOptions sortingProperty;
  final String optionName;
  final bool isSelected;

  @override
  Widget build(BuildContext context) {
    return ElevatedButton(
      onPressed: () => onClick.call(sortingProperty),
      style: ButtonStyle(
        backgroundColor: MaterialStateProperty.all(colors.background),
        foregroundColor: MaterialStateProperty.all(colors.foreground),
        overlayColor: MaterialStateProperty.all(colors.softBackground),
        padding: MaterialStateProperty.all(EdgeInsets.zero),
        elevation: MaterialStateProperty.all(0),
        shadowColor: MaterialStateProperty.all(Colors.transparent),
        minimumSize: MaterialStateProperty.all(const Size(50, 50)),
      ),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          Text(
            optionName,
            style: isSelected ? TextStyle(color: colors.blueText) : null,
          ),
          Icon(
            sortDirectionOptions == DerivativesSortDirectionOptions.asc ? Icons.arrow_drop_up : Icons.arrow_drop_down,
            color: isSelected ? colors.foreground : Colors.transparent,
            size: 20,
          )
        ],
      ),
    );
  }
}

class _DerivativeSearchResults extends StatelessWidget {
  const _DerivativeSearchResults({Key? key, required this.derivativeListResults}) : super(key: key);
  final List<TrDerivativeSearchResult> derivativeListResults;

  @override
  Widget build(BuildContext context) {
    return ListView.builder(
      itemCount: derivativeListResults.length,
      shrinkWrap: true,
      itemBuilder: (context, index) => _SingleDerivativeSearchResult(
        result: derivativeListResults[index],
      ),
    );
  }
}

class _SingleDerivativeSearchResult extends StatelessWidget with STWidget {
  _SingleDerivativeSearchResult({Key? key, required this.result}) : super(key: key);
  final TrDerivativeSearchResult result;

  @override
  Widget build(BuildContext context) {
    return Column(
      children: [
        InkWell(
          onTap: () => _onClickDerivative(context),
          child: Container(
            margin: const EdgeInsets.symmetric(vertical: 5),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  "${result.issuerDisplayName} ${result.name}",
                  style: TextStyle(color: Colors.grey[300]),
                ),
                const SizedBox(height: 5),
                _UnifiedRow(
                  children: [
                    Text(result.leverageOrStrike.toString() + "x"),
                    Text(result.knockoutBarrierOrDelta.toString() + "\$"),
                    Text(result.size.toString()),
                    Container(
                      padding: const EdgeInsets.all(2.5),
                      color: colors.navigationBackground,
                      child: Text(
                        result.expiryText.toString(),
                        overflow: TextOverflow.clip,
                        style: TextStyle(color: Colors.grey[400], fontSize: 13.5),
                      ),
                    )
                  ],
                ),
              ],
            ),
          ),
        ),
        Divider(
          color: colors.softForeground,
          thickness: 2,
        )
      ],
    );
  }

  Future<void> _onClickDerivative(BuildContext context) async {
    final trProductPriceService = GetIt.instance.get<TrProductPriceService>();
    var trProductInfo = await trProductPriceService.requestTrProductPriceByIsinWithoutExtension(result.isin);

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

class _UnifiedRow extends StatelessWidget {
  const _UnifiedRow({Key? key, required this.children}) : super(key: key);
  final List<Widget> children;

  Iterable<Widget> get _children => children.map((child) => Expanded(child: Center(child: child)));

  @override
  Widget build(BuildContext context) {
    return Row(
      mainAxisAlignment: MainAxisAlignment.spaceEvenly,
      children: [
        ..._children
      ],
    );
  }
}

enum _CurrentPageLongShort {
  long,
  short,
}

enum _SortByPropertyOptions {
  leverageOrStrike,
  knockoutOrDelta,
  size,
  expiry,
}

extension DerivativesSortOptionsExtension on DerivativesSortOptions {
  _SortByPropertyOptions get toSortByPropertyOptions {
    switch (this) {
      case DerivativesSortOptions.leverage:
      case DerivativesSortOptions.strike:
        return _SortByPropertyOptions.leverageOrStrike;
      case DerivativesSortOptions.knockout:
      case DerivativesSortOptions.delta:
        return _SortByPropertyOptions.knockoutOrDelta;
      case DerivativesSortOptions.size:
        return _SortByPropertyOptions.size;
      case DerivativesSortOptions.expiry:
        return _SortByPropertyOptions.expiry;
    }
  }
}
