using OutWeb.Enums;
using OutWeb.Modules.Manage;
using System;

namespace OutWeb.Service
{
    public class ListFactoryService 
    {
        public static ListModuleService Create(ListMethodType methodType)
        {
            ListModuleService listManageModule;
            switch (methodType)
            {
 

                case ListMethodType.WORKS:
                    listManageModule = new CaseModule();
                    break;

                case ListMethodType.COURSE:
                    listManageModule = new CourseModule();
                    break;

                case ListMethodType.TYPEMANAGE:
                    listManageModule = new TypeManageModule();
                    break;

                case ListMethodType.TRAIN:
                    listManageModule = new TrainModule();
                    break;

                case ListMethodType.TRAINAPPLY:
                    listManageModule = new TrainApplyModule();
                    break;

                case ListMethodType.EMAIL:
                    listManageModule = new EmailModule();
                    break;

                case ListMethodType.LINK:
                    listManageModule = new LinkModule();
                    break;

                default:
                    listManageModule = null;
                    break;
            }
            return listManageModule;
        }

  
    }
}