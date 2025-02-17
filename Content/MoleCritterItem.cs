using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MoleMod.Content
{
    public class MoleCritterItem : ModItem
    {
        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.Frog);
            Item.width = 26;
            Item.height = 20;
            Item.makeNPC = ModContent.NPCType<MoleCritter>();
            Item.value += Item.buyPrice(0, 0, 30, 0);
            Item.rare = ItemRarityID.Blue;
        }
    }
}