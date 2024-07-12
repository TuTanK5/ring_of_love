using BepInEx;
using UnityEngine;

namespace ring_of_love
{
    [BepInPlugin("tu_tan_k.RingOfLoveMod", "RingOfLoveMod", "0.1.0")]
    public class RingOfLoveModPlugin : BaseUnityPlugin
    {
        void Awake()
        {
            Debug.Log("Hello world c:");

            RingBoxUtils.RegisterItem(new TokenOfYes());
            RingBoxUtils.RegisterItem(new UltimateRing());

            On.BossRoomEventHandler.ActivateExitPortal += BossRoomEventHandler_ActivateExitPortal;
        }

        private System.Collections.IEnumerator BossRoomEventHandler_ActivateExitPortal(On.BossRoomEventHandler.orig_ActivateExitPortal orig, BossRoomEventHandler self)
        {

            if (RingBoxUtils.ringDelivered)
            {
                Debug.Log("Ring Delivered... No drop");
                yield return orig(self);
                yield break;
            }

            RingBoxUtils.bossRoomHandler = self;
            RingBoxUtils.orig_ActivateExitPortal = orig;

            StageTitleUI stageTitleUI = GameController.stageTitleUI;
            string uIText = "Good fight! Here goes some extra gift for you.";
            float duration = 7f;
            Color? textColor = Color.red;
            stageTitleUI.Announce(uIText, duration, 60, textColor, null, showUnderline: false, 1f, 0f, numberMode: false, StageTitleUI.redOutlineColor);
            while (TimeScaleController.delayInProgress)
            {
                yield return null;
            }
            RingBoxUtils.spawnLocation = self.bossSpawnPosition;
            RingBoxUtils.spawnLocation.x -= 1f;
            UnityEngine.Object.Instantiate(TreasureChestParty.Prefab, self.bossSpawnPosition, Quaternion.identity);
            On.TreasureChestParty.Break += TreasureChestParty_Break;
            yield return new WaitForSeconds(3f);

            SoundManager.PlayBGM(string.Empty);
            SoundManager.PlayAudio("StageVictory");

            yield return null;
        }

        private void TreasureChestParty_Break(On.TreasureChestParty.orig_Break orig, TreasureChestParty self)
        {
            if (RingBoxUtils.ringDelivered)
            {
                Debug.Log("Ring Delivered... No drop");
                orig(self);
                return;
            }

            self.destroyed = true;

            GameUI.BroadcastNoticeMessage(" ", 1f);
            StageTitleUI stageTitleUI2 = GameController.stageTitleUI;
            string uIText = "...But it comes with a decision to make.";
            float duration = 7f;
            Color? textColor = Color.red;
            stageTitleUI2.Announce(uIText, duration, 60, textColor, null, showUnderline: false, 1f, 0f, numberMode: false, StageTitleUI.redOutlineColor);

            if (self.dropLoot)
            {
                self.spawnLocation = RingBoxUtils.spawnLocation;
                LootManager.DropHealth(self.spawnLocation, RingBoxUtils.healthDropCount);
                Debug.Log(self.spawnLocation);
                LootManager.DropItem(self.spawnLocation, 1, itemID: TokenOfYes.staticID, curseChance: 0);
                Debug.Log("Drop ring.......");
                RingBoxUtils.ringDelivered = true;

                PoolManager.GetPoolItem<ConfettiEffect>().EmitSingle(50, self.spawnLocation);
                SoundManager.PlayAudioWithDistance("StandardChestOpen", self.spawnLocation);
            }
        }
    }
}
