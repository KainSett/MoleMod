using MoleMod.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MoleMod.Content
{
    public class MoleCritterItem : ModItem
    {
        public override void SetStaticDefaults()
        {
            //ItemID.Sets.IsLavaBait[Type] = true; // While this item is not bait, this will require a lava bug net to catch.
        }

        public override void SetDefaults()
        {
            // useStyle = 1;
            // autoReuse = true;
            // useTurn = true;
            // useAnimation = 15;
            // useTime = 10;
            // maxStack = CommonMaxStack;
            // consumable = true;
            // width = 12;
            // height = 12;
            // makeNPC = 361;
            // noUseGraphic = true;

            // Cloning ItemID.Frog sets the preceding values
            Item.CloneDefaults(ItemID.Frog);
            Item.width = 26;
            Item.height = 20;
            Item.makeNPC = ModContent.NPCType<MoleCritter>();
            Item.value += Item.buyPrice(0, 0, 30, 0); // Make this critter worth slightly more than the frog
            Item.rare = ItemRarityID.Blue;
        }
    }
    public class GoldenMoleCritterItem : ModItem
    {
        public override void SetStaticDefaults()
        {
            //ItemID.Sets.IsLavaBait[Type] = true; // While this item is not bait, this will require a lava bug net to catch.
        }

        public override void SetDefaults()
        {
            // useStyle = 1;
            // autoReuse = true;
            // useTurn = true;
            // useAnimation = 15;
            // useTime = 10;
            // maxStack = CommonMaxStack;
            // consumable = true;
            // width = 12;
            // height = 12;
            // makeNPC = 361;
            // noUseGraphic = true;

            // Cloning ItemID.Frog sets the preceding values
            Item.CloneDefaults(ItemID.Frog);
            Item.width = 26;
            Item.height = 20;
            Item.makeNPC = ModContent.NPCType<GoldenMoleCritter>();
            Item.value += Item.buyPrice(0, 10);
            Item.rare = ItemRarityID.Blue;
        }
    }
}