using System.Diagnostics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;

namespace HatHairToggle;

public sealed class ModEntry : Mod
{
#if DEBUG
    private const LogLevel DEFAULT_LOG_LEVEL = LogLevel.Debug;
#else
    private const LogLevel DEFAULT_LOG_LEVEL = LogLevel.Trace;
#endif

    public const string ModId = "mushymato.HatHairToggle";
    private static IMonitor mon = null!;
    internal static IModHelper help = null!;
    private IBetterGameMenuApi? BGM;

    public override void Entry(IModHelper helper)
    {
        I18n.Init(helper.Translation);
        mon = Monitor;
        help = helper;

        help.Events.GameLoop.GameLaunched += OnGameLaunched;
        help.Events.Input.ButtonsChanged += OnButtonsChanged;
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        BGM = Helper.ModRegistry.GetApi<IBetterGameMenuApi>("leclair.bettergamemenu");
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
        if (!Game1.didPlayerJustRightClick())
            return;
        if (Game1.player.hat.Value is not Hat hat)
            return;
        if (GetInventoryPage(Game1.activeClickableMenu) is not InventoryPage menu)
            return;
        int x = Game1.getOldMouseX();
        int y = Game1.getOldMouseY();
        foreach (ClickableComponent cc in menu.equipmentIcons)
        {
            if (cc.containsPoint(x, y))
            {
                int hairDrawType = hat.hairDrawType.Value;
                int newHairDrawType = (hairDrawType + 1) % 3;
                hat.hairDrawType.Value = newHairDrawType;
                return;
            }
        }
    }

    /// <summary>SMAPI static monitor Log wrapper</summary>
    /// <param name="msg"></param>
    /// <param name="level"></param>
    internal static void Log(string msg, LogLevel level = DEFAULT_LOG_LEVEL)
    {
        mon.Log(msg, level);
    }

    /// <summary>SMAPI static monitor LogOnce wrapper</summary>
    /// <param name="msg"></param>
    /// <param name="level"></param>
    internal static void LogOnce(string msg, LogLevel level = DEFAULT_LOG_LEVEL)
    {
        mon.LogOnce(msg, level);
    }

    /// <summary>SMAPI static monitor Log wrapper, debug only</summary>
    /// <param name="msg"></param>
    /// <param name="level"></param>
    [Conditional("DEBUG")]
    internal static void LogDebug(string msg, LogLevel level = DEFAULT_LOG_LEVEL)
    {
        mon.Log(msg, level);
    }
}
