using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HealthcareApi.Application.DTO
{
    public class AccountDto
    {
        public int? AccountId { get; set; } // Nullable for creation, required for update/get/delete
        public int? PersonId { get; set; }
        public int? ClinicId { get; set; }
        public decimal Balance { get; set; } // Current balance
    }
}
