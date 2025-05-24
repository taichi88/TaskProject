using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskProject.Models;

namespace Domain.Models.Dto
{
    public class PersonDto
    {
       
        public string Name { get; set; }
        public string Surname { get; set; }

        public string? Email { get; set; }
        public string PersonalNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Phone { get; set; }
        public string? Address { get; set; }

        [EnumDataType(typeof(RoleType), ErrorMessage = "Role must be Patient or Doctor")]
        public RoleType? Role { get; set; } = RoleType.Patient;

    }
    public enum RoleType
    {
        Patient = 1,
        Doctor
    }
}
