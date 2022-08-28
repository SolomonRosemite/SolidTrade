import 'package:flutter/foundation.dart';
import 'package:flutter/material.dart';
import 'package:get_it/get_it.dart';
import 'package:solidtrade/config/config_reader.dart';
import 'package:solidtrade/data/entities/base/base_position.dart';
import 'package:solidtrade/data/entities/portfolio.dart';
import 'package:solidtrade/data/models/common/constants.dart';
import 'package:solidtrade/data/models/common/tuple.dart';
import 'package:solidtrade/data/models/enums/client_enums/name_for_large_number.dart';
import 'package:solidtrade/data/models/enums/shared_enums/position_type.dart';
import 'package:solidtrade/data/models/trade_republic/tr_aggregate_history.dart';
import 'package:solidtrade/data/models/trade_republic/tr_product_info.dart';
import 'package:solidtrade/data/models/trade_republic/tr_product_price.dart';
import 'package:solidtrade/data/models/trade_republic/tr_stock_details.dart';
import 'package:solidtrade/providers/app/app_configuration_provider.dart';
import 'package:solidtrade/providers/theme/app_theme.dart';
import 'package:solidtrade/services/util/extensions/iterable_extensions.dart';
import 'package:solidtrade/services/util/extensions/string_extensions.dart';

class TrUtil {
  static final _configurationProvider = GetIt.instance.get<ConfigurationProvider>();
  static final _baseUrl = "https://" + ConfigReader.getBaseUrl();

  static TrUiProductDetails getTrUiProductDetails(TrProductPrice priceInfo, TrProductInfo productInfo, PositionType positionType, {List<TrAggregateHistoryEntry>? entires}) {
    final isStockPosition = positionType == PositionType.stock;

    final positionChange = entires != null ? _getCustomRangePositionChange(entires) : _getTodaysPositionChange(priceInfo);

    final percentageChange = positionChange.t1;
    final absoluteChange = positionChange.t2;

    final isUp = percentageChange == 1 || 1 < percentageChange;
    final plusMinus = isUp ? "+" : "";

    final productTitle = isStockPosition ? productInfo.shortName : _getDerivativesProductTitle(productInfo);
    final productSubtitle = isStockPosition ? _getStockProductSubtitle(productInfo.shortName, productInfo.name) : _getDerivativesProductSubtitle(productInfo);

    final colorMode = _configurationProvider.themeProvider.theme.themeColorType == ColorThemeType.light ? "light" : "dark";

    var imageIsin = isStockPosition ? productInfo.isin : productInfo.derivativeInfo!.underlying.isin;

    return TrUiProductDetails(
      percentageChange: percentageChange,
      absoluteChange: absoluteChange,
      isUp: isUp,
      plusMinusProductNamePrefix: plusMinus,
      productTitle: productTitle,
      productSubtitle: productSubtitle,
      imageUrl: _baseUrl + "/image?Isin=$imageIsin&ThemeColor=$colorMode&IsWeb=$kIsWeb",
      textColor: isUp ? _configurationProvider.themeProvider.theme.stockGreen : _configurationProvider.themeProvider.theme.stockRed,
    );
  }

  static Tuple<double, double> _getTodaysPositionChange(TrProductPrice priceInfo) {
    final percentageChange = priceInfo.bid.price / priceInfo.pre.price;
    final absoluteChange = priceInfo.bid.price - priceInfo.pre.price;
    return Tuple(t1: percentageChange, t2: absoluteChange);
  }

  static Tuple<double, double> _getCustomRangePositionChange(List<TrAggregateHistoryEntry> entires) {
    final percentageChange = entires.last.close / entires.first.close;
    final absoluteChange = entires.last.close - entires.first.close;
    return Tuple(t1: percentageChange, t2: absoluteChange);
  }

  static Tuple<NameForLargeNumber, double> getNameForNumber(double number) {
    if (number > Constants.trillion) {
      return Tuple(t1: NameForLargeNumber.trillion, t2: number / Constants.trillion);
    } else if (number > Constants.billion) {
      return Tuple(t1: NameForLargeNumber.billion, t2: number / Constants.billion);
    } else if (number > Constants.million) {
      return Tuple(t1: NameForLargeNumber.million, t2: number / Constants.million);
    }
    return Tuple(t1: NameForLargeNumber.thousand, t2: number / Constants.thousand);
  }

  static String _getStockProductSubtitle(String shortName, String name) {
    return name.length > 18 ? shortName : name;
  }

  static String _getDerivativesProductTitle(TrProductInfo info) {
    return "${info.derivativeInfo!.properties.optionType.capitalize()} @${info.derivativeInfo!.properties.strike.toStringAsFixed(2)}";
  }

  static String _getDerivativesProductSubtitle(TrProductInfo info) {
    return "${info.derivativeInfo!.productCategoryName} ${info.derivativeInfo!.underlying.name}";
  }

  static int productPageGetAnalystsCount(Recommendations recommendations) {
    return recommendations.buy + recommendations.hold + recommendations.outperform + recommendations.sell + recommendations.underperform;
  }

  static IPosition? getPositionOrDefault(Portfolio portfolio, String isin) {
    var stockPosition = portfolio.stockPositions.firstWhereOrDefault((position) => position.isin == isin);
    if (stockPosition != null) return stockPosition;

    var knockOutPosition = portfolio.knockOutPositions.firstWhereOrDefault((position) => position.isin == isin);
    if (knockOutPosition != null) return knockOutPosition;

    var warrantPosition = portfolio.warrantPositions.firstWhereOrDefault((position) => position.isin == isin);
    if (warrantPosition != null) return warrantPosition;

    return null;
  }
}

class TrUiProductDetails {
  final double percentageChange;
  final double absoluteChange;
  final bool isUp;
  final String plusMinusProductNamePrefix;
  final String productTitle;
  final String productSubtitle;
  final String imageUrl;
  final Color textColor;

  TrUiProductDetails({
    required this.percentageChange,
    required this.absoluteChange,
    required this.isUp,
    required this.plusMinusProductNamePrefix,
    required this.productTitle,
    required this.productSubtitle,
    required this.imageUrl,
    required this.textColor,
  });
}
