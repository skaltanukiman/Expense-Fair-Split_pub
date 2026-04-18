using Expense_Fair_Split.Commons;
using Expense_Fair_Split.Models.PickerModels;
using Expense_Fair_Split.Services;
using Expense_Fair_Split.Services.Ocr;
using Expense_Fair_Split.Services.Sessions;
using Expense_Fair_Split.Views.ImageRecognition;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expense_Fair_Split.ViewModels
{
    public partial class BeforeImageRecognitionViewModel : Prism.Mvvm.BindableBase
    {
        private readonly BillingDataEntryViewModel _billingDataEntryViewModel;
        private readonly ILogDataService _logDataService;
        private readonly UserSessionService _userSessionService;
        private readonly OcrService_Android? _ocrService_Android;

        public bool IsProcessing { get; set; } = false;

        private bool _rotateState = false;
        private bool _procState = false;
        private string _imageRecognitionStr = "画像認識中";

        public PostVisionDto? VisionDto { get; private set; }

        private class OutputContents
        {
            public int OutputNum { get; set; } = -1;
            public string OutputText { get; set; } = string.Empty;
        }

        public BeforeImageRecognitionViewModel(BillingDataEntryViewModel billingDataEntryViewModel)
        {
            _billingDataEntryViewModel = billingDataEntryViewModel;

            var serviceProvider = App.Services;
            _logDataService = serviceProvider.GetRequiredService<ILogDataService>();
            _userSessionService = serviceProvider.GetRequiredService<UserSessionService>();
#if ANDROID
            _ocrService_Android = serviceProvider.GetRequiredService<OcrService_Android>();
#endif

            for (int i = 0; i < _billingDataEntryViewModel.InputModePick.Count; i++)
            {
                var pick = _billingDataEntryViewModel.InputModePick[i];

                if (pick.CalcInputModeDisplayName == "個別")
                {
                    // 入力モードを「個別」に
                    _billingDataEntryViewModel.InputModePickSelectedIndex = i;
                    break;
                }
            }
        }

        #region UI Binding Properties

        private List<RecognitionModePickerItem> _recognitionModePick = new List<RecognitionModePickerItem>()
        {
            new() { RecognitionMode = 1, RecognitionModeDisplayName = "レシート金額が¥表示" }
        };
        public List<RecognitionModePickerItem> RecognitionModePick
        {
            get => _recognitionModePick;
            set => SetProperty(ref _recognitionModePick, value, nameof(RecognitionModePick));
        }
        private RecognitionModePickerItem _recognitionModePickSelectedItem = null!;
        public RecognitionModePickerItem RecognitionModePickSelectedItem
        {
            get => _recognitionModePickSelectedItem;
            set
            {
                if (SetProperty(ref _recognitionModePickSelectedItem, value, nameof(RecognitionModePickSelectedItem)))
                {
                    if (_recognitionModePickSelectedItem is not null)
                    {
                        _outputContents.ForEach(content =>
                        {
                            if (content.OutputNum == _recognitionModePickSelectedItem.RecognitionMode)
                            {
                                OutputText = content.OutputText;
                            }
                        });
                    }
                    else
                    {
                        OutputText = "テキストを取得出来ませんでした。";                        
                    }
                }
            }
        }
        private int _recognitionModePickSelectedIndex = 0;
        public int RecognitionModePickSelectedIndex
        {
            get => _recognitionModePickSelectedIndex;
            set => SetProperty(ref _recognitionModePickSelectedIndex, value, nameof(RecognitionModePickSelectedIndex));
        }

        private readonly List<OutputContents> _outputContents = new List<OutputContents>()
        {
            new OutputContents() { OutputNum = 1, OutputText = $"レシートの¥から始まる数値を出力します。{Environment.NewLine}また、商品名も出力します。" }
        };

        private string _outputText = string.Empty;
        public string OutputText
        {
            get => _outputText;
            set => SetProperty(ref _outputText, value, nameof(OutputText));
        }

        private string _procMsg = "画像認識中";
        public string ProcMsg
        {
            get => _procMsg;
            set => SetProperty(ref _procMsg, value, nameof(ProcMsg));
        }

        #endregion

        #region Visible Property

        private bool _procStatusOnDisplay = false;
        public bool ProcStatusOnDisplay
        {
            get => _procStatusOnDisplay;
            set => SetProperty(ref _procStatusOnDisplay, value, nameof(ProcStatusOnDisplay));
        }

        private bool _errMsgOnDisplay = false;
        public bool ErrMsgOnDisplay
        {
            get => _errMsgOnDisplay;
            set => SetProperty(ref _errMsgOnDisplay, value, nameof(ErrMsgOnDisplay));
        }

        #endregion

        #region Enable Property

        private bool _buttonIsEnable = true;
        public bool ButtonIsEnable
        {
            get => _buttonIsEnable;
            set => SetProperty(ref _buttonIsEnable, value, nameof(ButtonIsEnable));
        }

        #endregion

        #region Motion Property

        private int _rotateTo = 0;
        public int RotateTo
        {
            get => _rotateTo;
            set => SetProperty(ref _rotateTo, value, nameof(RotateTo));
        }

        #endregion

        #region Msg Properties

        private string _errMsg = string.Empty;
        public string ErrMsg
        {
            get => _errMsg;
            set
            {
                if (SetProperty(ref _errMsg, value, nameof(ErrMsg)))
                {
                    if (ErrMsg == string.Empty)
                    {
                        ErrMsgOnDisplay = false;
                    }
                    else
                    {
                        ErrMsgOnDisplay = true;
                    }
                }
            }
        }

        #endregion

        #region Validate method

        /// <summary>
        /// ビューモデルのプロセス判定変数を参照し、処理実行中であればエラーメッセージとtrue(実行中)、それ以外はfalseを返します。
        /// </summary>
        /// <returns>
        ///     true: 実行中
        ///     false: 待機中
        /// </returns>
        public bool GetProcessingStatus()
        {
            if (IsProcessing)
            {
                ErrMsg = $"・{Properties.Resources.isProcessing}";
                return true;
            }
            return false;
        }

        #endregion

        #region SetProperties Method

        /// <summary>
        /// 処理中アイコンをバックグラウンドスレッドにて回転開始します。
        /// </summary>
        private void StartIconRotation()
        {
            _rotateState = true;

            _ = Task.Run(async () =>
            {
                while (_rotateState)
                {
                    RotateTo++;
                    await Task.Delay(5);
                }
            });
        }

        /// <summary>
        /// 処理中アイコンの回転を止めます。
        /// </summary>
        private void StopIconRotation()
        {
            _rotateState = false;
            RotateTo = 0;
        }

        /// <summary>
        /// 処理中メッセージの...の動作を開始（バックグラウンドスレッドにて動作）
        /// </summary>
        private void StartProcMsgIncrementWithWrap()
        {
            const string INCREMENT_STR = "..............................................................";

            int counter = 0;

            _procState = true;

            Task.Run(async () =>
            {
                while (_procState)
                {
                    counter++;

                    string suffixStr = INCREMENT_STR.Substring(0, counter);

                    ProcMsg = string.Concat(_imageRecognitionStr, suffixStr);

                    if (counter >= 3)    // 表示数を変えたい場合はここを弄る
                    {
                        counter = 0;
                    }

                    await Task.Delay(500);
                }
            });
        }

        /// <summary>
        /// 処理中メッセージの...の動作を停止
        /// </summary>
        private void StopProcMsgIncrementWithWrap()
        {
            _procState = false;
            ProcMsg =  _imageRecognitionStr;
        }

        #endregion

        /// <summary>
        /// OCR処理
        /// </summary>
        /// <returns></returns>
        public async Task<bool> StartOCRFunc()
        {
            ErrMsg = string.Empty;

            try
            {
                if (DeviceInfo.Current.Platform.ToString() != "Android")
                {
                    // デバイスがAndroid以外の場合はここでリターン
                    ErrMsg = CommonUtil.InsertNewLineWhenNotBlank(ErrMsg);
                    ErrMsg += $"・対応プラットフォームはAndroidのみです。";
                    return false;
                }

                if (MediaPicker.Default.IsCaptureSupported)
                {
                    FileResult? photo = await MediaPicker.Default.CapturePhotoAsync();
                    if (photo is null)
                    {
                        // カメラをキャンセルした場合の処理
                        return false;
                    }

                    ProcStatusOnDisplay = true;

                    // アイコンの回転開始
                    StartIconRotation();

                    // 文字のインクリメント開始
                    StartProcMsgIncrementWithWrap();

                    string path = Path.Combine(FileSystem.CacheDirectory, photo.FileName);

                    using (Stream stream = await photo.OpenReadAsync())
                    using (FileStream fileStream = File.OpenWrite(path))
                    {
                        await stream.CopyToAsync(fileStream);
                    }

                    string convertdImage = ImageConversion.ConvertImageToBase64(path);

                    (bool isSuccess, PostVisionDto dto) = await _ocrService_Android!.PerformOCRAsync(convertdImage, OcrService_Android.TextRecognitionType.Document);

                    if (!isSuccess)
                    {
                        // 処理失敗時
                        ErrMsg = CommonUtil.InsertNewLineWhenNotBlank(ErrMsg);
                        ErrMsg += $"・{Properties.Resources.ImageRecognitionProcFailed}";
                        return false;
                    }

                    if (dto.ExtractList.Count == 0)
                    {
                        // 画像の文字が認識されない or 画像に対象文字が存在しなかった場合の処理
                        ErrMsg = CommonUtil.InsertNewLineWhenNotBlank(ErrMsg);
                        ErrMsg += $"・{Properties.Resources.TaegetCharacterNotExistsOnImage}";
                        return false;
                    }

                    //int count = 1;
                    //foreach (var item in dto.ExtractList)
                    //{
                    //    Debug.WriteLine($"[{count}]: {item}");
                    //    count++;
                    //}

                    OcrHelper ocrHelper = new OcrHelper();
                    if (RecognitionModePickSelectedItem is not null && RecognitionModePickSelectedItem.RecognitionMode == 1)
                    {
                        // レシート金額が¥表示
                        dto = ocrHelper.PrepareForDisplayFormat(dto, OcrHelper.FormatMode.ItemsAndYenPrices);
                    }
                    else
                    {
                        // 条件外入力 or null時の処理
                        ErrMsg = CommonUtil.InsertNewLineWhenNotBlank(ErrMsg);
                        ErrMsg += $"・認識モードが取得できませんでした。{Properties.Resources.PleaseContactToAdmin}";
                        return false;

                    }

                    // ここでプロパティのDTOにセットし、一度ページオブジェクトに戻る

                    VisionDto = dto;

                    if (VisionDto is null)
                    {
                        ErrMsg = CommonUtil.InsertNewLineWhenNotBlank(ErrMsg);
                        ErrMsg += $"・{Properties.Resources.ProblemsInProccessing}何度か試して解決しない場合、{Properties.Resources.PleaseContactToAdmin}";
                        return false;
                    }

                    return true;
                }
                else
                {
                    // カメラがサポートされていない場合の処理
                    ErrMsg = CommonUtil.InsertNewLineWhenNotBlank(ErrMsg);
                    ErrMsg += $"・{Properties.Resources.DeviceNotSupportOnCamera}";
                    return false;
                }
            }
            catch (PermissionException ex)
            {
                await _logDataService.InsertLog(EnumResource.LogLevel.WARN.ToString(), ex.Message, _userSessionService?.UserId ?? null, nameof(StartOCRFunc), null);
                return false;
            }
            catch (Exception ex)
            {
                await _logDataService.InsertLog(EnumResource.LogLevel.WARN.ToString(), ex.Message, _userSessionService?.UserId ?? null, nameof(StartOCRFunc), null);
                return false;
            }
            finally
            {
                if (_rotateState) StopIconRotation();
                if (_procState) StopProcMsgIncrementWithWrap();
                ProcStatusOnDisplay = false;
            }
        }
    }
}
