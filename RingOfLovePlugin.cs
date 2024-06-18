using BepInEx;
using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using UnityEngine;
using LegendAPI;

namespace ring_of_love
{
    [BepInPlugin("tu_tan_k.RingOfLoveMod", "RingOfLoveMod", "0.1.0")]
    public class RingOfLoveModPlugin: BaseUnityPlugin
    {
        void Awake()
        {
            Debug.Log("Hello world c:");

            RegisterItem(new TokenOfYes());
            RegisterItem(new UltimateRing());

            On.BossRoomEventHandler.OnTriggerEnter2D += BossRoomEventHandler_OnTriggerEnter2D;
            On.BossRoomEventHandler.ActivateExitPortal += BossRoomEventHandler_ActivateExitPortal;
            //On.NextLevelLoader.LoadNextLevel += NextLevelLoader_LoadNextLevel;
        }

        private static void RegisterItem(ICustomItem customItem)
        {
            ItemInfo itemInfo = new ItemInfo();
            itemInfo.name = customItem.StaticID;
            itemInfo.text = new TextManager.ItemInfo()
            {
                itemID = customItem.StaticID,
                displayName = customItem.DisplayName,
                description = customItem.Description,
            };
            itemInfo.tier = 1;
            itemInfo.priceMultiplier = 1;
            itemInfo.icon = ImageHandler.LoadSprite(customItem.SpritePath);
            itemInfo.item = (Item)customItem;
            Items.Register(itemInfo);
        }

        private System.Collections.IEnumerator BossRoomEventHandler_ActivateExitPortal(On.BossRoomEventHandler.orig_ActivateExitPortal orig, BossRoomEventHandler self)
        {
            RingBox.spawnLocation = self.bossSpawnPosition;
            Debug.Log(RingBox.spawnLocation);
            UnityEngine.Object.Instantiate(RingBox.Prefab, self.bossSpawnPosition, Quaternion.identity);
            On.TreasureChestParty.Break += TreasureChestParty_Break;

            SoundManager.PlayBGM(string.Empty);
            SoundManager.PlayAudio("StageVictory");

            float exitSpawnTime = 1.5f;
            string animName = "Draw";
            Vector3 spawnPos = self.exitPosition;
            bool disableWithParentVal = false;
            CastingCirclePool.Spawn(animName, spawnPos, disableWithParentVal, null, true, true, exitSpawnTime, 0f, 0f, null, exitSpawnTime);
            yield return new WaitForSeconds(exitSpawnTime);
            UnityEngine.Object.Instantiate(self.exitPortalPrefab, self.exitPosition, Quaternion.identity);
            PoolManager.GetPoolItem<AnimEffect>("TeleportEffect").Play(self.exitPosition, string.Empty, 0.125f, default(Vector2), 1f, 1.5f);
            yield return null;
        }

        private void TreasureChestParty_Break(On.TreasureChestParty.orig_Break orig, TreasureChestParty self)
        {
            if (RingBox.ringDelivered)
            {
                Debug.Log("Ring Delivered... No drop");
                orig(self);
                return;
            }

            self.destroyed = true;
            if (self.dropLoot)
            {
                self.spawnLocation = RingBox.spawnLocation;
                LootManager.DropHealth(self.spawnLocation, RingBox.healthDropCount);
                Debug.Log(self.spawnLocation);
                LootManager.DropItem(self.spawnLocation, 1, itemID: TokenOfYes.staticID, curseChance: 0);
                Debug.Log("Drop ring.......");
                RingBox.ringDelivered = true;

                PoolManager.GetPoolItem<ConfettiEffect>().EmitSingle(50, self.spawnLocation);
                //SoundManager.PlayAudioWithDistance("StandardChestOpen", self.spawnLocation);
            }

        }

        //private void NextLevelLoader_LoadNextLevel(On.NextLevelLoader.orig_LoadNextLevel orig, NextLevelLoader self)
        //{
        //    if (self.nextLevelName == string.Empty)
        //    {
        //        self.nextLevelName = NextLevelLoader.GetCurrentTierScene();
        //        if (Level.isBossRushMode)
        //        {
        //            RunData.AddTimeStamp();
        //        }
        //        if (NextLevelLoader.InTierScene(string.Empty))
        //        {
        //            if (GameController.stageCount > ((!Level.isBossRushMode) ? GameController.stagesPerTier : GameController.stagesPerRush))
        //            {
        //                self.nextLevelName = GameController.bossLevelNameDict[Application.loadedLevelName];
        //            }
        //        }
        //        else if (Application.loadedLevelName.Contains("BossLevel"))
        //        {
        //            GameController.stageCount = 0;
        //            GameController.tierCount++;
        //            if (Level.isBossRushMode)
        //            {
        //                switch (GameController.tierCount)
        //                {
        //                    default:
        //                        self.nextLevelName = "FinalBossLevel";
        //                        break;
        //                    case 6:
        //                        self.nextLevelName = "TitleScreen";
        //                        break;
        //                    //default:
        //                    //    self.nextLevelName = NextLevelLoader.GetCurrentTierScene();
        //                    //    break;
        //                }
        //            }
        //            else
        //            {
        //                switch (GameController.tierCount)
        //                {
        //                    default:
        //                        self.nextLevelName = "FinalBossLevel";
        //                        break;
        //                    case 4:
        //                        self.nextLevelName = "Ending";
        //                        break;
        //                    //default:
        //                    //    self.nextLevelName = NextLevelLoader.GetCurrentTierScene();
        //                    //    break;
        //                }
        //            }
        //        }
        //    }
        //    if (self.showLevelSummary)
        //    {
        //        GameController.levelSummary.Activate(self.nextLevelName);
        //    }
        //    else
        //    {
        //        GameController.LoadLevel(self.nextLevelName);
        //    }
        //}

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
            StartCoroutine(self.ActivateExitPortal());
        }
    }
}
