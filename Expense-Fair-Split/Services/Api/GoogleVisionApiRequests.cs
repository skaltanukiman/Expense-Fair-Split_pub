using Expense_Fair_Split.Commons;
using Expense_Fair_Split.Services.Ocr;
using Expense_Fair_Split.Services.Sessions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Expense_Fair_Split.Services.Api
{
    public class GoogleVisionApiRequests
    {
        private readonly ILogDataService _logDataService;
        private readonly UserSessionService _userSessionService;
        private readonly ApiClient _apiClient;
        private readonly ConfigurationService _configurationService;

        public static class ResponseMsg
        {
            public const string RESPONSE_FAILED = "リクエストに対するレスポンスが不正です。";
            public const string EXCEPTION_OCCURRENCE = "例外が発生しました。";
            public const string NOT_RECOGNIZED = "認識できませんでした。";
            public const string PARSE_FAILED = "変換処理中に問題が発生しました。";
            public const string PARSE_MODE_OUT_OF_RANGE = "変換処理モードの指定が範囲外です。";
        }

        public enum ParseMode
        {
            Normal = 0,
            Paragraphs = 1
        }

        public GoogleVisionApiRequests()
        {
            var serviceProvider = App.Services;
            _logDataService = serviceProvider.GetRequiredService<ILogDataService>();
            _userSessionService = serviceProvider.GetRequiredService<UserSessionService>();
            _configurationService = serviceProvider.GetRequiredService<ConfigurationService>();
            _apiClient = serviceProvider.GetRequiredService<ApiClient>();
        }

        /// <summary>
        /// 画像をjsonへ変換、変換後のjsonから指定の変換モードにて変換し変換後の結果を返します。
        /// </summary>
        /// <param name="visionRequest">GoogleAPIへ画像認識のため渡すImageを持つオブジェクト</param>
        /// <param name="parseMode">文字抽出の変換モード指定</param>
        /// <returns></returns>
        public async Task<(bool, PostVisionDto)> PostVisionRequestAsync(VisionRequest visionRequest, ParseMode parseMode)
        {
            PostVisionDto dto = new PostVisionDto();
            try
            {
                HttpResponseMessage response = await _apiClient.PostAsync("api/GoogleVisionOcr", visionRequest);

                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    switch (parseMode)
                    {
                        case ParseMode.Normal:
                            (bool, string) parseResult = ParseOCRResult(jsonResponse);
                            dto.ResponseStr = parseResult.Item2;
                            return (parseResult.Item1, dto);

                        case ParseMode.Paragraphs:
                            (bool, List<string>) parseResult2 = ParseOCRResultExtractParagraphs(jsonResponse);
                            if (parseResult2.Item1)
                            {
                                dto.ExtractList = parseResult2.Item2;
                            }
                            else
                            {
                                dto.ResponseStr = ResponseMsg.PARSE_FAILED;
                            }
                            return (parseResult2.Item1, dto);
                    }
                }
                else
                {
                    var result = await response.Content.ReadAsStringAsync();
                    await _logDataService.InsertLog(EnumResource.LogLevel.WARN.ToString(), $"失敗: {response.StatusCode},{result}", _userSessionService?.UserId ?? null, nameof(PostVisionRequestAsync), null);
                    dto.ResponseStr = ResponseMsg.RESPONSE_FAILED;
                    return (false, dto);
                }
            }
            catch (Exception ex)
            {
                await _logDataService.InsertLog(EnumResource.LogLevel.WARN.ToString(), ex.Message, _userSessionService?.UserId ?? null, nameof(PostVisionRequestAsync), null);
                dto.ResponseStr = ResponseMsg.EXCEPTION_OCCURRENCE;
                return (false, dto);
            }
            return (false, dto);
        }

        /// <summary>
        /// テキスト形式でjsonの内容を文字列にして返します。
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        private static (bool, string) ParseOCRResult(string json)
        {
            VisionResponseRoot? result = JsonSerializer.Deserialize<VisionResponseRoot>(json);
            string ocrStr = result?.Responses?.FirstOrDefault()?.FullTextAnnotation?.Text ?? ResponseMsg.NOT_RECOGNIZED;

            if (ocrStr == ResponseMsg.NOT_RECOGNIZED)
            {
                return (false, ocrStr);
            }
            else
            {
                return (true, ocrStr);
            }
        }

        /// <summary>
        /// 段落毎にリストへ文字列として格納し返します。
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        private static (bool, List<string>) ParseOCRResultExtractParagraphs(string json)
        {
            VisionResponseRoot? result = JsonSerializer.Deserialize<VisionResponseRoot>(json);
            List<string> paragraphs = new List<string>();

            if (result?.Responses is null) return (false, paragraphs);

            foreach (VisionResponseRoot.Page page in result.Responses[0].FullTextAnnotation?.Pages ?? Enumerable.Empty<VisionResponseRoot.Page>()) 
            {
                foreach (VisionResponseRoot.Block block in page.Blocks ?? Enumerable.Empty<VisionResponseRoot.Block>())
                {
                    foreach (VisionResponseRoot.Paragraph paragraph in block.Paragraphs ?? Enumerable.Empty<VisionResponseRoot.Paragraph>())
                    {
                        List<string> words = new List<string>();

                        foreach (VisionResponseRoot.Word word in paragraph.Words ?? Enumerable.Empty<VisionResponseRoot.Word>())
                        {
                            string wordText = string.Concat(word.Symbols?.Select(s => s.Text) ?? Enumerable.Empty<string>());
                            words.Add(wordText);
                        }

                        paragraphs.Add(string.Join(" ", words));
                    }
                }                            
            }

            return (true, paragraphs);
        }
    }
}
