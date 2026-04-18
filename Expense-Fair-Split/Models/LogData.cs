using Expense_Fair_Split.Commons;
using Expense_Fair_Split.Models.Interface;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expense_Fair_Split.Models
{
    [Table("Logs")]
    public class LogData : IHasSyncStatus, IHasTimeStamp2
    {
        public long Id { get; set; }
        public DateTime Timestamp { get; set; }

        private string _logLevel = EnumResource.LogLevel.UNKNOWN.ToString();
        [Required(ErrorMessage = "ログレベルは必須項目です。")]
        [MaxLength(10)]
        public string LogLevel 
        {
            get => _logLevel;
            set => _logLevel = ValidateLogLevel(value);
        }

        public string? Message { get; set; }

        public int? UserId { get; set; }

        [MaxLength(50)]
        public string? Source { get; set; }

        public string? ExtraData {  get; set; } // ← JSONを文字列で保存する

        [Column("IsSynced")]
        public bool IsSynced { get; set; } = false;


        /// <summary>
        /// ログレベルの入力値が規定値か検証します。
        /// </summary>
        /// <param name="val">入力値</param>
        /// <returns>規定値であれば規定値、それ以外であればUNKNOWN（不明）に置き換えます。</returns>
        private string ValidateLogLevel(string? val)
        {
            if (string.IsNullOrEmpty(val)) return EnumResource.LogLevel.UNKNOWN.ToString();

            bool isValid = Enum.IsDefined(typeof(EnumResource.LogLevel), val.ToUpper());  // valの値がEnumResource.LogLevelの中に存在するか

            return isValid ? val.ToUpper() : EnumResource.LogLevel.UNKNOWN.ToString();  // 存在しなければLogLevelをUNKNOWN（不明）に置き換える
        }
    }
}
