namespace WebApi.Entities
{
    public class Department: BaseEntity
    {

        public string Name { get; set; }

        public string Phone { get; set; }

        public string Email { get; set; }

        public Organization Org { get; set; }

        public List<Account> Users { get; set; }
    }
}
