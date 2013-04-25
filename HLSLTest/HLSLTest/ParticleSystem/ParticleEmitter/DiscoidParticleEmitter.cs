using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace HLSLTest
{
	public class DiscoidParticleEmitter : ParticleEmitter//ExplosionParticleEmitter
	{
		protected override void MoveParticle()
		{
			var speed = 4.0f;
			var innerRadius = 0.5f;
			var outerRadius = 1.5f;

			if (frameCount <= maxEmitFrameCount) {
				for (int i = 0; i < emitNumPerFrame; i++) {
					Vector3 ruv = RandomUnitVectorInPlane(Matrix.CreateTranslation(Position), Vector3.Up);// 方向を決める
					Vector3 newPos = Position + ((ruv * innerRadius) + (ruv * (float)NextDouble(rand, innerRadius, outerRadius)));
					Vector3 pos = newPos;
					Vector3 velocity = ruv * speed;
					AddParticle(Position + pos + new Vector3(0, 2, 0), velocity, speed);
				}
			}
		}
		private Vector3 RandomUnitVectorInPlane(Matrix xform, Vector3 axis)
		{
			xform *= Matrix.CreateFromAxisAngle(axis, (float)NextDouble(rand, 0.0, 360.0));
			Vector3 ruv = xform.Right;
			ruv.Normalize();
			return ruv;
		}
		private double NextDouble(Random r, double min, double max)
		{
			return min + r.NextDouble() * (max - min);
		}


		public DiscoidParticleEmitter(GraphicsDevice graphicsDevice, ContentManager content, Texture2D texture, int particleNum,
			Vector2 particleSize, float lifespan, float FadeInTime)
			:base(graphicsDevice, content, texture, particleNum, particleSize, lifespan, FadeInTime)
		{
			emitNumPerFrame = 100;//20
		}
	}
}
