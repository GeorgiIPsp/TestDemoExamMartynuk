using System;
using System.Collections.Generic;

namespace ExamenDem_First.Models;

public partial class Post
{
    public int IdPost { get; set; }

    public string TitlePost { get; set; } = null!;

    public virtual ICollection<Worker> Workers { get; set; } = new List<Worker>();
}
