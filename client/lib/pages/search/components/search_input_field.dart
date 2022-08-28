import 'package:flutter/material.dart';
import 'package:solidtrade/components/base/st_widget.dart';

class SearchInputField extends StatelessWidget with STWidget {
  SearchInputField({
    Key? key,
    this.autofocus = false,
    this.enableField = true,
    this.customActionWidget,
    this.customLeadingWidget,
    this.leftPadding,
    this.onGestureTap,
    this.onInputChanged,
    this.textEditingController,
  }) : super(key: key);
  final bool autofocus;
  final bool enableField;
  final Widget? customLeadingWidget;
  final Widget? customActionWidget;
  final SizedBox? leftPadding;
  final void Function()? onGestureTap;
  final void Function(String)? onInputChanged;
  final TextEditingController? textEditingController;

  @override
  Widget build(BuildContext context) {
    return Row(
      children: [
        leftPadding ?? const SizedBox(width: 10),
        customLeadingWidget ??
            Icon(
              Icons.search,
              size: 20,
              color: colors.foreground,
            ),
        Expanded(
          child: GestureDetector(
            onTap: onGestureTap,
            child: TextFormField(
              controller: textEditingController,
              autofocus: autofocus,
              enabled: enableField,
              onChanged: onInputChanged,
              decoration: InputDecoration(
                contentPadding: const EdgeInsets.all(10),
                hintText: 'Search companies...',
                border: InputBorder.none,
                hintStyle: TextStyle(color: colors.foreground),
              ),
              style: TextStyle(fontSize: 16, color: colors.foreground),
            ),
          ),
        ),
        customActionWidget ?? const SizedBox.shrink()
      ],
    );
  }
}
