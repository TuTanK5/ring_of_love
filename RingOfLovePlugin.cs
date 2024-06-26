using BepInEx;
using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using UnityEngine;
using LegendAPI;

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

            On.BossRoomEventHandler.OnTriggerEnter2D += BossRoomEventHandler_OnTriggerEnter2D;
            On.BossRoomEventHandler.ActivateExitPortal += BossRoomEventHandler_ActivateExitPortal;
            On.ExitRoomEventHandler.CreateSurvivalRoom += ExitRoomEventHandler_CreateSurvivalRoom;
            On.BossRoomEventHandler.HandlePvP += BossRoomEventHandler_HandlePvP;
            On.BossRoomEventHandler.Update += BossRoomEventHandler_Update;
        }

        private void BossRoomEventHandler_Update(On.BossRoomEventHandler.orig_Update orig, BossRoomEventHandler self)
        {
            if (!self.bossSpawned || self.exitSpawned)
            {
                Debug.Log("return");
                return;
            }
            int num = 0;
            int num2 = 0;
            Player[] activePlayers = GameController.activePlayers;
            foreach (Player player in activePlayers)
            {
                num2++;
                if ((player.fsm.currentStateName.Contains("Dead") || player.health.CurrentHealthValue <= 0) && player.inventory.GetResurrectItem() == null)
                {
                    num++;
                }
            }
            if (!self.bossDefeated && num != num2)
            {
                self.bossDefeated = true;
                Player[] activePlayers2 = GameController.activePlayers;
                foreach (Player player2 in activePlayers2)
                {
                    player2.dotManager.RemoveAll();
                }
                StartCoroutine(self.BossOutro());
            }
            else if (self.bossDespawned && !self.HandlePvP() && !self.exitSpawned)
            {
                self.exitSpawned = true;
                StartCoroutine(self.ActivateExitPortal());
            }
        }

        private bool BossRoomEventHandler_HandlePvP(On.BossRoomEventHandler.orig_HandlePvP orig, BossRoomEventHandler self)
        {
            if (!GameController.coopOn || (self.pvpAttempted && !self.pvpActive))
            {
                return false;
            }
            if (!self.pvpAttempted && Inventory.EitherPlayerHasItem(BuffWithFriendship.staticID))
            {
                return false;
            }
            self.alivePlayerCount = 0;
            Player[] activePlayers = GameController.activePlayers;
            foreach (Player player in activePlayers)
            {
                if (!player.fsm.currentStateName.Contains("Dead"))
                {
                    self.alivePlayerCount++;
                }
            }
            if (!self.pvpAttempted)
            {
                self.pvpAttempted = true;
                if (self.alivePlayerCount > 1)
                {
                    self.playerOriginalHealthAmounts = new Dictionary<int, int>();
                    Player[] activePlayers2 = GameController.activePlayers;
                    foreach (Player player2 in activePlayers2)
                    {
                        self.playerOriginalHealthAmounts[player2.playerID] = (int)player2.health.healthStat.CurrentValue;
                    }
                    StartCoroutine(self.StartPvP());
                    self.pvpStarted = true;
                    self.pvpActive = true;
                }
                else
                {
                    RunData.SetPvpWinner(-1);
                }
            }
            else if (self.pvpActive && self.alivePlayerCount < 2)
            {
                self.pvpActive = false;
                TimeScaleController.StandardFreezeIntoEaseOutTimeScale();
                Player[] activePlayers3 = GameController.activePlayers;
                foreach (Player player3 in activePlayers3)
                {
                    if (!player3.fsm.currentStateName.Contains("Dead"))
                    {
                        RunData.SetPvpWinner(player3.playerID);
                        break;
                    }
                }
            }
            return self.pvpActive;
        }

        private SurvivalRoom ExitRoomEventHandler_CreateSurvivalRoom(On.ExitRoomEventHandler.orig_CreateSurvivalRoom orig, ExitRoomEventHandler self, float givenValue)
        {
            self.currentSRoom = Globals.ChaosInst<SurvivalRoom>(SurvivalRoom.Prefab, base.transform, self.centerSpawnLocation + Vector2.down);
            BoxCollider2D[] components = self.currentSRoom.GetComponents<BoxCollider2D>();
            foreach (BoxCollider2D boxCollider2D in components)
            {
                boxCollider2D.enabled = false;
            }
            return self.currentSRoom;
        }

        private System.Collections.IEnumerator BossRoomEventHandler_ActivateExitPortal(On.BossRoomEventHandler.orig_ActivateExitPortal orig, BossRoomEventHandler self)
        {
            RingBoxUtils.bossRoomHandler = self;
            RingBoxUtils.orig_ActivateExitPortal = orig;

            StageTitleUI stageTitleUI = GameController.stageTitleUI;
            string uIText = "Good fight! Here goes some extra gift for you.";
            float duration = 5f;
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
            float duration = 10f;
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

        private void BossRoomEventHandler_OnTriggerEnter2D(On.BossRoomEventHandler.orig_OnTriggerEnter2D orig, BossRoomEventHandler self, Collider2D col)
        {
            if (!(col.gameObject.tag == "Player") || (!(col.gameObject.name == "AllyFloorContact") && !(col.gameObject.name == "IgnoreFloorContact")))
            {
                return;
            }
            self.triggeredPosition = col.transform.position;

            if (!self.triggerCollider.enabled)
            {
                return;
            }
            self.triggerCollider.enabled = false;
            if (self.triggerSFX != string.Empty)
            {
                SoundManager.PlayAudio(self.triggerSFX);
            }
            if (self.useTriggerWall)
            {
                self.triggerWall.SetActive(value: true);
            }
            self.bossSpawned = true;
            self.bossDefeated = true;
            self.bossDespawned = true;
        }
    }
}
