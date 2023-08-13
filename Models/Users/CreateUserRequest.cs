﻿using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using WebApi.Entities;

namespace WebApi.Models.Users;

public class CreateUserRequest
{
    public bool IsActive { get; set; }

    [Required]
    public string FirstName { get; set; }

    [Required]
    public string LastName { get; set; }

    [Required]
    public string Email { get; set; }

    [Required]
    public string PasswordHash { get; set; }

    public string AvatarUrl { get; set; }

    [Required]
    public string Phone { get; set; }

    [Required]
    public string OrgId { get; set; }

    public Guid DepartmentId { get; set; }

    public Guid RoleId { get; set; }
}
