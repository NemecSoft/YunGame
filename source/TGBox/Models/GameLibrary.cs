using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TGBox.Models;

/// <summary>
/// 游戏库管理类
/// 负责管理多个游戏库路径和路径拼接功能
/// </summary>
public class GameLibrary
{
    // 存储所有游戏库路径
    private static readonly List<string> _libraryPaths = new List<string>();
    
    // 默认游戏库路径
    private const string DefaultLibraryFolder = "TGBoxGames";
    
    /// <summary>
    /// 静态构造函数，初始化默认游戏库路径
    /// </summary>
    static GameLibrary()
    {
        // 添加默认游戏库路径
        var defaultPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), DefaultLibraryFolder);
        if (!_libraryPaths.Contains(defaultPath))
        {
            _libraryPaths.Add(defaultPath);
        }
        
        // 根据用户需求添加指定的游戏库路径
        AddLibraryPath("d:\\games");
        AddLibraryPath("e:\\mygames");
    }
    
    /// <summary>
    /// 添加游戏库路径
    /// </summary>
    /// <param name="path">要添加的游戏库路径</param>
    public static void AddLibraryPath(string path)
    {
        if (!string.IsNullOrEmpty(path) && !_libraryPaths.Contains(path))
        {
            _libraryPaths.Add(path);
        }
    }
    
    /// <summary>
    /// 移除游戏库路径
    /// </summary>
    /// <param name="path">要移除的游戏库路径</param>
    /// <returns>是否成功移除</returns>
    public static bool RemoveLibraryPath(string path)
    {
        // 不能移除默认路径
        if (path == Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), DefaultLibraryFolder))
        {
            return false;
        }
        
        return _libraryPaths.Remove(path);
    }
    
    /// <summary>
    /// 获取所有游戏库路径
    /// </summary>
    /// <returns>游戏库路径列表</returns>
    public static List<string> GetAllLibraryPaths()
    {
        return new List<string>(_libraryPaths);
    }
    
    /// <summary>
    /// 拼接完整的游戏路径
    /// </summary>
    /// <param name="libraryPath">游戏库路径</param>
    /// <param name="gameFolder">游戏文件夹名称</param>
    /// <param name="executablePath">可执行文件相对路径</param>
    /// <returns>完整的可执行文件路径</returns>
    public static string CombineGamePath(string libraryPath, string gameFolder, string executablePath)
    {
        if (string.IsNullOrEmpty(libraryPath) || string.IsNullOrEmpty(gameFolder))
        {
            return string.Empty;
        }
        
        // 构建游戏主目录路径
        string gameDirectory = Path.Combine(libraryPath, gameFolder);
        
        // 如果有相对执行路径，则拼接完整路径
        if (!string.IsNullOrEmpty(executablePath))
        {
            // 处理可能的路径分隔符问题
            string normalizedExecutablePath = executablePath.Replace('/', Path.DirectorySeparatorChar);
            return Path.Combine(gameDirectory, normalizedExecutablePath);
        }
        
        return gameDirectory;
    }
    
    /// <summary>
    /// 尝试查找游戏可执行文件
    /// 遍历所有游戏库，查找匹配的游戏
    /// </summary>
    /// <param name="gameFolder">游戏文件夹名称</param>
    /// <param name="executablePath">可执行文件相对路径</param>
    /// <returns>找到的完整路径，如果未找到则返回空字符串</returns>
    public static string FindGameExecutable(string gameFolder, string executablePath)
    {
        foreach (var libraryPath in _libraryPaths)
        {
            string fullPath = CombineGamePath(libraryPath, gameFolder, executablePath);
            if (File.Exists(fullPath))
            {
                return fullPath;
            }
        }
        
        return string.Empty;
    }
    
    /// <summary>
    /// 验证游戏库路径是否存在
    /// </summary>
    /// <param name="path">要验证的路径</param>
    /// <returns>路径是否存在</returns>
    public static bool IsValidLibraryPath(string path)
    {
        return !string.IsNullOrEmpty(path) && Directory.Exists(path);
    }
}