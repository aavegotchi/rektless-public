# Contributing

Contributions are welcome. Please keep changes focused, documented, and easy to
review.

## Contribution License

By contributing to this repository, you agree that your contribution is licensed
under the same license as the part of the project you modify:

- Code, scripts, editor tooling, build tooling, and documentation:
  Apache-2.0, unless otherwise noted.
- Original project assets: governed by `ASSET_LICENSE.md`, unless otherwise
  noted.
- Third-party files: governed by their existing upstream licenses.

Do not contribute code, assets, audio, fonts, or other content unless you have
the right to license it under the applicable terms.

## Developer Certificate of Origin

By submitting a contribution, you certify that:

1. You wrote the contribution yourself, or you have the right to submit it under
   the applicable project license.
2. You understand that the contribution and project history may be public.
3. You are not knowingly submitting confidential information, private keys,
   passwords, API keys, tokens, or other secrets.

For asset contributions, include the source, owner, and license or permission
details in the pull request.

## Secrets

Do not commit secrets. Client-side Unity scenes and project settings must not
contain:

- PlayFab developer secret keys or developer account tokens
- Thirdweb server or private keys
- Wallet private keys, seed phrases, or signing credentials
- Deployment tokens, cloud credentials, or webhook secrets

PlayFab `DeveloperSecretKey` must remain empty in this client project.

## Pull Requests

- Keep PRs scoped to one concern.
- Explain what changed and why.
- Include testing notes.
- Preserve third-party license files and notices.
- Do not reformat generated Unity YAML unless the change requires it.
