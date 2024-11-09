using Terraria.ModLoader;

namespace ShopNeverFull
{
    // Please read https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Modding-Guide#mod-skeleton-contents for more information about the various files in a mod.
    public class ShopNeverFull : Mod
    {
        private Mod _calamityMod;
        public ModItem FabStaff;
        public ModItem TheDanceOfLight;

        public override void PostSetupContent()
        {
            if (ModLoader.TryGetMod("CalamityMod", out _calamityMod))
            {
                // 获取绝学法杖
                if (_calamityMod.TryFind("Fabstaff", out FabStaff))
                {
                }

                // 获取光之舞
                if (_calamityMod.TryFind("TheDanceofLight", out TheDanceOfLight))
                {
                }
            }
        }
    }
}