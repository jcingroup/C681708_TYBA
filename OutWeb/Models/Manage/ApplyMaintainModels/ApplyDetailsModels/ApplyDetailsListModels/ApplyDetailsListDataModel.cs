namespace OutWeb.Models.Manage.ApplyMaintainModels.ApplyDetailsModels.ApplyDetailsListModels
{
    public class ApplyDetailsListDataModel
    {
        public int ID { get; set; }
        public string ApplyDate { get; set; }
        public string ApplyNumber { get; set; }
        public string ApplyTeamName { get; set; }
        public string ContactPhone { get; set; }
        public int ApplyTeamMemberCount { get; set; }

        public bool ApplySuccessStatus { get; set; }
        public int GroupID { get; set; }
    }

}