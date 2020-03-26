using SqlSugar;
using System;
using System.Collections.Generic;
using System.Text;

namespace Anniversary.Model.Models
{
    public class Info
    {
        /// <summary>
        /// InfoId
        /// </summary>
        [SugarColumn(IsNullable = false, IsPrimaryKey = true, IsIdentity = true)]
        public int InfoId { get; set; }

        /// <summary>
        /// OpenId
        /// </summary>
        [SugarColumn(IsNullable = false)]

        public string OpenId { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        [SugarColumn(IsNullable = false)]

        public string Name { get; set; }
    }
}
