using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace HLSLTest
{
	public class LaserBillboardBullet : LaserBillboard
	{
		protected static float MAX_DISTANCE = 10000;
		/// <summary>
		/// インターフェース実装で関数として機能があるからプロパティは消してもいいかも
		/// </summary>
		//public bool IsActive { get; set; }
		public float Speed { get; private set; }
		public Vector3 Direction { get; private set; }
		private float distanceTravelled;
		public Vector3 StartPosition { get; private set; }

		public int Mode { get; private set; }
		private int count;

		public override void Update(GameTime gameTime)
		{
			count++;
			if (Mode == 0) {
				for (int i = 0; i < particles.Length; i++) {
					particles[i].StartPosition += Direction * Speed;
					particles[i].DirectedPosition += Direction * Speed;// これをUpdateしていないせいでは？？？
				}


				vertexBuffers.SetData<ParticleVertex>(particles);
				indexBuffers.SetData<int>(indices);
			}

			IsActive = IsActiveNow();
		}
		public bool IsActiveNow()
		{
			if (Mode == 0) {
				distanceTravelled = Vector3.Distance(StartPosition, particles[0].StartPosition);
				if (distanceTravelled > MAX_DISTANCE) {
					return false;
				} else {
					return true;
				}
			} else {//else if (Mode == 1) {
				return count >= 15 ? false : true;// 一瞬でターゲットにたどり着くタイプ：stational
			}
		}
		public override bool IsHitWith(Drawable d)
		{
			BoundingSphere bs = new BoundingSphere();
			return base.IsHitWith(d);
		}
		public override bool IsHitWith(Object o)
		{
			BoundingSphere bs = new BoundingSphere(Position, billboardSize.Y);
			//return (bs.Center - o.Model.Meshes[0].BoundingSphere.Center).Length() < bs.Radius + o.Model.Meshes[0].BoundingSphere.Radius;
			//return (o.Model.Meshes[0].BoundingSphere.Center - End).Length() < bs.Radius;
			//return (o.Position - End).Length() < o.Model.Meshes[0].BoundingSphere.Radius;//bs.Radius;

			//return End == o.Position;
			return (End - o.Position).Length() < 50;
		}

		// Constructors
		public LaserBillboardBullet(GraphicsDevice graphicsDevice,
			ContentManager content, Vector3 position, Vector3 direction, float speed, Texture2D texture, Vector2 billboardSize)
			//:base(graphicsDevice, content, texture, billboardSize, position, position + direction)
			: this(graphicsDevice, content, position, direction, speed, texture, billboardSize, 0)
		{
		}
		public LaserBillboardBullet(GraphicsDevice graphicsDevice,
			ContentManager content, Vector3 position, Vector3 direction, float speed, Texture2D texture, Vector2 billboardSize, int mode)
			//: base(graphicsDevice, content, texture, billboardSize, position, position + direction)
			: base(graphicsDevice, content, texture, billboardSize, position, position + Vector3.Normalize(direction) * billboardSize.X)
		{
			this.Direction = direction;
			this.Speed = speed;
			IsActive = true;
			Direction.Normalize();
			StartPosition = position;
			this.Mode = mode;
		}

		public LaserBillboardBullet(GraphicsDevice graphicsDevice,
			ContentManager content, Vector3 startPosition, Vector3 endPosition, Vector3 direction, float speed, Texture2D texture, Color laserColor, BlendState laserBlendState, Vector2 billboardSize, int mode)
			: base(graphicsDevice, content, texture, billboardSize, startPosition, endPosition, laserColor, laserBlendState)
		{
			this.Direction = direction;
			this.Speed = speed;
			IsActive = true;
			Direction.Normalize();
			StartPosition = startPosition;
			this.Mode = mode;
		}
	}
}
