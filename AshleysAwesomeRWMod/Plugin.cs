using System.Reflection;
using System.Security.Permissions;
using BepInEx;
using HUD;
using MonoMod.RuntimeDetour;

[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
namespace AshleysAwesomeRWMod
{
    [BepInPlugin("ashleyn.manual_dialogue_progression", "Manual Dialogue Progression", "1.1")]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin instance; 

        public delegate bool orig_RevealMap(Player self);

        public const int MAP_ACTION = 11;

        public bool dialogOnScreen;
        public bool dialogShouldTerminate;
        public DialogBox dialogBox;

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
            // prevents the death screen from triggering while dialogue is on screen
            On.HUD.TextPrompt.Update += TextPrompt_Update;
            // bug fix: ending a cycle while dialogue is on screen prevents shelters from opening
            On.ProcessManager.PreSwitchMainProcess += ProcessManager_PreSwitchMainProcess;
        }

        private void TextPrompt_Update(On.HUD.TextPrompt.orig_Update orig, TextPrompt self)
        {
            if (self.gameOverMode && (dialogOnScreen || dialogShouldTerminate))
                self.restartNotAllowed++;
            orig(self);
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

        private void ProcessManager_PreSwitchMainProcess(On.ProcessManager.orig_PreSwitchMainProcess orig, ProcessManager self, ProcessManager.ProcessID ID)
        {
            dialogOnScreen = false;
            dialogShouldTerminate = false;
            if (dialogBox != null)
            {
                // force the dialog box to go kaput
                dialogShouldTerminate = true;
                dialogBox.messages.Clear();
                dialogBox.Update();
            }
            orig(self, ID);
        }
    }
}
