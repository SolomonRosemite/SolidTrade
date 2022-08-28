import 'dart:typed_data';
import 'package:flutter/material.dart';
import 'dart:async';
import 'package:image/image.dart' as im;
import 'package:solidtrade/components/base/st_page.dart';
import 'dart:math';

import 'package:solidtrade/services/util/util.dart';

class Cropper extends StatefulWidget {
  final Uint8List image;

  const Cropper({Key? key, required this.image}) : super(key: key);

  @override
  _CropperState createState() => _CropperState();
}

class _CropperState extends State<Cropper> {
  late Uint8List resultImg;
  late double scale = 1.0;
  late double zeroScale; //Initial scale to fit image in bounding crop box.
  late Offset offset = const Offset(0.0, 0.0); //Used in translation of image.
  late double cropRatio = 1 / 1; //aspect ratio of desired crop.
  late im.Image decoded; //decoded image to get pixel dimensions
  late double imgWidth; //img pixel width
  late double imgHeight; //img pixel height
  late Size cropArea; //Size of crop bonding box
  late double cropPad; //Aesthetic crop box padding.
  late double pXa; //Positive X available in translation
  late double pYa; //Positive Y available in translation
  late double totalX; //Total X of scaled image
  late double totalY; //Total Y of scaled image
  final Completer _decoded = Completer<bool>();
  final Completer _encoded = Completer<Uint8List>();

  @override
  initState() {
    _decodeImg();
    super.initState();
  }

  _decodeImg() {
    if (_decoded.isCompleted) return;
    decoded = im.decodeImage(widget.image)!;
    imgWidth = decoded.width.toDouble();
    imgHeight = decoded.height.toDouble();
    _decoded.complete(true);
  }

  _encodeImage(im.Image cropped) async {
    resultImg = Uint8List.fromList(im.encodePng(cropped));
    _encoded.complete(resultImg);
  }

  void _cropImage() async {
    var closeDialog = Util.showLoadingDialog(context, showIndicator: false, waitingText: "Loading. This might take a while...");

    await Future.delayed(const Duration(milliseconds: 500));

    double xPercent = pXa != 0.0 ? 1.0 - (offset.dx + pXa) / (2 * pXa) : 0.0;
    double yPercent = pYa != 0.0 ? 1.0 - (offset.dy + pYa) / (2 * pYa) : 0.0;
    double cropXpx = imgWidth * cropArea.width / totalX;
    double cropYpx = imgHeight * cropArea.height / totalY;
    double x0 = (imgWidth - cropXpx) * xPercent;
    double y0 = (imgHeight - cropYpx) * yPercent;
    im.Image cropped = im.copyCrop(decoded, x0.toInt(), y0.toInt(), cropXpx.toInt(), cropYpx.toInt());
    _encodeImage(cropped);

    closeDialog();
    Navigator.pop(context, _encoded.future);
  }

  computeRelativeDim(double newScale) {
    totalX = newScale * cropArea.height * imgWidth / imgHeight;
    totalY = newScale * cropArea.height;
    pXa = 0.5 * (totalX - cropArea.width);
    pYa = 0.5 * (totalY - cropArea.height);
  }

  bool init = true;

  @override
  Widget build(BuildContext context) {
    return STPage(
      page: () => Scaffold(
        appBar: AppBar(
          title: const Text('Crop Photo'),
          centerTitle: true,
          leading: IconButton(
            onPressed: _cropImage,
            tooltip: 'Crop',
            icon: const Icon(Icons.crop),
          ),
          actions: [
            ElevatedButton(
              onPressed: () => Navigator.pop(context, null),
              child: const Text('Cancel'),
            )
          ],
        ),
        body: Column(
          children: <Widget>[
            Expanded(
              child: FutureBuilder(
                future: _decoded.future,
                builder: (ctx, snap) {
                  if (!snap.hasData) {
                    return const Center(
                      child: Text('Loading...'),
                    );
                  }
                  return LayoutBuilder(
                    builder: (ctx, cstr) {
                      if (init) {
                        cropPad = cstr.maxHeight * 0.05;
                        double tmpWidth = cstr.maxWidth - 2 * cropPad;
                        double tmpHeight = cstr.maxHeight - 2 * cropPad;
                        cropArea = (tmpWidth / cropRatio > tmpHeight) ? Size(tmpHeight * cropRatio, tmpHeight) : Size(tmpWidth, tmpWidth / cropRatio);
                        zeroScale = cropArea.height / imgHeight;
                        computeRelativeDim(scale);
                        init = false;
                      }
                      return GestureDetector(
                        onPanUpdate: (pan) {
                          double dy;
                          double dx;
                          if (pan.delta.dy > 0) {
                            dy = min(pan.delta.dy, pYa - offset.dy);
                          } else {
                            dy = max(pan.delta.dy, -pYa - offset.dy);
                          }
                          if (pan.delta.dx > 0) {
                            dx = min(pan.delta.dx, pXa - offset.dx);
                          } else {
                            dx = max(pan.delta.dx, -pXa - offset.dx);
                          }
                          setState(() => offset += Offset(dx, dy));
                        },
                        child: Stack(
                          children: [
                            Container(
                              color: Colors.black.withOpacity(0.5),
                              height: cstr.maxHeight,
                              width: cstr.maxWidth,
                              child: ClipRect(
                                child: Container(
                                  alignment: Alignment.center,
                                  height: cropArea.height,
                                  width: cropArea.width,
                                  child: Transform.translate(
                                    offset: offset,
                                    child: Transform.scale(
                                      scale: scale * zeroScale,
                                      child: OverflowBox(
                                        maxWidth: imgWidth,
                                        maxHeight: imgHeight,
                                        child: Image.memory(
                                          widget.image,
                                        ),
                                      ),
                                    ),
                                  ),
                                ),
                              ),
                            ),
                            IgnorePointer(
                              child: Center(
                                child: Container(
                                  height: cropArea.height,
                                  width: cropArea.width,
                                  decoration: BoxDecoration(
                                    border: Border.all(color: Colors.white, width: 2),
                                  ),
                                ),
                              ),
                            ),
                          ],
                        ),
                      );
                    },
                  );
                },
              ),
            ),
            Row(
              children: <Widget>[
                const Text('Scale:'),
                Expanded(
                  child: SliderTheme(
                    data: Theme.of(context).sliderTheme,
                    child: Slider(
                      divisions: 50,
                      value: scale,
                      min: 1,
                      max: 2,
                      label: '$scale',
                      onChanged: (n) {
                        double dy;
                        double dx;
                        computeRelativeDim(n);
                        dy = (offset.dy > 0) ? min(offset.dy, pYa) : max(offset.dy, -pYa);
                        dx = (offset.dx > 0) ? min(offset.dx, pXa) : max(offset.dx, -pXa);
                        setState(() {
                          offset = Offset(dx, dy);
                          scale = n;
                        });
                      },
                    ),
                  ),
                ),
              ],
            ),
          ],
        ),
      ),
    );
  }
}
