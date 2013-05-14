using System;
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
			:this(DEF_POSITION, DEF_STAR_POSITION, graphics, content)
		{
			Type = PlanetType.Water;
		}
		public GasGiant(Vector3 position, Vector3 starPosition, GraphicsDevice graphics, ContentManager content)
			: base(position, starPosition, graphics, content)
		{
			Type = PlanetType.Gas;
			rotate = true;
			revolution = true;
		}

		protected override void LoadContent(ContentManager content)
		{
			draw = content.Load<Effect>("Planets\\ColorMap");
			base.LoadContent(content);
			//model = content.Load<Model>("Models\\SkySphereMesh");
			Model = content.Load<Model>("Models\\sphere2");
			//model = content.Load<Model>("Models\\sphereNoTex");
			//model = content.Load<Model>("Models\\sphereTransparentTex");
			terrain = content.Load<Effect>("Planets\\GasGiant");
			
			Palette = content.Load<Texture2D>("Textures\\Planet\\gas2");
			Nz = 12800;

			Generate(graphicsDevice);
		}
	}
}
