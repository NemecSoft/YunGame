using System;
using System.Collections.Generic;
using System.IO;

namespace TGBox.Models;

/// <summary>
/// 游戏实体类
/// </summary>
public class Game
{
    /// <summary>
    /// 游戏ID
    /// </summary>
    public Guid GameId { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// 主要游戏名称（显示用）
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// 替代名称/副标题
    /// </summary>
    public string AlternativeName { get; set; } = string.Empty;
    
    /// <summary>
    /// 游戏版本
    /// </summary>
    public string Version { get; set; } = string.Empty;
    
    /// <summary>
    /// 游戏的所有名称集合
    /// </summary>
    public List<string> Names { get; set; } = new List<string>();
    
    /// <summary>
    /// 排序顺序字段
    /// </summary>
    public int SortOrder { get; set; } = 0;
    
    /// <summary>
    /// 游戏描述
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// 游戏库路径
    /// </summary>
    public string LibraryPath { get; set; } = string.Empty;
    
    /// <summary>
    /// 游戏文件夹名称
    /// </summary>
    public string GameFolder { get; set; } = string.Empty;
    
    /// <summary>
    /// 可执行文件相对路径
    /// </summary>
    public string ExecutablePath { get; set; } = string.Empty;
    
    /// <summary>
    /// 游戏路径
    /// 自动从LibraryPath, GameFolder和ExecutablePath拼接
    /// </summary>
    public string Path
    {
        get
        {
            if (!string.IsNullOrEmpty(LibraryPath) && !string.IsNullOrEmpty(GameFolder))
            {
                return GameLibrary.CombineGamePath(LibraryPath, GameFolder, ExecutablePath);
            }
            return _path;
        }
        set { _path = value; }
    }
    
    private string _path = string.Empty;
    
    /// <summary>
    /// 游戏图标路径
    /// </summary>
    public string IconPath { get; set; } = string.Empty;
    
    /// <summary>
    /// 游戏封面路径
    /// </summary>
    public string CoverPath { get; set; } = string.Empty;
    
    /// <summary>
    /// 截图1路径
    /// </summary>
    public string Screenshot1 { get; set; } = string.Empty;
    
    /// <summary>
    /// 截图2路径
    /// </summary>
    public string Screenshot2 { get; set; } = string.Empty;
    
    /// <summary>
    /// 截图3路径
    /// </summary>
    public string Screenshot3 { get; set; } = string.Empty;
    
    /// <summary>
    /// 截图4路径
    /// </summary>
    public string Screenshot4 { get; set; } = string.Empty;
    
    /// <summary>
    /// 游戏类型
    /// </summary>
    public string Genre { get; set; } = string.Empty;
    
    /// <summary>
    /// 游戏级别
    /// 用于控制用户是否可以玩特定游戏
    /// </summary>
    public GameLevel GameLevel { get; set; } = GameLevel.Beginner;
    
    /// <summary>
    /// 发布日期
    /// </summary>
    public DateTime? ReleaseDate { get; set; }
    
    /// <summary>
    /// 发行年份
    /// 这个属性是为了向后兼容而保留的
    /// </summary>
    public int ReleaseYear 
    {
        get => ReleaseDate.HasValue ? ReleaseDate.Value.Year : 0;
        set { if (value > 0) ReleaseDate = new DateTime(value, 1, 1); }
    }
    
    /// <summary>
    /// 游玩时间（分钟）
    /// </summary>
    public int PlayTime { get; set; }
    
    /// <summary>
    /// 最后游玩时间
    /// </summary>
    public DateTime? LastPlayed { get; set; }
    
    /// <summary>
    /// 是否已安装
    /// </summary>
    public bool IsInstalled { get; set; }
    
    /// <summary>
    /// 游戏平台
    /// </summary>
    public string Platform { get; set; } = string.Empty;
    
    /// <summary>
    /// 添加一个游戏名称
    /// </summary>
    /// <param name="name">要添加的游戏名称</param>
    public void AddName(string name)
    {
        if (!string.IsNullOrEmpty(name) && !Names.Contains(name))
        {
            Names.Add(name);
        }
    }
    
    /// <summary>
    /// 移除一个游戏名称
    /// </summary>
    /// <param name="name">要移除的游戏名称</param>
    /// <returns>是否成功移除</returns>
    public bool RemoveName(string name)
    {
        return Names.Remove(name);
    }
    
    /// <summary>
    /// 检查是否包含指定名称
    /// </summary>
    /// <param name="name">要检查的名称</param>
    /// <returns>是否包含</returns>
    public bool ContainsName(string name)
    {
        return Names.Contains(name);
    }
}