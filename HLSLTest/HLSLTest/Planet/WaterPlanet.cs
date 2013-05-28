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
		protected override void LoadContent(ContentManager content)
		{
			terrain = content.Load<Effect>("Planets\\WaterPlanet");
			draw = content.Load<Effect>("Planets\\ColorMap");
			Palette = content.Load<Texture2D>("Textures\\Planet\\water2");

			base.LoadContent(content);
			//model = content.Load<Model>("Models\\SkySphereMesh");
			//model = content.Load<Model>("Models\\sphereNoTex");
			//model = content.Load<Model>("Models\\sphereTransparentTex");

			baseTexture = content.Load<Texture2D>("Textures\\Terrain\\water10");
			baseTexture = content.Load<Texture2D>("Textures\\Terrain\\water10_normal");
			gTexture = content.Load<Texture2D>("Textures\\Terrain\\grass2");
			bTexture = content.Load<Texture2D>("Textures\\Terrain\\stone");

			renderType = PlanetRenderType.OneTexMultiColored;
			SubType = 0;
			Nz = 1;//128;

			Generate(graphicsDevice);
			Scale = 50;
		}
		


		// Constructors
		public WaterPlanet(Vector3 starPosition, GraphicsDevice graphics, ContentManager content)
			:this(DEF_POSITION, starPosition, graphics, content)
		{
		}
		public WaterPlanet(Vector3 position, Vector3 starPosition, GraphicsDevice graphics, ContentManager content)
			: base(position, starPosition, graphics, content)
		{
			Type = PlanetType.Water;
			rotate = true;
			//revolution = true;
		}

		
	}
}
