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
    public class RingBox : TreasureChestParty
    {
        public static bool ringDelivered = false;
        public new static int healthDropCount = 13;
        public new static Vector3 spawnLocation = new Vector3(0f,0f,0f);
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
        public static string description = "Drop to say YES";
        public static string srpitePath = "Super Necklace";

        public string StaticID => staticID;
        public string DisplayName => displayName;
        public string Description => description;
        public string SpritePath => srpitePath;

        protected NumVarStatMod damageMod;

        public TokenOfYes()
        {
            this.ID = TokenOfYes.staticID;
            this.category = Item.Category.Misc;
            this.damageMod = new NumVarStatMod(this.ID, 1.0f, 10, VarStatModType.Multiplicative, false); //Doubles damage
            this.notForSale = true;
            this.destroyOnDrop = true;
        }
        public override string ExtraInfo
        {
            get
            {
                return base.PercentToStr(this.damageMod, "+");
            }
        }

        public override void Activate()
        {
            this.SetModStatus(true);
        }
        public override void Deactivate()
        {
            this.SetModStatus(false);
            Vector3 spawnLocation = RingBox.spawnLocation;
            LootManager.DropItem(spawnLocation, 1, itemID: UltimateRing.staticID, curseChance: 0);
        }

        public virtual void SetModStatus(bool givenStatus)
        {
            StatManager.ModifyAllStatData(this.damageMod, this.parentSkillCategory, StatData.damageStr, new StatManager.ModApplyConditional(base.IgnoreStatusConditional), givenStatus);
        }
    }

    public class UltimateRing : Item, ICustomItem
    {

        public static string staticID = "RingOfLovePlugin::UltimateRing";
        public static string displayName = "The Ultimate Ring";
        public static string description = "The Ultimate Ring";
        public static string srpitePath = "Super Necklace";

        public string StaticID => staticID;
        public string DisplayName => displayName;
        public string Description => description;
        public string SpritePath => srpitePath;
 
        protected NumVarStatMod damageMod;

        public UltimateRing()
        {
            this.ID = UltimateRing.staticID;
            this.category = Item.Category.Misc;
            this.damageMod = new NumVarStatMod(this.ID, 1.0f, 10, VarStatModType.Multiplicative, false); //Doubles damage
            //this.notForSale = true;
        //    this.isCursed = true;
        }
        public override string ExtraInfo
        {
            get
            {
                return base.PercentToStr(this.damageMod, "+");
            }
        }

        public override void Activate()
        {
            this.SetModStatus(true);
        }
        public override void Deactivate()
        {
            this.SetModStatus(false);
        }

        public virtual void SetModStatus(bool givenStatus)
        {
            StatManager.ModifyAllStatData(this.damageMod, this.parentSkillCategory, StatData.damageStr, new StatManager.ModApplyConditional(base.IgnoreStatusConditional), givenStatus);
        }
    }
}
