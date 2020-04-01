using SqlSugar;
using System;
using System.Collections.Generic;
using System.Text;

namespace Anniversary.Model.Models
{
    public class InfoDetail
    {
        /// <summary>
        /// InfoDetailId
        /// </summary>
        [SugarColumn(IsNullable = false, IsPrimaryKey = true, IsIdentity = true)]
        public int InfoDetailId { get; set; }

        /// <summary>
        /// InfoId
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public int InfoId { get; set; }

        /// <summary>
        /// Date
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public DateTime? Date { get; set; }

        /// <summary>
        /// Days
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public Int64 Count { get; set; }

        /// <summary>
        /// InfoTitle
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public string Type { get; set; }
    }
}
