# Decisions Context

Record important project decisions here.

## Decision log

| Date | Decision | Reason | Impact | Source |
| --- | --- | --- | --- | --- |
| 2026-06-27 | Build an Android app in .NET MAUI. | Required by the approved request. | Defines the client stack. | Human approval |
| 2026-06-27 | Use direct-to-OpenAI requests with no backend. | Keeps the demo simple and aligned with scope. | Client must handle the request flow directly. | Human approval |
| 2026-06-27 | Use voice-first interaction in Brazilian Portuguese. | Accessibility requirement for visually impaired users. | TTS becomes a core part of the UX. | Human approval |
| 2026-06-27 | Keep local read-only history with clear-all. | Supports review of prior captures without editing complexity. | Requires local storage and delete-all support. | Human approval |
| 2026-06-27 | Implement local history as a JSON file for the MVP. | Keeps the client dependency-light and avoids extra storage risk. | History persists locally and remains read-only. | Tech Lead implementation |
| 2026-06-27 | Store the OpenAI API key in local `secrets.local.json` for development. | Avoids hardcoding the key in the APK while keeping the setup simple. | The app reads the key from the packaged local secrets file. | Tech Lead implementation |
| 2026-06-29 | Keep the native Android camera as an explicit fallback capture path. | Improves compatibility when OTG is unavailable. | App UI now labels the native camera path clearly. | Tech Lead implementation |
| 2026-06-30 | Show camera source/status in a main-screen preview panel. | Improves emulator usability and makes the active source obvious. | UI now exposes active source and fallback status. | Tech Lead implementation |
| 2026-07-01 | User camera selection has precedence over the older OTG-first default. | The approved camera-selection spec lets the user choose front, rear, or OTG and requires fallback only when the selected source is unavailable. | Preview and capture must follow the selected source; fallback must be visible and deterministic. | Approved camera-selection spec |
| 2026-07-03 | Split real OTG/UVC AUSBC work into Wave 2 tasks. | Android Camera2 did not expose the target USB camera, and the earlier AUSBC task was too broad for direct execution. | AUSBC binding, USB session, preview/capture routing, fallback/security, and validation now have separate tasks. | Tech Lead planning |

## Pending setup decisions

- Model routing policy.
- Developer environment/bootstrap instructions.

## Additional decisions

- 2026-06-27: Repository platform set to GitHub. Source: human approval.
- 2026-06-27: AI platform set to OpenAI. Source: human approval.
- 2026-06-27: Model routing will be defined later in technical planning. Source: human approval.
- 2026-06-27: Android trigger handling includes supported keyboard/remote key events. Source: implementation.

## Superseded decisions

Move or mark decisions here when they are replaced by newer decisions.

- 2026-06-29: `Try OTG/USB camera first, then fall back to native camera` is superseded by the 2026-07-01 camera-selection decision. OTG remains supported and may be selected, but user selection now has precedence and fallback is used only when the selected source is unavailable.
