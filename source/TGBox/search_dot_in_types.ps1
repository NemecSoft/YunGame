# 搜索所有XAML文件中包含点的类型名称
$files = Get-ChildItem -Path . -Filter *.axaml -Recurse
foreach ($file in $files) {
    $content = Get-Content -Path $file.FullName
    $lineNum = 1
    foreach ($line in $content) {
        # 查找DataType属性中的类型名
        if ($line -match 'DataType\s*=\s*"([^"]+)"') {
            $typeName = $matches[1]
            if ($typeName -match '\.') {
                Write-Host "文件: $($file.FullName)"
                Write-Host "行号: $lineNum"
                Write-Host "类型名: $typeName"
                Write-Host "-------------------"
            }
        }
        # 查找其他可能的类型引用
        if ($line -match '\b([a-zA-Z0-9_]+):([a-zA-Z0-9_]+\.[a-zA-Z0-9_]+)\b') {
            $typeName = $matches[2]
            Write-Host "文件: $($file.FullName)"
            Write-Host "行号: $lineNum"
            Write-Host "类型名: $typeName"
            Write-Host "-------------------"
        }
        $lineNum++
    }
}