using System.ComponentModel.DataAnnotations;

namespace WebApi.Models.DigitalSignature;

public class CreateDigitalSignature
{
    [Required]
    public string FileId { get; set; }
    
    [Required]
    public string Password { get; set; }
}
