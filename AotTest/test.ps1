dotnet publish -r win-x64 -c Release -v:m
dotnet publish -r win-x64 -c Release-AOT -v:m

echo "`nNo AOT:"
.\bin\Release\net8.0\win-x64\publish\AotTest.exe

echo "`nAOT:"
.\bin\Release-AOT\net8.0\win-x64\publish\AotTest.exe