using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;
using Terraria.ObjectData;
using Terraria.Audio;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;

namespace MoleMod.Content
{
    public class MoleCageItem : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<MoleCage>());
            Item.width = 48;
            Item.height = 32;
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<MoleCritterItem>()
                .AddIngredient(ItemID.Terrarium)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
    public class MoleCage : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolidTop[Type] = true;
            Main.tileTable[Type] = true;
            Main.tileFrameImportant[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.StyleSmallCage);
            TileObjectData.addTile(Type);
            DustType = DustID.Glass;
            AddMapEntry(Color.LightSkyBlue);
        }
        public override bool KillSound(int i, int j, bool fail)
        {
            if (!fail)
            {
                SoundEngine.PlaySound(SoundID.Shatter, new Vector2(i, j).ToWorldCoordinates());
                return false;
            }
            return base.KillSound(i, j, fail);
        }
        public override void AnimateTile(ref int frame, ref int frameCounter)
        {
            frameCounter++;
            if ((frame == 3 && frameCounter >= 150) || (frameCounter >= 11 && frame != 3 && (frame > 0 || Main.rand.NextFloat() < 0.01f)))
            {
                frameCounter = 0;
                frame = (frame + 1) % 5;
            }
        }
        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        {
            Tile tile = Main.tile[i, j];

            // If you are using ModTile.SpecialDraw or PostDraw or PreDraw, use this snippet and add zero to all calls to spriteBatch.Draw
            // The reason for this is to accommodate the shift in drawing coordinates that occurs when using the different Lighting mode
            // Press Shift+F9 to change lighting modes quickly to verify your code works for all lighting modes
            Vector2 zero = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);

            // Because height of third tile is different we change it
            int height = 16;

            //Main.tileFrame[Type] = (Main.tileFrame[Type] + 1) % 5;
            AnimationFrameHeight = TextureAssets.Tile[Type].Value.Height / 5;
            int frameYOffset = Main.tileFrame[Type] * AnimationFrameHeight;

            spriteBatch.Draw(
                TextureAssets.Tile[Type].Value,
                new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y) + zero,
                new Rectangle(tile.TileFrameX, tile.TileFrameY + frameYOffset, 16, height),
                Lighting.GetColor(i, j), 0f, default, 1f, SpriteEffects.None, 0f);

            return false;
        }
    }

    public class GoldenMoleCageItem : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<GoldenMoleCage>());
            Item.width = 48;
            Item.height = 32;
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<GoldenMoleCritterItem>()
                .AddIngredient(ItemID.Terrarium)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
    public class GoldenMoleCage : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolidTop[Type] = true;
            Main.tileTable[Type] = true;
            Main.tileFrameImportant[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.StyleSmallCage);
            TileObjectData.addTile(Type);
            DustType = DustID.Glass;
            AddMapEntry(Color.LightSkyBlue);
        }
        public override bool KillSound(int i, int j, bool fail)
        {
            if (!fail)
            {
                SoundEngine.PlaySound(SoundID.Shatter, new Vector2(i, j).ToWorldCoordinates());
                return false;
            }
            return base.KillSound(i, j, fail);
        }
        public override void AnimateTile(ref int frame, ref int frameCounter)
        {
            frameCounter++;
            if ((frame == 3 && frameCounter >= 150) || (frameCounter >= 11 && frame != 3 && (frame > 0 || Main.rand.NextFloat() < 0.01f)))
            {
                frameCounter = 0;
                frame = (frame + 1) % 5;
            }
        }
        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        {
            Tile tile = Main.tile[i, j];

            // If you are using ModTile.SpecialDraw or PostDraw or PreDraw, use this snippet and add zero to all calls to spriteBatch.Draw
            // The reason for this is to accommodate the shift in drawing coordinates that occurs when using the different Lighting mode
            // Press Shift+F9 to change lighting modes quickly to verify your code works for all lighting modes
            Vector2 zero = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);

            // Because height of third tile is different we change it
            int height = 16;

            //Main.tileFrame[Type] = (Main.tileFrame[Type] + 1) % 5;
            AnimationFrameHeight = TextureAssets.Tile[Type].Value.Height / 5;
            int frameYOffset = Main.tileFrame[Type] * AnimationFrameHeight;

            spriteBatch.Draw(
                TextureAssets.Tile[Type].Value,
                new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y) + zero,
                new Rectangle(tile.TileFrameX, tile.TileFrameY + frameYOffset, 16, height),
                Lighting.GetColor(i, j), 0f, default, 1f, SpriteEffects.None, 0f);

            return false;
        }
    }
}