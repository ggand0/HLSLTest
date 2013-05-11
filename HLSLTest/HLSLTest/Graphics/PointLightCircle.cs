using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HLSLTest
{
	/// <summary>
	/// デモ用の回転するライト
	/// </summary>
	public class PointLightCircle : PointLight
	{
		public Vector3 Center { get; private set; }
		public float Speed { get; private set; }
		public float Radius { get; private set; }
		public float Angle { get; private set; }

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			Angle += MathHelper.ToRadians(Speed);
			Vector3 velocity = new Vector3((float)Math.Cos(Angle), 0, (float)Math.Sin(Angle));
			Vector3 tmp = Center + velocity * Radius;//new Vector3((float)Math.Cos(Angle), 0, (float)Math.Sign(Angle)) * 200;

			Position = tmp;
		}

		public PointLightCircle(Vector3 center, float radius, Color Color, float Attenuation)
			:base(Vector3.Zero, Color, Attenuation)
		{
			this.Center = center;
			this.Radius = radius;
			Speed = 1;
		}
	}
}
