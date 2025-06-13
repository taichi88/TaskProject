using System;
using System.Collections.Generic;

namespace HealthcareApi.Domain.Models;

public partial class DeskStaff
{
    public int Id { get; set; }

    public int PersonId { get; set; }

    public virtual Person Person { get; set; } = null!;
}
