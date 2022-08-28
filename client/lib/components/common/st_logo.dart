import 'package:flutter/material.dart';

class STLogo extends StatefulWidget {
  const STLogo(
    this.gifAsset, {
    required UniqueKey key,
    this.size = 100,
    this.animationDuration = const Duration(milliseconds: 4000),
  }) : super(key: key);
  final String gifAsset;
  final double size;
  final Duration animationDuration;

  @override
  State<STLogo> createState() => _STLogoState();
}

class _STLogoState extends State<STLogo> {
  late Future _awaitAnimationDuration;
  late String _assetAsImage;
  bool _showLogoAsImage = false;
  bool _isDisposed = false;

  AssetImage? image;
  late Image myImage;

  @override
  void didChangeDependencies() {
    super.didChangeDependencies();
    precacheImage(myImage.image, context);
  }

  @override
  void initState() {
    super.initState();

    _assetAsImage = widget.gifAsset.substring(0, widget.gifAsset.indexOf(".gif")) + ".jpg";
    myImage = Image.asset(
      _assetAsImage,
      width: widget.size,
      height: widget.size,
    );

    _awaitAnimationDuration = Future.delayed(widget.animationDuration, () {
      if (_isDisposed) {
        return;
      }

      setState(() {
        _showLogoAsImage = true;
      });
    });
  }

  @override
  void dispose() {
    _isDisposed = true;
    _awaitAnimationDuration.ignore();
    image?.evict();
    super.dispose();
  }

  AssetImage getImage() {
    image ??= AssetImage(widget.gifAsset);

    return image!;
  }

  @override
  Widget build(BuildContext context) {
    return AnimatedCrossFade(
      firstChild: Image(
        image: getImage(),
        width: widget.size,
        height: widget.size,
      ),
      secondChild: myImage,
      crossFadeState: _showLogoAsImage ? CrossFadeState.showSecond : CrossFadeState.showFirst,
      duration: const Duration(milliseconds: 10),
    );
  }
}
