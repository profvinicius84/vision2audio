# AUSBC artifacts

Place AndroidUSBCamera/AUSBC build artifacts here before enabling the binding:

- `libausbc-release.aar`
- optional dependency AARs if built separately:
  - `libuvc-release.aar` (must match `libausbc-release.aar`; for current AUSBC builds this must provide `com/jiangdg/usb/USBMonitor.class`, not only the older `com/serenegiant/usb/USBMonitor.class`)
  - `libnative-release.aar`
  - `libuvc-3.2.9.aar`, `libnative-3.2.9.aar`, `libutils-3.2.9.aar`, and `libuvccommon-3.2.9.aar` extracted from `AndroidUSBCamera-3.3.3.zip` are currently present as the compatible runtime set.

Recommended source for the current bound API: AndroidUSBCamera/AUSBC artifacts that keep the `com.jiangdg.*` package names.

Build or download the AUSBC artifacts, then rebuild `Vision2Audio.AusbcBinding` and inspect generated C# namespaces before wiring direct preview/capture calls. If OTG startup fails with `Failed resolution of: Lcom/jiangdg/usb/USBMonitor;`, replace the UVC dependency artifact with the same AndroidUSBCamera/AUSBC version used to build `libausbc-release.aar`; the runtime class is not supplied by the older Serenegiant-package UVC AAR.

Current note: `Vision2Audio.AusbcBinding.csproj` prefers the `3.2.9` AARs when present and falls back to the older `*-release.aar` artifacts only when the compatible files are absent.

`AndroidUSBCamera-3.6.0.zip` was re-inspected in July 2026. It contains updated source and `libnative/aar/libnative-3.2.9.aar` plus `libuvc/aar/libuvc-3.2.9.aar`, but it does not contain a prebuilt `libausbc-release.aar`. The two packaged AARs are byte-identical to the current files in `external/ausbc/`, so there is no artifact-level migration to apply from the zip alone. Current app binding still depends on `libausbc-release.aar`; a full 3.6.0 migration requires building a matching `libausbc` AAR first. Do not mix a newly built dependency set with the old libausbc unless validating the full class/API set together.

3.6.0 source inspection confirmed `CameraUVC.captureImageInternal(...)` still waits for `mNV21DataQueue`, and that queue is populated only when AUSBC registers `setFrameCallback(...)`. In OpenGL render mode that callback is registered when `CameraRequest.isRawPreviewData` or `isCaptureRawImage` is true. The app therefore requests both raw preview data and capture raw image for OTG sessions so `CaptureImage(...)` has frame data to encode instead of timing out.
