using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Windows;

namespace ExamenDem_First.Models;

public partial class Equipment
{
    public int IdEquipment { get; set; }

    public string TitleEquipment { get; set; } = null!;

    public int? IdAudience { get; set; }

    public double WeightInKg { get; set; }

    public DateOnly DateTransferToCompanyBalance { get; set; }

    public string InventoryNumber { get; set; } = null!;

    public int? IdWorker { get; set; }

    public string? Photo { get; set; }

    public int StandardServiceLife { get; set; }

    public string Description { get; set; } = null!;

    public int? IdOffices { get; set; }

    public virtual ICollection<EquipmentWriteOff> EquipmentWriteOffs { get; set; } = new List<EquipmentWriteOff>();

    public virtual Audience? IdAudienceNavigation { get; set; }

    public virtual Office? IdOfficesNavigation { get; set; }

    public virtual Worker? IdWorkerNavigation { get; set; }
    [NotMapped]

    public string NumberAudience { get; set; }
    [NotMapped]
    public string OfficesString { get; set; }
    [NotMapped]
    public Visibility StatusVisibility { get; set; }
    [NotMapped]
    public string Status { get; set; }
    [NotMapped]
    public string BushStatus { get; set; }

}
