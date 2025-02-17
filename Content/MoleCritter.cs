using Terraria.GameContent.Bestiary;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;
using System;
using Microsoft.Xna.Framework;
using Terraria.Utilities;
using Microsoft.Xna.Framework.Graphics;

namespace MoleMod.Content
{
    public class MoleCritter : ModNPC
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 6;
            Main.npcCatchable[Type] = true;
            NPCID.Sets.CountsAsCritter[Type] = true;
            NPCID.Sets.TakesDamageFromHostilesWithoutBeingFriendly[Type] = true;
            NPCID.Sets.TownCritter[Type] = true;
            NPCID.Sets.NormalGoldCritterBestiaryPriority.Insert(NPCID.Sets.NormalGoldCritterBestiaryPriority.IndexOf(NPCID.Mouse), Type);
        }
        public override void SetDefaults()
        {
            NPC.CloneDefaults(NPCID.Mouse);
            AnimationType = NPCID.Mouse;
            NPC.width = 30;
            NPC.height = 20;
            NPC.damage = 0;
            NPC.defense = 0;
            NPC.lifeMax = 5;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.catchItem = ModContent.ItemType<MoleCritterItem>();
            NPC.lavaImmune = true;
            var distance = 2000f * 2000f;
            foreach (var gamer in Main.ActivePlayers)
            {
                if (gamer != null && gamer.Center.DistanceSQ(NPC.Center) < distance)
                {
                    distance = gamer.Center.DistanceSQ(NPC.Center);
                    Target = gamer.Center;
                }
            }
            if (Target == Vector2.Zero)
            {
                Target = Main.screenPosition + new Vector2 (-Main.screenWidth, 2 * Main.screenHeight);
            }
            else
            {
                for (int i = 150; i < 400; i++)
                {
                    for (int a = 150; a < 400; a++)
                    {
                        var block = Target.ToTileCoordinates();

                        for (int x = 1; x == 1; x = -1)
                        {
                            for (int y = 1; y == 1; y = -1)
                            {
                                block += new Point(a * x, i * y);
                                if (WorldGen.SolidOrSlopedTile(block.X, block.Y) && !WorldGen.SolidOrSlopedTile(block.X, block.Y - 1))
                                {
                                    Target = block.ToWorldCoordinates();
                                    return;
                                }
                            }
                        }
                    }
                }
            }
        }
        public enum State
        {
            Default,
            Stationary,
            Scared
        }
        public State state
        {
            get => (State)NPC.ai[0];
            set => NPC.ai[0] = (float)value;
        }
        public Vector2 Target
        {
            get => new(NPC.ai[1], NPC.ai[2]);
            set => NPC.ai = [NPC.ai[0], value.X, value.Y];
        }
        public override void AI()
        {
            if (state == State.Stationary)
            {
                foreach (var threat in Main.ActiveProjectiles)
                {
                    if (threat != null && threat.damage > 0 && threat.Center.DistanceSQ(NPC.Center) < 100 * 100)
                    {
                        Target += new Vector2(0, 2000).RotatedBy(Main.rand.NextFloat() * MathHelper.Pi - MathHelper.PiOver2);
                        state = State.Scared; break;
                    }
                }
                foreach (var threat in Main.ActiveNPCs)
                {
                    if (threat != null && threat.damage > 0 && threat.Center.DistanceSQ(NPC.Center) < 100 * 100)
                    {
                        Target += new Vector2(0, 2000).RotatedBy(Main.rand.NextFloat() * MathHelper.Pi - MathHelper.PiOver2);
                        state = State.Scared; break;
                    }
                }
                foreach (var threat in Main.ActivePlayers)
                {
                    if (threat != null && threat.Center.DistanceSQ(NPC.Center) < 100 * 100)
                    {
                        Target += new Vector2(0, 2000).RotatedBy(Main.rand.NextFloat() * MathHelper.Pi - MathHelper.PiOver2);
                        state = State.Scared; break;
                    }
                }
            }

            switch (state)
            {
                case State.Default:
                    Burrow();
                    break;

                case State.Stationary:
                    NPC.frameCounter++;

                    break;

                case State.Scared:
                    NPC.frameCounter++;
                    if (NPC.Center.DistanceSQ(Target) < 100 * 100)
                        NPC.despawnEncouraged = true;

                    
                    break;
            }
        }
        public void Burrow()
        {
            var underground = NPC.Center.DistanceSQ(Target) < 10 * 10;
            if (underground)
            {
                NPC.velocity *= 0;
                NPC.frameCounter++;
                return;
            }

            for (int y = NPC.height / 8; y > 0; y--)
            {
                for (int x = NPC.width / 8; x > 0; x--)
                {
                    var block = (NPC.position).ToTileCoordinates() + new Point(x, y);
                    if (WorldGen.SolidOrSlopedTile(block.X, block.Y))
                    {
                        Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Dirt);
                        underground = true;
                    }
                }
            }

            if (!underground)
            {
                NPC.despawnEncouraged = true;
                NPC.velocity *= 1.1f;
            }

            NPC.ai[1] = NPC.Center.X > Target.X + 99 ? -MathHelper.PiOver4 * 1.3f : NPC.Center.X < Target.X - 99 ? MathHelper.PiOver4 * 1.3f : NPC.ai[1];
            NPC.velocity = NPC.Center.DirectionTo(Target).RotatedBy(NPC.ai[1]).SafeNormalize(Vector2.Zero) * 9;
            NPC.rotation = NPC.velocity.ToRotation() + MathHelper.PiOver2;
        }
        public override void FindFrame(int frameHeight)
        {
            switch (state)
            {
                case State.Stationary:
                    if (NPC.frameCounter > 7)
                    {
                        NPC.frameCounter = 0;
                        int num = Main.rand.NextFloat() < 0.02f && NPC.frame.Y != 4 * frameHeight ? 5 : 0;
                        NPC.frame.Y = NPC.frame.Y != 5 * frameHeight ? num * frameHeight : NPC.frame.Y - frameHeight;
                    }
                    break;

                case State.Default:
                    if (NPC.frameCounter > 6)
                    {
                        NPC.frameCounter = 0;
                        if (NPC.frame.Y <= 0)
                            state = State.Stationary;
                        else
                            NPC.frame.Y -= frameHeight;
                    }
                    break;

                case State.Scared:
                    if (NPC.frameCounter > 6)
                    {
                        NPC.frameCounter = 0;
                        if (NPC.frame.Y < 3 * frameHeight)
                            NPC.frame.Y += frameHeight;
                        else
                            state = State.Default;
                    }
                    break;
            }
            base.FindFrame(frameHeight);
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            var texture = Terraria.GameContent.TextureAssets.Npc[NPC.type].Value;


            if (Main.LocalPlayer.HasBuff(BuffID.WeaponImbueGold))
                texture = MoleMod.MoleCritter_Alt.Value;

            var scale = NPC.scale * Main.GameZoomTarget;

            spriteBatch.Draw(texture, NPC.Center - Main.screenPosition + Vector2.UnitY * 5, NPC.frame, drawColor, NPC.rotation, NPC.frame.Size() / 2, scale, SpriteEffects.None, 0);

            return false;
        }
        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.AddTags(BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Underground,
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface,
                new FlavorTextBestiaryInfoElement("Mole creature"));
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            return Math.Max(SpawnCondition.Underworld.Chance * 0.1f, SpawnCondition.Overworld.Chance * 0.1f);
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life <= 0)
            {
                for (int i = 0; i < 6; i++)
                {
                    Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Worm, 2 * hit.HitDirection, -2f);
                    if (Main.rand.NextBool(2))
                    {
                        dust.noGravity = true;
                        dust.scale = 1.2f * NPC.scale;
                    }
                    else
                    {
                        dust.scale = 0.7f * NPC.scale;
                    }
                }
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, GoreID.Rat1, NPC.scale);
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, GoreID.Rat2, NPC.scale);
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, GoreID.Rat3, NPC.scale);
            }
        }
    }
}