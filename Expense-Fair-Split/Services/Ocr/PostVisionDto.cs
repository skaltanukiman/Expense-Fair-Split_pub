using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expense_Fair_Split.Services.Ocr
{
    public class PostVisionDto
    {
        public string ResponseStr { get; set; } = string.Empty;
        public List<string> ExtractList { get; set; } = new List<string>();
        public List<string> DisplayDataList { get; set; } = new List<string>();
    }
}
