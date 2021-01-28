# Soundboard
WPF Based audio soudboard for a virtual audio cable

The app runs on .NET 5, runtime is required to use it, SDK required to build it.

## Usage
The app will list all playback devices on your computer.

It will also scan for audio files in the sub-folder Files next to the Prism.Soundboard.exe executable. At first run if the folder does not exist it will create it. For now, it will not scan the folder other than at application startup.

Simply select the file you want to play and the audio playback device and hit play. The audio file will play on the selected device.
