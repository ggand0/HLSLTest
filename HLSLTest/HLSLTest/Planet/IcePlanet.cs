using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace HLSLTest
{
	public class IcePlanet : Planet
	{
		public IcePlanet(GraphicsDevice graphics, ContentManager content)
			:base(graphics, content)
		{
			Type = PlanetType.Water;
		}

		protected override void LoadContent(ContentManager content)
		{
			base.LoadContent(content);
			//model = content.Load<Model>("Models\\SkySphereMesh");
			Model = content.Load<Model>("Models\\sphere2");
			//model = content.Load<Model>("Models\\sphereNoTex");
			//model = content.Load<Model>("Models\\sphereTransparentTex");
			terrain = content.Load<Effect>("Planets\\IcePlanet");
			draw = content.Load<Effect>("Planets\\ColorMap");
			Palette = content.Load<Texture2D>("Textures\\Planet\\frozen2");
			Nz = 256;//128;

			Generate(graphicsDevice);
			SubType = 2;
		}
	}
}
