using SqlSugar;
using System;
using System.Collections.Generic;
using System.Text;

namespace Anniversary.Model.Models
{
    public class User
    {
        /// <summary>
        /// OpenId
        /// </summary>
        [SugarColumn(IsNullable = false, IsPrimaryKey = true)]
        public string OpenId { get; set; }

        /// <summary>
        /// Version
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public int Version { get; set; }
    }
}
