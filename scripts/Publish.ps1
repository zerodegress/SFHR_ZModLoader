dotnet publish -c Release -o .\publish
Compress-Archive -Path .\publish\* -Destination publish.zip