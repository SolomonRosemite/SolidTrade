import 'package:flutter/material.dart';
import 'package:get_it/get_it.dart';
import 'package:solidtrade/components/base/st_widget.dart';
import 'package:solidtrade/components/base/st_stream_builder.dart';
import 'package:solidtrade/data/entities/user.dart';
import 'package:solidtrade/pages/settings/app_preferences_page.dart';
import 'package:solidtrade/services/stream/user_service.dart';
import 'package:solidtrade/services/util/util.dart';

class UserAppBar extends StatelessWidget with STWidget {
  UserAppBar({Key? key}) : super(key: key);

  final userService = GetIt.instance.get<UserService>();

  void _openUserSettings(BuildContext context, User user) => Util.pushToRoute(context, AppPreferences(user: user));

  @override
  Widget build(BuildContext context) {
    return Container(
      color: colors.softBackground,
      child: STStreamBuilder<User>(
        stream: userService.stream$,
        builder: (_, user) {
          return Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              Row(
                children: [
                  Container(
                    margin: const EdgeInsets.only(left: 10),
                    child: TextButton(
                      onPressed: () => _openUserSettings(context, user),
                      child: Card(
                        margin: const EdgeInsets.symmetric(vertical: 5),
                        shape: RoundedRectangleBorder(
                          borderRadius: BorderRadius.circular(90),
                          side: BorderSide(width: 0.5, color: colors.softBackground),
                        ),
                        child: ClipRRect(
                          borderRadius: BorderRadius.circular(90),
                          child: Container(
                            color: colors.background,
                            child: Util.loadImage(
                              user.profilePictureUrl,
                              50,
                            ),
                          ),
                        ),
                      ),
                    ),
                  ),
                  Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      const SizedBox(height: 10),
                      const Text(
                        "Your Portfolio🚀",
                        style: TextStyle(fontSize: 18, fontWeight: FontWeight.w800),
                      ),
                      Text(
                        "@${user.username}",
                        style: TextStyle(fontSize: 14, color: colors.lessSoftForeground),
                      ),
                    ],
                  )
                ],
              ),
              Row(
                mainAxisAlignment: MainAxisAlignment.end,
                children: [
                  IconButton(
                    onPressed: null,
                    icon: Icon(Icons.bar_chart_sharp, color: colors.foreground),
                    padding: const EdgeInsets.only(right: 10),
                    constraints: const BoxConstraints(),
                  ),
                  IconButton(
                    onPressed: null,
                    icon: Icon(Icons.notifications_rounded, color: colors.foreground),
                    padding: const EdgeInsets.only(right: 10),
                    constraints: const BoxConstraints(),
                  ),
                  IconButton(
                    onPressed: () => _openUserSettings(context, user),
                    icon: Icon(Icons.settings_rounded, color: colors.foreground),
                    padding: const EdgeInsets.only(right: 12.5),
                    constraints: const BoxConstraints(),
                  ),
                ],
              )
            ],
          );
        },
      ),
    );
  }
}
