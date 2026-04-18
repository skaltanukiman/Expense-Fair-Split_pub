using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expense_Fair_Split.Models.PickerModels
{
    public class ContactContentsPickerDTO
    {
        public string ContactType { get; set; } = string.Empty;
        public int SelectNum { get; set; }
        public string Content { get; set; } = string.Empty;
    }
}
