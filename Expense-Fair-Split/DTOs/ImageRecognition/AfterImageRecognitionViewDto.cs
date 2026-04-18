using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expense_Fair_Split.DTOs.ImageRecognition
{
    public class AfterImageRecognitionViewDto
    {
        public bool IsTarget {  get; set; } = false;
        public string Discription { get; set; } = string.Empty;
        public string DisplayAmount { get; set; } = string.Empty;
        public int Amount { get; set; } = -1;
    }
}
