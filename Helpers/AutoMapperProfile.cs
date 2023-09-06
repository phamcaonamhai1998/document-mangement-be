namespace WebApi.Helpers;

using AutoMapper;
using WebApi.Entities;
using WebApi.Models.Accounts;
using WebApi.Models.Departments;
using WebApi.Models.DigitalSignature;
using WebApi.Models.Documents;
using WebApi.Models.Organizations;
using WebApi.Models.Permissions;
using WebApi.Models.Procedures;
using WebApi.Models.Role;
using WebApi.Models.Roles;
using WebApi.Models.Users;

public class AutoMapperProfile : Profile
{
    // mappings between model and entity objects
    public AutoMapperProfile()
    {
        // Users/Accounts
        CreateMap<Account, AccountResponse>();

        CreateMap<Account, AuthenticateResponse>();

        CreateMap<RegisterRequest, Account>();

        CreateMap<CreateRequest, Account>();

        CreateMap<CreateRequest, Account>();

        CreateMap<UpdateRequest, Account>()
            .ForAllMembers(x => x.Condition(
                (src, dest, prop) =>
                {
                    // ignore null & empty string properties
                    if (prop == null) return false;
                    if (prop.GetType() == typeof(string) && string.IsNullOrEmpty((string)prop)) return false;

                    // ignore null role
                    if (x.DestinationMember.Name == "Role" && src.Role == null) return false;

                    return true;
                }
            ));

        CreateMap<CreateUserRequest, Account>();
        CreateMap<Account, UserDto>();

        //role
        CreateMap<Role, RoleDto>();

        //Organization

        CreateMap<Organization, OrganizationDto>();
        CreateMap<CreateOrganizationRequest, Organization>();
        CreateMap<UpdateOrganizationRequest, Organization>();

        //Department
        CreateMap<Department, DepartmentDto>();
        CreateMap<CreateDepartmentRequest, Department>();
        CreateMap<UpdateDepartmentRequest, Department>();

        //Permission
        CreateMap<Permission, PermissionDto>();

        //Role Permission
        CreateMap<RolePermission, RolePermissionDto>();

        //Procedure
        CreateMap<Procedure, ProcedureDto>();
        CreateMap<CreateProcedureRequest, Procedure>();
        CreateMap<UpdateProcedureRequest, Procedure>();

        //ProcedureStep
        CreateMap<ProcedureStep, ProcedureStepDto>();
        CreateMap<CreateProcedureStepItemDto, ProcedureStep>();
        // CreateMap<UpdateProcedureStepRequest, ProcedureStep>();

        //Role
        CreateMap<Role, RoleDto>();
        CreateMap<CreateRoleRequest, Role>();
        CreateMap<UpdateRoleRequest, Role>();

        //Documents
        CreateMap<Document, DocumentDto>();
        CreateMap<Document, DocumentProcedureStepDto>();
        CreateMap<DocumentDto, DocumentProcedureStepDto>();
        CreateMap<DocumentDto, AssignDocumentDto>();
        CreateMap<Document, AssignDocumentDto>();
        CreateMap<ProcedureStep, AssignStepDto>();

        CreateMap<DocumentProcedureStep, DocStepDto>();
        CreateMap<DigitalSignature, DigitalSignDto>();
    }
}