# Corex UO

[![GitHub build](https://img.shields.io/github/actions/workflow/status/CorexUO/CorexUO/build.yml?logo=github)](https://github.com/CorexUO/CorexUO/actions)
[![GitHub issues](https://img.shields.io/github/issues/corexuo/corexuo.svg)](https://github.com/CorexUO/CorexUO/issues)
[![GitHub last commit](https://img.shields.io/github/last-commit/CorexUO/CorexUO.svg)](https://github.com/CorexUO/CorexUO/)
[![GitHub repo size](https://img.shields.io/github/repo-size/CorexUO/CorexUO.svg)](https://github.com/CorexUO/CorexUO/)

CorexUO is an Ultima Online server emulator based on RunUO [https://github.com/runuo/runuo]
The main objective of CorexUO is to update the server code to new technologies, improve the performance and give support for better customization.

For change Era configuration and some basic server configuration you can use the ```bin\settings.ini``` file
## Era Support
```
- T2A: Some mechanics
- UOR: Pub 16 acurrate
- AOS: Mostly completed
- ML: Few things
- Newests Era: Without updates
```

## .Net 8 Support
Change the core to work with .Net 8 framework.

## Requirements to Compile
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)


### Requirements for run in Ubuntu/Debian

```shell
apt-get install zlib1g-dev
```

## Run

```shell
dotnet run bin/CorexUO.dll
```

### Misc

CorexUO supports Intel's hardware random number generator (Secure Key, Bull Mountain, rdrand, etc).
If rdrand32.dll/rdrand64.dll are present in the base directory and the hardware supports that functionality, it will be used automatically. You can find those libraries here: https://github.com/msturgill/rdrand/releases/latest

### Thanks

- RunUO Community
- ServUO Community
