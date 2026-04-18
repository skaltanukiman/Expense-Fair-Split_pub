using Expense_Fair_Split.Commons;
using Expense_Fair_Split.Exceptions;
using Expense_Fair_Split.Models;
using Expense_Fair_Split.Models.PickerModels;
using Expense_Fair_Split.Models.TransportModels;
using Expense_Fair_Split.Services;
using Expense_Fair_Split.Services.Api;
using Expense_Fair_Split.Services.Sessions;
using Expense_Fair_Split.Views.Contact;
using Expense_Fair_Split.Views.Error;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using static Expense_Fair_Split.Commons.EnumResource;

namespace Expense_Fair_Split.ViewModels
{
    public partial class ContactViewModel : Prism.Mvvm.BindableBase
    {
        public DelegateCommand ContactSendCommand { get; }
        public readonly string sessionMissingMsg = "名前の取得に問題が発生しました";
        public readonly string otherTextEnableStr = "その他";

        private readonly UserSessionService _userSessionService;
        private readonly IMContactContentService _contactContentService;
        private readonly ILogDataService _logDataService;
        private readonly ApiClient _apiClient;
        private readonly ContactPage _contactPage;

        private bool _isProcessing = false;

        [GeneratedRegex(@"^\s+.")]  // 先頭空白 + 任意の1文字
        private static partial Regex LeadingWhitespaceRegex();

        public ContactViewModel(ContactPage contactPage)
        {
            _contactPage = contactPage;
            var serviceProvider = App.Services;
            _userSessionService = serviceProvider.GetRequiredService<UserSessionService>();
            _contactContentService = serviceProvider.GetRequiredService<IMContactContentService>();
            _logDataService = serviceProvider.GetRequiredService<ILogDataService>();
            _apiClient = serviceProvider.GetRequiredService<ApiClient>();
            ContactSendCommand = new DelegateCommand(async () => await ContactSendAsync(), CanExecuteSendContent)
                                        .ObservesProperty(() => MainText)
                                        .ObservesProperty(() => OtherText);
        }

        #region CanExecuteMethod

        private bool CanExecuteSendContent()
        {
            if (_contactContentSelectedItem is null || _contactContentSelectedIndex == 0) 
            {
                return false;
            }
            else
            {
                if (_contactContentSelectedItem.Content == otherTextEnableStr)
                {
                    return FromUserName != sessionMissingMsg && !StringValids.IsFirstCharWhitespace(_mainText, true) && !string.IsNullOrWhiteSpace(_otherText) && !_isProcessing;  // 名前が取得出来ている&本文が入力されている&本文の先頭が空白ではない&お問い合わせ内容が入力されている&処理未実行時のみ押下可能
                }
                else
                {
                    return FromUserName != sessionMissingMsg && !StringValids.IsFirstCharWhitespace(_mainText, true) && !_isProcessing;  // 名前が取得出来ている&本文が入力されている&本文の先頭が空白ではない&処理未実行時のみ押下可能
                }
            }            
        }

        #endregion

        #region Command Method

