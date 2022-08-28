import 'dart:async';
import 'package:flutter/material.dart';

class TimerButton extends StatefulWidget {
  const TimerButton({
    Key? key,
    required this.onPressed,
    required this.text,
    required this.initialSecondsLeft,
    this.constructMessage = defaultMessage,
    this.disabledButtonStyle = const ButtonStyle(),
    this.enabledButtonStyle = const ButtonStyle(),
    this.disabledTextStyle,
    this.enabledTextStyle,
  }) : super(key: key);
  final String Function(String, int) constructMessage;
  final ButtonStyle disabledButtonStyle;
  final ButtonStyle enabledButtonStyle;
  final TextStyle? disabledTextStyle;
  final TextStyle? enabledTextStyle;
  final void Function() onPressed;
  final String text;
  final int initialSecondsLeft;

  static String defaultMessage(String text, int secondsLeft) => "$text in $secondsLeft";

  @override
  State<TimerButton> createState() => _TimerButtonState();
}

class _TimerButtonState extends State<TimerButton> {
  late ButtonStyle disabledButtonStyle = widget.disabledButtonStyle.copyWith(
    backgroundColor: MaterialStateProperty.all<Color>(Colors.grey),
    foregroundColor: MaterialStateProperty.all<Color>(Colors.white),
  );
  late ButtonStyle enabledButtonStyle = widget.enabledButtonStyle.copyWith(
    backgroundColor: MaterialStateProperty.all<Color>(Colors.red),
    foregroundColor: MaterialStateProperty.all<Color>(Colors.white),
  );

  late String Function(String, int) constructMessage = widget.constructMessage;
  late int secondsLeft = widget.initialSecondsLeft;
  late Timer _timer;

  @override
  void initState() {
    super.initState();
    startTimer();
  }

  void startTimer() {
    _timer = Timer.periodic(const Duration(seconds: 1), onTimerInterval);
  }

  bool get noTimeLeft => secondsLeft == 0;

  void onTimerInterval(Timer _) {
    if (noTimeLeft) {
      _timer.cancel();
      return;
    }

    setState(() {
      secondsLeft--;
    });
  }

  @override
  void dispose() {
    _timer.cancel();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return TextButton(
      onPressed: noTimeLeft ? widget.onPressed : null,
      style: noTimeLeft ? enabledButtonStyle : disabledButtonStyle,
      child: Text(
        noTimeLeft ? widget.text : constructMessage.call(widget.text, secondsLeft),
        style: noTimeLeft ? widget.enabledTextStyle : widget.disabledTextStyle,
      ),
    );
  }
}
