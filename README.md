# Corex UO

[![GitHub build](https://img.shields.io/github/workflow/status/corexuo/CorexUO/Build?logo=github)](https://github.com/CorexUO/CorexUO/actions)
[![GitHub issues](https://img.shields.io/github/issues/corexuo/corexuo.svg)](https://github.com/CorexUO/CorexUO/issues)
[![GitHub last commit](https://img.shields.io/github/last-commit/CorexUO/CorexUO.svg)](https://github.com/CorexUO/CorexUO/)
[![Github code lines](https://img.shields.io/tokei/lines/github/CorexUO/CorexUO.svg)](https://github.com/CorexUO/CorexUO/)
[![GitHub repo size](https://img.shields.io/github/repo-size/CorexUO/CorexUO.svg)](https://github.com/CorexUO/CorexUO/)


CorexUO is an Ultima Online server emulator based on RunUO [https://github.com/runuo/runuo]

## .Net 5 Support
Change the core to work with .Net Core framework.

## Requirements to Compile
- [.NET 5 SDK](https://dotnet.microsoft.com/download/dotnet/5.0)


### Requirements for run in Ubuntu/Debian

```shell
apt-get install zlib1g-dev
```

## Run

```shell
dotnet run Binaries/CorexUO.dll
```

### Binaries

New folder where all dlls are generated and required files are exported. Data folder exported to the new Binaries folder.

### Misc

CorexUO supports Intel's hardware random number generator (Secure Key, Bull Mountain, rdrand, etc).
If rdrand32.dll/rdrand64.dll are present in the base directory and the hardware supports that functionality, it will be used automatically. You can find those libraries here: https://github.com/msturgill/rdrand/releases/latest

### Thanks

- RunUO Community
- ServUO Community
