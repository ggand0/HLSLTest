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
	public class PointLightCircle : PPPointLight
	{
		public Vector3 Center { get; private set; }
		public float Speed { get; private set; }
		public float Angle { get; private set; }

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			/*float radius = 200;
			float freq = 10;
			Speed = (0.001f * 2 * (float)Math.PI) / freq;
			float angle = (float)gameTime.ElapsedGameTime.TotalMilliseconds * Speed;
			Vector3 velocity = new Vector3((float)Math.Cos(MathHelper.ToRadians(angle)), 0, (float)Math.Sign(MathHelper.ToRadians(angle)));*/

			Angle += MathHelper.ToRadians(Speed);
			//Vector3 velocity = new Vector3((float)Math.Cos(Angle), 0, (float)Math.Sign(MathHelper.ToRadians(Angle)));
			Vector3 velocity = new Vector3((float)Math.Cos(Angle), 0, (float)Math.Sin(Angle));
			Vector3 tmp = Center + velocity * 200;//new Vector3((float)Math.Cos(Angle), 0, (float)Math.Sign(Angle)) * 200;

			Position = tmp;
		}

		public PointLightCircle(Vector3 center, Vector3 position, Color Color, float Attenuation)
			:base(position, Color, Attenuation)
		{
			this.Center = center;
			Speed = 1;
		}
	}
}
