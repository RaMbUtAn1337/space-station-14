using Content.Client.Message;
using Content.Client.UserInterface.Systems.EscapeMenu;
using Robust.Client.AutoGenerated;
using Robust.Client.Console;
using Robust.Client.State;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.XAML;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using System.Numerics;
using Content.Client._Sunrise.ServersHub;
using Content.Client.Parallax.Managers;
using Content.Client.Resources;
using Content.Shared._Sunrise.SunriseCCVars;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Shared.Configuration;
using Robust.Shared.Input;
using Content.Shared._Sunrise.SunriseCCVars;  // Sunrise

namespace Content.Client.Lobby.UI
{
    [GenerateTypedNameReferences]
    public sealed partial class LobbyGui : UIScreen
    {
        [Dependency] private readonly IClientConsoleHost _consoleHost = default!;
        [Dependency] private readonly IUserInterfaceManager _userInterfaceManager = default!;
        [Dependency] private readonly IGameTiming _timing = default!;
        [Dependency] private readonly IParallaxManager _parallaxManager = default!;
        [Dependency] private readonly IRobustRandom _random = default!;
        [Dependency] private readonly IResourceCache _resourceCache = default!;
        [Dependency] private readonly IConfigurationManager _configurationManager = default!;

        public string LobbyParalax = "FastSpace"; // Sunrise-edit
        [ViewVariables(VVAccess.ReadWrite)] public Vector2 Offset { get; set; } // Sunrise-edit

        private readonly StyleBoxTexture _back;

        public LobbyGui()
        {
            RobustXamlLoader.Load(this);
            IoCManager.InjectDependencies(this);
            SetAnchorPreset(MainContainer, LayoutPreset.Wide);
            SetAnchorPreset(Background, LayoutPreset.Wide);

            LobbySong.SetMarkup(Loc.GetString("lobby-state-song-no-song-text"));

            LeaveButton.OnPressed += _ => _consoleHost.ExecuteCommand("disconnect");
            OptionsButton.OnPressed += _ => UserInterfaceManager.GetUIController<OptionsUIController>().ToggleWindow();

            ChatHeader.OnKeyBindUp += args =>
            {
                if (args.Function != EngineKeyFunctions.Use)
                    return;

                ChatContent.Visible = !ChatContent.Visible;
            };

            ServerInfoHeader.OnKeyBindUp += args =>
            {
                if (args.Function != EngineKeyFunctions.Use)
                    return;

                ServerInfoContent.Visible = !ServerInfoContent.Visible;
            };

            CharacterInfoHeader.OnKeyBindUp += args =>
            {
                if (args.Function != EngineKeyFunctions.Use)
                    return;

                CharacterInfoContent.Visible = !CharacterInfoContent.Visible;
            };

            ServersHubHeader.OnKeyBindUp += args =>
            {
                if (args.Function != EngineKeyFunctions.Use)
                    return;

                ServersHubContent.Visible = !ServersHubContent.Visible;
            };

            ChangelogHeader.OnKeyBindUp += args =>
            {
                if (args.Function != EngineKeyFunctions.Use)
                    return;

                ChangelogContent.Visible = !ChangelogContent.Visible;
            };

            // Sunrise-start
            Offset = new Vector2(_random.Next(0, 1000), _random.Next(0, 1000));

            _parallaxManager.LoadParallaxByName(LobbyParalax);
            RectClipContent = true;

            var panelTex = _resourceCache.GetTexture("/Textures/Interface/Nano/button.svg.96dpi.png");
            _back = new StyleBoxTexture
            {
                Texture = panelTex,
                Modulate = new Color(37, 37, 42),
            };
            _back.SetPatchMargin(StyleBox.Margin.All, 10);

            LeftTopPanel.PanelOverride = _back;

            RightTopPanel.PanelOverride = _back;

            RightBottomPanel.PanelOverride = _back;

            LeftBottomPanel.PanelOverride = _back;

            LeftTopPanel.PanelOverride = _back;

            LobbySongPanel.PanelOverride = _back;

            _configurationManager.OnValueChanged(SunriseCCVars.LobbyOpacity, OnLobbyOpacityChanged);
            _configurationManager.OnValueChanged(SunriseCCVars.LobbyBackground, OnLobbyBackgroundChanged);
            _configurationManager.OnValueChanged(SunriseCCVars.ServersHubEnable, OnServersHubEnableChanged);

            SetLobbyOpacity(_configurationManager.GetCVar(SunriseCCVars.LobbyOpacity));
            SetLobbyBackgroundType(_configurationManager.GetCVar(SunriseCCVars.LobbyBackground));
            SetServersHubEnable(_configurationManager.GetCVar(SunriseCCVars.ServersHubEnable));

            Chat.SetChatOpacity();

            ServerName.Text = Loc.GetString("ui-lobby-cfgwelcome", ("name", _configurationManager.GetCVar(SunriseCCVars.ServerName)));
            // Sunrise-end
        }

