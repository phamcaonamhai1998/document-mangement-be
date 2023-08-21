using System.Collections.ObjectModel;
using Entities = WebApi.Entities;

namespace WebApi.Common.Constants
{

    public class RoleConstants
    {
        public const string ADMIN_ROLE_ID = "aadb999a-3aa7-11ee-be56-0242ac120002";
        public const string ORG_OWNER_ID = "b5c49a0a-3aa7-11ee-be56-0242ac120002";
        public const string DEP_OWNER_ID = "ae6d324e-3aa7-11ee-be56-0242ac120002";
    }

    // public static class PermissionList
    // {

    //     public static readonly string UserList = "User:List";
    //     public static readonly string UserCreate = "User:Create";
    //     public static readonly string UserUpdate = "User:Update";
    //     public static readonly string UserDelete = "User:Delete";


    //     public static readonly string DocumentList = "Document:List";
    //     public static readonly string DocumentCreate = "Document:Create";
    //     public static readonly string DocumentUpdate = "Document:Update";
    //     public static readonly string DocumentDelete = "Document:Delete";

    // }

    public static class PermissionGroupCode
    {
        public static readonly string User = "User";
        public static readonly string Role = "Role";
        public static readonly string Document = "Document";
        public static readonly string Organization = "Organization";
        public static readonly string Department = "Department";
        public static readonly string Procedure = "Procedure";
    }

    public static class PermissionCode
    {
        public static readonly string List = "List";
        public static readonly string Create = "Create";
        public static readonly string Update = "Update";
        public static readonly string Delete = "Delete";
        public static readonly string Approve = "Approve";
    }

    public static class SystemOrg
    {
        public static readonly string SystemOrgId = "b7759ac0-3aa7-11ee-be56-0242ac120002";
        public static readonly string AdminId = "504f9ee8-0d19-4fea-9340-9e3d31614995";
    }

    public static class SysRole
    {

        public static readonly string Admin = RoleConstants.ADMIN_ROLE_ID;
        public static readonly string OrgOwner = RoleConstants.ORG_OWNER_ID;
        public static readonly string DepOwner = RoleConstants.DEP_OWNER_ID;
    }

    public class ConfigRolePermissionConstants
    {
        public static readonly IList<Entities.Permission> PERMISSION_SEEDS = new ReadOnlyCollection<Entities.Permission>(new List<Entities.Permission>
            {
                new Entities.Permission(PermissionGroupCode.User, PermissionGroupCode.User, PermissionCode.List),
                new Entities.Permission(PermissionGroupCode.User, PermissionGroupCode.User, PermissionCode.Create),
                new Entities.Permission(PermissionGroupCode.User, PermissionGroupCode.User, PermissionCode.Update),
                new Entities.Permission(PermissionGroupCode.User, PermissionGroupCode.User, PermissionCode.Delete),

                new Entities.Permission(PermissionGroupCode.Document, PermissionGroupCode.Document, PermissionCode.List),
                new Entities.Permission(PermissionGroupCode.Document, PermissionGroupCode.Document, PermissionCode.Create),
                new Entities.Permission(PermissionGroupCode.Document, PermissionGroupCode.Document, PermissionCode.Update),
                new Entities.Permission(PermissionGroupCode.Document, PermissionGroupCode.Document, PermissionCode.Delete),
            });


        public static readonly IList<Entities.Role> ROLE_SEEDS = new ReadOnlyCollection<Entities.Role>(new List<Entities.Role>
            {
                new Entities.Role(Guid.Parse(SysRole.Admin), "Admin", null, null),
                new Entities.Role(Guid.Parse(SysRole.OrgOwner), "Organization Owner", null, null),
                new Entities.Role(Guid.Parse(SysRole.DepOwner), "Department Owner", null, null),
            });

