using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HealthcareApi.Application.DTO
{
    public class PaymentDto
    {
        public int PaymentId { get; set; }
        public int AppointmentId { get; set; }
        //public int FromAccountId { get; set; }

        //public int ToAccountId { get; set; }
        public int PatientPersonId { get; set; }
        public string ClinicName { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; }
        public string Notes { get; set; }

    }
}
