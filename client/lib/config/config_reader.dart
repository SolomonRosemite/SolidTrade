import 'dart:convert';
import 'package:flutter/services.dart';

abstract class ConfigReader {
  static late Map<String, dynamic> _config;

  static Future<void> initialize() async {
    final configString = await rootBundle.loadString('assets/config/app_config.json');
    _config = json.decode(configString) as Map<String, dynamic>;
  }

  static String getBaseUrl() {
    return _config['baseUrl'];
  }

  static String getTrEndpoint() {
    return _config['trEndpoint'];
  }

  static String getTrConnectString() {
    return _config['initialTrConnectString'];
  }
}
