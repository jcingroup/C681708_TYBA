using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OutWeb.Models.FrontEnd.DownloadFrontModel
{
    public class DownloadFrontResultModel : IPaginationModel
    {
        private List<DownloadFrontDataModel> m_data = new List<DownloadFrontDataModel>();
        public List<DownloadFrontDataModel> Data { get { return m_data; } set { m_data = value; } }

        private PaginationResult m_pagination = new PaginationResult();

        public PaginationResult Pagination
        { get { return this.m_pagination; } set { this.m_pagination = value; } }
    
    }
}