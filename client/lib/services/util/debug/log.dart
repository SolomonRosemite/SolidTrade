import 'package:enum_to_string/enum_to_string.dart';
import 'package:flutter/foundation.dart';
import 'package:logger/logger.dart';
import 'package:solidtrade/data/enums/environment.dart';
import 'package:solidtrade/app//main_common.dart';
import 'package:solidtrade/services/util/extensions/string_extensions.dart';

class Log {
  static final logger = Logger(printer: SimpleLogPrinter(), filter: ProductionFilter());

  static bool get shouldLog => environment != Environment.production;

  static void d(Object? value) {
    if (shouldLog) logger.d(value);
  }

  static void i(Object? value) {
    if (shouldLog) logger.i(value);
  }

  static void w(Object? value) {
    if (shouldLog) logger.w(value);
  }

  static void f(Object? value) {
    if (shouldLog) logger.e(value);
  }
}

class SimpleLogPrinter extends LogPrinter {
  @override
  List<String> log(LogEvent event) {
    var color = PrettyPrinter.levelColors[event.level];
    var emoji = PrettyPrinter.levelEmojis[event.level];

    var c = StackTrace.current.toString();

    return [
      color!('$emoji[${EnumToString.convertToString(event.level).capitalize()}] ${_getClassName(c)} - ${event.message}')
    ];
  }

  String _getClassName(String stackTrace) {
    if (kIsWeb) {
      return _getMethodNameFromWeb(stackTrace);
    }
    return _getClassNameFromMobile(stackTrace);
  }

  String _getMethodNameFromWeb(String stackTrace) {
    final s1 = stackTrace.substring(2);
    final s2 = s1.substring(0, s1.indexOf(":/") - 2);
    final methodName = s2.substring(s2.lastIndexOf(" ") + 1);

    return methodName;
  }

  String _getClassNameFromMobile(String stackTrace) {
    final s1 = stackTrace.substring(stackTrace.indexOf("Log.") + 1);
    final s2 = s1.substring(s1.indexOf("#") + 2).trim();
    return s2.substring(0, s2.indexOf("(") - 1).trim();
  }
}
