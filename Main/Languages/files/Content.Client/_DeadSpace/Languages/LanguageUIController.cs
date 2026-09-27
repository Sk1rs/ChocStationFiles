// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT
// Sunrise: rebuilt on top of SimpleRadialMenu (the same menu the emotes use), since Sunrise has no RadialUiController.

using System.Linq;
using Content.Client.Gameplay;
using Content.Client.UserInterface.Controls;
using Content.Shared.DeadSpace.Languages;
using Content.Shared.DeadSpace.Languages.Components;
using Content.Shared.DeadSpace.Languages.Prototypes;
using Content.Shared.Input;
using JetBrains.Annotations;
using Robust.Client.Player;
using Robust.Client.UserInterface.Controllers;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Input.Binding;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Client.DeadSpace.Languages;

[UsedImplicitly]
public sealed class LanguageUIController : UIController, IOnStateChanged<GameplayState>
{
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

    private static readonly SpriteSpecifier DefaultIcon =
        new SpriteSpecifier.Texture(new ResPath("/Textures/_DeadSpace/LanguageIcons/default.png"));

    private MenuButton? LanguageButton =>
        UIManager.GetActiveUIWidgetOrNull<UserInterface.Systems.MenuBar.Widgets.GameTopMenuBar>()?.LanguageButton;

    private SimpleRadialMenu? _menu;

    public void OnStateEntered(GameplayState state)
    {
        CommandBinds.Builder
            .Bind(ContentKeyFunctions.OpenLanguageMenu,
                InputCmdHandler.FromDelegate(_ => ToggleMenu(false)))
            .Register<LanguageUIController>();
    }

    public void OnStateExited(GameplayState state)
    {
        CommandBinds.Unregister<LanguageUIController>();
        CloseMenu();
    }

    public void LoadButton()
    {
        if (LanguageButton != null)
            LanguageButton.OnPressed += ActionButtonPressed;
    }

    public void UnloadButton()
    {
        if (LanguageButton != null)
            LanguageButton.OnPressed -= ActionButtonPressed;
    }

    private void ActionButtonPressed(BaseButton.ButtonEventArgs args)
    {
        ToggleMenu(true);
    }

    private void ToggleMenu(bool centered)
    {
        if (_menu != null)
        {
            CloseMenu();
            return;
        }

        if (_player.LocalEntity is not { } player
            || !EntityManager.TryGetComponent<LanguageComponent>(player, out var component))
        {
            return;
        }

        var models = new List<RadialMenuOptionBase>();
        foreach (var id in component.KnownLanguages.Except(component.CantSpeakLanguages))
        {
            if (!_proto.TryIndex(id, out var language))
                continue;

            var name = Loc.GetString(language.Name);
            if (id == component.SelectedLanguage)
                name = Loc.GetString("language-menu-selected", ("language", name));

            models.Add(new RadialMenuActionOption<ProtoId<LanguagePrototype>>(SelectLanguage, id)
            {
                IconSpecifier = RadialMenuIconSpecifier.With(language.Icon ?? DefaultIcon),
                ToolTip = name,
            });
        }

        if (models.Count == 0)
            return;

        _menu = new SimpleRadialMenu();
        _menu.SetButtons(models);
        _menu.OnClose += OnMenuClosed;
        _menu.Open();

        if (LanguageButton != null)
            LanguageButton.SetClickPressed(true);

        if (centered)
            _menu.OpenCentered();
        else
            _menu.OpenOverMouseScreenPosition();
    }

    private void SelectLanguage(ProtoId<LanguagePrototype> id)
    {
        if (_player.LocalEntity is not { } player)
            return;

        EntityManager.RaisePredictiveEvent(new SelectLanguageEvent(player.Id, id));
    }

    private void OnMenuClosed()
    {
        CloseMenu();
    }

    private void CloseMenu()
    {
        if (LanguageButton != null)
            LanguageButton.Pressed = false;

        if (_menu == null)
            return;

        _menu.OnClose -= OnMenuClosed;
        _menu.Close();
        _menu = null;
    }
}
