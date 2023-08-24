using Microsoft.EntityFrameworkCore;
using WebApi.Common.Constants;
using Entities = WebApi.Entities;
using WebApi.Helpers;
using WebApi.Entities;
using System;
using WebApi.Common.Enum;

namespace WebApi.Common.Seed
{
    public class SeedService : ISeeder
    {
        private readonly DataContext _dbContext;

        public SeedService(DataContext dataContext)
        {
            _dbContext = dataContext;
        }

        public async void SeedSystemRolePermissions()
        {
            //seed permissions
            foreach (var item in ConfigRolePermissionConstants.PERMISSION_SEEDS)
            {
                Entities.Permission permission = _dbContext.Permissions.SingleOrDefault(p => p.Name == item.Name && p.GroupCode == item.GroupCode && p.Code == item.Code);
                if (permission == null)
                {
                    _dbContext.Permissions.Add(item);
                }
            }
            _dbContext.SaveChanges();

            //seed system roles permissions
            foreach (var item in ConfigRolePermissionConstants.ROLE_SEEDS)
            {
                Entities.Role role = _dbContext.Roles.SingleOrDefault(r => r.Id == item.Id);
                if (role == null)
                {
                    _dbContext.Roles.Add(item);
                }
            }

            _dbContext.SaveChanges();

            //seed system roles permissions
            foreach (var item in ConfigRolePermissionConstants.ROLE_PERMISSION_SEEDS)
            {
                Entities.RolePermission role = _dbContext.RolePermissions.FirstOrDefault(r => r.RoleId == item.RoleId && r.Name == item.Name && r.Code == item.Code);
                if (role == null)
                {
                    _dbContext.RolePermissions.Add(item);
                }
            }

            _dbContext.SaveChanges();


            IList<string> sysRoleIds = new List<string>() {
                SysRole.Admin, SysRole.OrgOwner, SysRole.DepOwner
            };

            IEnumerable<Entities.RolePermission> rolePermissions = _dbContext.RolePermissions.ToList();
            IEnumerable<Entities.RolePermission> sysRolePermissions = rolePermissions.Where(rp => sysRoleIds.Any(id => Guid.Parse(id) == rp.RoleId));
            IEnumerable<Entities.RolePermission> removeRolePermissions = sysRolePermissions.Where(srp => !ConfigRolePermissionConstants.ROLE_PERMISSION_SEEDS.Any(rps => rps.RoleId == srp.RoleId && rps.Code == srp.Code && rps.Name == srp.Name));

            removeRolePermissions.ToList().ForEach(item =>
            {
                _dbContext.RolePermissions.Remove(item);
            });

            Organization org = _dbContext.Organizations.FirstOrDefault(o => o.Id == Guid.Parse(SystemOrg.SystemOrgId));
            if(org == null)
            {
                Organization root = new Organization();
                root.Id = Guid.Parse(SystemOrg.SystemOrgId);
                root.Name = "Root";
                root.Email = string.Empty;
                root.Phone = string.Empty;

                _dbContext.Organizations.Add(root);
                _dbContext.SaveChanges();

            }

            Account user = _dbContext.Accounts.FirstOrDefault(a => a.Id == Guid.Parse(SystemOrg.AdminId));
            if (user == null)
            {
                Account admin = new Account();
                admin.Email = "sysadmin@gmail.com";
                admin.PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456");
                admin.FirstName = "SysAdmin";
                admin.LastName = "";
                admin.Phone = "0000000000";
                admin.IsActive = true;
                admin.AccountType = AccountTypeEnum.Admin;
                admin.OrgId = SystemOrg.SystemOrgId;
                admin.Id = Guid.Parse(SystemOrg.AdminId);


                Role role = _dbContext.Roles.Where(r => r.Id == Guid.Parse(SysRole.Admin)).SingleOrDefault();
                admin.Role = role;

                _dbContext.Accounts.Add(admin);
                _dbContext.SaveChanges();
            }


            _dbContext.SaveChanges();
        }


        public void Hello()
        {
            Console.WriteLine("Hello");
        }

    }
}
