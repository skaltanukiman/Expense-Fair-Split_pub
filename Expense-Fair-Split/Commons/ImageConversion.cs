using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expense_Fair_Split.Commons
{
    public static class ImageConversion
    {
        public static string ConvertImageToBase64(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"{nameof(ConvertImageToBase64)}: 渡されたファイルパスにファイルが見つかりませんでした。");
            }

            byte[] imageBytes = File.ReadAllBytes(filePath);

            if (imageBytes == null || imageBytes.Length == 0)
            {
                throw new InvalidDataException($"{nameof(ConvertImageToBase64)}: 画像データが空またはnullです。");
            }

            return Convert.ToBase64String(imageBytes);
        }
    }
}
