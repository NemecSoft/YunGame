using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Text.Json;

namespace TGBox.Models;

/// <summary>
/// 游戏数据库管理类
/// 负责游戏数据的存储、检索和管理
/// </summary>
public class GameDatabase
{
    private readonly string _connectionString;
    private const string DatabaseFileName = "tgbox.db";
    
    public GameDatabase(string? databasePath = null)
    {
        if (string.IsNullOrEmpty(databasePath))
        {
            // 使用指定路径作为数据库路径
            var dbDirectory = "C:\\Users\\Administrator\\AppData\\Local\\TGBox";
            
            if (!Directory.Exists(dbDirectory))
            {
                Directory.CreateDirectory(dbDirectory);
            }
            
            databasePath = Path.Combine(dbDirectory, DatabaseFileName);
        }
        
        _connectionString = $"Data Source={databasePath};Version=3;";
        
        try
        {
            InitializeDatabase();
            
            // 检查数据库是否为空
            var gameCount = GetGameCount();
            
            if (gameCount == 0)
            {
                InitializeSampleData();
            }
        }
        catch (Exception ex)
        {
            Log($"[ERROR] 数据库初始化失败: {ex.Message}\n{ex.StackTrace}");
            throw;
        }
    }
    
    /// <summary>
    /// 初始化数据库结构
    /// </summary>
    private void InitializeDatabase()
    {
        using (var connection = new SQLiteConnection(_connectionString))
        {
            connection.Open();
            
            // 创建表（如果不存在）
            using (var command = new SQLiteCommand(
                "CREATE TABLE IF NOT EXISTS Games (" +
                "GameId TEXT PRIMARY KEY, " +
                "Name TEXT NOT NULL, " +
                "AlternativeName TEXT, " +
                "Version TEXT, " +
                "Names TEXT, " +
                "SortOrder INTEGER DEFAULT 0, " +
                "Description TEXT, " +
                "Path TEXT, " +
                "IconPath TEXT, " +
                "CoverPath TEXT, " +
                "Screenshot1 TEXT, " +
                "Screenshot2 TEXT, " +
                "Screenshot3 TEXT, " +
                "Screenshot4 TEXT, " +
                "Genre TEXT, " +
                "ReleaseYear INTEGER, " +
                "PlayTime INTEGER DEFAULT 0, " +
                "LastPlayed TEXT, " +
                "IsInstalled INTEGER DEFAULT 0, " +
                "Platform TEXT, " +
                "GameLevel INTEGER DEFAULT 1, " +
                "ReleaseDate TEXT, " +
                "LibraryPath TEXT, " +
                "GameFolder TEXT, " +
                "ExecutablePath TEXT" +
                ")", connection))
            {
                command.ExecuteNonQuery();
            }
            
            // 创建game_name表（如果不存在）
            using (var command = new SQLiteCommand(
                "CREATE TABLE IF NOT EXISTS game_name (" +
                "game_id TEXT NOT NULL, " +
                "name TEXT NOT NULL, " +
                "FOREIGN KEY (game_id) REFERENCES Games(GameId)" +
                ")", connection))
            {
                command.ExecuteNonQuery();
            }
            
            // 检查并添加缺失的列（用于数据库迁移）
            AddColumnIfNotExists(connection, "Games", "Names", "TEXT");
            AddColumnIfNotExists(connection, "Games", "SortOrder", "INTEGER DEFAULT 0");
            AddColumnIfNotExists(connection, "Games", "AlternativeName", "TEXT");
            AddColumnIfNotExists(connection, "Games", "Version", "TEXT");
            AddColumnIfNotExists(connection, "Games", "Screenshot1", "TEXT");
            AddColumnIfNotExists(connection, "Games", "Screenshot2", "TEXT");
            AddColumnIfNotExists(connection, "Games", "Screenshot3", "TEXT");
            AddColumnIfNotExists(connection, "Games", "Screenshot4", "TEXT");
            AddColumnIfNotExists(connection, "Games", "GameLevel", "INTEGER DEFAULT 1");
            AddColumnIfNotExists(connection, "Games", "ReleaseDate", "TEXT");
            AddColumnIfNotExists(connection, "Games", "LibraryPath", "TEXT");
            AddColumnIfNotExists(connection, "Games", "GameFolder", "TEXT");
            AddColumnIfNotExists(connection, "Games", "ExecutablePath", "TEXT");
        }
    }
    
