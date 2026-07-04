# Architecture Context

## Architecture overview

Single Android .NET MAUI client app. It captures an image from a scene source, reads GPS coordinates from the device, sends both directly to OpenAI, and plays the response through text-to-speech while also showing it on screen. Capture and preview share a camera-source coordinator so the user's selected camera source has precedence and fallback is explicit when that source is unavailable.

## Module or component boundaries

- Capture/input handling
- Camera integration
- Location retrieval
- OpenAI request/response handling
- Voice output and UI presentation
- Local history storage

## Current implementation notes

- The app uses a core library for orchestration, models, and history storage.
- Local read-only history is stored in a device file as JSON for the current MVP implementation.
- The Android trigger path listens for supported keyboard/remote key events and dispatches the capture flow.
- OTG camera capture is isolated behind USB-specific Android services and a core source coordinator.
- The main screen includes a preview panel that shows selected source, active camera source, and fallback status.
- Real OTG/UVC support is planned through an AUSBC .NET Android binding because the target Android 11 device did not expose the USB camera via Camera2.

## Dependency rules

- The app depends on Android device capabilities for camera, GPS, and Bluetooth input.
- The app depends on OpenAI as an external service.
- No backend dependency is allowed.
- Camera preview and still capture must use the same active source selected by the camera-source coordinator.
- AUSBC Java/Kotlin/vendor types must remain isolated to Android platform code; shared code should depend on interfaces and DTOs.

## Architecture decisions source

Track source references when available, such as specs, ADRs, issues or Tech Lead decisions.
