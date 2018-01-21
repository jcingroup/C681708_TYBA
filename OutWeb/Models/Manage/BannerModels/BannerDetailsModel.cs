﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OutWeb.Models.Manage.BannerModels
{
    public class BannerDetailsModel
    {
        List<FileViewModel> m_files = new List<FileViewModel>();
        public List<FileViewModel> Files { get { return m_files; } set { m_files = value; } }
        public int ID { get; set; }

        public string Title { get; set; }
        public double Sort { get; set; }



        public bool Disable { get; set; }
    }
}