# Third-Party Notices

This repository includes third-party software, SDKs, Unity packages, fonts, and
tools. They remain under their respective licenses and terms. This file is a
best-effort notice index and does not replace the license text included with
each dependency.

## Unity and Unity Packages

Unity Editor, Unity runtime modules, and Unity packages are governed by Unity's
terms and package licenses.

Package references are listed in:

- `Packages/manifest.json`
- `Packages/packages-lock.json`

## PlayFab Unity SDK

Location:

- `Assets/PlayFabSDK/`

PlayFab is a Microsoft service and SDK. The SDK and service usage are governed
by their respective Microsoft and PlayFab terms and notices.

This repository is intended to use PlayFab client APIs only. Do not commit
PlayFab developer secret keys or developer account tokens.

## Thirdweb Unity SDK and Related Libraries

Location:

- `Assets/Thirdweb/`

This directory includes Thirdweb Unity SDK files and bundled dependencies such
as wallet, WebGL, cryptography, JSON, and Ethereum-related libraries. Retain any
license files and notices included by upstream packages when redistributing.

## DOTween

Location:

- `Assets/Plugins/Demigiant/DOTween/`

DOTween is distributed by Demigiant. See the files included in that directory
and Demigiant's published terms for the applicable license.

## Build Report Tool

Location:

- `Assets/BuildReport/`

The Build Report Tool includes its own notice file:

- `Assets/BuildReport/license.txt`

It also includes:

- FuzzyString, licensed under the Eclipse Public License 1.0
- MiniJSON, licensed under the MIT License

## Asset Usage Detector

Location:

- `Assets/Plugins/AssetUsageDetector/`

See:

- `Assets/Plugins/AssetUsageDetector/README.txt`

Verify the applicable upstream license before redistributing or modifying this
plugin.

## TextMesh Pro and Liberation Sans

Location:

- `Assets/TextMesh Pro/`

Liberation Sans is included with an Open Font License notice:

- `Assets/TextMesh Pro/Fonts/LiberationSans - OFL.txt`

TextMesh Pro package files are governed by Unity package terms.

## Project Fonts

Locations include:

- `Assets/Fonts/PixelEmulator-xq08.ttf`
- `Assets/Fonts/EndlessBossBattleRegular-v7Ey.ttf`

Verify the source and license for each font before making the repository public
or redistributing builds.

## Project Audio

Locations include:

- `Assets/Sounds/`

Verify the source and license for each audio file before making the repository
public or redistributing builds.

## Project Art and Media

Locations include:

- `Assets/Textures/`
- `Assets/Anims/`
- `Assets/Resources/`
- `Assets/Scenes/`

Original project assets are governed by `ASSET_LICENSE.md`. Third-party or
community-provided assets remain governed by their own rights and permissions.

## Missing or Unverified Notices

Some bundled assets and plugins may not include complete license metadata in the
repository. Before switching this repository to public, review and verify every
third-party SDK, plugin, font, audio file, texture, sprite, and other media file
that will be redistributed.
