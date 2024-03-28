# TaraTools
TaraTools is a small and simple utility for operating  ```*.tara``` files in Unity. <br>
Tara file is a simple archive format created by Alternativa Games. It was used to create prop libraries for Tanki Online.
## Installation


### Unity Package Manager

Window -> Package Manager -> + sign -> Add via git url:

```
https://github.com/Spectralon/TaraTools.git#upm
```
## Usage
### Tara files direct operation:
```cs 
TaraTools.WriteTara(FileInfo[] files, string outputFileName, string filesRoot);
TaraTools.ReadTara(string path);
```
### Extensions:
```cs
new DirectoryInfo(".").ToTara();
new DirectoryInfo(".").ToTara(string outputFileName);
```