# TaraTools
TaraTools is a small and simple command line utility for packing/unpacking ```*.tara``` files (ported to C# from https://github.com/TheEntropyShard/TaraTool). <br>
Tara file is a simple archive format created by Alternativa Games. It was used to create prop libraries for Tanki Online.

### Quick start
```shell
git clone https://github.com/Spectralon/TaraTools.git
cd TaraTools
TaraTools
```
Then you will see usage:
```
No args specified
Usage: TaraTools <mode> <input/output file> <input/output folder>
  Modes:
    pack - Pack tara
      Args: <input folder> <output file>
    unpack - Unpack tara
      Args: <input file> <output folder>
```