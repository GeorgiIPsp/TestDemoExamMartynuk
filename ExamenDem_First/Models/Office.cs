using System;
using System.Collections.Generic;

namespace ExamenDem_First.Models;

public partial class Office
{
    public int IdOffice { get; set; }

    public string? FullTitle { get; set; }

    public string Abbreviated { get; set; } = null!;

    public virtual ICollection<Equipment> Equipment { get; set; } = new List<Equipment>();

    public virtual ICollection<OfficesAudience> OfficesAudiences { get; set; } = new List<OfficesAudience>();

    public virtual ICollection<Worker> Workers { get; set; } = new List<Worker>();
}
