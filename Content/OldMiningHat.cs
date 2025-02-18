using Microsoft.Xna.Framework;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace MoleMod.Content
{
    public class OldMiningHat : ModItem
    {
        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.ZephyrFish);
            Item.width = 28;
            Item.height = 22;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<MolePet>();
            Item.buffType = ModContent.BuffType<MolePetBuff>(); 
        }
        public override bool? UseItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                player.AddBuff(Item.buffType, 3600);
            }
            return true;
        }
        public override bool CanShoot(Player player)
        {
            if (player.HasBuff(Item.buffType))
                return false;
            return base.CanShoot(player);
        }
    }
    public class MoleDropNPC : GlobalNPC
    {
        public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
        {
            if (npc.type == NPCID.UndeadMiner)
            {
                npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<OldMiningHat>(), 11));
            }
            base.ModifyNPCLoot(npc, npcLoot);
        }
    }
    public class OldMiningHatChestLoot : ModSystem
    {
        public override void PostWorldGen()
        {
            int items = 11;
            for (int chestIndex = 0; chestIndex < Main.maxChests; chestIndex++)
            {
                Chest chest = Main.chest[chestIndex];
                if (chest == null)
                {
                    continue;
                }
                Tile chestTile = Main.tile[chest.x, chest.y];
                if (chestTile.TileType == TileID.Containers && chestTile.TileFrameX == 1 * 36)
                {
                    if (WorldGen.genRand.NextFloat() > 0.03f)
                        continue;
                    for (int inventoryIndex = 0; inventoryIndex < Chest.maxItems; inventoryIndex++)
                    {
                        if (chest.item[inventoryIndex].type == ItemID.None)
                        {
                            // Place the item
                            chest.item[inventoryIndex].SetDefaults(ModContent.ItemType<OldMiningHat>());
                            items--;
                            break;
                        }
                    }
                }
                if (items <= 0)
                    break;
            }
        }
    }
}