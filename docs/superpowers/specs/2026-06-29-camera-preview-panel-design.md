# Specification: Vision 2 Audio Camera Preview Panel

## Metadata

- Spec ID: `2026-06-29-camera-preview-panel-design`
- Status: `approved`
- Owner: `Product Owner / Human stakeholder`
- Created: `2026-06-29`
- Updated: `2026-06-29`
- Source: `Approved request captured in chat`

## Human approval

- Approved by: `Human stakeholder`
- Approval date: `2026-06-29`
- Approval notes: `Add a preview panel on the main screen that shows the active camera source. OTG/USB should be tried first, native camera is fallback, and the preview must reflect the source that will be captured.`

## Problem

The user needs immediate visual confirmation of the active camera source and framing before triggering capture, especially when testing on the emulator or when OTG hardware is not available.

## Goal

Provide a live camera preview panel inside the main screen that shows the current active source (OTG/USB first, native fallback) and keeps the preview aligned with the capture source.

## Users or systems affected

- Android end users
- Emulator users during development and QA
- OTG/USB camera hardware
- Native Android camera fallback

## Product value

The preview panel reduces uncertainty, improves testability, and helps users confirm what will be captured before they press the capture button.

## Scope

### In scope

- Main-screen live preview panel
- Visible indicator of the active camera source
- OTG/USB camera preview as the preferred source
- Native camera preview as fallback
- Preview and capture using the same selected source
- Clear status when the source changes or cannot be opened

### Out of scope

- Separate full-screen camera app
- Editing or annotating captured images
- Gallery browsing
- Video recording
- Offline scene description generation

## Acceptance criteria

- Given the app is open on Android
  When the camera subsystem starts
  Then the main screen shows a preview panel for the active camera source.

- Given OTG/USB camera hardware is available
  When the camera subsystem starts
  Then the preview panel shows the OTG/USB source first.

- Given OTG/USB camera hardware is unavailable or cannot be opened
  When the camera subsystem starts
  Then the preview panel switches to the native camera fallback and shows that source clearly.

- Given the preview is visible
  When the user presses capture
  Then the app captures from the same source currently shown in the preview.

- Given neither camera source can be opened
  When the camera subsystem starts
  Then the app shows a clear status message explaining that no camera preview is available.

## Business rules

- OTG/USB preview is preferred over native camera preview.
- The preview source and capture source must stay synchronized.
- The UI must clearly show when fallback is in use.

## Constraints

- Android only
- Implemented in .NET MAUI
- Must not break the current GPS → OpenAI → TTS → history flow
- Must remain usable on the emulator even if OTG hardware is absent

## Assumptions

- The emulator may only support native camera preview.
- The active camera source can be represented in the UI without blocking the rest of the app.
- The current capture pipeline can be refactored to share the selected source with preview.

## Open questions

- None.

## Risks

- Android camera APIs and emulator capabilities may vary by device and host.
- Preview/capture synchronization could become inconsistent if source selection is duplicated.
- OTG camera availability may not be detectable on all devices.

## Non-functional requirements

- Performance: preview should start quickly and remain responsive.
- Accessibility: source status and errors must be readable and announced clearly.
- Reliability: fallback behavior must be deterministic.
- Compatibility: Android 11 and emulator usage must remain supported.

## Context updates required

- `context/architecture.md`: `yes`
- `context/current-state.md`: `yes`
- `context/decisions.md`: `yes`
- `context/stack.md`: `yes`

## Planning readiness checklist

- [x] Product intent is approved with the human.
- [x] Scope is clear.
- [x] Out-of-scope items are explicit.
- [x] Acceptance criteria are testable.
- [x] Constraints are documented.
- [x] Open questions do not block planning.
