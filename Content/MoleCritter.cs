using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;

namespace MoleMod.Content
{
    public class MoleCritter : ModNPC
    {
        private const int ClonedNPCID = NPCID.Frog;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = Main.npcFrameCount[6];
            Main.npcCatchable[Type] = true;

            NPCID.Sets.CountsAsCritter[Type] = true;
            NPCID.Sets.TakesDamageFromHostilesWithoutBeingFriendly[Type] = true;

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
            Burrowing,
            Unhiding,
            Stationary,
            Hiding
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
            if (!NPC.HasBuff(BuffID.Darkness))
                NPC.AddBuff(BuffID.Darkness, 601);

            if (NPC.buffTime[NPC.FindBuffIndex(BuffID.Darkness)] >= 600)
            {
                var distance = 1000f * 1000f;
                foreach (var gamer in Main.ActivePlayers)
                {
                    if (gamer != null && gamer.Center.DistanceSQ(NPC.Center) < distance)
                    {
                        distance = gamer.Center.DistanceSQ(NPC.Center);

                        for (int y = 14; y > -14; y--)
                        {
                            for (int x = 7; x < 21; x++)
                            {
                                var block = gamer.Center.ToTileCoordinates() + new Point(x, y);
                                if (WorldGen.SolidOrSlopedTile(block.X, block.Y) && !WorldGen.SolidOrSlopedTile(block.X, block.Y - 1))
                                {
                                    Target = block.ToWorldCoordinates();
                                    return;
                                }

                                block = gamer.Center.ToTileCoordinates() + new Point(-x, y);
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
            else if (Target.Y < 300)
            {
                NPC.buffImmune[BuffID.Darkness] = true;
            }

            if (!NPC.HasBuff(BuffID.Darkness))
            {
                NPC.active = false;
                NPC.life = 0;
            }
            else if (state != State.Burrowing)
            {
                NPC.buffTime[NPC.FindBuffIndex(BuffID.Darkness)]++;

                if (state == State.Stationary && NPC.frameCounter > 5)
                {
                    bool scared = false;


                    foreach (var threat in Main.ActiveNPCs)
                    {
                        if (threat != null && threat.damage > 0 && threat.Center.DistanceSQ(NPC.Center) < 150 * 150)
                        {
                            scared = true;
                            break;
                        }
                    }

                    if (!scared)
                    {
                        foreach (var threat in Main.ActiveProjectiles)
                        {
                            if (threat != null && threat.damage > 0 && threat.Center.DistanceSQ(NPC.Center) < 150 * 150)
                            {
                                scared = true;
                                break;
                            }
                        }
                    }

                    if (!scared)
                    {
                        foreach (var threat in Main.ActivePlayers)
                        {
                            if (threat != null && threat.Center.DistanceSQ(NPC.Center) < 150 * 150)
                            {
                                scared = true;
                                break;
                            }
                        }
                    }

                    if (scared)
                    {
                        int dir = NPC.Center.DirectionTo(Target).ToRotation() > MathHelper.Pi ? 4 : 5;
                        NPC.frame.Y = texture.Value.Height / 6 * dir;
                        state = State.Hiding;
                        Target += Vector2.UnitY.RotatedBy(Main.rand.NextFloat(-MathHelper.PiOver4, MathHelper.PiOver4)) * 400;
                        NPC.buffTime[NPC.FindBuffIndex(BuffID.Darkness)] = 100;
                    }
                }
            }

            else if (NPC.Center.DistanceSQ(Target) > NPC.Size.LengthSquared() + 16 * 16)
            {
                NPC.velocity = NPC.Center.DirectionTo(Target) * 5;
                NPC.velocity.Y *= 1.02f;

                for (int y = NPC.height / 8; y > 0; y--)
                {
                    for (int x = NPC.width / 8; x > 0; x--)
                    {
                        var block = (NPC.position).ToTileCoordinates() + new Point(x, y);

                        if (WorldGen.SolidOrSlopedTile(block.X, block.Y))
                            Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Dirt);
                    }
                }
            }
            else
            {
                state = State.Unhiding;
                NPC.noTileCollide = false;
                NPC.velocity *= 0;
            }

            NPC.rotation = NPC.velocity.SafeNormalize(-Vector2.UnitY).ToRotation() + MathHelper.PiOver2;
        }
        public Asset<Texture2D> texture;
        public override void FindFrame(int frameHeight)
        {
            texture ??= Terraria.GameContent.TextureAssets.Npc[NPC.type];
            frameHeight = texture.Value.Height / 6;

            NPC.frame = new Rectangle(0, NPC.frame.Y, texture.Value.Width, texture.Value.Height / 6);
            switch (state)
            {
                case State.Burrowing:
                    NPC.noTileCollide = true;
                    NPC.frame.Y = frameHeight * 3;
                    break;

                case State.Unhiding:
                    NPC.frameCounter++;

                    if (NPC.frameCounter > 6)
                    {
                        NPC.frameCounter = 0;
                        NPC.frame.Y -= frameHeight;

                        if (NPC.frame.Y == 0)
                            state = State.Stationary;
                    }
                    break;

                case State.Stationary:
                    NPC.frameCounter++;

                    if (NPC.frameCounter > 6)
                    {

                        NPC.frameCounter = 0;
                        if (NPC.frame.Y == 0 && Main.rand.NextFloat() < 0.03f)
                            NPC.frame.Y = 5 * frameHeight;

                        else if (NPC.frame.Y == 5 * frameHeight)
                            NPC.frame.Y -= frameHeight;

                        else
                            NPC.frame.Y = 0;
                    }
                    break;

                case State.Hiding:
                    NPC.frameCounter++;

                    if ((NPC.frameCounter > 50 && NPC.frame.Y >= frameHeight * 4) || NPC.frameCounter > 10)
                    {
                        NPC.frameCounter = 0;

                        if (NPC.frame.Y >= frameHeight * 4)
                            NPC.frame.Y = 0;

                        else
                        {
                            NPC.frame.Y += frameHeight;

                            if (NPC.frame.Y >= frameHeight * 3)
                                state = State.Burrowing;
                        }
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
            texture ??= Terraria.GameContent.TextureAssets.Npc[NPC.type];
            texture = Terraria.GameContent.TextureAssets.Npc[NPC.type];


            var scale = NPC.scale * Main.GameZoomTarget;

            spriteBatch.Draw(texture.Value, NPC.Center - screenPos + Vector2.UnitY * 5, NPC.frame, drawColor, NPC.rotation, NPC.frame.Size() / 2, scale, SpriteEffects.None, 0);

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
    public class GoldenMoleCritter : ModNPC
    {
        private const int ClonedNPCID = NPCID.Frog; // Easy to change type for your modder convenience

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = Main.npcFrameCount[6]; 
            Main.npcCatchable[Type] = true;


            NPCID.Sets.CountsAsCritter[Type] = true;
            NPCID.Sets.TakesDamageFromHostilesWithoutBeingFriendly[Type] = true;


            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;

            // This is so it appears between the frog and the gold frog
            NPCID.Sets.NormalGoldCritterBestiaryPriority.Insert(NPCID.Sets.NormalGoldCritterBestiaryPriority.IndexOf(ClonedNPCID) + 2, Type);
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
            NPC.catchItem = ModContent.ItemType<GoldenMoleCritterItem>();
            NPC.lavaImmune = true;
        }
        public enum State
        {
            Burrowing,
            Unhiding,
            Stationary,
            Hiding
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
            if (!NPC.HasBuff(BuffID.Darkness))
                NPC.AddBuff(BuffID.Darkness, 601);

            if (NPC.buffTime[NPC.FindBuffIndex(BuffID.Darkness)] >= 600)
            {
                var distance = 1000f * 1000f;
                foreach (var gamer in Main.ActivePlayers)
                {
                    if (gamer != null && gamer.Center.DistanceSQ(NPC.Center) < distance)
                    {
                        distance = gamer.Center.DistanceSQ(NPC.Center);

                        for (int y = 14; y > -14; y--)
                        {
                            for (int x = 7; x < 21; x++)
                            {
                                var block = gamer.Center.ToTileCoordinates() + new Point(x, y);
                                if (WorldGen.SolidOrSlopedTile(block.X, block.Y) && !WorldGen.SolidOrSlopedTile(block.X, block.Y - 1))
                                {
                                    Target = block.ToWorldCoordinates();
                                    return;
                                }

                                block = gamer.Center.ToTileCoordinates() + new Point(-x, y);
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
            else if (Target.Y < 300)
            {
                NPC.buffImmune[BuffID.Darkness] = true;
            }

            if (!NPC.HasBuff(BuffID.Darkness))
            {
                NPC.active = false;
                NPC.life = 0;
            }
            else if (state != State.Burrowing)
            {
                NPC.buffTime[NPC.FindBuffIndex(BuffID.Darkness)]++;

                if (state == State.Stationary && NPC.frameCounter > 5)
                {
                    bool scared = false;


                    foreach (var threat in Main.ActiveNPCs)
                    {
                        if (threat != null && threat.damage > 0 && threat.Center.DistanceSQ(NPC.Center) < 150 * 150)
                        {
                            scared = true;
                            break;
                        }
                    }

                    if (!scared)
                    {
                        foreach (var threat in Main.ActiveProjectiles)
                        {
                            if (threat != null && threat.damage > 0 && threat.Center.DistanceSQ(NPC.Center) < 150 * 150)
                            {
                                scared = true;
                                break;
                            }
                        }
                    }

                    if (!scared)
                    {
                        foreach (var threat in Main.ActivePlayers)
                        {
                            if (threat != null && threat.Center.DistanceSQ(NPC.Center) < 150 * 150)
                            {
                                scared = true;
                                break;
                            }
                        }
                    }

                    if (scared)
                    {
                        int dir = NPC.Center.DirectionTo(Target).ToRotation() > MathHelper.Pi ? 4 : 5;
                        NPC.frame.Y = texture.Value.Height / 6 * dir;
                        state = State.Hiding;
                        Target += Vector2.UnitY.RotatedBy(Main.rand.NextFloat(-MathHelper.PiOver4, MathHelper.PiOver4)) * 400;
                        NPC.buffTime[NPC.FindBuffIndex(BuffID.Darkness)] = 100;
                    }
                }
            }

            else if (NPC.Center.DistanceSQ(Target) > NPC.Size.LengthSquared() + 16 * 16)
            {
                NPC.velocity = NPC.Center.DirectionTo(Target) * 5;
                NPC.velocity.Y *= 1.02f;

                for (int y = NPC.height / 8; y > 0; y--)
                {
                    for (int x = NPC.width / 8; x > 0; x--)
                    {
                        var block = (NPC.position).ToTileCoordinates() + new Point(x, y);

                        if (WorldGen.SolidOrSlopedTile(block.X, block.Y))
                            Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Dirt);
                    }
                }
            }
            else
            {
                state = State.Unhiding;
                NPC.noTileCollide = false;
                NPC.velocity *= 0;
            }

            NPC.rotation = NPC.velocity.SafeNormalize(-Vector2.UnitY).ToRotation() + MathHelper.PiOver2;
        }
        public Asset<Texture2D> texture;
        public override void FindFrame(int frameHeight)
        {
            texture ??= Terraria.GameContent.TextureAssets.Npc[NPC.type];
            frameHeight = texture.Value.Height / 6;

            NPC.frame = new Rectangle(0, NPC.frame.Y, texture.Value.Width, texture.Value.Height / 6);
            switch (state)
            {
                case State.Burrowing:
                    NPC.noTileCollide = true;
                    NPC.frame.Y = frameHeight * 3;
                    break;

                case State.Unhiding:
                    NPC.frameCounter++;

                    if (NPC.frameCounter > 6)
                    {
                        NPC.frameCounter = 0;
                        NPC.frame.Y -= frameHeight;

                        if (NPC.frame.Y == 0)
                            state = State.Stationary;
                    }
                    break;

                case State.Stationary:
                    NPC.frameCounter++;

                    if (NPC.frameCounter > 6)
                    {

                        NPC.frameCounter = 0;
                        if (NPC.frame.Y == 0 && Main.rand.NextFloat() < 0.03f)
                            NPC.frame.Y = 5 * frameHeight;

                        else if (NPC.frame.Y == 5 * frameHeight)
                            NPC.frame.Y -= frameHeight;

                        else
                            NPC.frame.Y = 0;
                    }
                    break;

                case State.Hiding:
                    NPC.frameCounter++;

                    if ((NPC.frameCounter > 50 && NPC.frame.Y >= frameHeight * 4) || NPC.frameCounter > 10)
                    {
                        NPC.frameCounter = 0;

                        if (NPC.frame.Y >= frameHeight * 4)
                            NPC.frame.Y = 0;

                        else
                        {
                            NPC.frame.Y += frameHeight;

                            if (NPC.frame.Y >= frameHeight * 3)
                                state = State.Burrowing;
                        }
                    }
                    break;
            }
            base.FindFrame(frameHeight);
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.AddTags(BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Underground,
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface,
                new FlavorTextBestiaryInfoElement("Golden mole thing"));
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            return Math.Max(SpawnCondition.Underground.Chance * 0.01f, SpawnCondition.Overworld.Chance * 0.01f);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            texture ??= Terraria.GameContent.TextureAssets.Npc[NPC.type];
            texture = Terraria.GameContent.TextureAssets.Npc[NPC.type];

            var scale = NPC.scale * Main.GameZoomTarget;

            spriteBatch.Draw(texture.Value, NPC.Center - screenPos + Vector2.UnitY * 5, NPC.frame, drawColor, NPC.rotation, NPC.frame.Size() / 2, scale, SpriteEffects.None, 0);

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
