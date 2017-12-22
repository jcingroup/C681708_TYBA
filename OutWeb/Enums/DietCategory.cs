using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace OutWeb.Enums
{
    public enum DietCategory
    {
        /// <summary>
        /// 葷食
        /// </summary>
        [Description("葷食")]
        Meat,

        /// <summary>
        /// 素食
        /// </summary>
        [Description("素食")]
        Vegetarian,
    }
}