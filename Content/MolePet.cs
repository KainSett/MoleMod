using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
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
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }
        public static List<Point> blockBlacklist = [new Point(0, 0)];
        public override void SetDefaults()
        {
            Projectile.width = 56;
            Projectile.height = 46;
            Projectile.damage = 0;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.velocity = new Vector2(0, 0);
            Target = (true, Main.player[Projectile.owner].Center);
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
        public (bool, Vector2) Target;
        public override void AI()
        {

            var owner = Main.player[Projectile.owner];

            if ((owner == null || !owner.active || !owner.HasBuff(ModContent.BuffType<MolePetBuff>()))) {
                blockBlacklist.Add(Target.Item2.ToTileCoordinates());
                Projectile.Kill();
                return;
            }

            if ((Projectile.oldPos.First().DistanceSQ(Projectile.oldPos.Last()) < 20 * 20 || Projectile.Center.DistanceSQ(owner.Center) > 1500 * 1500) && CurrentAnimation == Animation.Burrowing)
                Projectile.timeLeft--;
            else
                Projectile.timeLeft = 120;


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

                        Projectile.spriteDirection = 1;
                        break;

                    case Animation.UnHiding:
                        if (Projectile.frame <= 0)
                            CurrentAnimation = Animation.Stationary;

                        else if (Projectile.frame > 2)
                            Projectile.frame = 2;

                        else Projectile.frame--;

                        Projectile.spriteDirection = 1;
                        break;

                    case Animation.Burrowing:
                        if (Projectile.frame == 7)
                            Projectile.spriteDirection = 0;

                        else if (Projectile.frame == 3)
                            Projectile.spriteDirection = 1;

                        if (Projectile.spriteDirection == 1)
                            Projectile.frame++;

                        else Projectile.frame--;

                        break;

                    default:
                        if (Projectile.frame == 0 && Main.rand.NextFloat() < 0.03f)
                        {
                            if (Main.rand.NextFloat() < 0.3f)
                                Projectile.frame = -1;
                            else if (Main.rand.NextBool())
                                Projectile.frame = -3;
                            else
                                Projectile.frame = -4;
                        }
                        else if (Projectile.frame == -1)
                            Projectile.frame = -2;
                        else if (Projectile.frame == -3)
                            Projectile.frame = -1;
                        else if (Projectile.frame == -4)
                            Projectile.frame = -2;
                        else
                            Projectile.frame = 0;

                        Projectile.spriteDirection = 1;
                        break;
                }
            }

            var top = Projectile.Center + new Vector2(0, Projectile.height / 4);
            var bottom = Projectile.Center + new Vector2(0, Projectile.height / 2f);


            if (CurrentAnimation == Animation.Stationary && Projectile.Center.DistanceSQ(owner.Center) > 500 * 500)
            {
                Target.Item1 = true;
                Target.Item2 = owner.Center;
                CurrentAnimation = Animation.Hiding;
                Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center - new Vector2(0, Projectile.height / 2), -Vector2.UnitY * 5, ModContent.ProjectileType<OldMiningHelmet>(), 0, 0, Projectile.owner);
            }

            if (WorldGen.SolidOrSlopedTile(top.ToTileCoordinates().X, top.ToTileCoordinates().Y) || !WorldGen.SolidOrSlopedTile(bottom.ToTileCoordinates().X, bottom.ToTileCoordinates().Y))
                CurrentAnimation = Animation.Burrowing;

            else if (Projectile.Center.DistanceSQ(Target.Item2) < 500 * 500 && CurrentAnimation == Animation.Burrowing)
            {
                CurrentAnimation = Animation.UnHiding;
                Target = (true, owner.Center);
            }


            if (Projectile.Center.DistanceSQ(owner.Center) < 60 * 60 && CurrentAnimation == Animation.Burrowing)
            {
                for (int i = 0; i < Main.worldSurface; i++)
                {
                    if (!Target.Item1)
                        break;

                    for (int x = 0; x < 200; x++)
                    {
                        var blockRight = (owner.Center).ToTileCoordinates() + new Point(x, i - 10);

                        if (WorldGen.SolidOrSlopedTile(blockRight.X, blockRight.Y) && !blockBlacklist.Contains(blockRight))
                        {
                            Target.Item2 = blockRight.ToWorldCoordinates();
                            Target.Item1 = false;
                            break;
                        }

                        var blockLeft = (owner.Center).ToTileCoordinates() + new Point(-x, i - 10);

                        if (WorldGen.SolidOrSlopedTile(blockLeft.X, blockLeft.Y) && !blockBlacklist.Contains(blockLeft))
                        {
                            Target.Item2 = blockLeft.ToWorldCoordinates();
                            Target.Item1 = false;
                            break;
                        }
                    }
                }
            }
            else if (Projectile.Center.DistanceSQ(owner.Center) > 500 * 500)
            {
                Target = (true, owner.Center);
            }


            switch (CurrentAnimation)
            {
                case Animation.Burrowing:
                    for (int y = Projectile.height / 8; y > 0; y--)
                    {
                        for (int x = Projectile.width / 8; x > 0; x--)
                        {
                            var block = (Projectile.position).ToTileCoordinates() + new Point(x, y);
                            if (WorldGen.SolidOrSlopedTile(block.X, block.Y))
                            {
                                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Dirt);

                            }
                        }
                    }
                    Projectile.ai[1] = Projectile.Center.X > Target.Item2.X + 69 ? -MathHelper.PiOver4 * 1.2f : Projectile.Center.X < Target.Item2.X - 69 ? MathHelper.PiOver4 * 1.2f : Projectile.ai[1];
                    Projectile.velocity = Projectile.Center.DirectionTo(Target.Item2).RotatedBy(Projectile.ai[1]).SafeNormalize(Vector2.Zero) * 7;
                    Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
                    break;

                default:
                    Projectile.velocity *= 0;
                    Projectile.rotation = 0;
                    break;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            var texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value; 


            int frameHeight = texture.Height / Main.projFrames[Type];

            int startY = frameHeight * Projectile.frame;
            var rect = new Rectangle(0, startY, texture.Width, frameHeight);

            if (Projectile.frame <= -1)
            {
                texture = MoleMod.MolePetSide.Value;
                rect = new Rectangle(0, texture.Height / 4 * (int.Abs(Projectile.frame) - 1), texture.Width, texture.Height / 4);
            }
            


            var scale = Projectile.scale * Main.GameZoomTarget;

            var color = lightColor;

            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition + Vector2.UnitY * 5, rect, color, Projectile.rotation, rect.Size() / 2, scale, (SpriteEffects)Projectile.spriteDirection, 0);

            return false;
        }
    }
    public class OldMiningHelmet : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
        }
        public override void AI()
        {
            float rot = Projectile.ai[1] * 0.03f + 0.05999f;
            Projectile.rotation = (Projectile.rotation + rot) % MathHelper.TwoPi;
            Projectile.velocity.Y += 0.3f;
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            switch (Projectile.rotation)
            {
                case < MathHelper.PiOver2:
                    Projectile.ai[1] = -1;
                    break;
                case < MathHelper.Pi:
                    Projectile.ai[1] = 2;
                    break;
                case < MathHelper.PiOver2 * 3:
                    Projectile.ai[1] = -2;
                    break;
                case < MathHelper.TwoPi:
                    Projectile.ai[1] = 1;
                    break;
            }
            Projectile.timeLeft -= 40;
            return false;
        }
    }
}