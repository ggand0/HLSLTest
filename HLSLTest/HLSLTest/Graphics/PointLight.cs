using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HLSLTest
{
	public class PointLight
	{
		public static Game1 game;
		public Vector3 Position { get; set; }
		public Color Color { get; set; }
		public float Attenuation { get; set; }

		protected BoundingSphereRenderer _boundingSphereRenderer;
		//public BoundingSphere transformedBoundingSphere { get; protected set; }

		public PointLight(Vector3 Position, Color Color, float Attenuation)
		{
			this.Position = Position;
			this.Color = Color;
			this.Attenuation = Attenuation;
			_boundingSphereRenderer = new BoundingSphereRenderer(game);
			_boundingSphereRenderer.OnCreateDevice();
		}

		public virtual void Update(GameTime gameTime)
		{
		}
		public void DrawBoundingSphere()
		{
			BoundingSphere defBS = new BoundingSphere(Position, 10);
			_boundingSphereRenderer.Draw(defBS, this.Color);
		}

		public void SetEffectParameters(Effect effect)
		{
			effect.Parameters["LightPosition"].SetValue(Position);
			effect.Parameters["LightAttenuation"].SetValue(Attenuation);
			effect.Parameters["LightColor"].SetValue(Color.ToVector3());
		}
	}
}
