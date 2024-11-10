using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace ShopNeverFull.Common.Configs
{
    public class BackgroundPanelConfig
    {
        [Range(0f, 1000f)] [DefaultValue(100f)]
        public float Width = 100;

        [Range(0f, 1000f)] [DefaultValue(60f)] public float Height = 60;

        [Range(0f, 1000f)] [DefaultValue(500f)]
        public float Left = 500;

        [Range(0f, 1000f)] [DefaultValue(362f)]
        public float Top = 362;
    }

    public class ToggleButtonConfig
    {
        [Range(0f, 500f)] [DefaultValue(45f)] public float Width = 45;

        [Range(0f, 500f)] [DefaultValue(45f)] public float Height = 45;

        [Range(0f, 500f)] [DefaultValue(0f)] public float Left;
    }

    public class ShopUIConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;

        [DefaultValue(true)] public bool Transparent;

        [DefaultValue(default(BackgroundPanelConfig))]
        public BackgroundPanelConfig BackgroundUIConfig = new();

        [DefaultValue(default(ToggleButtonConfig))]
        public ToggleButtonConfig PrevButtonConfig = new();

        [DefaultValue(default(ToggleButtonConfig))]
        public ToggleButtonConfig NextButtonConfig = new() { Left = 40 };
    }
}