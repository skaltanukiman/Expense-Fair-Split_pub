using Expense_Fair_Split.Commons;
using Expense_Fair_Split.Exceptions;
using Expense_Fair_Split.Models.Interface;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Expense_Fair_Split.Models
{
    [Index(nameof(Name), IsUnique = true)]
    [Index(nameof(Email), IsUnique = true)]
    public class User : IHasSyncStatus
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "名前は必須項目です。")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "メールアドレスは必須項目です。")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "パスワードは必須項目です。")]
        public string PasswordHash { get; set; } = null!;

        [Column("IsSynced")]
        public bool IsSynced { get; set; } = false;



        #region Custom Method
        public override bool Equals(object? obj)
        {
            if (obj is User otherUser)
            {
                return this.Id == otherUser.Id;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
        #endregion

        /// <summary>
        /// 引数で渡された文字列とUserモデルの持つハッシュ化されたパスワードが等しいか比較します。
        /// </summary>
        /// <param name="inputPassword">比較対象文字列</param>
        /// <returns>
        ///     等しい: true
        ///     等しくない: false
        /// </returns>
        /// <exception cref="PasswordHashingException"></exception>
        public bool VerifyPassword(string inputPassword)
        {
            string? inputPasswordHashed = CommonUtil.HashPassword(inputPassword, out bool convSucceed);
            if (!convSucceed || inputPasswordHashed is null) 
            {
                throw new PasswordHashingException();
            }
            return PasswordHash == inputPasswordHashed;
        }
    }
}