        /// <summary>
        /// お問い合わせに関する登録の処理
        /// </summary>
        /// <returns></returns>
        private async Task ContactSendAsync()
        {
            if (_isProcessing)
            {
                ShowProccessMsg = true;
                return;
            }

            _isProcessing = true;
            ContactSendCommand.RaiseCanExecuteChanged();  // 実行処理中はボタンを無効化

            try
            {
                ErrMsg = string.Empty;

                // 各パラメーターのチェック
                if (!IsSendParameterValid()) return;

                // ポップアップにて送信するかを確認する
                if (!await _contactPage.DisplayAlert("確認", "お問い合わせ内容を送信しますか？", Properties.Resources.Hai, Properties.Resources.Iie)) return;
                
                // 「その他」以外の場合は、OtherTextの入力値を強制的に空にする
                if (_contactContentSelectedItem.Content != otherTextEnableStr) OtherText = string.Empty;

                // サーバーDBにお問い合わせ内容の登録処理を行う
                ContactRequest contactRequest = new ContactRequest();
                if (!await CreateContactData(contactRequest)) return;

                if (!await _apiClient.CommonHttpRequetsAsync(contactRequest, "ContactRequest", HTTPKey.Post))
                {
                    ErrMsg = CommonUtil.InsertNewLineWhenNotBlank(ErrMsg);
                    ErrMsg += $"・サーバー処理中に問題が発生しました。再度送信してください。";

                    return;
                }

                try
                {
                    // GASへの書き込み（GASへの書き込みは、エラーが発生してもそのまま正常終了。DBにデータが登録されていればよしとする。）
                    ContactDataIf contactDataIf = new ContactDataIf();
                    if (await BuildGasPostData(contactDataIf, contactRequest))
                    {
                        await _apiClient.GasApiFuncExec(contactDataIf);
                    }
                }
                catch (Exception)
                {
                    // （GASへの書き込みは、エラーが発生してもそのまま正常終了。DBにデータが登録されていればよしとする。）
                }

                // 登録成功時、ポップアップ表示&画面の項目を初期値に戻す
                ResetUIProperties();

                await _contactPage.DisplayAlert("お問い合わせ完了", "お問い合わせありがとうございました。", Properties.Resources.Ok);
            }
            catch (Exception ex) 
            {
                await _logDataService.InsertLog(EnumResource.LogLevel.ERROR.ToString(), ex.Message, _userSessionService.UserId, nameof(ContactSendAsync), null);
                Application.Current!.MainPage = new ErrorPage();
            }
            finally
            {
                _isProcessing = false;
                ShowProccessMsg = false;
                ContactSendCommand.RaiseCanExecuteChanged();
            }
        }

        #endregion

        #region Private Method

        /// <summary>
        /// 画面のUIプロパティを初期値に戻します
        /// </summary>
        private void ResetUIProperties()
        {
            ContactContentSelectedIndex = 0;
            OtherText = string.Empty;
            MainText = string.Empty;
        }

        /// <summary>
        /// お問い合わせ関するプロパティをContactRequestにセットします。
        /// </summary>
        /// <param name="contactRequest"></param>
        /// <returns></returns>
        /// <exception cref="NotUserSessionException"></exception>
        private async Task<bool> CreateContactData(ContactRequest contactRequest)
        {
            if (_userSessionService is null)
            {
                throw new NotUserSessionException();
            }

            try
            {
                contactRequest.FromUserID = _userSessionService.UserId;

                contactRequest.ContentID = ContactContentSelectedItem.SelectNum;

                contactRequest.InquiryText = MainText;

                contactRequest.OtherText = OtherText;

                contactRequest.CreateDate = CommonUtil.CreateTokyoJapanCurrentDateTime();

                contactRequest.Platform = DeviceInfo.Current.Platform.ToString();

                return true;
            }
            catch (Exception ex)
            {
                ErrMsg = CommonUtil.InsertNewLineWhenNotBlank(ErrMsg);
                ErrMsg += $"・お問い合わせに失敗しました。再度送信してください。";

                await _logDataService.InsertLog(EnumResource.LogLevel.WARN.ToString(), ex.Message, _userSessionService.UserId, nameof(CreateContactData), null);
                return false;
            }
        }

        /// <summary>
        /// GASに書き込むデータを作成します。
        /// </summary>
        /// <param name="contactDataIf">GASに書き込むプロパティを持つオブジェクト</param>
        /// <param name="contactRequest"></param>
        /// <returns></returns>
        private async Task<bool> BuildGasPostData(ContactDataIf contactDataIf, ContactRequest contactRequest)
        {
            try
            {
                if (_userSessionService is null)
                {
                    throw new NotUserSessionException();
                }

                contactDataIf.ContactName = FromUserName;

                // 選択された項目が「その他」の場合
                if (ContactContentSelectedItem.Content == otherTextEnableStr)
                {
                    contactDataIf.ContactContent = contactRequest.OtherText;  // ユーザーの入力したテキストをそのまま出力する
                }
                else
                {
                    contactDataIf.ContactContent = ContactContentSelectedItem.Content;
                }

                contactDataIf.InquiryText = contactRequest.InquiryText;

                contactDataIf.Platform = contactRequest.Platform ?? string.Empty;

                contactDataIf.CreateDate = contactRequest.CreateDate.ToString();

                return true;
            }
            catch (Exception ex)
            {
                await _logDataService.InsertLog(EnumResource.LogLevel.WARN.ToString(), ex.Message, _userSessionService.UserId, nameof(BuildGasPostData), null);
                return false;
            }
        }

