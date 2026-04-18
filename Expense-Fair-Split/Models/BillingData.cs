using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Expense_Fair_Split.Models.Interface;

namespace Expense_Fair_Split.Models
{
    public class BillingData : IHasSyncStatus, IHasBillingDate
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BillingNo { get; set; }
        [Required]
        public DateTime BillingDate { get; set; }
        [Required]
        public int AccountCode { get; set; }  // AccountData
        [NotMapped]
        public string AccountName { get; set; } = string.Empty;
        [Required]
        public int RatioTypeCode { get; set; }  // M_DistRatio
        public int? RatioCode { get; set; } = null;
        [NotMapped]
        public string RatioName { get; set; } = string.Empty;
        [NotMapped]
        public string RatioDisplayName { get; set; } = string.Empty;
        [Required]
        public int FromUserCode { get; set; }  // User
        [NotMapped]
        public string FromUserName { get; set; } = string.Empty;
        [Required]
        public int ToUserCode { get; set; }
        [NotMapped]
        public string ToUserName { get; set; } = string.Empty;
        [Required]
        public int TotalAmount { get; set; }
        [Required]
        public int BillingAmount { get; set; }
        [Required]
        public int StatusCode { get; set; }
        public string Note { get; set; } = string.Empty;
        public string DeleteFlag { get; set; } = string.Empty;
        [Column("IsSynced")]
        public bool IsSynced { get; set; } = false;
    }
}
