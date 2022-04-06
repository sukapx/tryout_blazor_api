using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tryout_blazor_api.Shared.Play;

public class CargoHold
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public ulong Id { get; set; }

    public uint Cargospace { get; set; } = 10;
    public Dictionary<MarketItemType, uint> Items { get; set; } = new();

    public uint GetFreeCargo()
    {
        return Cargospace - GetCargo();
    }

    public uint GetCargo(MarketItemType? type = null)
    {
        uint used = 0;
        if (Items.Count == 0)
            return used;

        if (type is not null)
        {
            Items.TryGetValue((MarketItemType)type!, out used);
        }
        else
        {
            foreach (var entry in Items)
            {
                if (type is null || type! == entry.Key)
                    used += entry.Value;
            }
        }
        return used;
    }

    public bool CanChangeCargo(MarketItemType type, int amount)
    {
        if (amount == 0)
            return true;

        if (amount > 0)
        {
            if (GetFreeCargo() < amount)
            {
                return false;
            }
        }
        else if (amount < 0)
        {
            if (GetCargo(type) < amount)
            {
                return false;
            }
        }
        return true;
    }

    public bool ChangeCargo(MarketItemType type, int amount)
    {
        if (!CanChangeCargo(type, amount))
            return false;

        if (amount > 0)
        {
            if (!Items.ContainsKey(type))
            {
                Items.Add(type, (uint)amount);
            }
            else
            {
                Items[type] += (uint)amount;
            }
        }
        else if (amount < 0)
        {
            Items[type] -= (uint)-amount;
            if (Items[type] == 0)
            {
                Items.Remove(type);
            }
        }
        return true;
    }
}
