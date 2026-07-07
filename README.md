# Soundboard
WPF Based audio soudboard for a virtual audio cable

The app runs on .NET 10, complete runtime is required to use it, SDK required to build it.

## Usage
The app will list all playback devices on your computer.

It will also scan for audio files in the sub-folder Files next to the Prism.Soundboard.exe executable. At first run if the folder does not exist it will create it. For now, it will not scan the folder other than at application startup.

Simply select the file you want to play and the audio playback device and hit play. The audio file will play on the selected device.

## Web UI

The app also serves a Blazor web interface so the soundboard can be controlled from another device (phone, tablet, another PC) on the same network. Browse to `http://<pc-ip>:5010` while the app is running. The buttons on the page play the same audio files through the selected playback device.

A small HTTP control API is served on the same host/port for external integrations (e.g. Stream Deck, scripts):

- `GET /files` — JSON map of sound name to file path
- `POST /play` — body is a JSON string with the sound name to play
- `GET /play` — plays the currently selected file
- `GET /ping`, `GET /isalive` — health checks

## Architecture

`Prism.Soundboard` is the WPF desktop app. `Prism.Soundboard.Web` is a Razor Class Library holding the Blazor UI. The WPF app hosts the Blazor Server UI **in-process**, inside the same ASP.NET Core host it uses for the control API, listening on `0.0.0.0:5010`. Playback requests coming from the web UI are marshalled onto the WPF UI thread and run through the same audio pipeline as the desktop buttons (`WebAudioService` bridges the two).

## Building and publishing

- Build: `dotnet build Prism.Soundboard/Prism.Soundboard.sln -c Debug`
- Publish: `dotnet publish Prism.Soundboard/Prism.Soundboard/Prism.Soundboard.csproj -c Release -o <output-dir>`, then ship the whole output folder and run `Prism.Soundboard.exe`.

## Quirks and findings

Hosting a Blazor Server UI in-process from a WPF (`WinExe`) app is not a supported project template, so a few non-obvious things were needed. If you touch the project files, keep these in mind:

- **Both projects use the Razor SDK** (`Microsoft.NET.Sdk.Razor`); the WPF host also sets `UseWPF=true`. Do **not** switch the host to the Web SDK (`Microsoft.NET.Sdk.Web`) — it fails to generate the WPF `Main` entry point on a clean build.
- **`_framework/blazor.web.js` is not a file on disk** — it ships in the `Microsoft.AspNetCore.App.Internal.Assets` NuGet package. Its built-in registration target only runs for `OutputType=Exe` under the Web SDK, so the host references that package and re-registers the file as a static web asset itself (the `_AddHostBlazorFrameworkStaticWebAssets` target in `Prism.Soundboard.csproj`). That package version must match the ASP.NET Core runtime version (currently `10.0.6`); bump it when the runtime is updated.
- **`MapStaticAssets` needs the static-web-assets file provider**, which `WebApplication.CreateBuilder()` only wires up automatically in the Development environment. The host calls `builder.WebHost.UseStaticWebAssets()` explicitly so assets resolve both when running from `bin` (dev) and when published (where the assets are copied into `wwwroot`). Without it, every asset returns HTTP 200 with an empty body — the symptom is a page with no CSS and no working Blazor connection.
- **`stylecop.json` is removed from `Content`** (kept as `AdditionalFiles`) in both projects. The SDK's default content glob would otherwise copy it to the output of both projects and they collide at the publish root (`NETSDK1152`).
- A `*.g.cs not found` build error only shows up when a project's `obj`/`bin` are left stale after changing its SDK. Delete both projects' `obj` and `bin` and a single clean build/publish succeeds.
