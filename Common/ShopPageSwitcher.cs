using System;
using System.Collections.Generic;
using System.Reflection;
using ShopNeverFull.Common.UI;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;
using Terraria.ModLoader.Default;
using Terraria.UI;

namespace ShopNeverFull.Common
{
    using HookList = GlobalHookList<GlobalNPC>;

    public static class ShopPageSwitcher
    {
        private static HookList _hookModifyActiveShop;

        public static void Load()
        {
            On_Chest.SetupShop_string_NPC += On_ChestOnSetupShop_string_NPC;
            _hookModifyActiveShop =
                (HookList)typeof(NPCLoader)
                    .GetField("HookModifyActiveShop", BindingFlags.NonPublic | BindingFlags.Static)?.GetValue(null);
        }

        public static void Unload()
        {
            On_Chest.SetupShop_string_NPC -= On_ChestOnSetupShop_string_NPC;
            _hookModifyActiveShop = null;
        }

        private static void On_ChestOnSetupShop_string_NPC(On_Chest.orig_SetupShop_string_NPC orig, Chest self,
            string shopName, NPC npc)
        {
            var itemList = new List<Item>();
            Array.Fill(self.item, null);

            var systemInstance = ModContent.GetInstance<ShopExpandSystem>();

            if (NPCShopDatabase.TryGetNPCShop(shopName, out var shop))
            {
                systemInstance.ShopIndexDict.TryAdd(shopName, null);

                shop.FillShop(itemList, npc);
            }

            if (npc == null)
            {
                return;
            }

            if (itemList.Count > Chest.maxItems - 1)
            {
                systemInstance.ShopIndexDict[shopName] ??= 0;
                systemInstance.ShowUI();
            }

            var shopPageContext = new ShopPageContext()
            {
                AllItems = itemList,
                SelfChest = self,
                Npc = npc,
                ShopName = shopName
            };

            SetShopPage(shopPageContext);

            var events = new UIElement.MouseEvent[2];
            var offsetDict = new Dictionary<int, int>()
            {
                { 0, -1 }, { 1, 1 }
            };

            for (var i = 0; i < events.Length; ++i)
            {
                var offset = offsetDict[i];
                events[i] += delegate
                {
                    var oldIndex = systemInstance.ShopIndexDict[shopName];
                    shopPageContext.CurPageIndex = oldIndex;

                    var newIndex = oldIndex + offset;
                    if (newIndex >= 0 && newIndex < shopPageContext.TotalPages)
                    {
                        shopPageContext.CurPageIndex = newIndex;
                    }

                    SetShopPage(shopPageContext);
                    systemInstance.ShopIndexDict[shopName] = shopPageContext.CurPageIndex;
                };
            }

            systemInstance.SetLeftClick(events);
        }

        private struct ShopPageContext
        {
            public List<Item> AllItems;
            public Chest SelfChest;
            public NPC Npc;
            public string ShopName;
            public int? CurPageIndex;
            public int TotalPages => (int)Math.Ceiling((double)AllItems.Count / (Chest.maxItems - 1));
        }

        private static void SetShopPage(ShopPageContext ctx)
        {
            var allItems = ctx.AllItems;
            var chest = ctx.SelfChest;
            var npc = ctx.Npc;
            var shopName = ctx.ShopName;

            var curPageIndex = ctx.CurPageIndex ??= 0;

            var startIndex = curPageIndex * (Chest.maxItems - 1);
            var copyCount = Math.Min(allItems.Count - startIndex, Chest.maxItems - 1);

            Array.Fill(chest.item, null);
            ctx.AllItems.CopyTo(startIndex, chest.item, 0, copyCount);

            for (var i = copyCount; i < Chest.maxItems; ++i)
            {
                chest.item[i] = new Item(0);
            }

            if (npc != null)
            {
                // 不使用NPCLoader的该方法，使用自定义方法
                ModifyActiveShop(npc, shopName, chest.item);
            }

            foreach (ref var item in chest.item.AsSpan())
            {
                item ??= new Item();
                item.isAShopItem = true;
            }
        }

        private static void ModifyActiveShop(NPC npc, string shopName, Item[] shopContents)
        {
            NPCLoader.GetNPC(npc.type)?.ModifyActiveShop(shopName, shopContents);
            foreach (var g in _hookModifyActiveShop.Enumerate(npc))
            {
                if (g is PylonShopNPC)
                {
                    continue;
                }

                g.ModifyActiveShop(npc, shopName, shopContents);
            }
        }
    }
}