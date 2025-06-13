using System;
using System.Collections.Generic;

namespace HealthcareApi.Domain.Models;

public partial class Account
{
    public int AccountId { get; set; }

    public int? PersonId { get; set; }

    public int? ClinicId { get; set; }

    public decimal Balance { get; set; }

    public virtual Clinic? Clinic { get; set; }

    public virtual Person? Person { get; set; }

    public virtual ICollection<Payment> PaymentFromAccounts { get; set; } = new List<Payment>();

    public virtual ICollection<Payment> PaymentToAccounts { get; set; } = new List<Payment>();

   
}
