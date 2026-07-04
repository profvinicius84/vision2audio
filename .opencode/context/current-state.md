# Current State Context

Record the current known state of the project here.

## Current implementation state

- MAUI Android solution scaffold created.
- Core scene-capture, location, OpenAI, speech, and local history flow implemented.
- Camera selection, preview panel, and camera-source coordination are implemented as a shared source/status flow.
- User camera selection now supersedes the older OTG-first default; fallback is expected only when the selected source is unavailable.
- The product requirement for Vision 2 Audio is approved and documented.

## Known gaps

- Model routing policy remains a deployment/ops decision.
- Real OTG camera hardware validation on Android 11 is still pending and was not executable from the 2026-07-03 validation environment.
- Real OTG/UVC frame decoding and preview/capture routing through AUSBC is implemented behind the Android platform boundary but remains pending acceptance until device validation succeeds.
- The `Failed resolution of: Lcom/jiangdg/usb/USBMonitor;` packaging blocker was resolved by adding compatible AndroidUSBCamera 3.2.9 dependency AARs from `AndroidUSBCamera-3.3.3.zip` and preferring them in the binding project.
- A later device run emitted an unhelpful `System.InvalidOperationException` without message/stack; targeted sanitized diagnostics/error handling were added around OTG/AUSBC preview, capture, cleanup, and lifecycle fire-and-forget tasks so the next manual run should expose `[Diagnostics]` lines without logging secrets, USB serials, image payloads, GPS, or local paths.
- Android 11 OTG capture then reported `Prévia OTG/AUSBC indisponível para captura.`; root cause was capture depending on `TextureView.Bitmap`. OTG capture now uses AUSBC `CameraUVC.CaptureImage` to an app-private cache JPEG with best-effort cleanup and timeout.
- Local OpenAI secret loading now supports both `secrets.local.json` and `secrets.local` in Debug/explicit local packaging while Release APK inspection confirms these secrets are not packaged by default.
- `AndroidUSBCamera-3.6.0.zip` was added by the human but has not yet replaced the current compatible `3.2.9` AAR set; it remains a follow-up candidate if AUSBC capture timeout persists.
- Priority UX follow-up implemented: Android Volume Up now triggers capture for the physical/remote path, capture flow prevents duplicate concurrent runs and permits subsequent captures, successful capture freezes the still image while pausing live preview, main screen is scrollable, and clutter labels/title were removed from the visible UI.
- App polish/accessibility follow-up implemented: `logo.jpeg` is configured as the app icon, AI loading indicator appears only during the OpenAI request, live preview resumes after audio finishes, trigger during audio interrupts TTS and returns the app to ready state, and the OpenAI prompt always includes location context plus visually-impaired navigation guidance.
- Location prompt correction implemented: GPS is reverse-geocoded to a human-readable approximate address/place before building the OpenAI prompt; raw latitude/longitude should not be exposed in user-facing descriptions.
- Prompt wording now explicitly asks the AI to translate/convert available location context into natural address/place language integrated with the image description, without speaking raw latitude/longitude.
- Repository platform is GitHub; task-management platform remains pending decision.
- OpenAI API key must be supplied in local `secrets.local.json` before runtime calls can succeed.
- Native Android camera is the explicit fallback capture path in the current implementation.
- The preview panel shows active source and status, and uses the same source-selection coordinator as capture.
- Release APK publish blocker for generated AUSBC/Xlog formatter interface errors was resolved by excluding the dependency-only Xlog managed API surface in binding metadata; Release publish now succeeds, but Android 16 native library page-size warnings remain.

## Active work

- Implementation and validation are underway.
- Wave 2 camera-source/AUSBC hardening is ready for execution planning handoff.
- OTG camera integration needs AUSBC implementation and Android 11 device validation.
- Task `vision2audio-2.6` validation found Debug/Release builds and core tests passing after the Xlog metadata fix, but manual device/emulator validation remains blocked/deferred.

## Recently completed work

- Approved product discussion captured.
- Spec saved to `docs/superpowers/specs/2026-06-27-vision-2-audio-design.md`.
- Wave plan and task files saved under `docs/superpowers/`.
- OTG camera integration plan saved under `docs/superpowers/plans/`.
- Wave 2 camera-source/AUSBC hardening plan saved to `docs/superpowers/waves/2026-07-03-vision-2-audio-wave-2-camera-source-hardening.md`.

## Notes for agents

Use this section for short operational notes that help agents avoid stale assumptions.

- Vision 2 Audio product scope is approved.
- Model routing remains pending, but current Wave 2 camera-source hardening can proceed because it does not change OpenAI model routing.
- Do not treat real OTG/UVC support as accepted until Android 11 hardware validation evidence is recorded or explicitly deferred.
- Wave 2 Release packaging command now succeeds after the Xlog metadata fix, but do not treat real OTG/UVC behavior as accepted until Android 11 hardware and emulator fallback validation evidence is recorded.
