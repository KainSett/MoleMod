using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MoleMod.Content
{
    public class MolePet : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 8; 
            
            ProjectileID.Sets.CharacterPreviewAnimations[Projectile.type] = ProjectileID.Sets.SimpleLoop(0, 3, 6)
                .WithOffset(-10, -20f)
                .WithSpriteDirection(-1)
                .WithCode(DelegateMethods.CharacterPreview.Float);
        }
        public override void SetDefaults()
        {
            Projectile.width = 56;
            Projectile.height = 46;
            Projectile.damage = 0;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
        }
        public enum Animation
        {
            Stationary,
            Hiding,
            UnHiding,
            Burrowing
        }
        public Animation CurrentAnimation
        {
            get => (Animation)Projectile.ai[0];
            set => Projectile.ai[0] = (float)value;
        }
        public float Rotation
        {
            get => Projectile.ai[2];
            set => Projectile.ai[2] = value;
        }
        public override void AI()
        {
            Projectile.timeLeft++;

            var owner = Main.player[Projectile.owner];

            if (owner == null || !owner.active || !owner.HasBuff(ModContent.BuffType<MolePetBuff>()))
                Projectile.Kill();


            Projectile.frameCounter++;
            if (Projectile.frameCounter > 5)
            {
                Projectile.frameCounter = 0;
                switch (CurrentAnimation)
                {
                    case Animation.Hiding:
                        if (Projectile.frame == 2)
                            CurrentAnimation = Animation.Burrowing;

                        else Projectile.frame++;

                        break;

                    case Animation.UnHiding:
                        if (Projectile.frame == 0)
                            CurrentAnimation = Animation.Stationary;

                        else Projectile.frame--;

                        break;

                    case Animation.Burrowing:
                        if (Projectile.frame == 7)
                            Projectile.ai[1] = 1;

                        else if (Projectile.frame == 3)
                            Projectile.ai[1] = 0;


                        if (Projectile.ai[1] == 0) {
                            Projectile.frame++;
                            Projectile.spriteDirection = 1;
                        }

                        else {
                            Projectile.frame--;
                            Projectile.spriteDirection = -1;
                        }

                        break;

                    default: break;
                }
            }

            var top = Projectile.Center - new Vector2(0, Projectile.height / 2);
            var bottom = Projectile.Center + new Vector2(0, Projectile.height / 2);

            if (WorldGen.SolidOrSlopedTile(top.ToTileCoordinates().X, top.ToTileCoordinates().Y))
                CurrentAnimation = Animation.Burrowing;

            else if (Main.tile[bottom.ToTileCoordinates()].HasTile)
            {
                if (CurrentAnimation != Animation.Stationary && Projectile.frame >= 2)
                    CurrentAnimation = Animation.Burrowing;

                else CurrentAnimation = Animation.Hiding;
            }

            else if (Projectile.Center.DistanceSQ(owner.Center) < 300 * 300 && CurrentAnimation == Animation.Burrowing)
                CurrentAnimation = Animation.UnHiding;

            else if (CurrentAnimation == Animation.Stationary && Projectile.Center.DistanceSQ(owner.Center) > 300 * 300)
                CurrentAnimation = Animation.Hiding;

            switch (CurrentAnimation)
            {
                case Animation.Burrowing:
                    var rot = Projectile.Center.X > owner.Center.X ? -MathHelper.PiOver4 : MathHelper.PiOver4;
                    Projectile.velocity = Projectile.Center.DirectionTo(owner.Center).RotatedBy(rot) * 5;
                    Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
                    break;

                default:
                    Projectile.velocity *= 0;
                    Projectile.rotation = 0;
                    break;
            }
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (oldVelocity.LengthSquared() > 1 && CurrentAnimation == Animation.Burrowing)
            {
                for (int i = 4; i > 0; i--)
                {
                    Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, Terraria.ID.DustID.Dirt);
                }
            }
            return false;
        }
        public override void PostDraw(Color lightColor)
        {
            var texture = Terraria.GameContent.TextureAssets.Item[ModContent.ItemType<OldMiningHat>()].Value;
            if (CurrentAnimation == Animation.Hiding || CurrentAnimation == Animation.UnHiding)
            {
                Rotation += 0.1f;
                Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition - new Vector2(0, Projectile.height / 2) * (Projectile.frame + 0.1f * Projectile.frameCounter), texture.Bounds, lightColor, Rotation, texture.Size() / 2, Projectile.scale, Microsoft.Xna.Framework.Graphics.SpriteEffects.None);
            }
        }
    }
}