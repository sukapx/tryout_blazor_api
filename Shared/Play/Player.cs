using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using tryout_blazor_api.Shared.Map;

namespace tryout_blazor_api.Shared.Play;


public class Player
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public ulong Id { get; set; }

    public string Owner { get; set; }

    [Required]
    public string Name { get; set; }
    public Location Location { get; set; } = new();

    public Market? DockedTo { get; set; }

    [Required]
    public CargoHold Cargo { get; set; } = new() { Cargospace = 10 };

}

