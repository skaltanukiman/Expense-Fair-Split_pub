using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;


namespace Expense_Fair_Split.Services
{
    public class ConfigurationService
    {
        public readonly IConfiguration _configuration;

        public ConfigurationService()
        {
            try
            {
                var appSettingsPath = Path.Combine(FileSystem.AppDataDirectory, "appsettings.json");

                if (!File.Exists(appSettingsPath))
                {
                    // 指定パスにappsettings.jsonが存在しない場合

                    using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Expense_Fair_Split.appsettings.json");
                    if (stream is null)
                    {
                        throw new Exception("アセンブリから対象のデータをストリーム出来ませんでした。");
                    }
                    using var reader = new StreamReader(stream);

                    // 読み取った内容をローカルファイルとして書き出す
                    File.WriteAllText(appSettingsPath, reader.ReadToEnd());
                }
                else
                {
                    // 指定パスにappsettings.jsonが存在する場合

                    using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Expense_Fair_Split.appsettings.json");
                    if (stream is null)
                    {
                        throw new Exception("アセンブリから対象のデータをストリーム出来ませんでした。");
                    }
                    using var reader = new StreamReader(stream);

                    string appSettings = File.ReadAllText(appSettingsPath);
                    string streamContent = reader.ReadToEnd();

                    // 既に書き出しているappsettingsとアセンブリからストリームした内容に差異（更新）がある場合は、appsettingsの中身を上書きする。
                    if (appSettings != streamContent) File.WriteAllText(appSettingsPath, streamContent);
                }

                // appsettings.jsonを設定に読み込む
                _configuration = new ConfigurationBuilder()
                    .SetBasePath(FileSystem.AppDataDirectory)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new Exception("ファイルへの書き込み権限がありません。権限を確認してください。", ex);
            }
            catch (Exception ex) 
            {
                throw new Exception($"予期しないエラーが発生しました: {ex.Message}", ex);
            }
        }
    }
}
