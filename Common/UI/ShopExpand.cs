using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
using Microsoft.Xna.Framework;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader.UI;

namespace ShopNeverFull.Common.UI
{
    class ShopExpandUiPanel : UIPanel
    {
        private UIButton<string> _prevButton;
        private UIButton<string> _nextButton;

        public override void OnInitialize()
        {
            _prevButton = new UIButton<string>("<")
            {
                ScalePanel = true,
                Height = { Pixels = 50 },
                HAlign = 0.1f,
                VAlign = 0.5f
            };

            _nextButton = new UIButton<string>(">")
            {
                ScalePanel = true,
                Height = { Pixels = 50 },
                HAlign = 0.9f,
                VAlign = 0.5f
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

    class ShopExpandUiState : UIState
    {
        private ShopExpandUiPanel _panel;

        public override void OnInitialize()
        {
            _panel = new ShopExpandUiPanel()
            {
                Width = { Pixels = 120 },
                Height = { Pixels = 70 },
                Left = { Pixels = 500 },
                Top = { Pixels = 355 }
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
        internal int? CurIndex;
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

        public void ShowUi()
        {
            _customInterface?.SetState(_customUiState);
        }

        private void HideUi()
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
                HideUi();
            }
        }

        public void SetLeftClick(UIElement.MouseEvent[] mouseEvents)
        {
            _customUiState?.SetLeftClick(mouseEvents);
        }
    }
}