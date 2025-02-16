using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using ReLogic.Content;

namespace MoleMod
{
	// Please read https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Modding-Guide#mod-skeleton-contents for more information about the various files in a mod.
	public class MoleMod : Mod
	{
		public static Asset<Texture2D> MolePet_Alt;
        public static Asset<Texture2D> MolePetSide_Alt;
        public static Asset<Texture2D> MolePetSide;
        public override void Load()
        {
            if (Main.netMode != NetmodeID.Server)
            {
                MolePetSide = ModContent.Request<Texture2D>("MoleMod/Content/MolePetSide");
                MolePetSide_Alt = ModContent.Request<Texture2D>("MoleMod/Content/MolePetSide_Alt");
                MolePet_Alt = ModContent.Request<Texture2D>("MoleMod/Content/MolePet_Alt");
            }
        }
        public override void Unload()
        {
            if (Main.netMode != NetmodeID.Server)
            {
                MolePetSide.Dispose();
                MolePetSide_Alt.Dispose();
                MolePet_Alt.Dispose();
            }
        }
    }
}
