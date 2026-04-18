using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expense_Fair_Split.Models.Interface
{
    public interface IHasSyncStatus
    {
        bool IsSynced { get; set; }
    }
}
