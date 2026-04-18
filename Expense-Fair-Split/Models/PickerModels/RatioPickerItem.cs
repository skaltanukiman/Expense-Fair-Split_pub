using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expense_Fair_Split.Models.PickerModels
{
    public class RatioPickerItem
    {
        public int RatioCode { get; set; }
        public string RatioName { get; set; } = null!;
        public string RatioDisplayName { get; set; } = null!;
    }

    public class CalcInputModePickerItem
    {
        public int CalcInputMode { get; set; }
        public string CalcInputModeDisplayName { get; set; } = null!;
    }

    public class RecognitionModePickerItem
    {
        public int RecognitionMode { get; set; }
        public string RecognitionModeDisplayName { get; set; } = null!;
    }
}
