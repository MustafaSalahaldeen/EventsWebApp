using System.ComponentModel.DataAnnotations;

namespace EventsApp.Models
{
    public class RoleModel
    {
        [Key]
        public string RoleName { get; set; }    
    }
}
