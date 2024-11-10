using System.Collections.Generic;
using Microsoft.Xna.Framework;
using ShopNeverFull.Common.Configs;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace ShopNeverFull.Common.UI;

internal class ShopExpandUiPanel : UIPanel
{
    public UIButton<string> NextButton;
    public UIButton<string> PrevButton;

    public override void OnInitialize()
    {
        var configInstance = ModContent.GetInstance<ShopUIConfig>();

        PrevButton = new UIButton<string>("<")
        {
            ScalePanel = true,
            Left = { Pixels = configInstance.PrevButtonConfig.Left },
            Width = { Pixels = configInstance.PrevButtonConfig.Width },
            Height = { Pixels = configInstance.PrevButtonConfig.Height },
            HAlign = configInstance.PrevButtonConfig.HAlign,
            VAlign = configInstance.PrevButtonConfig.VAlign
        };
        PrevButton.SetPadding(configInstance.PrevButtonConfig.Padding);

        NextButton = new UIButton<string>(">")
        {
            ScalePanel = true,
            Left = { Pixels = configInstance.NextButtonConfig.Left },
            Width = { Pixels = configInstance.NextButtonConfig.Width },
            Height = { Pixels = configInstance.NextButtonConfig.Height },
            HAlign = configInstance.NextButtonConfig.HAlign,
            VAlign = configInstance.NextButtonConfig.VAlign
        };
        NextButton.SetPadding(configInstance.NextButtonConfig.Padding);

        Append(PrevButton);
        Append(NextButton);
    }

    public void SetLeftClick(MouseEvent[] mouseEvents)
    {
        if (mouseEvents == null) return;

        if (mouseEvents.Length < 2) return;

        PrevButton.OnLeftClick += mouseEvents[0];
        NextButton.OnLeftClick += mouseEvents[1];
    }
}

internal class ShopExpandUiState : UIState
{
    private readonly Color _defaultBackgroundColor = new Color(63, 82, 151) * 0.7f;
    private readonly Color _defaultBorderColor = Color.Black;
    private ShopExpandUiPanel _panel;

    public override void OnInitialize()
    {
        var configInstance = ModContent.GetInstance<ShopUIConfig>();

        _panel = new ShopExpandUiPanel
        {
            Width = { Pixels = configInstance.BackgroundConfig.Width },
            Height = { Pixels = configInstance.BackgroundConfig.Height },
            Left = { Pixels = configInstance.BackgroundConfig.Left },
            Top = { Pixels = configInstance.BackgroundConfig.Top }
        };
        _panel.SetPadding(0);

        Append(_panel);
    }

    private void SetPanelTransparent(bool transparent)
    {
        if (transparent)
        {
            _panel.BackgroundColor = Color.Transparent;
            _panel.BorderColor = Color.Transparent;
        }
        else
        {
            _panel.BackgroundColor = _defaultBackgroundColor;
            _panel.BorderColor = _defaultBorderColor;
        }
    }

    public void SetBackgroundConfig(BackgroundPanelConfig config)
    {
        SetPanelTransparent(config.Transparent);

        _panel.Left.Pixels = config.Left;
        _panel.Top.Pixels = config.Top;
        _panel.Width.Pixels = config.Width;
        _panel.Height.Pixels = config.Height;
        _panel.SetPadding(0);
    }

    public void SetToggleButtonConfig(ToggleButtonConfig config, bool prev = true)
    {
        var button = _panel.NextButton;
        if (prev) button = _panel.PrevButton;

        button.Left.Pixels = config.Left;
        button.Width.Pixels = config.Width;
        button.Height.Pixels = config.Height;
        button.SetPadding(config.Padding);
        button.HAlign = config.HAlign;
        button.VAlign = config.VAlign;
    }

    public void SetLeftClick(MouseEvent[] mouseEvents)
    {
        _panel.SetLeftClick(mouseEvents);
    }
}

[Autoload(Side = ModSide.Client)]
internal class ShopExpandSystem : ModSystem
{
    internal readonly Dictionary<string, int?> ShopIndexDict = new();
    private UserInterface _customInterface;
    private ShopExpandUiState _customUiState;
    private GameTime _lastUpdateUiGameTime;

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        var inventoryIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Inventory"));
        if (inventoryIndex != -1)
            layers.Insert(
                inventoryIndex - 1,
                new LegacyGameInterfaceLayer(
                    "ShopNeverFull: ShopExpandInterface",
                    delegate
                    {
                        if (_lastUpdateUiGameTime != null && _customInterface?.CurrentState != null)
                            _customInterface.Draw(Main.spriteBatch, _lastUpdateUiGameTime);

                        return true;
                    },
                    InterfaceScaleType.UI
                )
            );
    }

    public void ShowUI()
    {
        var configInstance = ModContent.GetInstance<ShopUIConfig>();
        _customUiState.SetBackgroundConfig(configInstance.BackgroundConfig);
        _customUiState.SetToggleButtonConfig(configInstance.PrevButtonConfig);
        _customUiState.SetToggleButtonConfig(configInstance.NextButtonConfig, false);

        _customInterface?.SetState(_customUiState);
    }

    private void HideUI()
    {
        _customInterface?.SetState(null);
    }

    public override void Load()
    {
        _customInterface = new UserInterface();

        _customUiState = new ShopExpandUiState();
        _customUiState.Activate();
    }

    public override void Unload()
    {
        _customUiState = null;
    }

    public override void UpdateUI(GameTime gameTime)
    {
        _lastUpdateUiGameTime = gameTime;
        if (_customInterface?.CurrentState != null) _customInterface.Update(gameTime);

        if (Main.LocalPlayer.TalkNPC == null) HideUI();
    }

    public void SetLeftClick(UIElement.MouseEvent[] mouseEvents)
    {
        _customUiState?.SetLeftClick(mouseEvents);
    }
}