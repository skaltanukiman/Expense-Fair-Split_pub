using Expense_Fair_Split.Commons;
using Syncfusion.Maui.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expense_Fair_Split.Services.Ocr
{
    public class OcrHelper
    {
        public enum FormatMode
        {
            ItemsAndYenPrices = 1
        }

        public PostVisionDto PrepareForDisplayFormat(PostVisionDto dto, FormatMode formatMode)
        {
            switch (formatMode)
            {
                case FormatMode.ItemsAndYenPrices:
                    dto.DisplayDataList = DataFormatter.ExtractItemsAndPrices(dto.ExtractList);
                    break;
            }

            return dto;
        }
    }
}
