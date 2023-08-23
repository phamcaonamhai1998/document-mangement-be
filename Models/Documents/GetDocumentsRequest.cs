using System.ComponentModel.DataAnnotations;

namespace WebApi.Models.Documents
{
    public class GetDocumentsRequest
    {
        public string OrgName { get; set; }
        public string OrgId { get; set; }
        public string DepartmentName { get; set; }
        public string DepartmentId { get; set; }
        public string ProcedureName { get; set; }
        public string UserFullName { get; set; }
        public string UserId { get; set; }
        public string Id { get; set; }
        public string Title { get; set; }
        public string Status { get; set; }
        public string CreatedBy { get; set; }

        public string AssignId { get; set; }
        public string RejectedBy { get; set; }


    }
}
