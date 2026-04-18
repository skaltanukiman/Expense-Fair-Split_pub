using Expense_Fair_Split.Models.Interface;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expense_Fair_Split.Models
{
    [Table("MContactContents")]
    public class MContactContent : IHasSyncStatus, IHasTimeStamp
    {
        public int Id { get; set; }

        public string ContactType { get; set; } = string.Empty;

        public int SelectNum { get; set; }

        public string Content { get; set; } = string.Empty;

        public DateTime CreateDate { get; set; }

        public string CreateUserName { get; set; } = string.Empty;

        public DateTime? UpdateDate { get; set; }

        public string? UpdateUserName { get; set; }

        public bool DelFlg { get; set; } = false;

        public bool IsSynced { get; set; } = false;
    }
}