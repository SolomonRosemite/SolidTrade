import 'package:get_it/get_it.dart';
import 'package:solidtrade/components/base/st_page.dart';
import 'package:solidtrade/components/base/st_widget.dart';
import 'package:solidtrade/pages/home/components/user_app_bar.dart';
import 'package:flutter/material.dart';
import 'package:solidtrade/pages/portfolio/portfolio_page.dart';
import 'package:solidtrade/pages/search/search_page.dart';
import 'package:solidtrade/providers/theme/app_theme.dart';
import 'package:solidtrade/services/stream/floating_action_button_update_service.dart';

class HomePage extends StatefulWidget {
  const HomePage({Key? key}) : super(key: key);

  @override
  State<HomePage> createState() => _HomePageState();
}

class _HomePageState extends State<HomePage> with STWidget {
  var floatingActionButtonUpdateService = GetIt.instance.get<FloatingActionButtonUpdateService>();

  int _selectedIndex = 0;

  static final List<Widget> _widgetOptions = <Widget>[
    const PortfolioPage(),
    const SearchPage(),
    const Text("data"),
    const Text("data"),
  ];

  void _handleItemIndexClick(int index) {
    setState(() {
      _selectedIndex = index;
    });
  }

  void _handleFloatingActionButtonClick() {
    floatingActionButtonUpdateService.onClickFloatingActionButtonOrScrollUpFarEnough();
  }

  Color _getItemColor(int itemIndex) {
    return itemIndex == _selectedIndex ? colors.selectedItem : colors.unselectedItem;
  }

  @override
  Widget build(BuildContext context) {
    return STPage(
      page: () => Scaffold(
        body: Column(
          children: [
            UserAppBar(),
            Expanded(
              child: LayoutBuilder(
                builder: (BuildContext context, BoxConstraints constraints) => SizedBox(
                  height: constraints.maxHeight,
                  child: _widgetOptions[_selectedIndex],
                ),
              ),
            )
          ],
        ),
        bottomNavigationBar: BottomNavigationBar(
          items: [
            BottomNavigationBarItem(label: translations.navigationBar.portfolio, icon: Icon(Icons.insights, color: _getItemColor(0))),
            BottomNavigationBarItem(label: translations.navigationBar.search, icon: Icon(Icons.search, color: _getItemColor(1))),
            BottomNavigationBarItem(label: translations.navigationBar.leaderboard, icon: Icon(Icons.leaderboard, color: _getItemColor(2))),
            BottomNavigationBarItem(label: translations.navigationBar.profile, icon: Icon(Icons.person, color: _getItemColor(3))),
          ],
          backgroundColor: colors.navigationBackground,
          type: BottomNavigationBarType.fixed,
          showSelectedLabels: false,
          showUnselectedLabels: false,
          currentIndex: _selectedIndex,
          onTap: _handleItemIndexClick,
        ),
        floatingActionButton: StreamBuilder<bool>(
          stream: floatingActionButtonUpdateService.stream$,
          builder: (context, snap) {
            if (snap.hasData && !snap.data!) {
              return const SizedBox.shrink();
            }

            return FloatingActionButton(
              onPressed: _handleFloatingActionButtonClick,
              tooltip: 'Scroll up',
              backgroundColor: colors.themeColorType == ColorThemeType.light ? colors.background : colors.softBackground,
              child: Icon(
                Icons.arrow_circle_up,
                color: colors.foreground,
              ),
            );
          },
        ),
      ),
    );
  }
}
