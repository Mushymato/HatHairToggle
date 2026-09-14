using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;

namespace HatHairToggle;

public sealed class ModConfig
{
    public KeybindList ToggleKey = KeybindList.Parse("MouseRight,LeftStick");
}

public sealed class ModEntry : Mod
{
    public const string ModId = "mushymato.HatHairToggle";
    private IBetterGameMenuApi? BGM;
    private ModConfig config = new();

    public override void Entry(IModHelper helper)
    {
        I18n.Init(helper.Translation);
        config = helper.ReadConfig<ModConfig>();

        helper.Events.GameLoop.GameLaunched += OnGameLaunched;
        helper.Events.Input.ButtonsChanged += OnButtonsChanged;
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        BGM = Helper.ModRegistry.GetApi<IBetterGameMenuApi>("leclair.bettergamemenu");
        if (
            Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu")
            is IGenericModConfigMenuApi gmcm
        )
        {
            gmcm.Register(
                ModManifest,
                reset: () =>
                {
                    config.ToggleKey = KeybindList.Parse("RightClick,LeftStick");
                    Helper.WriteConfig(config);
                },
                save: () =>
                {
                    Helper.WriteConfig(config);
                }
            );
            gmcm.AddKeybindList(
                ModManifest,
                () => config.ToggleKey,
                (value) => config.ToggleKey = value,
                I18n.Config_ToggleKey_Name,
                I18n.Config_ToggleKey_Desc
            );
        }
    }

    public bool IsGameMenu(IClickableMenu menu)
    {
        if (menu == null)
            return false;
        if (menu is GameMenu)
            return true;
        return BGM?.IsMenu(menu) ?? false;
    }

    public InventoryPage? GetInventoryPage(IClickableMenu menu)
    {
        if (menu == null)
            return null;
        IClickableMenu? page;
        if (menu is GameMenu gameMenu)
        {
            page = gameMenu.GetCurrentPage();
        }
        else
        {
            page = BGM?.GetCurrentPage(menu);
        }
        if (page is InventoryPage inventoryPage)
        {
            return inventoryPage;
        }
        return null;
    }

    private void OnButtonsChanged(object? sender, ButtonsChangedEventArgs e)
    {
        if (Game1.player.hat.Value is not Hat hat)
            return;
        if (!config.ToggleKey.JustPressed())
            return;
        if (GetInventoryPage(Game1.activeClickableMenu) is not InventoryPage menu)
            return;
        int x = Game1.getOldMouseX();
        int y = Game1.getOldMouseY();
        foreach (ClickableComponent cc in menu.equipmentIcons)
        {
            if (cc.name == "Hat" && cc.containsPoint(x, y))
            {
                int hairDrawType = hat.hairDrawType.Value;
                int newHairDrawType = (hairDrawType + 1) % 3;
                hat.hairDrawType.Value = newHairDrawType;
                Monitor.Log($"hat.hairDrawType.Value {hairDrawType} -> {newHairDrawType}");
                return;
            }
        }
    }
}
