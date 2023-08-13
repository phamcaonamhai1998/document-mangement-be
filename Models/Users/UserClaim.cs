using WebApi.Entities;
using WebApi.Models.Role;

namespace WebApi.Models.Users
{
    public class UserClaims
    {

        public UserClaims (Guid id, string firtName, string lastName, RoleDto role, Department dep, Organization org, List<string> rights) {
            Id = id;
            FirstName = firtName;
            LastName = lastName;
            Role = role;
            Organization = org;
            Department = dep;
            Rights = rights;

        }
        public Guid Id { get; set; } 
        
        public string FirstName { get; set; }
        
        public string LastName { get; set; }
        
        public RoleDto Role  { get; set; }
        
        public Department Department { get; set; }
        
        public Organization Organization { get; set; }
        
        public List<string> Rights { get; set; }
    }
}