    /// <summary>
    /// 检查表中是否存在指定列，如果不存在则添加
    /// </summary>
    /// <param name="connection">SQLite连接</param>
    /// <param name="tableName">表名</param>
    /// <param name="columnName">列名</param>
    /// <param name="columnType">列类型</param>
    private void AddColumnIfNotExists(SQLiteConnection connection, string tableName, string columnName, string columnType)
    {
        try
        {
            // 检查列是否存在
            using (var checkCommand = new SQLiteCommand(
                $"PRAGMA table_info({tableName})", connection))
            using (var reader = checkCommand.ExecuteReader())
            {
                bool columnExists = false;
                while (reader.Read())
                {
                    if (reader["name"].ToString() == columnName)
                    {
                        columnExists = true;
                        break;
                    }
                }
                
                // 如果列不存在，则添加
                if (!columnExists)
                {
                    using (var addColumnCommand = new SQLiteCommand(
                        $"ALTER TABLE {tableName} ADD COLUMN {columnName} {columnType}", connection))
                    {
                        addColumnCommand.ExecuteNonQuery();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // 记录异常，但不中断程序执行
            Log($"[ERROR] 添加列 {columnName} 到表 {tableName} 时出错: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 添加新游戏到数据库
    /// </summary>
    /// <param name="game">要添加的游戏对象</param>
    public void AddGame(Game game)
    {
        using (var connection = new SQLiteConnection(_connectionString))
        {
            connection.Open();
            using (var command = new SQLiteCommand(
                "INSERT INTO Games (GameId, Name, AlternativeName, Version, Names, SortOrder, Description, Path, IconPath, CoverPath, Screenshot1, Screenshot2, Screenshot3, Screenshot4, Genre, ReleaseYear, PlayTime, LastPlayed, IsInstalled, Platform, LibraryPath, GameFolder, ExecutablePath) " +
                "VALUES (@GameId, @Name, @AlternativeName, @Version, @Names, @SortOrder, @Description, @Path, @IconPath, @CoverPath, @Screenshot1, @Screenshot2, @Screenshot3, @Screenshot4, @Genre, @ReleaseYear, @PlayTime, @LastPlayed, @IsInstalled, @Platform, @LibraryPath, @GameFolder, @ExecutablePath)", connection))
            {
                // 序列化Names集合为JSON字符串
                string namesJson = game.Names != null ? JsonSerializer.Serialize(game.Names) : "[]";
                
                command.Parameters.AddWithValue("@GameId", game.GameId.ToString());
                command.Parameters.AddWithValue("@Name", game.Name);
                command.Parameters.AddWithValue("@AlternativeName", game.AlternativeName ?? string.Empty);
                command.Parameters.AddWithValue("@Version", game.Version ?? string.Empty);
                command.Parameters.AddWithValue("@Names", namesJson);
                command.Parameters.AddWithValue("@SortOrder", game.SortOrder);
                command.Parameters.AddWithValue("@Description", game.Description ?? string.Empty);
                command.Parameters.AddWithValue("@Path", game.Path ?? string.Empty);
                command.Parameters.AddWithValue("@IconPath", game.IconPath ?? string.Empty);
                command.Parameters.AddWithValue("@CoverPath", game.CoverPath ?? string.Empty);
                command.Parameters.AddWithValue("@Screenshot1", game.Screenshot1 ?? string.Empty);
                command.Parameters.AddWithValue("@Screenshot2", game.Screenshot2 ?? string.Empty);
                command.Parameters.AddWithValue("@Screenshot3", game.Screenshot3 ?? string.Empty);
                command.Parameters.AddWithValue("@Screenshot4", game.Screenshot4 ?? string.Empty);
                command.Parameters.AddWithValue("@Genre", game.Genre ?? string.Empty);
                command.Parameters.AddWithValue("@ReleaseYear", game.ReleaseYear);
                command.Parameters.AddWithValue("@PlayTime", game.PlayTime);
                command.Parameters.AddWithValue("@LastPlayed", game.LastPlayed?.ToString("yyyy-MM-dd HH:mm:ss") ?? string.Empty);
                command.Parameters.AddWithValue("@IsInstalled", game.IsInstalled ? 1 : 0);
                command.Parameters.AddWithValue("@Platform", game.Platform ?? string.Empty);
                command.Parameters.AddWithValue("@GameLevel", (int)game.GameLevel);
                command.Parameters.AddWithValue("@ReleaseDate", game.ReleaseDate?.ToString("yyyy-MM-dd") ?? string.Empty);
                command.Parameters.AddWithValue("@LibraryPath", game.LibraryPath ?? string.Empty);
                command.Parameters.AddWithValue("@GameFolder", game.GameFolder ?? string.Empty);
                command.Parameters.AddWithValue("@ExecutablePath", game.ExecutablePath ?? string.Empty);
                
                command.ExecuteNonQuery();
            }
        }
    }
    
    /// <summary>
    /// 获取所有游戏
    /// </summary>
    /// <returns>游戏列表</returns>
    public List<Game> GetAllGames()
    {
        var games = new List<Game>();
        
        try
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                
                using (var command = new SQLiteCommand("SELECT * FROM Games", connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var game = ParseGameFromReader(reader);
                        games.Add(game);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Log($"[ERROR] 获取游戏列表失败: {ex.Message}\n{ex.StackTrace}");
        }
        
        return games;
    }
    
    /// <summary>
    /// 检查数据库是否为空
    /// </summary>
    /// <returns>如果数据库为空则返回true，否则返回false</returns>
    public bool IsEmpty()
    {
        using (var connection = new SQLiteConnection(_connectionString))
        {
            connection.Open();
            using (var command = new SQLiteCommand("SELECT COUNT(*) FROM Games", connection))
            {
                long count = (long)command.ExecuteScalar();
                return count == 0;
            }
        }
    }
    
    /// <summary>
    /// 初始化测试数据（当数据库为空时）
    /// </summary>
    public void InitializeSampleData()
    {
        if (!IsEmpty())
        {
            return;
        }
        
        var sampleGames = new List<Game>
        {
            new Game
            {
                Name = "赛博朋克 2077",
                AlternativeName = "Cyberpunk 2077",
                Description = "一款开放世界角色扮演游戏，背景设定在未来科技高度发达的城市。",
                Genre = "角色扮演",
                ReleaseYear = 2020,
                Version = "v2.1",
                IsInstalled = true,
                Platform = "PC"
            },
            new Game
            {
                Name = "巫师 3: 狂猎",
                AlternativeName = "The Witcher 3: Wild Hunt",
                Description = "一款获奖无数的开放世界角色扮演游戏，讲述猎魔人杰洛特的冒险故事。",
                Genre = "角色扮演",
                ReleaseYear = 2015,
                Version = "v1.32",
                PlayTime = 1200,
                LastPlayed = DateTime.Now.AddDays(-5),
                IsInstalled = true,
                Platform = "PC"
            },
            new Game
            {
                Name = "我的世界",
                AlternativeName = "Minecraft",
                Description = "一款开放世界沙盒游戏，玩家可以在其中建造和探索无限的世界。",
                Genre = "沙盒",
                ReleaseYear = 2011,
                Version = "1.20",
                PlayTime = 500,
                LastPlayed = DateTime.Now.AddDays(-1),
                IsInstalled = true,
                Platform = "多平台"
            },
            new Game
            {
                Name = "英雄联盟",
                AlternativeName = "League of Legends",
                Description = "一款多人在线战术竞技游戏，由Riot Games开发。",
                Genre = "MOBA",
                ReleaseYear = 2009,
                Version = "14.2",
                PlayTime = 2000,
                LastPlayed = DateTime.Now.AddHours(-3),
                IsInstalled = true,
                Platform = "PC"
            }
        };
        
        int addedCount = 0;
        foreach (var game in sampleGames)
        {
            try
            {
                AddGame(game);
                addedCount++;
            }
            catch (Exception ex)
            {
                Log($"[ERROR] 添加示例游戏失败: {game.Name} - {ex.Message}");
            }
        }
    }
    
    /// <summary>
    /// 获取所有游戏并按SortOrder排序
    /// </summary>
    /// <returns>按排序顺序排列的游戏列表</returns>
    public List<Game> GetAllGamesSortedBySortOrder()
    {
        var games = new List<Game>();
        
        using (var connection = new SQLiteConnection(_connectionString))
        {
            connection.Open();
            using (var command = new SQLiteCommand("SELECT * FROM Games ORDER BY SortOrder ASC, Name ASC", connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    games.Add(ParseGameFromReader(reader));
                }
            }
        }
        
        return games;
    }

    /// <summary>
    /// 获取数据库中的游戏数量
    /// </summary>
    /// <returns>游戏数量</returns>
    public int GetGameCount()
    {
        using (var connection = new SQLiteConnection(_connectionString))
        {
            connection.Open();
            using (var command = new SQLiteCommand("SELECT COUNT(*) FROM Games", connection))
            {
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }
    }

    /// <summary>
    /// 获取数据库统计信息
    /// </summary>
    /// <returns>数据库统计信息字符串</returns>
    public string GetDatabaseStats()
    {
        var stats = new System.Text.StringBuilder();
        
        try
        {
            var count = GetGameCount();
            stats.AppendLine($"游戏总数: {count}");
            
            if (count > 0)
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new SQLiteCommand("SELECT Name, Genre, ReleaseYear, IsInstalled FROM Games ORDER BY Name", connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var name = reader.GetString(0);
                            var genre = reader.GetString(1);
                            var year = reader.GetInt32(2);
                            var installed = reader.GetInt32(3) == 1;
                            stats.AppendLine($"  {name} ({genre}, {year}) - {(installed ? "已安装" : "未安装")}");
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            stats.AppendLine($"获取数据库统计信息时出错: {ex.Message}");
        }
        
        return stats.ToString();
    }
    
    /// <summary>
    /// 根据ID获取游戏
    /// </summary>
    /// <param name="id">游戏ID</param>
    /// <returns>游戏对象</returns>
    public Game? GetGameById(Guid id)
    {
        Game? game = null;
        
        using (var connection = new SQLiteConnection(_connectionString))
        {
            connection.Open();
            using (var command = new SQLiteCommand("SELECT * FROM Games WHERE GameId = @GameId", connection))
            {
                command.Parameters.AddWithValue("@GameId", id.ToString());
                
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        game = ParseGameFromReader(reader);
                    }
                }
            }
        }
        
        return game;
    }
    
    /// <summary>
    /// 更新游戏信息
    /// </summary>
    /// <param name="game">要更新的游戏对象</param>
    public void UpdateGame(Game game)
    {
        using (var connection = new SQLiteConnection(_connectionString))
        {
            connection.Open();
            using (var command = new SQLiteCommand(
                "UPDATE Games SET " +
                "Name = @Name, " +
                "AlternativeName = @AlternativeName, " +
                "Version = @Version, " +
                "Names = @Names, " +
                "SortOrder = @SortOrder, " +
                "Description = @Description, " +
                "Path = @Path, " +
                "IconPath = @IconPath, " +
                "CoverPath = @CoverPath, " +
                "Screenshot1 = @Screenshot1, " +
                "Screenshot2 = @Screenshot2, " +
                "Screenshot3 = @Screenshot3, " +
                "Screenshot4 = @Screenshot4, " +
                "Genre = @Genre, " +
                "ReleaseYear = @ReleaseYear, " +
                "PlayTime = @PlayTime, " +
                "LastPlayed = @LastPlayed, " +
                "IsInstalled = @IsInstalled, " +
                "Platform = @Platform, " +
                "LibraryPath = @LibraryPath, " +
                "GameFolder = @GameFolder, " +
                "ExecutablePath = @ExecutablePath " +
                "WHERE GameId = @GameId", connection))
            {
                // 序列化Names集合为JSON字符串
                string namesJson = game.Names != null ? JsonSerializer.Serialize(game.Names) : "[]";
                
                command.Parameters.AddWithValue("@GameId", game.GameId.ToString());
                command.Parameters.AddWithValue("@Name", game.Name);
                command.Parameters.AddWithValue("@AlternativeName", game.AlternativeName ?? string.Empty);
                command.Parameters.AddWithValue("@Version", game.Version ?? string.Empty);
                command.Parameters.AddWithValue("@Names", namesJson);
                command.Parameters.AddWithValue("@SortOrder", game.SortOrder);
                command.Parameters.AddWithValue("@Description", game.Description ?? string.Empty);
                command.Parameters.AddWithValue("@Path", game.Path ?? string.Empty);
                command.Parameters.AddWithValue("@IconPath", game.IconPath ?? string.Empty);
                command.Parameters.AddWithValue("@CoverPath", game.CoverPath ?? string.Empty);
                command.Parameters.AddWithValue("@Screenshot1", game.Screenshot1 ?? string.Empty);
                command.Parameters.AddWithValue("@Screenshot2", game.Screenshot2 ?? string.Empty);
                command.Parameters.AddWithValue("@Screenshot3", game.Screenshot3 ?? string.Empty);
                command.Parameters.AddWithValue("@Screenshot4", game.Screenshot4 ?? string.Empty);
                command.Parameters.AddWithValue("@Genre", game.Genre ?? string.Empty);
                command.Parameters.AddWithValue("@ReleaseYear", game.ReleaseYear);
                command.Parameters.AddWithValue("@PlayTime", game.PlayTime);
                command.Parameters.AddWithValue("@LastPlayed", game.LastPlayed?.ToString("yyyy-MM-dd HH:mm:ss") ?? string.Empty);
                command.Parameters.AddWithValue("@IsInstalled", game.IsInstalled ? 1 : 0);
                command.Parameters.AddWithValue("@Platform", game.Platform ?? string.Empty);
                command.Parameters.AddWithValue("@LibraryPath", game.LibraryPath ?? string.Empty);
                command.Parameters.AddWithValue("@GameFolder", game.GameFolder ?? string.Empty);
                command.Parameters.AddWithValue("@ExecutablePath", game.ExecutablePath ?? string.Empty);
            
                command.ExecuteNonQuery();
            }
        }
    }
    
    /// <summary>
    /// 删除游戏
    /// </summary>
    /// <param name="id">游戏ID</param>
    public void DeleteGame(Guid id)
    {
        using (var connection = new SQLiteConnection(_connectionString))
        {
            connection.Open();
            using (var command = new SQLiteCommand("DELETE FROM Games WHERE GameId = @GameId", connection))
            {
                command.Parameters.AddWithValue("@GameId", id.ToString());
                command.ExecuteNonQuery();
            }
        }
    }
    
    /// <summary>
    /// 从SQLite数据读取器解析游戏对象
    /// </summary>
    /// <param name="reader">SQLite数据读取器</param>
    /// <returns>游戏对象</returns>
    private Game ParseGameFromReader(SQLiteDataReader reader)
    {


        try
        {
            // 安全地获取值，避免null引用问题
            string idString = reader["GameId"]?.ToString() ?? string.Empty;
            string name = reader["Name"]?.ToString() ?? string.Empty;
            string alternativeName = reader["AlternativeName"]?.ToString() ?? string.Empty;
            string version = reader["Version"]?.ToString() ?? string.Empty;
            string namesJson = reader["Names"]?.ToString() ?? "[]";
            int sortOrder = reader["SortOrder"] != DBNull.Value ? Convert.ToInt32(reader["SortOrder"]) : 0;
            string description = reader["Description"]?.ToString() ?? string.Empty;
            string path = reader["Path"]?.ToString() ?? string.Empty;
            string iconPath = reader["IconPath"]?.ToString() ?? string.Empty;
            string coverPath = reader["CoverPath"]?.ToString() ?? string.Empty;
            string screenshot1 = reader["Screenshot1"]?.ToString() ?? string.Empty;
            string screenshot2 = reader["Screenshot2"]?.ToString() ?? string.Empty;
            string screenshot3 = reader["Screenshot3"]?.ToString() ?? string.Empty;
            string screenshot4 = reader["Screenshot4"]?.ToString() ?? string.Empty;
            string genre = reader["Genre"]?.ToString() ?? string.Empty;
            int releaseYear = reader["ReleaseYear"] != DBNull.Value ? Convert.ToInt32(reader["ReleaseYear"]) : 0;
            int playTime = reader["PlayTime"] != DBNull.Value ? Convert.ToInt32(reader["PlayTime"]) : 0;
            int gameLevel = reader["GameLevel"] != DBNull.Value ? Convert.ToInt32(reader["GameLevel"]) : 1;
            string releaseDateString = reader["ReleaseDate"]?.ToString() ?? string.Empty;
            DateTime? releaseDate = string.IsNullOrEmpty(releaseDateString) ? 
                (releaseYear > 0 ? (DateTime?)new DateTime(releaseYear, 1, 1) : null) : 
                DateTime.Parse(releaseDateString);
            string lastPlayedString = reader["LastPlayed"]?.ToString() ?? string.Empty;
            DateTime? lastPlayed = string.IsNullOrEmpty(lastPlayedString) ? 
                (DateTime?)null : 
                DateTime.Parse(lastPlayedString);
            bool isInstalled = reader["IsInstalled"] != DBNull.Value && Convert.ToInt32(reader["IsInstalled"]) == 1;
            string platform = reader["Platform"]?.ToString() ?? string.Empty;
            string libraryPath = reader["LibraryPath"]?.ToString() ?? string.Empty;
            string gameFolder = reader["GameFolder"]?.ToString() ?? string.Empty;
            string executablePath = reader["ExecutablePath"]?.ToString() ?? string.Empty;

            // 反序列化Names集合
            List<string> names = new List<string>();
            if (!string.IsNullOrEmpty(namesJson))
            {
                try
                {
                    names = JsonSerializer.Deserialize<List<string>>(namesJson) ?? new List<string>();
                }
                catch (Exception ex)
                {
                    Log($"[ERROR] 解析Names失败: {ex.Message}");
                    names = new List<string>();
                }
            }
            else
            {
                names = new List<string>();
            }

            // 确保ID不为空
            Guid id = string.IsNullOrEmpty(idString) ? Guid.NewGuid() : Guid.Parse(idString);

            var game = new Game
            {
                GameId = id,
                Name = name,
                AlternativeName = alternativeName,
                Version = version,
                Names = names,
                SortOrder = sortOrder,
                Description = description,
                Path = path,
                IconPath = iconPath,
                CoverPath = coverPath,
                Screenshot1 = screenshot1,
                Screenshot2 = screenshot2,
                Screenshot3 = screenshot3,
                Screenshot4 = screenshot4,
                Genre = genre,
                ReleaseYear = releaseYear, // 保留以实现向后兼容
                PlayTime = playTime,
                LastPlayed = lastPlayed,
                IsInstalled = isInstalled,
                Platform = platform,
                GameLevel = (GameLevel)gameLevel,
                ReleaseDate = releaseDate,
                LibraryPath = libraryPath,
                GameFolder = gameFolder,
                ExecutablePath = executablePath
            };
            return game;
        }
        catch (Exception ex)
        {
            Log($"[ERROR] 解析游戏对象时出错: {ex.Message}\n{ex.StackTrace}");
            throw; // 重新抛出异常，以便上层处理
        }
    }
    
    /// <summary>
    /// 写入日志到控制台和文件
    /// </summary>
    /// <param name="message">日志消息</param>
    private void Log(string message)
    {
        var logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}";
        
        // 输出到控制台
        
        
        // 写入到文件
        try
        {
            var logFilePath = Path.Combine(AppContext.BaseDirectory, "game_db.log");
            File.AppendAllText(logFilePath, logMessage + Environment.NewLine);
        }
        catch (Exception ex)
        {
            // 保留控制台输出作为最后的错误报告机制，即使日志文件写入失败
            try
            {
                System.Console.WriteLine($"[ERROR] 写入日志文件失败: {ex.Message}");
            }
            catch { }
        }
    }
}