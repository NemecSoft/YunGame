using System;
using System.Collections.Generic;

namespace TGBox.Models;

/// <summary>
/// 游戏实体类
/// </summary>
public class Game
{
    /// <summary>
    /// 游戏ID
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// 主要游戏名称（显示用）
    /// </summary>
    public string Name { get; set; }
    
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
    public string Description { get; set; }
    
    /// <summary>
    /// 游戏路径
    /// </summary>
    public string Path { get; set; }
    
    /// <summary>
    /// 游戏图标路径
    /// </summary>
    public string IconPath { get; set; }
    
    /// <summary>
    /// 游戏封面路径
    /// </summary>
    public string CoverPath { get; set; }
    
    /// <summary>
    /// 游戏类型
    /// </summary>
    public string Genre { get; set; }
    
    /// <summary>
    /// 发行年份
    /// </summary>
    public int ReleaseYear { get; set; }
    
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
    public string Platform { get; set; }
    
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