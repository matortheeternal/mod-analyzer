# Mod Analyzer
Project to produce a mod analysis from an input zip, rar, or 7z archive.

- [Skyrim Nexus Mods Page](http://www.nexusmods.com/skyrim/mods/80258)
- [Skyrim: Special Edition Nexus Mods Page](http://www.nexusmods.com/skyrimspecialedition/mods/5478/?)

## Plugin Dumps
Plugin dumps are produced by [ModDump](https://github.com/matortheeternal/mod-dump).

## FOMOD and BAIN Parsing
We have functional FOMOD and BAIN parsing.  Each FOMOD/BAIN option will be iterated in the `mod_options` array with its own asset file map and plugin dumps.

## Compiling
You will need to use NuGet to get the GalaSoft, IniParser, Newtonsoft, Octokit, and SharpCompress packages to compile the project.  ModDumpLib.dll, libbsa.dll, and BA2Lib.dll are all included in the project.
