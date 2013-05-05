﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace HLSLTest
{
	public class GasGiant : Planet
	{
		public GasGiant(GraphicsDevice graphics, ContentManager content)
			: base(graphics, content)
		{
			Type = PlanetType.Water;
		}

		protected override void LoadContent(ContentManager content)
		{
			base.LoadContent(content);
			//model = content.Load<Model>("Models\\SkySphereMesh");
			model = content.Load<Model>("Models\\sphere2");
			//model = content.Load<Model>("Models\\sphereNoTex");
			//model = content.Load<Model>("Models\\sphereTransparentTex");
			terrain = content.Load<Effect>("Planets\\GasGiant");
			draw = content.Load<Effect>("Planets\\ColorMap");
			Palette = content.Load<Texture2D>("Textures\\Planet\\gas2");
			Nz = 12800;

			Generate(graphics);
		}
	}
}
