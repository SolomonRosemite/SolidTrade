import 'package:flutter/material.dart';
import 'package:get_it/get_it.dart';
import 'package:solidtrade/components/base/st_widget.dart';
import 'package:solidtrade/providers/app/app_configuration_provider.dart';

class UserInputValidationResult {
  static final _translation = GetIt.instance.get<ConfigurationProvider>().languageProvider.language;

  final bool isValid;
  final String? errorMessage;

  UserInputValidationResult(this.isValid, this.errorMessage);

  static UserInputValidationResult errorPriceCanNotBeZero() => UserInputValidationResult(false, _translation.editOrderSettingsView.errorMessagePriceCannotBeEmptyOrZero);
  static UserInputValidationResult errorPriceMustBeHigher() => UserInputValidationResult(false, _translation.editOrderSettingsView.errorMessagePriceMustBeHigher);
  static UserInputValidationResult errorPriceMustBeLower() => UserInputValidationResult(false, _translation.editOrderSettingsView.errorMessagePriceMustBeLower);
  static UserInputValidationResult insufficientFunds() => UserInputValidationResult(false, _translation.editOrderSettingsView.errorMessageInsufficientFunds);
  static UserInputValidationResult validInput() => UserInputValidationResult(true, null);
}

class OrderValidationHint extends StatelessWidget with STWidget {
  OrderValidationHint({Key? key, required this.inputValidation}) : super(key: key);
  final UserInputValidationResult inputValidation;

  @override
  Widget build(BuildContext context) {
    return !inputValidation.isValid
        ? LayoutBuilder(
            builder: (context, constraints) => Row(
              children: [
                Container(
                  margin: const EdgeInsets.symmetric(horizontal: 5),
                  child: Icon(
                    Icons.info_outline,
                    color: colors.redErrorText,
                    size: 20,
                  ),
                ),
                SizedBox(
                  width: constraints.maxWidth - 30,
                  child: Text(inputValidation.errorMessage!, style: TextStyle(fontSize: 13, color: colors.redErrorText)),
                ),
              ],
            ),
          )
        : const SizedBox.shrink();
  }
}
