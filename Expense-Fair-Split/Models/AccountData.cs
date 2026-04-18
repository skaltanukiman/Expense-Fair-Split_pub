using Expense_Fair_Split.Models.Interface;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Expense_Fair_Split.Models
{
    [Index(nameof(AccName), IsUnique = true)]
    public class AccountData : IHasSyncStatus, IHasTimeStamp
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AccId { get; set; }
        [Required]
        public string AccName { get; set; } = string.Empty;
        [Required]
        public int CreateUserId { get; set; }
        [NotMapped]
        public string CreateUser { get; set; } = string.Empty;
        [Required]
        public DateTime CreateDate { get; set; }
        public int? UpdateUserId { get; set; }
        [NotMapped]
        public string UpdateUser { get; set; } = string.Empty;
        public DateTime? UpdateDate { get; set; } = null;
        [Column("DelFlg")]
        public bool DelFlg { get; set; } = false;
        [Column("IsSynced")]
        public bool IsSynced { get; set; } = false;
    }
}
