using Expense_Fair_Split.Commons;
using Expense_Fair_Split.Services.Api;
using Microsoft.EntityFrameworkCore.Storage.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Expense_Fair_Split.Services.Ocr
{
    public class OcrService_Android : IOcrService
    {
        private readonly ApiClient _apiClient;

        public const string invalidType = "無効なタイプパラメータが渡されました。";

        private static class TextRecognitionTypeSelector
        {
            public const string normal = "TEXT_DETECTION";
            public const string document = "DOCUMENT_TEXT_DETECTION";
        }

        public enum TextRecognitionType
        {
            Normal = 0,
            Document = 1
        }

        public OcrService_Android()
        {
            var serviceProvider = App.Services;
            _apiClient = serviceProvider.GetRequiredService<ApiClient>();
        }

        private static string GetTextRecognitionType(TextRecognitionType textRecognitionType)
        {
            switch (textRecognitionType)
            {
                case TextRecognitionType.Normal:
                    return TextRecognitionTypeSelector.normal;
                case TextRecognitionType.Document:
                    return TextRecognitionTypeSelector.document;
                default:
                    return string.Empty;
            }
        }

        public async Task<(bool, PostVisionDto)> PerformOCRAsync(string base64Image, TextRecognitionType textRecognitionType)
        {
            string recognitionType = GetTextRecognitionType(textRecognitionType);
            if (recognitionType == string.Empty) return (false, new PostVisionDto { ResponseStr = invalidType });

            VisionRequest request = new VisionRequest()
            {
                Requests = new List<VisionRequest.Request>()
                {
                    new VisionRequest.Request()
                    {
                        Image = new VisionRequest.Image { Content = base64Image },
                        Features = new List<VisionRequest.Feature>()
                        {
                            new VisionRequest.Feature() { Type = recognitionType}
                        }
                    }
                }
            };

            return await _apiClient.GoogleVisionApiExec(request, GoogleVisionApiRequests.ParseMode.Paragraphs);
        }
    }
}
