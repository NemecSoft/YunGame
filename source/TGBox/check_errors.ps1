# 清理之前的构建
Write-Host "正在清理构建..."
& dotnet clean

# 创建输出文件
$outputFile = "error_log.txt"

# 执行构建并捕获所有输出
Write-Host "正在执行构建并捕获输出..."
& dotnet build 2>&1 | Out-File -FilePath $outputFile -Encoding utf8

# 搜索错误信息
Write-Host "搜索错误信息..."
$errorLines = Get-Content -Path $outputFile | Select-String -Pattern 'error AXN0002|error:' -Context 5, 5

if ($errorLines) {
    Write-Host "\n找到的错误信息:\n"
    $errorLines
    Write-Host "\n完整错误日志已保存到 $outputFile"
} else {
    Write-Host "\n未找到明确的错误信息。查看完整日志: $outputFile"
}