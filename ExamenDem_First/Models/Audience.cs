using System;
using System.Collections.Generic;

namespace ExamenDem_First.Models;

public partial class Audience
{
    public int IdAudience { get; set; }

    public string NumberAudience { get; set; } = null!;

    public int IdFloor { get; set; }

    public virtual ICollection<Equipment> Equipment { get; set; } = new List<Equipment>();

    public virtual Floor IdFloorNavigation { get; set; } = null!;

    public virtual ICollection<OfficesAudience> OfficesAudiences { get; set; } = new List<OfficesAudience>();
}
