import 'package:flutter/material.dart';
import 'package:solidtrade/components/base/st_page.dart';
import 'package:solidtrade/components/base/st_widget.dart';
import 'package:solidtrade/data/dtos/user/request/update_user_dto.dart';
import 'package:solidtrade/services/util/util.dart';

class ProfileSettingsPage extends StatefulWidget {
  const ProfileSettingsPage({Key? key, required this.updateUserDto}) : super(key: key);
  final UpdateUserDto updateUserDto;

  @override
  State<ProfileSettingsPage> createState() => _ProfileSettingsPageState();
}

class _ProfileSettingsPageState extends State<ProfileSettingsPage> with STWidget {
  late bool _hasPublicPortfolio;
  late UpdateUserDto _userDto;

  // Controllers
  TextEditingController bioController = TextEditingController();
  TextEditingController displayNameController = TextEditingController();
  TextEditingController emailController = TextEditingController();
  TextEditingController usernameController = TextEditingController();

  bool get _hasUpdatedProfile => _userDto.hasUpdatedProfileByDto(widget.updateUserDto);

  @override
  void initState() {
    super.initState();

    _userDto = UpdateUserDto.copyWith(widget.updateUserDto);

    _hasPublicPortfolio = _userDto.publicPortfolio;

    bioController.text = _userDto.bio;
    displayNameController.text = _userDto.displayName;
    emailController.text = _userDto.email;
    usernameController.text = _userDto.username;
  }

  void _bioChanged(String bio) => _userDto.bio = bio;
  void _emailChanged(String email) => _userDto.email = email;
  void _displayNameChanged(String displayName) => _userDto.displayName = displayName;
  void _usernameChanged(String username) => _userDto.username = username;
  void _hasPublicPortfolioChanged(bool hasPublicPortfolio) {
    _userDto.publicPortfolio = hasPublicPortfolio;

    setState(() {
      _hasPublicPortfolio = hasPublicPortfolio;
    });
  }

  // TODO: Maybe use the same text inputs, used in the sign up process, in the future...
  // TextFormField(
  //     controller: _nameController,
  //     cursorColor: colors.foreground,
  //     decoration: getInputDecoration("Name"),
  //   ),

  Widget _niceInputField({
    required String title,
    required TextEditingController controller,
    required Function(String) callback,
    required String hintText,
    int maxLines = 1,
  }) {
    return Container(
      padding: const EdgeInsets.only(left: 20, right: 20),
      width: double.infinity,
      height: maxLines == 1 ? 90 : 200,
      child: Column(
        mainAxisSize: MainAxisSize.max,
        mainAxisAlignment: MainAxisAlignment.start,
        crossAxisAlignment: CrossAxisAlignment.start,
        children: <Widget>[
          const SizedBox(height: 10),
          TextFormField(
            maxLines: maxLines,
            onChanged: (content) => callback(content),
            controller: controller,
            decoration: InputDecoration(
              border: const OutlineInputBorder(),
              hintText: hintText,
              labelText: title,
            ),
          ),
        ],
      ),
    );
  }

  void _handleClickDiscard() async {
    if (!_hasUpdatedProfile || await Util.showUnsavedChangesWarningDialog(context)) {
      Navigator.of(context).pop(null);
    }
  }

  @override
  Widget build(BuildContext context) {
    return STPage(
      page: () => Scaffold(
        appBar: AppBar(
          leading: IconButton(
            icon: Icon(Icons.close, size: 25, color: Colors.red[300]),
            onPressed: _handleClickDiscard,
          ),
          title: const Text("Customize your Profile"),
          elevation: 5,
          centerTitle: true,
          backgroundColor: colors.background,
          foregroundColor: colors.foreground,
          actions: <Widget>[
            IconButton(
              icon: Icon(Icons.done, size: 25, color: Colors.blue[300]),
              onPressed: () => Navigator.of(context).pop(_userDto),
            )
          ],
        ),
        body: SingleChildScrollView(
          child: Column(
            children: <Widget>[
              const SizedBox(height: 15),
              // Text(
              //   "Customize your Profile",
              //   textAlign: TextAlign.center,
              //   style: TextStyle(
              //     color: alertColor,
              //     fontSize: 22.5,
              //   ),
              // ),
              const SizedBox(height: 15),
              _niceInputField(title: 'Bio', controller: bioController, callback: (content) => _bioChanged(content), maxLines: 5, hintText: bioController.text),
              _niceInputField(title: 'Email', controller: emailController, callback: (content) => _emailChanged(content), hintText: emailController.text),
              _niceInputField(title: 'Name', controller: displayNameController, callback: (content) => _displayNameChanged(content), hintText: displayNameController.text),
              _niceInputField(title: 'Username', controller: usernameController, callback: (content) => _usernameChanged(content), hintText: '@' + usernameController.text),
              SwitchListTile(title: const Text("Public portfolio"), value: _hasPublicPortfolio, onChanged: _hasPublicPortfolioChanged),
              const SizedBox(height: 15),
            ],
          ),
        ),
      ),
    );
  }
}
