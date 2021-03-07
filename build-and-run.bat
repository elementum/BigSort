dotnet build -c Release
"FileGenerator/bin/Release/net5.0/FileGenerator.exe" --size 10 --outFile "%cd%\file_to_sort.txt"
"FileSorter/bin/Release/net5.0/FileSorter.exe" --inFile "%cd%\file_to_sort.txt" --outFile "%cd%\sorted.txt"