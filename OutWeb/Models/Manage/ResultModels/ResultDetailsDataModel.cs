using OutWeb.Entities;
using OutWeb.Models.Manage.FileModels;
using System.Collections.Generic;

namespace OutWeb.Models.Manage.ResultModels
{
    public class ResultDetailsDataModel
    {
 
        private RESULT m_details = new RESULT();

        public RESULT Data
        { get { return this.m_details; } set { this.m_details = value; } }
    }
}