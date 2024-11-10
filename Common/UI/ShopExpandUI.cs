using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
using Microsoft.Xna.Framework;
using ShopNeverFull.Common.Configs;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader.UI;

namespace ShopNeverFull.Common.UI
{
    internal class ShopExpandUiPanel : UIPanel
    {
        public UIButton<string> PrevButton;
        public UIButton<string> NextButton;

        public override void OnInitialize()
        {
            PrevButton = new UIButton<string>("<")
            {
                ScalePanel = true,
                Width = { Pixels = 45 },
                Height = { Pixels = 45 },
                Left = { Pixels = 0 }
            };

            NextButton = new UIButton<string>(">")
            {
                ScalePanel = true,
                Width = { Pixels = 45 },
                Height = { Pixels = 45 },
                Left = { Pixels = 40 }
            };

            Append(PrevButton);
            Append(NextButton);
        }

        public void SetLeftClick(MouseEvent[] mouseEvents)
        {
            if (mouseEvents == null)
            {
                return;
            }

            if (mouseEvents.Length < 2)
            {
                return;
            }

            PrevButton.OnLeftClick += mouseEvents[0];
            NextButton.OnLeftClick += mouseEvents[1];
        }
    }

    internal class ShopExpandUiState : UIState
    {
        private ShopExpandUiPanel _panel;
        private readonly Color _defaultBackgroundColor = new Color(63, 82, 151) * 0.7f;
        private readonly Color _defaultBorderColor = Color.Black;

        public override void OnInitialize()
        {
            _panel = new ShopExpandUiPanel
            {
                Width = { Pixels = 100 },
                Height = { Pixels = 60 },
                Left = { Pixels = 500 },
                Top = { Pixels = 362 }
            };

            Append(_panel);
        }

        public void SetPanelTransparent(bool transparent)
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
            _panel.Left.Pixels = config.Left;
            _panel.Top.Pixels = config.Top;
            _panel.Width.Pixels = config.Width;
            _panel.Height.Pixels = config.Height;
        }

        public void SetToggleButtonConfig(ToggleButtonConfig config, bool prev = true)
        {
            var button = _panel.NextButton;
            if (prev)
            {
                button = _panel.PrevButton;
            }

            button.Left.Pixels = config.Left;
            button.Width.Pixels = config.Width;
            button.Height.Pixels = config.Height;
        }

        public void SetLeftClick(MouseEvent[] mouseEvents)
        {
            _panel.SetLeftClick(mouseEvents);
        }
    }

    [Autoload(Side = ModSide.Client)]
    internal class ShopExpandSystem : ModSystem
    {
        private UserInterface _customInterface;
        private ShopExpandUiState _customUiState;
        internal readonly Dictionary<string, int?> ShopIndexDict = new();
        private GameTime _lastUpdateUiGameTime;

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            var inventoryIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Inventory"));
            if (inventoryIndex != -1)
            {
                layers.Insert(
                    inventoryIndex - 1,
                    new LegacyGameInterfaceLayer(
                        "ShopNeverFull: ShopExpandInterface",
                        delegate
                        {
                            if (_lastUpdateUiGameTime != null && _customInterface?.CurrentState != null)
                            {
                                _customInterface.Draw(Main.spriteBatch, _lastUpdateUiGameTime);
                            }

                            return true;
                        },
                        InterfaceScaleType.UI
                    )
                );
            }
        }

        public void ShowUI()
        {
            var configInstance = ModContent.GetInstance<ShopUIConfig>();
            _customUiState.SetPanelTransparent(configInstance.Transparent);
            _customUiState.SetBackgroundConfig(configInstance.BackgroundUIConfig);
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
            if (_customInterface?.CurrentState != null)
            {
                _customInterface.Update(gameTime);
            }

            if (Main.LocalPlayer.TalkNPC == null)
            {
                HideUI();
            }
        }

        public void SetLeftClick(UIElement.MouseEvent[] mouseEvents)
        {
            _customUiState?.SetLeftClick(mouseEvents);
        }
    }
}