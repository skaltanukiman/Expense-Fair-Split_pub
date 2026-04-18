using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expense_Fair_Split.Services.RemoteDB
{
    public class SyncFlagService
    {
        private bool _isSyncBlocked = false;
        private readonly object _lock = new object();

        /// <summary>
        /// 同期処理状態を保持するフラグを確認します。
        /// </summary>
        /// <returns>
        ///     処理中: true
        ///     停止中: false
        /// </returns>
        public bool IsSyncFlagActive()
        {
            lock (_lock) 
            {
                return _isSyncBlocked;
            }
        }

        /// <summary>
        /// 同期処理を実行中の状態にします。（処理の再実行をブロック）
        /// </summary>
        public void ActivateSyncBlock()
        {
            lock (_lock)
            {
                _isSyncBlocked = true;
            }
        }

        /// <summary>
        /// 同期処理を未実行状態にします。（処理実行可能状態）
        /// </summary>
        public void DeactivateSyncBlock() 
        {
            lock (_lock) 
            {
                _isSyncBlocked = false;            
            }
        }
    }
}
