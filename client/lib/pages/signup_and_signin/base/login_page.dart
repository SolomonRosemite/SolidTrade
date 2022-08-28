import 'package:flutter/material.dart';
import 'package:solidtrade/components/base/st_page.dart';
import 'package:solidtrade/components/base/st_widget.dart';
import 'package:solidtrade/pages/signup_and_signin/login_home_page.dart';
import 'package:solidtrade/pages/signup_and_signin/signin/login_signin_page.dart';
import 'package:solidtrade/pages/signup_and_signin/signup/login_signup_page.dart';

class LoginPage extends StatefulWidget {
  const LoginPage({Key? key}) : super(key: key);

  @override
  State<LoginPage> createState() => _LoginPageState();
}

class _LoginPageState extends State<LoginPage> with STWidget, SingleTickerProviderStateMixin {
  late TabController _tabController;

  @override
  void initState() {
    _tabController = TabController(length: 3, vsync: this, initialIndex: 1);
    super.initState();
  }

  @override
  void dispose() {
    super.dispose();
    _tabController.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return STPage(
      page: () => Scaffold(
        body: Container(
          color: colors.softBackground,
          child: Column(
            children: [
              Expanded(
                child: TabBarView(
                  controller: _tabController,
                  children: [
                    Container(margin: const EdgeInsets.all(8.0), child: const LoginSignUp()),
                    Container(margin: const EdgeInsets.all(8.0), child: LoginHome(tabController: _tabController)),
                    Container(margin: const EdgeInsets.all(8.0), child: LoginSignIn()),
                  ],
                ),
              ),
              Container(
                margin: const EdgeInsets.symmetric(horizontal: 8.0),
                height: 45,
                decoration: BoxDecoration(
                  color: colors.background,
                  borderRadius: BorderRadius.circular(
                    10.0,
                  ),
                ),
                child: TabBar(
                  controller: _tabController,
                  indicator: BoxDecoration(
                    borderRadius: BorderRadius.circular(10.0),
                    color: colors.softForeground,
                  ),
                  labelColor: colors.foreground,
                  unselectedLabelColor: colors.foreground,
                  tabs: const [
                    Tab(
                      text: 'Register',
                    ),
                    Tab(
                      icon: Icon(Icons.home),
                    ),
                    Tab(
                      text: 'Sign In',
                    ),
                  ],
                ),
              ),
              const SizedBox(height: 10)
            ],
          ),
        ),
      ),
    );
  }
}
