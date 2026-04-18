using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using System.Diagnostics;

namespace Expense_Fair_Split.Data
{
    public class DatabaseInitializer
    {
        private readonly AppDbContext _context;

        public DatabaseInitializer(AppDbContext dbContext)
        {
            _context = dbContext;
        }

        public void InitializeDatabase()
        {
            try
            {
                // データベースがなければ作成
                _context.Database.EnsureCreated();

                using var connection = _context.Database.GetDbConnection();
                connection.Open();
                using var command = connection.CreateCommand();

                try
                {
                    // スキーマバージョン管理テーブル
                    command.CommandText = @"
                            CREATE TABLE IF NOT EXISTS SchemaVersion (
                                Version INTEGER PRIMARY KEY
                            );";
                    command.ExecuteNonQuery();

                    // 現在のバージョンを取得
                    command.CommandText = "SELECT MAX(Version) FROM SchemaVersion";
                    var result = command.ExecuteScalar();
                    int currentVersion = result == DBNull.Value ? 0 : Convert.ToInt32(result);

                    Console.WriteLine($"現在のスキーマバージョン: {currentVersion}");

                    // 必要に応じてスキーマ更新を適用
                    ApplyMigrations(currentVersion, command);
                }
                catch (Exception ex) 
                {
                    throw new Exception("スキーマ更新処理時にエラーが発生しました。", ex);
                }
            }
            catch (InvalidOperationException ex) 
            {
                throw new Exception("データベースの接続に失敗しました。", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"予期しないエラーが発生しました: {ex.Message}", ex);
            }
        }

