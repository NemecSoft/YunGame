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
    private const string DatabaseFileName = "games.db";
    
    public GameDatabase(string databasePath = null)
    {
        if (string.IsNullOrEmpty(databasePath))
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var appFolderPath = Path.Combine(appDataPath, "TGBox");
            Directory.CreateDirectory(appFolderPath);
            databasePath = Path.Combine(appFolderPath, DatabaseFileName);
        }
        
        _connectionString = $"Data Source={databasePath};Version=3;";
        InitializeDatabase();
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
                "Id TEXT PRIMARY KEY, " +
                "Name TEXT NOT NULL, " +
                "Names TEXT, " +
                "SortOrder INTEGER DEFAULT 0, " +
                "Description TEXT, " +
                "Path TEXT, " +
                "IconPath TEXT, " +
                "CoverPath TEXT, " +
                "Genre TEXT, " +
                "ReleaseYear INTEGER, " +
                "PlayTime INTEGER DEFAULT 0, " +
                "LastPlayed TEXT, " +
                "IsInstalled INTEGER DEFAULT 0, " +
                "Platform TEXT" +
                ")", connection))
            {
                command.ExecuteNonQuery();
            }
            
            // 检查并添加缺失的列（用于数据库迁移）
            AddColumnIfNotExists(connection, "Games", "Names", "TEXT");
            AddColumnIfNotExists(connection, "Games", "SortOrder", "INTEGER DEFAULT 0");
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
            Console.WriteLine($"添加列 {columnName} 到表 {tableName} 时出错: {ex.Message}");
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
                "INSERT INTO Games (Id, Name, Names, SortOrder, Description, Path, IconPath, CoverPath, Genre, ReleaseYear, PlayTime, LastPlayed, IsInstalled, Platform) " +
                "VALUES (@Id, @Name, @Names, @SortOrder, @Description, @Path, @IconPath, @CoverPath, @Genre, @ReleaseYear, @PlayTime, @LastPlayed, @IsInstalled, @Platform)", connection))
            {
                // 序列化Names集合为JSON字符串
                string namesJson = game.Names != null ? JsonSerializer.Serialize(game.Names) : "[]";
                
                command.Parameters.AddWithValue("@Id", game.Id.ToString());
                command.Parameters.AddWithValue("@Name", game.Name);
                command.Parameters.AddWithValue("@Names", namesJson);
                command.Parameters.AddWithValue("@SortOrder", game.SortOrder);
                command.Parameters.AddWithValue("@Description", game.Description ?? string.Empty);
                command.Parameters.AddWithValue("@Path", game.Path ?? string.Empty);
                command.Parameters.AddWithValue("@IconPath", game.IconPath ?? string.Empty);
                command.Parameters.AddWithValue("@CoverPath", game.CoverPath ?? string.Empty);
                command.Parameters.AddWithValue("@Genre", game.Genre ?? string.Empty);
                command.Parameters.AddWithValue("@ReleaseYear", game.ReleaseYear);
                command.Parameters.AddWithValue("@PlayTime", game.PlayTime);
                command.Parameters.AddWithValue("@LastPlayed", game.LastPlayed?.ToString("yyyy-MM-dd HH:mm:ss") ?? string.Empty);
                command.Parameters.AddWithValue("@IsInstalled", game.IsInstalled ? 1 : 0);
                command.Parameters.AddWithValue("@Platform", game.Platform ?? string.Empty);
                
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
        
        using (var connection = new SQLiteConnection(_connectionString))
        {
            connection.Open();
            using (var command = new SQLiteCommand("SELECT * FROM Games", connection))
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
    /// 根据ID获取游戏
    /// </summary>
    /// <param name="id">游戏ID</param>
    /// <returns>游戏对象</returns>
    public Game GetGameById(Guid id)
    {
        Game game = null;
        
        using (var connection = new SQLiteConnection(_connectionString))
        {
            connection.Open();
            using (var command = new SQLiteCommand("SELECT * FROM Games WHERE Id = @Id", connection))
            {
                command.Parameters.AddWithValue("@Id", id.ToString());
                
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
                "Names = @Names, " +
                "SortOrder = @SortOrder, " +
                "Description = @Description, " +
                "Path = @Path, " +
                "IconPath = @IconPath, " +
                "CoverPath = @CoverPath, " +
                "Genre = @Genre, " +
                "ReleaseYear = @ReleaseYear, " +
                "PlayTime = @PlayTime, " +
                "LastPlayed = @LastPlayed, " +
                "IsInstalled = @IsInstalled, " +
                "Platform = @Platform " +
                "WHERE Id = @Id", connection))
            {
                // 序列化Names集合为JSON字符串
                string namesJson = game.Names != null ? JsonSerializer.Serialize(game.Names) : "[]";
                
                command.Parameters.AddWithValue("@Id", game.Id.ToString());
                command.Parameters.AddWithValue("@Name", game.Name);
                command.Parameters.AddWithValue("@Names", namesJson);
                command.Parameters.AddWithValue("@SortOrder", game.SortOrder);
                command.Parameters.AddWithValue("@Description", game.Description ?? string.Empty);
                command.Parameters.AddWithValue("@Path", game.Path ?? string.Empty);
                command.Parameters.AddWithValue("@IconPath", game.IconPath ?? string.Empty);
                command.Parameters.AddWithValue("@CoverPath", game.CoverPath ?? string.Empty);
                command.Parameters.AddWithValue("@Genre", game.Genre ?? string.Empty);
                command.Parameters.AddWithValue("@ReleaseYear", game.ReleaseYear);
                command.Parameters.AddWithValue("@PlayTime", game.PlayTime);
                command.Parameters.AddWithValue("@LastPlayed", game.LastPlayed?.ToString("yyyy-MM-dd HH:mm:ss") ?? string.Empty);
                command.Parameters.AddWithValue("@IsInstalled", game.IsInstalled ? 1 : 0);
                command.Parameters.AddWithValue("@Platform", game.Platform ?? string.Empty);
                
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
            using (var command = new SQLiteCommand("DELETE FROM Games WHERE Id = @Id", connection))
            {
                command.Parameters.AddWithValue("@Id", id.ToString());
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
        // 安全地获取值，避免null引用问题
        string idString = reader["Id"]?.ToString() ?? string.Empty;
        string name = reader["Name"]?.ToString() ?? string.Empty;
        string namesJson = reader["Names"]?.ToString() ?? "[]";
        int sortOrder = reader["SortOrder"] != DBNull.Value ? Convert.ToInt32(reader["SortOrder"]) : 0;
        string description = reader["Description"]?.ToString() ?? string.Empty;
        string path = reader["Path"]?.ToString() ?? string.Empty;
        string iconPath = reader["IconPath"]?.ToString() ?? string.Empty;
        string coverPath = reader["CoverPath"]?.ToString() ?? string.Empty;
        string genre = reader["Genre"]?.ToString() ?? string.Empty;
        int releaseYear = reader["ReleaseYear"] != DBNull.Value ? Convert.ToInt32(reader["ReleaseYear"]) : 0;
        int playTime = reader["PlayTime"] != DBNull.Value ? Convert.ToInt32(reader["PlayTime"]) : 0;
        string lastPlayedString = reader["LastPlayed"]?.ToString() ?? string.Empty;
        DateTime? lastPlayed = string.IsNullOrEmpty(lastPlayedString) ? 
            (DateTime?)null : 
            DateTime.Parse(lastPlayedString);
        bool isInstalled = reader["IsInstalled"] != DBNull.Value && Convert.ToInt32(reader["IsInstalled"]) == 1;
        string platform = reader["Platform"]?.ToString() ?? string.Empty;
        
        // 反序列化Names集合
        List<string> names = new List<string>();
        if (!string.IsNullOrEmpty(namesJson))
        {
            try
            {
                names = JsonSerializer.Deserialize<List<string>>(namesJson) ?? new List<string>();
            }
            catch { /* 如果反序列化失败，保持空列表 */ }
        }
        
        // 确保ID不为空
        Guid id = string.IsNullOrEmpty(idString) ? Guid.NewGuid() : Guid.Parse(idString);
        
        return new Game
        {
            Id = id,
            Name = name,
            Names = names,
            SortOrder = sortOrder,
            Description = description,
            Path = path,
            IconPath = iconPath,
            CoverPath = coverPath,
            Genre = genre,
            ReleaseYear = releaseYear,
            PlayTime = playTime,
            LastPlayed = lastPlayed,
            IsInstalled = isInstalled,
            Platform = platform
        };
    }
}