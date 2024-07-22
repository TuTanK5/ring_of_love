using UnityEngine;
using LegendAPI;


namespace ring_of_love
{
    public class RingBoxUtils
    {
        public static bool ringDelivered = false;
        public static int healthDropCount = 3;
        public static Vector3 spawnLocation = new Vector3(0f, 0f, 0f);
        public static BossRoomEventHandler bossRoomHandler;
        public static On.BossRoomEventHandler.orig_ActivateExitPortal orig_ActivateExitPortal;

        public static void RegisterItem(ICustomItem customItem)
        {
            ItemInfo itemInfo = new ItemInfo();
            itemInfo.name = customItem.StaticID;
            itemInfo.text = new TextManager.ItemInfo()
            {
                itemID = customItem.StaticID,
                displayName = customItem.DisplayName,
                description = customItem.Description,
            };
            itemInfo.tier = 5;
            itemInfo.priceMultiplier = 1;
            itemInfo.icon = ImageHandler.LoadSprite(customItem.SpritePath);
            itemInfo.item = (Item)customItem;
            Items.Register(itemInfo);
        }
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
        public static string displayName = "Token of Confirmation";
        public static string description = "Set your health to 1.\nDrop to remove the effect and say YES to the ultimate question:\nVy, will you marry Tan?";
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
            this.healthMod = new NumVarStatMod(this.ID, 1f, 10, VarStatModType.OverrideWithMods, true); // Set to 1
            this.notForSale = true;
            this.destroyOnDrop = true;
            this.useSimpleInfo = true;
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
            LootManager.DropItem(spawnLocation, 2, itemID: UltimateRing.staticID);
            if (RingBoxUtils.bossRoomHandler != null)
            {
                RingBoxUtils.bossRoomHandler.StartCoroutine(RingBoxUtils.orig_ActivateExitPortal(RingBoxUtils.bossRoomHandler));
            }
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
        public static string description = "Vy Said YESSSSSSSSSSSSSSSSSSSSSS!!!!  <3";
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
            this.useSimpleInfo = true;
        }

        public override void Activate()
        {
            parentEntity.health.RestoreHealth(0, fullyRestoreHealth: true);
        }
    }
}
