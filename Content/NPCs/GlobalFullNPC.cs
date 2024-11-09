using System;
using Terraria.ModLoader;
using Terraria;
using System.Collections.Generic;
using System.Reflection;
using ShopNeverFull.Common.UI;
using Terraria.ModLoader.Core;
using Terraria.UI;

namespace ShopNeverFull.Content.NPCs
{
    public class GlobalFullNpc : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        private static readonly FieldInfo HookModifyActiveShopFieldInfo =
            typeof(NPCLoader).GetField("HookModifyActiveShop", BindingFlags.NonPublic | BindingFlags.Static)!;

        private static GlobalHookList<GlobalNPC> _hookModifyActiveShop = null!;

        public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
        {
            // 已经加载过并且是城镇NPC
            return lateInstantiation && entity.townNPC;
        }

        public override void Load()
        {
            _hookModifyActiveShop = (GlobalHookList<GlobalNPC>)HookModifyActiveShopFieldInfo.GetValue(null)!;
            On_Chest.SetupShop_string_NPC += On_ChestOnSetupShop_string_NPC;
        }

        public override void Unload()
        {
            _hookModifyActiveShop = null!;
            On_Chest.SetupShop_string_NPC -= On_ChestOnSetupShop_string_NPC;
        }

        private void On_ChestOnSetupShop_string_NPC(On_Chest.orig_SetupShop_string_NPC orig, Chest self,
            string shopName, NPC npc)
        {
            var items = new List<Item>();

            if (NPCShopDatabase.TryGetNPCShop(shopName, out var shop))
            {
                shop.FillShop(items, npc);
            }

            self.item = npc != null
                ? ModifyActiveShop(npc, shopName, items)
                : items.ToArray();

            foreach (ref var item in self.item.AsSpan())
            {
                item ??= new Item();
                item.isAShopItem = true;
            }

            items.RemoveAll(item => item == null);
            if (items.Count <= Chest.maxItems - 1)
            {
                return;
            }

            var systemInstance = ModContent.GetInstance<ShopExpandSystem>();

            systemInstance.CurIndex ??= 0;
            systemInstance.ShowUi();
        }

        private static Item[] ModifyActiveShop(NPC npc, string shopName, List<Item> items)
        {
            const int extraCapacity = Chest.maxItems;

            items.EnsureCapacity(items.Count + extraCapacity);
            for (var i = 0; i < extraCapacity; ++i)
            {
                items.Add(null);
            }

            var shopContentsArray = items.ToArray();

            NPCLoader.GetNPC(npc.type)?.ModifyActiveShop(shopName, shopContentsArray);
            foreach (var g in _hookModifyActiveShop.Enumerate(npc))
            {
                g.ModifyActiveShop(npc, shopName, shopContentsArray);
            }

            return shopContentsArray;
        }
    }
}