import 'package:flutter/material.dart';
import 'package:get_it/get_it.dart';
import 'package:solidtrade/components/base/st_widget.dart';
import 'package:solidtrade/components/base/st_stream_builder.dart';
import 'package:solidtrade/data/entities/portfolio.dart';
import 'package:solidtrade/data/entities/user.dart';
import 'package:solidtrade/pages/portfolio/components/portfolio_positions.dart';
import 'package:solidtrade/services/stream/portfolio_service.dart';
import 'package:solidtrade/services/stream/user_service.dart';
import 'package:solidtrade/services/util/extensions/build_context_extensions.dart';

class PortfolioOverviewPage extends StatelessWidget with STWidget {
  PortfolioOverviewPage({Key? key, this.isViewingOutstandingOrders = false}) : super(key: key);
  final bool isViewingOutstandingOrders;

  final _portfolioService = GetIt.instance.get<PortfolioService>();
  final _userService = GetIt.instance.get<UserService>();

  @override
  Widget build(BuildContext context) {
    final chartHeight = context.screenHeight * .25;

    return STStreamBuilder<Portfolio>(
      stream: _portfolioService.stream$,
      builder: (context, portfolio) {
        return Column(
          children: [
            SizedBox(
              height: chartHeight,
              // TODO: Add chart
            ),
            STStreamBuilder<User>(
              stream: _userService.stream$,
              builder: (context, user) => Container(
                margin: const EdgeInsets.symmetric(horizontal: 35, vertical: 5),
                child: Column(
                  children: [
                    Align(
                      alignment: Alignment.centerLeft,
                      child: Text(
                        'Good morning,\n${user.displayName}👋.',
                        style: Theme.of(context).textTheme.headlineMedium!.copyWith(fontSize: 30),
                        textAlign: TextAlign.left,
                      ),
                    ),
                    const SizedBox(height: 5),
                    RichText(
                      text: TextSpan(
                        text: 'Your portfolio is up today ',
                        style: DefaultTextStyle.of(context).style.copyWith(fontSize: 15),
                        children: <TextSpan>[
                          TextSpan(text: '+3.20%', style: TextStyle(fontWeight: FontWeight.bold, color: colors.stockGreen)),
                          const TextSpan(text: ' while the S&P 500 is down '),
                          const TextSpan(text: '-1.30%', style: TextStyle(fontWeight: FontWeight.bold, color: Colors.grey)),
                          const TextSpan(text: '. Therefore outperforming the index by '),
                          const TextSpan(text: '+4.50%', style: TextStyle(fontWeight: FontWeight.bold, color: Colors.grey)),
                        ],
                      ),
                    ),
                    const SizedBox(height: 10),
                    PortfolioPositions(isViewingOutstandingOrders: isViewingOutstandingOrders)
                  ],
                ),
              ),
            ),
          ],
        );
      },
    );
  }
}
