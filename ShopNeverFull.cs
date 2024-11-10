using ShopNeverFull.Common;
using Terraria.ModLoader;

namespace ShopNeverFull;

public class ShopNeverFull : Mod
{
    public override void Load()
    {
        ShopPageSwitcher.Load();
    }

    public override void Unload()
    {
        ShopPageSwitcher.Unload();
    }
}