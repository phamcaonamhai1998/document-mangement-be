using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace WebApi.Entities
{
    public class Organization: BaseEntity
    {
        public static void ConfigurationEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Organization>().Property(b => b.Id).HasDefaultValueSql("uuid_generate_v4()");
            modelBuilder.Entity<Organization>().Property(b => b.CreatedAt).HasDefaultValueSql("now()");
        }

        public string Name { get; set; }

        public string Phone { get; set; }
        
        public string Email { get; set; }

        public string OrgDriveFolderId { get; set; }

        public string  WebsiteAddress { get; set; }

        public List<Department> Departments { get; set; }

        public Organization(Guid id, string name, string phone, string email, string orgDriveFolderId, string websiteAddress)
        {
            Id = id;
            Name = name;
            Phone = phone;
            Email = email;
            OrgDriveFolderId = orgDriveFolderId;
            WebsiteAddress = websiteAddress;
        }
        
        public Organization(Guid id, string name){
            Id = id;
            Name = name;
        }

        public Organization() { }
    }
}
