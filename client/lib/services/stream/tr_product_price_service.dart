import 'dart:async';

import 'package:get_it/get_it.dart';
import 'package:rxdart/subjects.dart';
import 'package:solidtrade/data/models/common/constants.dart';
import 'package:solidtrade/data/models/request_response/request_response.dart';
import 'package:solidtrade/data/models/trade_republic/tr_product_info.dart';
import 'package:solidtrade/data/models/trade_republic/tr_product_price.dart';
import 'package:solidtrade/data/models/trade_republic/tr_request_response.dart';
import 'package:solidtrade/services/request/data_request_service.dart';
import 'package:solidtrade/services/stream/tr_product_info_service.dart';

import 'base/transient_base_service.dart';

class TrProductPriceService extends ITransientService<TrProductPrice> {
  TrProductPriceService() : super(BehaviorSubject.seeded(null));

  void requestTrProductPrice(String isinWithExtension) {
    StreamSubscription<TrRequestResponse<TrProductPrice>?>? subscription;

    subscription = DataRequestService.trApiDataRequestService
        .makeRequestAsync<TrProductPrice>(
          Constants.getTrProductPriceRequestString(isinWithExtension),
        )
        .listen((event) => onEvent(event, subscription));
  }

  Future<RequestResponse<TrProductInfo>> requestTrProductPriceByIsinWithoutExtension(String isinWithoutExtension) async {
    var info = await GetIt.instance.get<TrProductInfoService>().requestTrProductInfo(isinWithoutExtension);

    if (!info.isSuccessful) {
      return info;
    }

    var isinWithExtension = "${info.result!.isin}.${info.result!.exchangeIds.first}";

    StreamSubscription<TrRequestResponse<TrProductPrice>?>? subscription;

    subscription = DataRequestService.trApiDataRequestService
        .makeRequestAsync<TrProductPrice>(
          Constants.getTrProductPriceRequestString(isinWithExtension),
        )
        .listen((event) => onEvent(event, subscription));

    return info;
  }
}
