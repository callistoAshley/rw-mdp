using System.Reflection;
using System.Security.Permissions;
using BepInEx;
using MonoMod.RuntimeDetour;

[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
namespace AshleysAwesomeRWMod
{
    [BepInPlugin("ashleyn.manual_dialogue_progression", "Manual Dialogue Progression", "1.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin instance; 

        public delegate bool orig_RevealMap(Player self);

        public const int MAP_ACTION = 11;

        public bool dialogOnScreen;
        public bool dialogShouldTerminate;

        public void OnEnable()
        {
            instance = this;

            // prevents the map from being revealed while dialogue is on screen
            Hook revealMapGetterHook = new Hook(
                typeof(Player).GetProperty("RevealMap", BindingFlags.Instance | BindingFlags.Public).GetGetMethod(),
                typeof(Plugin).GetMethod("Player_RevealMap_get", BindingFlags.Static | BindingFlags.Public)
            );
            // helps track whether dialogue is on screen
            On.HUD.DialogBox.InitNextMessage                += DialogBoxHooks.DialogBox_InitNextMessage;
            On.MoreSlugcats.ChatLogDisplay.InitNextMessage  += ChatLogDisplayHooks.ChatLogDisplay_InitNextMessage;
            // input logic
            On.HUD.DialogBox.Update                         += DialogBoxHooks.DialogBox_Update; 
            On.MoreSlugcats.ChatLogDisplay.Draw             += ChatLogDisplayHooks.ChatLogDisplay_Draw;
            // pauses the rain timer if dialogue is on screen
            On.RainWorldGame.AllowRainCounterToTick         += RainWorldGame_AllowRainCounterToTick; 
        }

        public static bool Player_RevealMap_get(orig_RevealMap orig, Player self)
        {
            if (instance.dialogOnScreen) return false;
            return orig(self);
        }

        private bool RainWorldGame_AllowRainCounterToTick(On.RainWorldGame.orig_AllowRainCounterToTick orig, RainWorldGame self)
        {
            if (dialogOnScreen) return false;
            return orig(self);
        }
    }
}
