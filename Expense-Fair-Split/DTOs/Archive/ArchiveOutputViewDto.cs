using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expense_Fair_Split.DTOs.Archive
{
    public class ArchiveOutputViewDto
    {
        public ArchiveOutputViewDto()
        {
        
        }

        public string BillingDate { get; set; } = string.Empty;
        public string FromUser { get; set; } = string.Empty;
        public string ToUser { get; set; } = string.Empty;
        public string AccountName { get; set;} = string.Empty;
        public string Ratio { get; set; } = string.Empty;
        public int TotalAmount { get; set; } = 0;
        public string Note { get; set; } = string.Empty;
        public string DelFlg {  get; set; } = string.Empty;
        public bool DisplayDeleteColor => DelFlg == "X";
    }
}