        private void ApplyMigrations(int currentVersion, DbCommand command)
        {
            const int LatestVersion = 6;  // スキーマバージョン追加に合わせて変更する

            if (currentVersion >= LatestVersion) 
            {
                Console.WriteLine("スキーマはすでに最新バージョンです。更新は不要です。");
                return;
            }

            int targetVersion = 0;
            /*** スキーマ更新処理 ***/
            for (targetVersion = currentVersion + 1; targetVersion <= LatestVersion; targetVersion++)
            {
                switch (targetVersion)
                {
                    case 1:
                        Debug.WriteLine("バージョン1スキーマ変更適用中…");
                        command.CommandText = @"
                        CREATE TABLE IF NOT EXISTS Users (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            Name TEXT NOT NULL UNIQUE,
                            Email TEXT NOT NULL UNIQUE,
                            PasswordHash TEXT NOT NULL,
                            IsSynced INTEGER NOT NULL DEFAULT 0
                        );
                        ";
                        command.ExecuteNonQuery();
                        break;
                    case 2:
                        Debug.WriteLine("バージョン2スキーマ変更適用中…");
                        command.CommandText = @"
                        CREATE TABLE IF NOT EXISTS AccountData (
                            AccId INTEGER PRIMARY KEY AUTOINCREMENT,
                            AccName TEXT NOT NULL UNIQUE,
                            CreateUserId INTEGER NOT NULL,
                            CreateDate TEXT NOT NULL,
                            UpdateUserId INTEGER,
                            UpdateDate TEXT,
                            DelFlg INTEGER NOT NULL DEFAULT 0,
                            IsSynced INTEGER NOT NULL DEFAULT 0
                        );
                        ";
                        command.ExecuteNonQuery();
                        break;
                    case 3:
                        Debug.WriteLine("バージョン3スキーマ変更適用中…");
                        command.CommandText = @"
                        CREATE TABLE IF NOT EXISTS M_DistRatio (
                            RatioTypeCode INTEGER NOT NULL,
                            RatioCode INTEGER NOT NULL,
                            RatioName TEXT NOT NULL DEFAULT '',
                            RatioDisplayName TEXT NOT NULL DEFAULT '',
                            PRIMARY KEY (RatioTypeCode, RatioCode)
                        );

                        INSERT INTO M_DistRatio (RatioTypeCode, RatioCode, RatioName, RatioDisplayName) VALUES
                        (1, 1, '請求者100%負担', '10:0'),
                        (1, 2, '9(請求者):1(受領者)', '9:1'),
                        (1, 3, '8(請求者):2(受領者)', '8:2'),
                        (1, 4, '7(請求者):3(受領者)', '7:3'),
                        (1, 5, '6(請求者):4(受領者)', '6:4'),
                        (1, 6, '5(請求者):5(受領者)', '5:5'),
                        (1, 7, '4(請求者):6(受領者)', '4:6'),
                        (1, 8, '3(請求者):7(受領者)', '3:7'),
                        (1, 9, '2(請求者):8(受領者)', '2:8'),
                        (1, 10, '1(請求者):9(受領者)', '1:9'),
                        (1, 11, '受領者100%負担', '0:10');
                        ";
                        command.ExecuteNonQuery();
                        break;
                    case 4:
                        Debug.WriteLine("バージョン4スキーマ変更適用中…");
                        command.CommandText = @"
                        CREATE TABLE IF NOT EXISTS BillingDataSet (
                            BillingNo INTEGER PRIMARY KEY AUTOINCREMENT,
                            BillingDate TEXT NOT NULL,
                            AccountCode INTEGER NOT NULL,
                            RatioTypeCode INTEGER NOT NULL,
                            RatioCode INTEGER,
                            FromUserCode INTEGER NOT NULL,
                            ToUserCode INTEGER NOT NULL,
                            TotalAmount INTEGER NOT NULL,
                            BillingAmount INTEGER NOT NULL,
                            StatusCode INTEGER NOT NULL,
                            Note TEXT DEFAULT '',
                            DeleteFlag TEXT NOT NULL DEFAULT '',
                            IsSynced INTEGER NOT NULL DEFAULT 0
                        );
                        ";
                        command.ExecuteNonQuery();
                        break;
                    case 5:
                        Debug.WriteLine("バージョン5スキーマ変更適用中…");
                        command.CommandText = @"
                        CREATE TABLE IF NOT EXISTS Logs (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            Timestamp TEXT NOT NULL,
                            LogLevel TEXT NOT NULL,
                            Message TEXT,
                            UserId INTEGER,
                            Source TEXT,
                            ExtraData TEXT,
                            IsSynced INTEGER NOT NULL DEFAULT 0
                        );
                        ";
                        command.ExecuteNonQuery();
                        break;
                    case 6:
                        Debug.WriteLine("バージョン6スキーマ変更適用中…");
                        command.CommandText = @"
                        CREATE TABLE IF NOT EXISTS MContactContents (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            ContactType TEXT NOT NULL,
                            SelectNum INTEGER NOT NULL,
                            Content TEXT NOT NULL,
                            CreateDate TEXT NOT NULL,
                            CreateUserName TEXT NOT NULL,
                            UpdateDate TEXT,
                            UpdateUserName TEXT,
                            DelFlg INTEGER NOT NULL DEFAULT 0,
                            IsSynced INTEGER NOT NULL DEFAULT 0,
                            UNIQUE (ContactType, SelectNum)
                        );
                        ";
                        command.ExecuteNonQuery();
                        break;
                    default:
                        throw new Exception($"不明なターゲットバージョン: {targetVersion}");
                }
            }
            targetVersion -= 1;  // for文を抜けるとき+1されるので-する

            /*** スキーマバージョン管理テーブル更新 ***/
            command.CommandText = "INSERT INTO SchemaVersion (Version) VALUES (@Version)";
            var parameter = command.CreateParameter();
            parameter.ParameterName = "@Version";
            parameter.Value = targetVersion;  
            command.Parameters.Clear();
            command.Parameters.Add(parameter);
            command.ExecuteNonQuery();

            Debug.WriteLine($"バージョン{targetVersion}のスキーマ変更が適用されました。");
        }
    }
}