        public static readonly IList<Entities.RolePermission> ROLE_PERMISSION_SEEDS = new ReadOnlyCollection<Entities.RolePermission>(new List<Entities.RolePermission>
        {
             //Admin
             new Entities.RolePermission(Guid.Parse(SysRole.Admin), PermissionGroupCode.Organization, PermissionCode.List),
             new Entities.RolePermission(Guid.Parse(SysRole.Admin), PermissionGroupCode.Organization, PermissionCode.Create),
             new Entities.RolePermission(Guid.Parse(SysRole.Admin), PermissionGroupCode.Organization, PermissionCode.Update),
             new Entities.RolePermission(Guid.Parse(SysRole.Admin), PermissionGroupCode.Organization, PermissionCode.Delete),

             new Entities.RolePermission(Guid.Parse(SysRole.Admin), PermissionGroupCode.Department, PermissionCode.List),
             new Entities.RolePermission(Guid.Parse(SysRole.Admin), PermissionGroupCode.Department, PermissionCode.Create),
             new Entities.RolePermission(Guid.Parse(SysRole.Admin), PermissionGroupCode.Department, PermissionCode.Update),
             new Entities.RolePermission(Guid.Parse(SysRole.Admin), PermissionGroupCode.Department, PermissionCode.Delete),


             new Entities.RolePermission(Guid.Parse(SysRole.Admin), PermissionGroupCode.User, PermissionCode.List),
             new Entities.RolePermission(Guid.Parse(SysRole.Admin), PermissionGroupCode.User, PermissionCode.Create),
             new Entities.RolePermission(Guid.Parse(SysRole.Admin), PermissionGroupCode.User, PermissionCode.Update),
             new Entities.RolePermission(Guid.Parse(SysRole.Admin), PermissionGroupCode.User, PermissionCode.Delete),

             new Entities.RolePermission(Guid.Parse(SysRole.Admin), PermissionGroupCode.Role, PermissionCode.List),
             new Entities.RolePermission(Guid.Parse(SysRole.Admin), PermissionGroupCode.Role, PermissionCode.Create),
             new Entities.RolePermission(Guid.Parse(SysRole.Admin), PermissionGroupCode.Role, PermissionCode.Update),
             new Entities.RolePermission(Guid.Parse(SysRole.Admin), PermissionGroupCode.Role, PermissionCode.Delete),

             new Entities.RolePermission(Guid.Parse(SysRole.Admin), PermissionGroupCode.Document, PermissionCode.List),
             new Entities.RolePermission(Guid.Parse(SysRole.Admin), PermissionGroupCode.Document, PermissionCode.Create),
             new Entities.RolePermission(Guid.Parse(SysRole.Admin), PermissionGroupCode.Document, PermissionCode.Update),
             new Entities.RolePermission(Guid.Parse(SysRole.Admin), PermissionGroupCode.Document, PermissionCode.Delete),

             new Entities.RolePermission(Guid.Parse(SysRole.Admin), PermissionGroupCode.Procedure, PermissionCode.List),
             new Entities.RolePermission(Guid.Parse(SysRole.Admin), PermissionGroupCode.Procedure, PermissionCode.Create),
             new Entities.RolePermission(Guid.Parse(SysRole.Admin), PermissionGroupCode.Procedure, PermissionCode.Update),
             new Entities.RolePermission(Guid.Parse(SysRole.Admin), PermissionGroupCode.Procedure, PermissionCode.Delete),

             // Organization
             new Entities.RolePermission(Guid.Parse(SysRole.OrgOwner), PermissionGroupCode.Department, PermissionCode.List),
             new Entities.RolePermission(Guid.Parse(SysRole.OrgOwner), PermissionGroupCode.Department, PermissionCode.Create),
             new Entities.RolePermission(Guid.Parse(SysRole.OrgOwner), PermissionGroupCode.Department, PermissionCode.Update),
             new Entities.RolePermission(Guid.Parse(SysRole.OrgOwner), PermissionGroupCode.Department, PermissionCode.Delete),

             new Entities.RolePermission(Guid.Parse(SysRole.OrgOwner), PermissionGroupCode.User, PermissionCode.List),
             new Entities.RolePermission(Guid.Parse(SysRole.OrgOwner), PermissionGroupCode.User, PermissionCode.Create),
             new Entities.RolePermission(Guid.Parse(SysRole.OrgOwner), PermissionGroupCode.User, PermissionCode.Update),
             new Entities.RolePermission(Guid.Parse(SysRole.OrgOwner), PermissionGroupCode.User, PermissionCode.Delete),

             new Entities.RolePermission(Guid.Parse(SysRole.OrgOwner), PermissionGroupCode.Role, PermissionCode.List),
             new Entities.RolePermission(Guid.Parse(SysRole.OrgOwner), PermissionGroupCode.Role, PermissionCode.Create),
             new Entities.RolePermission(Guid.Parse(SysRole.OrgOwner), PermissionGroupCode.Role, PermissionCode.Update),
             new Entities.RolePermission(Guid.Parse(SysRole.OrgOwner), PermissionGroupCode.Role, PermissionCode.Delete),

            
             // Department
             new Entities.RolePermission(Guid.Parse(SysRole.DepOwner), PermissionGroupCode.User, PermissionCode.List),
             new Entities.RolePermission(Guid.Parse(SysRole.DepOwner), PermissionGroupCode.User, PermissionCode.Create),
             new Entities.RolePermission(Guid.Parse(SysRole.DepOwner), PermissionGroupCode.User, PermissionCode.Update),
             new Entities.RolePermission(Guid.Parse(SysRole.DepOwner), PermissionGroupCode.User, PermissionCode.Delete),

             new Entities.RolePermission(Guid.Parse(SysRole.DepOwner), PermissionGroupCode.Role, PermissionCode.List),
             new Entities.RolePermission(Guid.Parse(SysRole.DepOwner), PermissionGroupCode.Role, PermissionCode.Create),
             new Entities.RolePermission(Guid.Parse(SysRole.DepOwner), PermissionGroupCode.Role, PermissionCode.Update),
             new Entities.RolePermission(Guid.Parse(SysRole.DepOwner), PermissionGroupCode.Role, PermissionCode.Delete),
        });
    }
}

