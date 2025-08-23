# 查找所有.axaml文件的第15行，并检查是否有类型名包含点
$files = Get-ChildItem -Path . -Filter *.axaml -Recurse
foreach ($file in $files) {
    $line = Get-Content -Path $file.FullName -TotalCount 15 | Select-Object -Last 1
    if ($line -match '\b\w+\.\w+\b') {
        Write-Host "文件: $($file.FullName)"
        Write-Host "第15行: $line"
        Write-Host "匹配到的带点类型名: $($matches[0])"
        Write-Host "-------------------"
    }
}