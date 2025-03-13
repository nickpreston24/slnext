dotnet tool uninstall -g slnext;
dotnet build;
dotnet pack;
dotnet tool install --add-source ./nupkg slnext --global | grep --invert-match warning --line-buffered

