namespace TGBox.Models
{
    /// <summary>
    /// 应用程序模式枚举
    /// </summary>
    public enum AppMode
    {
        /// <summary>
        /// 游玩模式 - 只读状态，只能启动游戏
        /// </summary>
        PlayMode,
        
        /// <summary>
        /// 管理模式 - 可编辑游戏信息，添加删除游戏
        /// </summary>
        ManageMode
    }

    /// <summary>
    /// 模式权限管理器
    /// </summary>
    public static class ModePermissionManager
    {
        /// <summary>
        /// 检查当前模式下是否允许执行指定操作
        /// </summary>
        /// <param name="mode">当前应用模式</param>
        /// <param name="action">操作类型</param>
        /// <returns>是否允许执行</returns>
        public static bool CanPerformAction(AppMode mode, UserAction action)
        {
            switch (mode)
            {
                case AppMode.PlayMode:
                    return action switch
                    {
                        UserAction.LaunchGame => true,
                        UserAction.ViewGameInfo => true,
                        UserAction.ViewStatistics => true,
                        UserAction.ChangeDisplayMode => true,
                        UserAction.ChangeTheme => true,
                        _ => false
                    };
                
                case AppMode.ManageMode:
                    return true; // 管理模式允许所有操作
                
                default:
                    return false;
            }
        }
    }

    /// <summary>
    /// 用户操作类型枚举
    /// </summary>
    public enum UserAction
    {
        LaunchGame,
        AddGame,
        EditGame,
        DeleteGame,
        ImportGames,
        EditGameCover,
        EditGameScreenshots,
        ManageGameSaves,
        ManageGameMods,
        ViewGameInfo,
        ViewStatistics,
        ChangeDisplayMode,
        ChangeTheme,
        BatchEdit
    }
}