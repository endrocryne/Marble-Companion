# Marble Companion

A .NET MAUI mobile app for tracking sustainable habits and monitoring your environmental impact.

## 📱 Download the Android APK

The latest APK is always available on the [**Releases page**](../../releases/latest).

### How to install on your Pixel 10
1. Go to [**Releases → Latest**](../../releases/latest) and download `MarbleCompanion.apk`.
2. Transfer the file to your Pixel 10 (via USB, email, or Google Drive).
3. On your phone: **Settings → Apps → Special app access → Install unknown apps**, then allow installs from your chosen source.
4. Open the APK file on your phone and tap **Install**.

> The APK is built automatically from the `main` branch via GitHub Actions every time code is merged.

---

## Development

### Prerequisites
- .NET 8 SDK
- .NET MAUI workloads (`dotnet workload install maui-android`)
- Android SDK

### Build
```bash
# Restore dependencies
dotnet restore MarbleCompanion.Mobile/MarbleCompanion.Mobile.csproj -f net8.0-android

# Build the Android APK
dotnet publish MarbleCompanion.Mobile/MarbleCompanion.Mobile.csproj \
  -f net8.0-android -c Release
```

> **Note:** `MarbleCompanion.Mobile/google-services.json` contains a placeholder Firebase configuration.  
> Replace `PLACEHOLDER_REPLACE_WITH_REAL_API_KEY` with real credentials from the [Firebase console](https://console.firebase.google.com/) before enabling push notifications.

### Run tests
```bash
dotnet test MarbleCompanion.Tests/MarbleCompanion.Tests.csproj -c Release
```
