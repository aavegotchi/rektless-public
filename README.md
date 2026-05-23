# Rektless Unity

Rektless Unity is a Unity 6 action game project with wallet onboarding and online progression.

## Highlights

- 2D action gameplay with enemies, bosses, hazards, and collectibles
- Wallet connection flow (Thirdweb/Reown)
- PlayFab integration for authentication, player stats, display names, and leaderboard

## Tech Stack

- Unity `6000.3.10f1`
- PlayFab Unity SDK (client-side usage in this repository)
- Thirdweb Unity SDK and Reown wallet integration

## First-Time Setup

1. Clone the repository.
2. Open the project with Unity Editor `6000.3.10f1`.
3. Allow Unity to restore packages and import assets.
4. Open `Assets/Scenes/menu.unity`.
5. Press Play.

## Backend Configuration

### PlayFab

PlayFab settings are stored in:

- `Assets/PlayFabSDK/Shared/Public/Resources/PlayFabSharedSettings.asset`

Set:

- `TitleId`: your PlayFab title ID
- `DeveloperSecretKey`: keep empty in this client project

Expected statistics used by gameplay:

- `MaxDistance`
- `MaxRekt`
- `MaxGems`

Optional CloudScript function used by exporter utility:

- `exportDataToGoogleSheet`

### Thirdweb

Thirdweb client configuration is currently scene-level through `ThirdwebManager` prefab overrides.

Set your client ID in:

- `Assets/Scenes/menu.unity` (main game flow)

Field to configure:

- `<ClientId>k__BackingField`

Do not put server/private thirdweb keys in this repository or in client scenes.

## Project Entry Points

- Main menu: `Assets/Scenes/menu.unity`
- Gameplay: `Assets/Scenes/level1.unity`

## License

Code in this repository is licensed under Apache-2.0 unless otherwise noted.
See `LICENSE` and `NOTICE`.

Game assets, including artwork, audio, logos, characters, names, UI art,
animations, textures, sprites, level art, and branding, are licensed separately.
See `ASSET_LICENSE.md`.

Third-party dependencies and bundled assets remain under their respective
licenses and terms. See `THIRD_PARTY_NOTICES.md` and any license files included
with those dependencies.

By contributing, you agree that your contributions are licensed under the same
terms as the part of the project you modify. See `CONTRIBUTING.md`.
