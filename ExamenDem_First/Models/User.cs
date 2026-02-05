using System;
using System.Collections.Generic;

namespace ExamenDem_First.Models;

public partial class User
{
    public int IdUser { get; set; }

    public string Login { get; set; } = null!;

    public string Password { get; set; } = null!;

    public int IdWorker { get; set; }

    public virtual Worker IdWorkerNavigation { get; set; } = null!;
}
