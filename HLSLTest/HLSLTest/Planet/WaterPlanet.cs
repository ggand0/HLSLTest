using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace HLSLTest
{
	public class WaterPlanet : Planet
	{
		

		public WaterPlanet(GraphicsDevice graphics, ContentManager content)
			:base(graphics, content)
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
			terrain = content.Load<Effect>("Planets\\WaterPlanet");
			draw = content.Load<Effect>("Planets\\ColorMap");
			Palette = content.Load<Texture2D>("Textures\\Planet\\water2");

			baseTexture = content.Load<Texture2D>("Textures\\water");
			gTexture = content.Load<Texture2D>("Textures\\grass2");
			bTexture = content.Load<Texture2D>("Textures\\stone");

			Nz = 1;//128;

			Generate(graphics);
		}
	}
}
