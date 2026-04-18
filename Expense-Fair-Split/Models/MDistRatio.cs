using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expense_Fair_Split.Models
{
    [Table("M_DistRatio")]
    [PrimaryKey(nameof(RatioTypeCode), nameof(RatioCode))]
    public class MDistRatio
    {
        public int RatioTypeCode { get; set; }
        public int RatioCode { get; set; }
        public string RatioName { get; set; } = string.Empty;
        public string RatioDisplayName { get; set; } = string.Empty;
    }
}
