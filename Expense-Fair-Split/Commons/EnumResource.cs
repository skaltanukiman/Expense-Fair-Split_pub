using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expense_Fair_Split.Commons
{
    public static class EnumResource
    {
        public enum ModeSelect
        {
            ZeroCheck = 0,
            ZeroOrNegativeValue = 1
        }

        public enum IsValidEmailErrType
        {
            success = 0,
            IsNullOrEmpty = 1,
            FormatErr = 2
        }

        public enum IsStringLengthValidErrType
        {
            success = 0,
            IsNullOrEmpty = 1,
            CharOverflow = 2
        }

        public enum RegexPatternSelect 
        {
            JapaneseAndSingleByteAlphaNumeric = 0,
            JapaneseAndDoubleByteAlphaNumeric = 1,
            JapaneseText = 2
        }

        public enum StatusCode
        {
            Rescind = -1,
            CancelConfirm = 0,
            BillingStart = 1,
            AwaitingPayment = 2,
            AfterPaymentConfirm = 3,
            Complete = 100
        }

        public enum LogLevel
        {
            DEBUG = 0,
            INFO = 1,
            WARN = 2,
            ERROR = 3,
            UNKNOWN = 4
        }

        public enum RefreshGridViewType
        {
            Normal = 0,
            Cache = 1
        }

        public enum OrderKey
        {
            Asc = 0,
            Desc = 1
        }

        public enum HTTPKey
        {
            Get = 0,
            Post = 1,
            Put = 2,
            Delete = 3
        }
    }
}
