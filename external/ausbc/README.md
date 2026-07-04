# AUSBC artifacts

Place AndroidUSBCamera/AUSBC build artifacts here before enabling the binding:

- `libausbc-release.aar`
- optional dependency AARs if built separately:
  - `libuvc-release.aar` (must match `libausbc-release.aar`; for current AUSBC builds this must provide `com/jiangdg/usb/USBMonitor.class`, not only the older `com/serenegiant/usb/USBMonitor.class`)
  - `libnative-release.aar`
  - `libuvc-3.2.9.aar`, `libnative-3.2.9.aar`, `libutils-3.2.9.aar`, and `libuvccommon-3.2.9.aar` extracted from `AndroidUSBCamera-3.3.3.zip` are currently present as the compatible runtime set.

Recommended source: https://github.com/jiangdongguo/AndroidUSBCamera

Build or download the AUSBC artifacts, then rebuild `Vision2Audio.AusbcBinding` and inspect generated C# namespaces before wiring direct preview/capture calls. If OTG startup fails with `Failed resolution of: Lcom/jiangdg/usb/USBMonitor;`, replace the UVC dependency artifact with the same AndroidUSBCamera/AUSBC version used to build `libausbc-release.aar`; the runtime class is not supplied by the older Serenegiant-package UVC AAR.

Current note: `Vision2Audio.AusbcBinding.csproj` prefers the `3.2.9` AARs when present and falls back to the older `*-release.aar` artifacts only when the compatible files are absent.