        /// <summary>
        /// 規定されたプロパティをチェックし結果によってメッセージ領域を確保します。
        /// </summary>
        private void CheckAndShowMessageArea()
        {
            if (_showErrMsg || _showProccessMsg || _showRegexMsg)
            {
                ShowMsgArea = true;
            }
            else
            {
                ShowMsgArea = false;
            }
        }

        /// <summary>
        /// 送信内容のパラメータをチェック、検証します。
        /// </summary>
        /// <returns></returns>
        private bool IsSendParameterValid()
        {
            const int OTHERTEXT_MAXLENGTH = 30;
            const int MAINTEXT_MAXLENGTH = 2000;

            bool checkResult = true;

            if (FromUserName == sessionMissingMsg)
            {
                ErrMsg = CommonUtil.InsertNewLineWhenNotBlank(ErrMsg);
                ErrMsg += $"・名前の取得に失敗しているため。実行できません。";
                checkResult = false;
            }

            if (ContactContentSelectedItem.SelectNum == -1)
            {
                ErrMsg = CommonUtil.InsertNewLineWhenNotBlank(ErrMsg);
                ErrMsg += $"・データの取得中またはデータの取得に問題があるため実行できません。";
                checkResult = false;
            }

            if (ContactContentSelectedItem.SelectNum == 0)
            {
                ErrMsg = CommonUtil.InsertNewLineWhenNotBlank(ErrMsg);
                ErrMsg += $"・お問い合わせ内容を選択してください。";
                checkResult = false;
            }

            if (ContactContentSelectedItem.Content == otherTextEnableStr)
            {
                if (string.IsNullOrWhiteSpace(OtherText))
                {
                    ErrMsg = CommonUtil.InsertNewLineWhenNotBlank(ErrMsg);
                    ErrMsg += $"・「{otherTextEnableStr}」の場合、お問い合わせ内容を記入してください。";
                    checkResult = false;
                }
                else if (OtherText.Length > OTHERTEXT_MAXLENGTH)
                {
                    ErrMsg = CommonUtil.InsertNewLineWhenNotBlank(ErrMsg);
                    ErrMsg += $"・「{otherTextEnableStr}」の内容は、{OTHERTEXT_MAXLENGTH}文字以内で記入してください。";
                    checkResult = false;
                }
            }

            if (string.IsNullOrWhiteSpace(MainText))
            {
                ErrMsg = CommonUtil.InsertNewLineWhenNotBlank(ErrMsg);
                ErrMsg += $"・「お問い合わせ本文」を入力してください。";
                checkResult = false;
            }
            else if (MainText.Length > MAINTEXT_MAXLENGTH)
            {
                ErrMsg = CommonUtil.InsertNewLineWhenNotBlank(ErrMsg);
                ErrMsg += $"・「お問い合わせ本文」は{MAINTEXT_MAXLENGTH}文字以内で記入してください。";
                checkResult = false;
            }

            if (StringValids.IsFirstCharWhitespace(MainText, true))
            {
                ErrMsg = CommonUtil.InsertNewLineWhenNotBlank(ErrMsg);
                ErrMsg += $"・「お問い合わせ本文」の先頭に空白や改行は使えません。";
                checkResult = false;
            }

            return checkResult;
        }

        #endregion

        #region Initialize

        /// <summary>
        /// 画面に表示するピッカープロパティの初期設定を行います。
        /// </summary>
        /// <returns></returns>
        public async Task PickerInitAsync()
        {
            // データの取得に時間がかかった時のため追加しておく
            this.ContactContentPick.Add(new ContactContentsPickerDTO { SelectNum = -1, ContactType = "データ取得中...", Content = "データ取得中..." });
            ContactContentSelectedItem = this.ContactContentPick[0];

            const string FINDKEY = "detail";  // 現在は'detail'しかないので定数で渡す
            List<MContactContent> contactContents = (await _contactContentService.GetAllByContactTypeFindAsync(FINDKEY, OrderKey.Asc)).ToList();

            this.ContactContentPick.Clear();
            if (contactContents.Count == 0)
            {
                this.ContactContentPick.Add(new ContactContentsPickerDTO { SelectNum = -1, ContactType = $"取得できませんでした。{Properties.Resources.SystemAlert}", Content = $"取得できませんでした。{Properties.Resources.SystemAlert}" });
            }
            else
            {
                foreach (MContactContent content in contactContents) 
                {
                    this.ContactContentPick.Add(new ContactContentsPickerDTO { SelectNum = content.SelectNum, ContactType = content.ContactType, Content = content.Content });
                }
            }
            ContactContentSelectedItem = this.ContactContentPick[0];
        }

