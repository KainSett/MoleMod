using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;

namespace MoleMod.Content
{
    /// <summary>
    /// This file shows off a critter npc. The unique thing about critters is how you can catch them with a bug net.
    /// The important bits are: Main.npcCatchable, NPC.catchItem, and Item.makeNPC.
    /// We will also show off adding an item to an existing RecipeGroup (see ExampleRecipes.AddRecipeGroups).
    /// Additionally, this example shows an involved IL edit.
    /// </summary>
    public class MoleCritter : ModNPC
    {
        private const int ClonedNPCID = NPCID.Frog; // Easy to change type for your modder convenience

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = Main.npcFrameCount[ClonedNPCID]; // Copy animation frames
            Main.npcCatchable[Type] = true; // This is for certain release situations

            // These three are typical critter values
            NPCID.Sets.CountsAsCritter[Type] = true;
            NPCID.Sets.TakesDamageFromHostilesWithoutBeingFriendly[Type] = true;
            NPCID.Sets.TownCritter[Type] = true;

            // The frog is immune to confused
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;

            // This is so it appears between the frog and the gold frog
            NPCID.Sets.NormalGoldCritterBestiaryPriority.Insert(NPCID.Sets.NormalGoldCritterBestiaryPriority.IndexOf(ClonedNPCID) + 1, Type);
        }

        public override void SetDefaults()
        {
            NPC.noTileCollide = true;
            NPC.width = 30;
            NPC.height = 20;
            NPC.damage = 0;
            NPC.defense = 0;
            NPC.lifeMax = 5;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.catchItem = ModContent.ItemType<MoleCritterItem>();
            NPC.lavaImmune = true;
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
            if (Target == Vector2.Zero)
            {
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
                    Target = Main.screenPosition + new Vector2(Main.screenWidth, Main.screenHeight);
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

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.AddTags(BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Underground,
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface,
                new FlavorTextBestiaryInfoElement("Mole thing"));
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            return Math.Max(SpawnCondition.Underground.Chance * 0.1f, SpawnCondition.Overworld.Chance * 0.1f);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            var texture = Terraria.GameContent.TextureAssets.Npc[NPC.type].Value;


            if (Main.LocalPlayer.HasBuff(BuffID.WeaponImbueGold))
                texture = MoleMod.MoleCritter_Alt.Value;

            var scale = NPC.scale * Main.GameZoomTarget;

            spriteBatch.Draw(texture, NPC.Center - screenPos + Vector2.UnitY * 5, NPC.frame, drawColor, NPC.rotation, NPC.frame.Size() / 2, scale, SpriteEffects.None, 0);

            return false;
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
            }
        }

    }

}