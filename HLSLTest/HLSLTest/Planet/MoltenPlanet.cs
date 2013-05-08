using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace HLSLTest
{
	public class MoltenPlanet : Planet
	{
		protected Texture2D lava;

		public MoltenPlanet(GraphicsDevice graphics, ContentManager content)
			: base(graphics, content)
		{
			Type = PlanetType.Water;
		}

		protected override void LoadContent(ContentManager content)
		{

			base.LoadContent(content);
			//model = content.Load<Model>("Models\\SkySphereMesh");
			model = content.Load<Model>("Models\\sphere2");
			lava = content.Load<Texture2D>("Textures\\Planet\\lava");
			//model = content.Load<Model>("Models\\sphereNoTex");
			//model = content.Load<Model>("Models\\sphereTransparentTex");
			terrain = content.Load<Effect>("Planets\\WaterPlanet");
			draw = content.Load<Effect>("Planets\\MoltenPlanet");
			Palette = content.Load<Texture2D>("Textures\\Planet\\rock");
			Nz = 128;//50

			Generate(graphics);
		}

		public override void Draw(Matrix View, Matrix Projection, Vector3 CameraPosition)
		{
			draw.Parameters["Lava"].SetValue(lava);// added

			base.Draw(View, Projection, CameraPosition);
		}
	}
}
