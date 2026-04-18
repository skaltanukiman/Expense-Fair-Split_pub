using Expense_Fair_Split.Commons;
using Expense_Fair_Split.DTOs.ImageRecognition;
using Expense_Fair_Split.Services.Ocr;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Expense_Fair_Split.ViewModels
{
    public partial class AfterImageRecognitionViewModel : Prism.Mvvm.BindableBase
    {
        private readonly BillingDataEntryViewModel _billingDataEntryViewModel;

        public PostVisionDto VisionDto { get; }

        public AfterImageRecognitionViewModel(BillingDataEntryViewModel billingDataEntryViewModel, PostVisionDto dto)
        {
            _billingDataEntryViewModel = billingDataEntryViewModel;
            VisionDto = dto;

            int rtnCount = PostVisionDataConvertToViewDto();
            if (rtnCount == 0)
            {
                ViewObjNotExists = true;
            }
        }

        #region UI Binding Properties

        private List<AfterImageRecognitionViewDto> _imageRecognitionDatas = new List<AfterImageRecognitionViewDto>();
        public List<AfterImageRecognitionViewDto> ImageRecognitionDatas
        {
            get => _imageRecognitionDatas;
            set => SetProperty(ref _imageRecognitionDatas, value, nameof(ImageRecognitionDatas));
        }

        private int _totalAmount = 0;
        public int TotalAmount
        {
            get => _totalAmount;
            set => SetProperty(ref _totalAmount, value, nameof(TotalAmount));
        }

        private bool _viewObjNotExists = false;
        public bool ViewObjNotExists
        {
            get => _viewObjNotExists;
            set => SetProperty(ref _viewObjNotExists, value, nameof(ViewObjNotExists));
        }

        #endregion

        #region Constructor Method

        /// <summary>
        /// PostVisionDataのデータを、適切な形式に変換し、View用のDtoに格納します。
        /// </summary>
        /// <returns></returns>
        private int PostVisionDataConvertToViewDto()
        {
            const char DELIMITER = '-';

            int counter = 0;

            // VisionDto形式 "商品名 - 金額"
            if (VisionDto.DisplayDataList.Count == 0) return counter;

            foreach (string item in VisionDto.DisplayDataList)
            {
                string inputGoodsName = string.Empty;
                string inputPriceStr = string.Empty;
                int inputPrice = 0;

                (string goodsStr, string priceStr)? extract = CommonUtil.ExtractSidesByDelimiter(item, DELIMITER, true, false);
                if (extract is null)
                {
                    // 抽出結果がnullの場合（商品名はブランク許容、金額はブランク不可）
                    continue;
                }

                inputGoodsName = extract.Value.goodsStr.Trim() == "" ? DataFormatter.UNKNOWN : extract.Value.goodsStr.Trim();

                Match numberMatch = Regex.Match(extract.Value.priceStr, RegexPatterns.RegexNumberWithComma);  // 金額部分のみを抽出（カンマが入っていてもよい）
                if (!numberMatch.Success)
                {
                    // 数値が見つからない場合の処理
                    continue;
                }
               
                string exAmountStr = numberMatch.Value.Replace(",", "");  // 数値のみにする（カンマが入っていればいったん消す）

                if(!int.TryParse(exAmountStr, out inputPrice))  // 計算用の金額を格納
                {
                    continue;
                }

                // exAmountStrのプレフィックスに¥を、数値部分を三桁毎にカンマを挿入し、表示用文字列に整形、格納する。
                if(!CommonUtil.FormatNumberStringWithComma(ref exAmountStr)) continue;
                inputPriceStr = string.Concat("¥", exAmountStr);
                //exAmountStr = int.Parse(exAmountStr).ToString("N0");


                // ループの最後にDTOに挿入する（実際の表示に反映される）
                ImageRecognitionDatas.Add(new AfterImageRecognitionViewDto { Discription = inputGoodsName
                                                                           , DisplayAmount = inputPriceStr
                                                                           , Amount = inputPrice });

                counter++;  // ImageRecognitionDatasに追加されたらカウンタを増やす
            }

            return counter;
        }

        #endregion
    }
}
