using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expense_Fair_Split.Models
{
    public class ContactRequest
    {
        public int ContactNum { get; set; }  // PK

        public int FromUserID { get; set; }  // PK

        public int ContentID { get; set; }
        
        public string OtherText { get; set; } = string.Empty;

        public string InquiryText { get; set; } = string.Empty;

        public string? Platform { get; set; } = null;

        public DateTime CreateDate { get; set; }
    }
}
