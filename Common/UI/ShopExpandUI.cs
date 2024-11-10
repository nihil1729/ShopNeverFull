using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
using Microsoft.Xna.Framework;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader.UI;

namespace ShopNeverFull.Common.UI
{
    internal class ShopExpandUiPanel : UIPanel
    {
        private UIButton<string> _prevButton;
        private UIButton<string> _nextButton;

        public override void OnInitialize()
        {
            _prevButton = new UIButton<string>("<")
            {
                ScalePanel = true,
                Height = { Pixels = 45 },
                HAlign = 0.01f
            };

            _nextButton = new UIButton<string>(">")
            {
                ScalePanel = true,
                Height = { Pixels = 45 },
                HAlign = 0.99f
            };

            Append(_prevButton);
            Append(_nextButton);
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

            _prevButton.OnLeftClick += mouseEvents[0];
            _nextButton.OnLeftClick += mouseEvents[1];
        }
    }

    internal class ShopExpandUiState : UIState
    {
        private ShopExpandUiPanel _panel;

        public override void OnInitialize()
        {
            _panel = new ShopExpandUiPanel()
            {
                Width = { Pixels = 100 },
                Height = { Pixels = 60 },
                Left = { Pixels = 500 },
                Top = { Pixels = 362 }
            };

            Append(_panel);
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