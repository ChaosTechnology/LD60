# Goblin Island

Submission for [Ludum Dare 60](https://ldjam.com/events/ludum-dare/60).

## Building and running

Supported platforms are Windows and Linux (experimental).

Windows builds are available on the [releases page](https://github.com/ChaosTechnology/LD60/releases/).

Requirements:
- [dotnet 8.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) or higher
- An OpenAL installation:
  - [Download for Windows](https://www.openal.org/downloads/)
  - for Linux, install via your package manager, for example:
  ```
  sudo apt install libopenal-dev
  ```

Then run the folliwing in a bash (git bash for Windows supported):

```
git clone --recurse-submodules https://github.com/ChaosTechnology/LD60.git
bash ./LD60/run.sh
```