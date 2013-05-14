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
		public RockPlanet(GraphicsDevice graphics, ContentManager content)
			: base(graphics, content)
		{
			Type = PlanetType.Water;
		}

		protected override void LoadContent(ContentManager content)
		{
			draw = content.Load<Effect>("Planets\\ColorMap");
			base.LoadContent(content);
			//model = content.Load<Model>("Models\\SkySphereMesh");
			Model = content.Load<Model>("Models\\sphere2");
			//model = content.Load<Model>("Models\\sphereNoTex");
			//model = content.Load<Model>("Models\\sphereTransparentTex");
			terrain = content.Load<Effect>("Planets\\RockPlanet");
			
			Palette = content.Load<Texture2D>("Textures\\Planet\\rock");
			Nz = 128;//50

			Generate(graphicsDevice);
		}
	}
}
