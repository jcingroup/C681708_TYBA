using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OutWeb.Models.Manage.ManageBookModels
{
    public class BookListViewModel
    {
        BookListFilterModel m_filter = new BookListFilterModel();
        BookListResultModel m_result = new BookListResultModel();

        public BookListFilterModel Filter { get { return this.m_filter; } set { this.m_filter = value; } }
        public BookListResultModel Result { get { return this.m_result; } set { this.m_result = value; } }
    }
}