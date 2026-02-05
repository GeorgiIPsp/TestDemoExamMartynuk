using System;
using System.Collections.Generic;

namespace ExamenDem_First.Models;

public partial class EquipmentWriteOff
{
    public int IdWriteoff { get; set; }

    public int IdEquipment { get; set; }

    public DateOnly WriteOffDate { get; set; }

    public string? Reason { get; set; }

    public int IdWorker { get; set; }

    public virtual Equipment IdEquipmentNavigation { get; set; } = null!;

    public virtual Worker IdWorkerNavigation { get; set; } = null!;
}
