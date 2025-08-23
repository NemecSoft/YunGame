Get-ChildItem -Path . -Recurse -Filter "*.axaml" | ForEach-Object {
    $file = $_
    try {
        $content = Get-Content -Path $file.FullName -TotalCount 15
        if ($content.Count -ge 15) {
            Write-Host "文件: $($file.FullName) 第15行:"
            Write-Host $content[14]
            Write-Host "-------------------"
        }
    }
    catch {
        Write-Host "读取文件 $($file.FullName) 时出错: $_"
    }
}