namespace TGBox.Models
{
    /// <summary>
    /// 游戏级别枚举
    /// 用于控制用户是否可以玩特定游戏
    /// </summary>
    public enum GameLevel
    {
        /// <summary>
        /// 新手级别 - 适合所有用户
        /// </summary>
        Beginner = 1,
        
        /// <summary>
        /// 中级 - 适合有一定经验的用户
        /// </summary>
        Intermediate = 2,
        
        /// <summary>
        /// 高级 - 适合经验丰富的用户
        /// </summary>
        Advanced = 3,
        
        /// <summary>
        /// 专家 - 适合专业用户
        /// </summary>
        Expert = 4,
        
        /// <summary>
        /// 管理员专用 - 仅管理员用户可访问
        /// </summary>
        Admin = 5
    }
    
    /// <summary>
    /// 用户级别枚举
    /// 表示用户的权限等级
    /// </summary>
    public enum UserLevel
    {
        /// <summary>
        /// 普通用户
        /// </summary>
        Normal = 1,
        
        /// <summary>
        /// 高级用户
        /// </summary>
        Premium = 3,
        
        /// <summary>
        /// 管理员用户
        /// </summary>
        Admin = 5
    }
    
    /// <summary>
    /// 游戏权限管理器
    /// 用于检查用户是否有权限玩特定游戏
    /// </summary>
    public static class GamePermissionManager
    {
        /// <summary>
        /// 检查用户是否有权限玩指定游戏
        /// </summary>
        /// <param name="userLevel">用户级别</param>
        /// <param name="gameLevel">游戏级别</param>
        /// <returns>是否有权限</returns>
        public static bool CanPlayGame(UserLevel userLevel, GameLevel gameLevel)
        {
            return (int)userLevel >= (int)gameLevel;
        }
    }
}