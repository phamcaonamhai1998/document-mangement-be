using WebApi.Common.Constants;

namespace WebApi.Models.ElasticSearch
{
    public class EsDocument
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public string DriveDocId { get; set; }
        public string Path { get; set; }
        public string OrgName { get; set; }
        public string OrgId { get; set; }
        public string DepartmentName { get; set; }
        public string DepartmentId { get; set; }
        public string ProcedureName { get; set; }
        public string ProcedureId { get; set; }
        public string UserFullName { get; set; }
        public string UserId { get; set; }
        public string Status { get; set; }
        public List<string> AssignIds { get; set; }
    }
}
