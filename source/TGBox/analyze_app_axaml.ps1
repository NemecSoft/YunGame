# 设置UTF-8编码
$OutputEncoding = [console]::InputEncoding = [console]::OutputEncoding = New-Object System.Text.UTF8Encoding

Write-Host "分析App.axaml文件..."

# 读取App.axaml文件内容
$appAxamlContent = Get-Content -Path "d:/Code/YunGameProject/YunGame/YunGame/source/TGBox/App.axaml" -Raw

# 显示第13-15行内容
Write-Host "\n第13-15行内容："
$lines = $appAxamlContent -split "`n"
for ($i = 12; $i -lt 15; $i++) {
    Write-Host ("{0,3}: {1}" -f ($i+1), $lines[$i])
}

# 检查ResourceDictionary.MergedDictionaries语法
Write-Host "\n检查ResourceDictionary.MergedDictionaries语法..."
if ($appAxamlContent -match '<ResourceDictionary\.MergedDictionaries>') {
    Write-Host "找到ResourceDictionary.MergedDictionaries标签"
    # 查找常见的语法错误模式
    if ($appAxamlContent -match '<ResourceDictionary\.MergedDictionaries\s+[^>]*>') {
        Write-Host "警告：ResourceDictionary.MergedDictionaries标签可能包含额外属性，这可能导致问题"
    }
}

# 直接使用dotnet build并过滤出App.axaml相关的错误
Write-Host "\n使用dotnet build查看App.axaml相关错误..."
dotnet build 2>&1 | Select-String -Pattern "App\.axaml", "AXN0002"

Write-Host "\n分析完成。"