        private void OnServersHubEnableChanged(bool enable)
        {
            SetServersHubEnable(enable);
        }

        private void SetServersHubEnable(bool enable)
        {
            ServersHubBox.Visible = enable;
        }

        private void OnLobbyBackgroundChanged(string lobbyBackgroundString)
        {
            SetLobbyBackgroundType(lobbyBackgroundString);
        }

        private void SetLobbyBackgroundType(string lobbyBackgroundString)
        {
            if (!Enum.TryParse(lobbyBackgroundString, out LobbyBackgroundType lobbyBackgroundTypeString))
            {
                lobbyBackgroundTypeString = default;
            }

            switch (lobbyBackgroundTypeString)
            {
                case LobbyBackgroundType.Paralax:
                    LobbyImage.Visible = true;
                    Background.Visible = false;
                    break;
                case LobbyBackgroundType.Art:
                    LobbyImage.Visible = false;
                    Background.Visible = true;
                    break;
            }
        }

        // Sunrise-Start
        private void OnLobbyOpacityChanged(float opacity)
        {
            SetLobbyOpacity(opacity);
        }

        private void SetLobbyOpacity(float opacity)
        {
            _back.Modulate = new Color(37, 37, 42).WithAlpha(opacity);
        }
        // Sunrise-End

        public void SwitchState(LobbyGuiState state)
        {
            DefaultState.Visible = false;
            CharacterSetupState.Visible = false;

            switch (state)
            {
                case LobbyGuiState.Default:
                    DefaultState.Visible = true;
                    break;
                case LobbyGuiState.CharacterSetup:
                    CharacterSetupState.Visible = true;

                    UserInterfaceManager.GetUIController<LobbyUIController>().ReloadCharacterSetup();

                    break;
            }
        }

        // Sunrise-start
        protected override void Draw(DrawingHandleScreen handle)
        {
            foreach (var layer in _parallaxManager.GetParallaxLayers(LobbyParalax))
            {
                var tex = layer.Texture;
                var texSize = new Vector2i(
                    (tex.Size.X * (int)Size.X * 1) / 1920,
                    (tex.Size.Y * (int)Size.X * 1) / 1920
                );
                var ourSize = PixelSize;

                var currentTime = (float) _timing.RealTime.TotalSeconds;
                var offset = Offset + new Vector2(currentTime * 100f, currentTime * 0f);

                if (layer.Config.Tiled)
                {
                    // Multiply offset by slowness to match normal parallax
                    var scaledOffset = (offset * layer.Config.Slowness).Floored();

                    // Then modulo the scaled offset by the size to prevent drawing a bunch of offscreen tiles for really small images.
                    scaledOffset.X %= texSize.X;
                    scaledOffset.Y %= texSize.Y;

                    // Note: scaledOffset must never be below 0 or there will be visual issues.
                    // It could be allowed to be >= texSize on a given axis but that would be wasteful.

                    for (var x = -scaledOffset.X; x < ourSize.X; x += texSize.X)
                    {
                        for (var y = -scaledOffset.Y; y < ourSize.Y; y += texSize.Y)
                        {
                            handle.DrawTextureRect(tex, UIBox2.FromDimensions(new Vector2(x, y), texSize));
                        }
                    }
                }
                else
                {
                    var origin = ((ourSize - texSize) / 2) + layer.Config.ControlHomePosition;
                    handle.DrawTextureRect(tex, UIBox2.FromDimensions(origin, texSize));
                }
            }
        }
        // Sunrise-end

        public enum LobbyGuiState : byte
        {
            /// <summary>
            ///  The default state, i.e., what's seen on launch.
            /// </summary>
            Default,
            /// <summary>
            ///  The character setup state.
            /// </summary>
            CharacterSetup
        }
    }
}
