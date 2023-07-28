using UnityEngine;
using HUD;

namespace AshleysAwesomeRWMod
{
    public static class DialogBoxHooks
    {
        public static void DialogBox_InitNextMessage(On.HUD.DialogBox.orig_InitNextMessage orig, DialogBox self)
        {
            Plugin.instance.dialogOnScreen = true;
            Plugin.instance.dialogBox = self;
            orig(self);
        }

        public static void DialogBox_Update(On.HUD.DialogBox.orig_Update orig, DialogBox self)
        {
            if (self.CurrentMessage != null && self.showCharacter >= self.CurrentMessage.text.Length)
            {
                self.lingerCounter--;

                if (RWInput.CheckSpecificButton(0, Plugin.MAP_ACTION, self.hud.rainWorld) && !Plugin.instance.dialogShouldTerminate)
                {
                    Plugin.instance.dialogShouldTerminate = true;
                }
                if (Plugin.instance.dialogShouldTerminate)
                {
                    self.showText = string.Empty;
                    if (self.sizeFac > 0)
                    {
                        self.sizeFac = Mathf.Max(0, self.sizeFac - 0.16666667f);
                    }
                    else
                    {
                        Plugin.instance.dialogOnScreen = false;
                        Plugin.instance.dialogShouldTerminate = false;

                        self.messages.RemoveAt(0);
                        if (self.messages.Count > 0)
                        {
                            self.InitNextMessage();
                        }
                        else
                        {
                            Plugin.instance.dialogBox = null;
                        }
                    }
                    return;
                }
            }

            orig(self);
        }
    }
}
