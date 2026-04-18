using Expense_Fair_Split.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Expense_Fair_Split.Data
{
    public class AppDbContext : DbContext
    {
        private readonly IConfiguration _configuration;

        public AppDbContext(IConfiguration Configuration)
        {
            _configuration = Configuration;
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<AccountData> AccountDataSet { get; set; } = null!;
        public DbSet<MDistRatio> MDistRatioSet { get; set; } = null!;
        public DbSet<BillingData> BillingDataSet { get; set; } = null!;
        public DbSet<LogData> LogDataSet { get; set; } = null!;
        public DbSet<MContactContent> MContactContentSet { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
            var dbName = _configuration.GetSection("DatabaseSettings")[environment];

            if (dbName is null)
            {
                throw new InvalidOperationException($"環境 '{environment}' に対応するデータベース名を取得できませんでした。DatabaseSettings が正しく構成されているか確認してください。");
            }
            string dbPath = Path.Combine(FileSystem.AppDataDirectory, dbName);

            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }
    }
}