        #endregion

        #region UI Binding Properties

        public string FromUserName => _userSessionService?.UserName ?? sessionMissingMsg;

        private string _otherText = string.Empty;
        public string OtherText
        {
            get => _otherText;
            set => SetProperty(ref _otherText, value, nameof(OtherText));
        }

        private string _mainText = string.Empty;
        public string MainText
        {
            get => _mainText;
            set
            {
                if (SetProperty(ref _mainText, value, nameof(MainText)))
                {
                    // 先頭がホワイトスペース（空白や改行）の場合にエラー文を出す。
                    if (StringValids.IsFirstCharWhitespace(MainText, false))
                    {
                        ShowRegexMsg = true;
                    }
                    else
                    {
                        ShowRegexMsg = false;
                    }
                }
            }
        }

        private bool _otherTextEnable;
        public bool OtherTextEnable
        {
            get => _otherTextEnable;
            set => SetProperty(ref _otherTextEnable, value, nameof(OtherTextEnable));
        }

        #region Picker Properties

        private ObservableCollection<ContactContentsPickerDTO> _contactContentPick = new ObservableCollection<ContactContentsPickerDTO>();
        public ObservableCollection<ContactContentsPickerDTO> ContactContentPick
        {
            get => _contactContentPick;
            set => SetProperty(ref _contactContentPick, value, nameof(ContactContentPick));
        }

        private ContactContentsPickerDTO _contactContentSelectedItem = null!;
        public ContactContentsPickerDTO ContactContentSelectedItem
        {
            get => _contactContentSelectedItem;
            set
            {
                if (SetProperty(ref _contactContentSelectedItem, value, nameof(ContactContentSelectedItem)))
                {
                    ContactSendCommand.RaiseCanExecuteChanged();

                    if (_contactContentSelectedItem is not null && _contactContentSelectedItem.Content == otherTextEnableStr)
                    {
                        OtherTextEnable = true;
                    }
                    else 
                    {
                        OtherTextEnable = false;                    
                    }
                }
            }
        }

        private int _contactContentSelectedIndex = 0;
        public int ContactContentSelectedIndex
        {
            get => _contactContentSelectedIndex;
            set => SetProperty(ref _contactContentSelectedIndex, value, nameof(ContactContentSelectedIndex));
        }

        #endregion

        #region Msg Properties

        private bool _showMsgArea = false;
        public bool ShowMsgArea
        {
            get => _showMsgArea;
            set => SetProperty(ref _showMsgArea, value, nameof(ShowMsgArea));
        }

        private string _errMsg = string.Empty;
        public string ErrMsg
        {
            get => _errMsg;
            set
            {
                if (SetProperty(ref _errMsg, value, nameof(ErrMsg)))
                {
                    ShowErrMsg = _errMsg != string.Empty;
                }
            }
        }

        private bool _showErrMsg = false;
        public bool ShowErrMsg
        {
            get => _showErrMsg;
            set
            {
                if (SetProperty(ref _showErrMsg, value, nameof(ShowErrMsg)))
                {
                    CheckAndShowMessageArea();
                }
            }
        }

        private bool _showProccessMsg = false;
        public bool ShowProccessMsg
        {
            get => _showProccessMsg;
            set
            {
                if (SetProperty(ref _showProccessMsg, value, nameof(ShowProccessMsg)))
                {
                    CheckAndShowMessageArea();
                }
            }
        }

        private bool _showRegexMsg = false;
        public bool ShowRegexMsg
        {
            get => _showRegexMsg;
            set
            {
                if (SetProperty(ref _showRegexMsg, value, nameof(ShowRegexMsg)))
                {
                    CheckAndShowMessageArea();
                }
            }
        }

        #endregion

        #endregion
    }
}
