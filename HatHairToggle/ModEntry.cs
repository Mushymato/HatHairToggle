using Netcode;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;

namespace HatHairToggle;

public sealed class ModConfig
{
    public KeybindList ToggleKey { get; set; } = KeybindList.Parse("MouseRight,LeftStick");
    public int DefaultHairMode { get; set; } = -1;
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
        helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
        helper.Events.Input.ButtonsChanged += OnButtonsChanged;
    }

    private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
    {
        Game1.player.hat.fieldChangeEvent += OnPlayerHatChanged;
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
            gmcm.AddTextOption(
                ModManifest,
                () => config.DefaultHairMode.ToString(),
                (value) =>
                {
                    if (int.TryParse(value, out int intValue))
                        config.DefaultHairMode = intValue;
                },
                I18n.Config_DefaultHairMode_Name,
                I18n.Config_DefaultHairMode_Desc,
                allowedValues: ["-1", "0", "1", "2"],
                formatAllowedValue: (value) =>
                    value == "-1"
                        ? I18n.Config_DefaultHairMode_Value_N1()
                        : I18n.GetByKey($"config.DefaultHairMode.value.{value}")
            );
        }
    }

    private void OnPlayerHatChanged(NetRef<Hat> field, Hat oldValue, Hat newValue)
    {
        if (newValue != null && config.DefaultHairMode != -1)
        {
            newValue.hairDrawType.Value = config.DefaultHairMode;
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
