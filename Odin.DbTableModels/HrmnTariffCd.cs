﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Odin.DbTableModels
{
    public class HrmnTariffCd
    {
        #region Public Properties

        /// <summary>
        ///     Gets or sets DESCR
        /// </summary>
        public string Descr { get; set; }

        /// <summary>
        ///     Gets or sets DESCRSHORT
        /// </summary>
        public string Descrshort { get; set; }

        /// <summary>
        ///     Gets or sets EFF_STATUS
        /// </summary>
        public string EffStatus { get; set; }

        /// <summary>
        ///     Gets or sets EFFDT
        /// </summary>
        public DateTime Effdt { get; set; }

        /// <summary>
        ///     Gets or sets HARMONIZED_CD
        /// </summary>
        public string HarmonizedCd { get; set; }

        /// <summary>
        ///     Gets or sets SETID
        /// </summary>
        public string Setid { get; set; }

        #endregion // Public Properties
    }
}
