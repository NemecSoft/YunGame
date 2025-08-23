chcp 65001
dotnet build > build_output_utf8.txt 2>&1
Get-Content build_output_utf8.txt