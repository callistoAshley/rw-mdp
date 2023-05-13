using MoreSlugcats;

namespace AshleysAwesomeRWMod
{
    // this is the transmissions in the spearmaster campaign! :]
    public static class ChatLogDisplayHooks
    {
        public static void ChatLogDisplay_InitNextMessage(On.MoreSlugcats.ChatLogDisplay.orig_InitNextMessage orig, ChatLogDisplay self)
        {
            Plugin.instance.dialogOnScreen = true;
            orig(self);
        }

        public static void ChatLogDisplay_Draw(On.MoreSlugcats.ChatLogDisplay.orig_Draw orig, ChatLogDisplay self, float timeStacker)
        {
            orig(self, timeStacker);

            if (self.showCharacter >= self.CurrentMessage.text.Length)
            {
                self.lingerCounter -=
                    RWInput.CheckSpecificButton(0, 0, self.hud.rainWorld) && !self.disable_fastDisplay ? 10
                    : 1;

                if (RWInput.CheckSpecificButton(0, Plugin.MAP_ACTION, self.hud.rainWorld))
                {
                    if (self.showLine < self.messages.Count - 1)
                    {
                        self.InitNextMessage();
                        return;
                    }

                    // once there are no more messages queued and the linger counter is high enough, the transmission starts fading out
                    Plugin.instance.dialogOnScreen = false;
                    self.lingerCounter = int.MaxValue;
                }
                if (self.mainAlpha < 0.01f)
                {
                    if (self.hud.owner is Player)
                    {
                        (self.hud.owner as Player).abstractCreature.world.game.pauseUpdate = false;
                    }
                    self.slatedForDeletion = true;
                }
            }
        }
    }
}
