using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Expense_Fair_Split.Commons;

namespace Expense_Fair_Split.DTOs.BillingData
{
    public class BillingDataInquiryGridViewDto
    {
        public bool CalcTarget { get; set; } = false;
        public bool OldCalcTarget { get; set; } = false;
        public int BillingNo { get; set; }
        public DateTime BillingDate { get; set; }
        public int AccountCode { get; set; }  // AccountData
        public string AccountName { get; set; } = string.Empty;
        public int RatioTypeCode { get; set; }  // M_DistRatio
        public int? RatioCode { get; set; } = null;
        public string RatioName { get; set; } = string.Empty;
        public string RatioDisplayName { get; set; } = string.Empty;
        public int FromUserCode { get; set; }  // User
        public string FromUserName { get; set; } = string.Empty;
        public int ToUserCode { get; set; }
        public string ToUserName { get; set; } = string.Empty;
        public int TotalAmount { get; set; }
        public int BillingAmount { get; set; }
        public int StatusCode { get; set; }
        public string StatusDisplayViewStr {  get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;
        public string DeleteFlag { get; set; } = string.Empty;
        public Color? StateColor { get; set; } = null; // 状態別背景色 null=初期色

        /// <summary>
        /// DTOが持つStatusDisplayViewStrに条件に応じた値をセットします。
        /// </summary>
        public void SetDisplayViewStatus()
        {
            bool? isPayer = CommonUtil.CheckUserRoleInBilling(FromUserCode, ToUserCode);
            StatusDisplayViewStr = CommonUtil.GetBillingStatusMessage(isPayer, StatusCode);

            GetStateColor();
        }

        /// <summary>
        /// 明細情報の状態に対応した色を取得します。（現状はAndroidのみ）
        /// </summary>
        private void GetStateColor()
        {
            DevicePlatform platform = DeviceInfo.Platform;

            if (platform == DevicePlatform.Android)
            {
                if (StatusDisplayViewStr.Trim() == BillingStateStrResource.取消)
                {
                    StateColor = CustomColor.DarkGray;
                }
                else if (StatusDisplayViewStr.Trim() == BillingStateStrResource.完了)
                {
                    StateColor = CustomColor.CornflowerBlue;
                }
                else if (StatusDisplayViewStr.Trim() == BillingStateStrResource.承認待ち || StatusDisplayViewStr.Trim() == BillingStateStrResource.未払い)
                {
                    StateColor = CustomColor.Coral;
                }
                else if (StatusDisplayViewStr.Trim() == BillingStateStrResource.確認中 || StatusDisplayViewStr.Trim() == BillingStateStrResource.支払い待ち)
                {
                    StateColor = CustomColor.Green;
                }
            }
        }
    }
}
