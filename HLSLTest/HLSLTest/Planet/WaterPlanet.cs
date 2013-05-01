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
		GraphicsDevice graphicsDevice;

		public WaterPlanet(GraphicsDevice graphics)
		{
			Type = PlanetType.Water;
			BuildPerm(graphics);
			graphicsDevice = graphics;
		}

		public void LoadContent(ContentManager content)
		{
			//model = content.Load<Model>("Models\\SkySphereMesh");
			model = content.Load<Model>("Models\\sphere2");
			//model = content.Load<Model>("Models\\sphereNoTex");
			//model = content.Load<Model>("Models\\sphereTransparentTex");
			terrain = content.Load<Effect>("Terrain\\WaterPlanet");
			draw = content.Load<Effect>("Terrain\\ColorMap");
			Palette = content.Load<Texture2D>("Textures//Palette");

			Generate(graphicsDevice);
		}
		
	}
}
