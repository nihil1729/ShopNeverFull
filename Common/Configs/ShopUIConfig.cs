using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace ShopNeverFull.Common.Configs;

public class BackgroundPanelConfig
{
    [DefaultValue(true)] public bool Transparent = true;

    [Range(0f, 200f)] [DefaultValue(100f)] [Increment(1f)]
    public float Width = 100;

    [Range(0f, 100f)] [DefaultValue(60f)] [Increment(1f)]
    public float Height = 60;

    [Range(300f, 700f)] [DefaultValue(500f)] [Increment(1f)]
    public float Left = 500;

    [Range(200f, 500f)] [DefaultValue(365f)] [Increment(1f)]
    public float Top = 365;
}

public class ToggleButtonConfig
{
    [Range(0f, 100f)] [DefaultValue(45f)] [Increment(0.5f)]
    public float Width = 45;

    [Range(0f, 100f)] [DefaultValue(45f)] [Increment(0.5f)]
    public float Height = 45;

    [Range(-20f, 80f)] [DefaultValue(0f)] [Increment(0.5f)]
    public float Left;

    [DefaultValue(12f)] [Range(8f, 16f)] [Increment(0.5f)]
    public float Padding = 12f;

    [DefaultValue(0f)] [Increment(0.05f)] public float HAlign;

    [DefaultValue(1f)] [Increment(0.05f)] public float VAlign = 1f;
}

public class ShopUIConfig : ModConfig
{
    [DefaultValue(default(BackgroundPanelConfig))]
    public BackgroundPanelConfig BackgroundConfig = new();

    [DefaultValue(default(ToggleButtonConfig))]
    public ToggleButtonConfig PrevButtonConfig = new();

    [DefaultValue(default(ToggleButtonConfig))]
    public ToggleButtonConfig NextButtonConfig = new() { Left = 50 };

    public override ConfigScope Mode => ConfigScope.ClientSide;
}