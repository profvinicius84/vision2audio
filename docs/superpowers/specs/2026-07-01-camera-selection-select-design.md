# Specification: Vision 2 Audio Camera Selection Select

## Metadata

- Spec ID: `2026-07-01-camera-selection-select-design`
- Status: `approved`
- Owner: `Product Owner / Human stakeholder`
- Created: `2026-07-01`
- Updated: `2026-07-01`
- Source: `Approved request captured in chat`

## Human approval

- Approved by: `Human stakeholder`
- Approval date: `2026-07-01`
- Approval notes: `Add a camera selection control so the user can choose front, rear, or OTG. Remember the last choice and restore it on next app start, but fall back automatically if the selected camera is unavailable.`

## Problem

Users need explicit control over which camera source is used, especially when the emulator cannot use OTG and when different devices expose different camera availability.

## Goal

Provide a camera selection control that lets the user choose front, rear, or OTG, persists the last selection, and automatically falls back to another available camera when the preferred source cannot be opened.

## Users or systems affected

- Android end users
- Emulator users during development and QA
- Physical Android 11 device users
- Front/rear device cameras
- OTG/USB camera hardware

## Product value

The selection control gives the user direct control and makes the app usable across emulator and physical-device scenarios without forcing a single camera path.

## Scope

### In scope

- Camera selection control in the main screen
- User choice among front, rear, and OTG
- Persisting the last selected camera source locally
- Restoring the saved choice at startup
- Automatic fallback when the selected source is unavailable
- Keeping preview and capture synchronized with the selected source

### Out of scope

- Separate full-screen camera settings screen
- Manual camera calibration
- Video recording
- Gallery management
- Social sharing

## Acceptance criteria

- Given the app is open on Android
  When the user views the main screen
  Then the app shows a camera selection control with front, rear, and OTG options.

- Given the user selects a camera source
  When the app stores the preference
  Then the selected source is restored the next time the app starts.

- Given the selected camera source is unavailable
  When the app starts or the source is opened
  Then the app automatically falls back to another available source and shows the fallback clearly.

- Given the user changes the selected source
  When the selection changes
  Then the preview and capture source update to match the new selection.

- Given the app is running on an emulator without OTG support
  When the user selects OTG
  Then the app clearly falls back to a supported camera source instead of failing silently.

## Business rules

- The last chosen camera source is remembered locally.
- OTG is preferred only when selected and available.
- Preview and capture must use the same camera source.
- The UI must indicate when a fallback source is in use.

## Constraints

- Android only
- Implemented in .NET MAUI
- Must not break the current GPS → OpenAI → TTS → history flow
- Must remain usable in the emulator even without OTG hardware

## Assumptions

- The app already has a camera-source coordinator that can be extended to honor a user-selected preferred source.
- Local persistence already exists and can store the selected camera source.
- The UI can expose the selected source without introducing a separate settings page.

## Open questions

- None.

## Risks

- Different devices may expose different camera IDs or availability.
- The emulator may not support OTG at all, requiring fallback behavior.
- Persisted preferences may become stale if the hardware configuration changes.

## Non-functional requirements

- Performance: source switching should feel immediate.
- Accessibility: selection and fallback status must be readable and announced clearly.
- Reliability: invalid or unavailable selections must degrade gracefully.
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
