using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ShopNeverFull.Common.GlobalNPCs;

public sealed class PylonPatchNPC : GlobalNPC
{
    private static List<NPCShop.Entry> _pylonEntries;

    public override void ModifyShop(NPCShop shop)
    {
        _pylonEntries ??= NPCShopDatabase.GetPylonEntries().ToList();
    }

    public override void Unload()
    {
        _pylonEntries = null;
    }

    public override void ModifyActiveShop(NPC npc, string shopName, Item[] items)
    {
        if (shopName == NPCShopDatabase.GetShopName(NPCID.DD2Bartender))
            AddPylonsToBartenderShop(npc, items);
    }

    private static void AddPylonsToBartenderShop(NPC npc, Item[] items)
    {
        var slot = 0;

        for (; slot < items.Length; ++slot)
        {
            if (!items[slot].IsAir) continue;

            break;
        }

        if (slot == items.Length || slot == Chest.maxItems - 1) return;

        foreach (var entry in _pylonEntries)
        {
            if (entry.Disabled || !entry.ConditionsMet())
                continue;

            items[slot] = entry.Item.Clone();
            entry.OnShopOpen(items[slot], npc);

            do
            {
                if (++slot >= items.Length)
                    return;
            } while (!items[slot].IsAir);
        }
    }
}