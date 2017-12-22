using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OutWeb.Models.Manage.TypeManageModels
{
    public class TypeManageListViewModel
    {
        TypeManageListFilterModel m_filter = new TypeManageListFilterModel();
        TypeManageListResultModel m_result = new TypeManageListResultModel();

        public TypeManageListFilterModel Filter { get { return this.m_filter; } set { this.m_filter = value; } }
        public TypeManageListResultModel Result { get { return this.m_result; } set { this.m_result = value; } }
    }
}