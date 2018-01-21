using OutWeb.Entities;
using OutWeb.Models.Manage.FileModels;
using System.Collections.Generic;

namespace OutWeb.Models.Manage.ManageNewsModels
{
    public class NewsDetailsDataModel 
    {
 
        private NEWS m_details = new NEWS();

        public NEWS Data
        { get { return this.m_details; } set { this.m_details = value; } }
    }
}