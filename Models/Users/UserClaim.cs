using WebApi.Entities;
using WebApi.Models.Role;

namespace WebApi.Models.Users
{
    public class UserClaims
    {

        public UserClaims(Guid id, string firstName, string lastName, RoleDto role, Department dep, Organization org, List<string> rights, string email)
        {
            Id = id;
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            Role = role;
            Organization = org;
            Department = dep;
            Rights = rights;

        }
        public Guid Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }
        public string Email { get; set; }

        public RoleDto Role { get; set; }

        public Department Department { get; set; }

        public Organization Organization { get; set; }

        public List<string> Rights { get; set; }
    }
}
