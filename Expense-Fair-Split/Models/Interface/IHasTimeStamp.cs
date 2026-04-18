using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expense_Fair_Split.Models.Interface
{
    /*
     DB同期処理の日付変換を汎用的に行うために日付プロパティを抽象化するためのインターフェースです。
    インターフェースを追加した際は「namespace Expense_Fair_Split.Services.RemoteDB.SyncService」内の、
    「LocalDataUpdateAsync」内の条件分岐に条件を書き加えてください。
     */

    public interface IHasTimeStamp
    {
        DateTime CreateDate { get; set; }
        DateTime? UpdateDate { get; set; }
    }

    public interface IHasTimeStamp2
    {
        DateTime Timestamp { get; set; }
    }
}
