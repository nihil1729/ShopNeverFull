using System;
using System.Collections.Generic;
using ShopNeverFull.Common.UI;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace ShopNeverFull
{
    public class ShopNeverFull : Mod
    {
        // private Mod _calamityMod;
        // public ModItem FabStaff;

        // public override void PostSetupContent()
        // {
        //     if (ModLoader.TryGetMod("CalamityMod", out _calamityMod))
        //     {
        //         // 获取绝学法杖
        //         if (_calamityMod.TryFind("Fabstaff", out FabStaff))
        //         {
        //         }
        //     }
        // }

        public override void Load()
        {
            On_Chest.SetupShop_string_NPC += On_ChestOnSetupShop_string_NPC;
        }

        public override void Unload()
        {
            On_Chest.SetupShop_string_NPC -= On_ChestOnSetupShop_string_NPC;
        }

        private void On_ChestOnSetupShop_string_NPC(On_Chest.orig_SetupShop_string_NPC orig, Chest self,
            string shopName, NPC npc)
        {
            var itemList = new List<Item>();
            Array.Fill(self.item, null);

            var systemInstance = ModContent.GetInstance<ShopExpandSystem>();

            if (NPCShopDatabase.TryGetNPCShop(shopName, out var shop))
            {
                if (!systemInstance.ShopIndexDict.ContainsKey(shopName))
                {
                    systemInstance.ShopIndexDict.Add(shopName, null);
                }

                shop.FillShop(itemList, npc);
            }

            if (npc == null)
            {
                return;
            }

            if (itemList.Count > Chest.maxItems - 1)
            {
                systemInstance.ShopIndexDict[shopName] ??= 0;
                systemInstance.ShowUi();
            }

            ShopPageContext shopPageContext = new ShopPageContext()
            {
                AllItems = itemList,
                SelfChest = self,
                NPC = npc,
                ShopName = shopName
            };

            SetShopPage(shopPageContext);

            var events = new UIElement.MouseEvent[2];
            var offsetDict = new Dictionary<int, int>()
            {
                { 0, -1 }, { 1, 1 }
            };

            for (int i = 0; i < events.Length; ++i)
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
            public NPC NPC;
            public string ShopName;
            public int? CurPageIndex;
            public int TotalPages => (int)Math.Ceiling((double)AllItems.Count / (Chest.maxItems - 1));
        }

        private void SetShopPage(ShopPageContext ctx)
        {
            var allItems = ctx.AllItems;
            var chest = ctx.SelfChest;
            var npc = ctx.NPC;
            var shopName = ctx.ShopName;
            var totalPages = ctx.TotalPages;

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
                NPCLoader.ModifyActiveShop(npc, shopName, chest.item);
            }

            foreach (ref var item in chest.item.AsSpan())
            {
                item ??= new Item();
                item.isAShopItem = true;
            }
        }
    }
}