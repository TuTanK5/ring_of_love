using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using Chaos.ListExtensions;

namespace ring_of_love
{
    public class RingBoxStatus
    {
        public static bool ringDelivered = false;
        public static int healthDropCount = 1;
        public static Vector3 spawnLocation = new Vector3(0f,0f,0f);
    }

    public interface ICustomItem
    {
        string StaticID { get; }
        string DisplayName { get; }
        string Description { get; }
        string SpritePath { get; }
    }

    public class TokenOfYes : Item, ICustomItem
    {

        public static string staticID = "RingOfLovePlugin::TokenOfYes";
        public static string displayName = "Token of Saying YES";
        public static string description = "Set your health to 1.\nDrop to remove the effect and to say YES to the ultimate question:\nVy, will you marry Tan?";
        public static string srpitePath = "TokenOfYesTickSized";

        public string StaticID => staticID;
        public string DisplayName => displayName;
        public string Description => description;
        public string SpritePath => srpitePath;

        protected NumVarStatMod healthMod;

        public TokenOfYes()
        {
            this.ID = TokenOfYes.staticID;
            this.category = Item.Category.Misc;
            this.healthMod = new NumVarStatMod(this.ID, 1f, 0, VarStatModType.OverrideWithMods, true); // Set to 1
            this.notForSale = true;
            this.destroyOnDrop = true;
            this.useSimpleInfo = true;
        }
        public override string ExtraInfo
        {
            get
            {
                return base.PercentToStr(this.healthMod, "+");
            }
        }

        public override void Activate()
        {
            this.SetModStatus(true);
        }
        public override void Deactivate()
        {
            this.SetModStatus(false);
            parentEntity.health.RestoreHealth(0, fullyRestoreHealth: true);
            Vector3 spawnLocation = parentEntity.transform.localPosition;
            LootManager.DropItem(spawnLocation, 1, itemID: UltimateRing.staticID);
            SoundManager.PlayBGM(string.Empty);
            SoundManager.PlayAudio("StageVictory");
        }

        public void SetModStatus(bool givenStatus)
        {
            parentEntity.health.healthStat.Modify(healthMod, givenStatus);
        }
    }

    public class UltimateRing : Item, ICustomItem
    {

        public static string staticID = "RingOfLovePlugin::UltimateRing";
        public static string displayName = "The Ultimate Ring of Love";
        public static string description = "You Said YES!!!!!!!!!!  <3";
        public static string srpitePath = "RingOfLoveSized";

        public string StaticID => staticID;
        public string DisplayName => displayName;
        public string Description => description;
        public string SpritePath => srpitePath;
 

        public UltimateRing()
        {
            this.ID = UltimateRing.staticID;
            this.category = Item.Category.Misc;
            this.notForSale = true;
            this.isCursed = true;
        }

        public override void Activate()
        {
            //AudioClip weddingAudioClip = ImageHandler.LoadClip();
            //SoundManager.PlayBGM(String.Empty);
            //SoundManager.;
        }

    }
}
