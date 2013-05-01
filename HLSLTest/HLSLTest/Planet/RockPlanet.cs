using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace HLSLTest
{
	public class RockPlanet : Planet
	{
		GraphicsDevice graphicsDevice;

		public RockPlanet(GraphicsDevice graphics, ContentManager content)
		{
			Type = PlanetType.Water;
			BuildPerm(graphics);
			graphicsDevice = graphics;

			LoadContent(content);
		}

		public void LoadContent(ContentManager content)
		{
			//model = content.Load<Model>("Models\\SkySphereMesh");
			model = content.Load<Model>("Models\\sphere2");
			//model = content.Load<Model>("Models\\sphereNoTex");
			//model = content.Load<Model>("Models\\sphereTransparentTex");
			terrain = content.Load<Effect>("Terrain\\RockPlanet");
			draw = content.Load<Effect>("Terrain\\ColorMap");
			Palette = content.Load<Texture2D>("Textures\\Planet\\rock");
			Nz = 128;//50

			Generate(graphicsDevice);
		}
	}
}
