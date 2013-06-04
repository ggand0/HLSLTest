using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace HLSLTest
{
	public class LaserBillboardBullet : Bullet//LaserBillboard
	{
        private int count;
		protected LaserBillboard laserRenderer;
		/// <summary>
		/// 移動する光線タイプか、始点・終点を結ぶタイプか
		/// </summary>
		public int Mode { get; private set; }

		public Drawable User { get; private set; }
		public Drawable Target { get; private set; }

		public override void Update(GameTime gameTime)
		{
			count++;
			if (Mode == 1 && User != null && Target != null) {
				//laserRenderer.ChangePosition(User.Position, Target.Position);
				laserRenderer.UpdatePositions(User.Position, Target.Position);
			}

			if (Mode == 0) {
				/*for (int i = 0; i < particles.Length; i++) {
					particles[i].StartPosition += Direction * Speed;
					particles[i].DirectedPosition += Direction * Speed;// これをUpdateしていないせいでは？？？
				}
				vertexBuffers.SetData<ParticleVertex>(particles);
				indexBuffers.SetData<int>(indices);*/
				laserRenderer.MoveLaser(Direction, Speed);
			}
			

			
			Position = laserRenderer.Mid;
			IsActive = IsActiveNow();
		}
		public override bool IsActiveNow()
		{
			if (Mode == 0) {
				//distanceTravelled = Vector3.Distance(StartPosition, laserRenderer.particles[0].StartPosition);
				distanceTravelled = laserRenderer.GetTraveledDistance(0);
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
			return base.IsHitWith(d);
		}
		/// <summary>
		/// http://www.antun.net/tips/algorithm/collision.html より。
		/// </summary>
		/// <param name="o"></param>
		/// <returns></returns>
		public override bool IsHitWith(Object o)
		{
			/*BoundingSphere bs = new BoundingSphere(Position, laserRenderer.billboardSize.Y);
			//return (bs.Center - o.Model.Meshes[0].BoundingSphere.Center).Length() < bs.Radius + o.Model.Meshes[0].BoundingSphere.Radius;
			//return (o.Model.Meshes[0].BoundingSphere.Center - End).Length() < bs.Radius;
			//return (o.Position - End).Length() < o.Model.Meshes[0].BoundingSphere.Radius;//bs.Radius;

			//return End == o.Position;
			//return (laserRenderer.End - o.Position).Length() < 50;
			return (laserRenderer.End - o.Position).Length() < bs.Radius;*/


			// Bullet側から移動してるparticlesの位置を参照するのは面倒なのでLaserBillboardに任せる
			return laserRenderer.IsHitWith(o);
		}
		public override void Draw(Camera camera)
		{
			//base.Draw(camera);
			laserRenderer.Draw(camera);
		}


        #region Constructors
        public LaserBillboardBullet(IFF id, GraphicsDevice graphicsDevice,
			ContentManager content, Vector3 position, Vector3 direction, float speed, Texture2D texture, Vector2 billboardSize)
			//:base(graphicsDevice, content, texture, billboardSize, position, position + direction)
			: this(id, graphicsDevice, content, position, direction, speed, texture, billboardSize, 0)
		{
		}
		public LaserBillboardBullet(IFF id, GraphicsDevice graphicsDevice,
			ContentManager content, Vector3 position, Vector3 direction, float speed, Texture2D texture, Vector2 billboardSize, int mode)
			//: base(graphicsDevice, content, texture, billboardSize, position, position + direction)
			//: base(graphicsDevice, content, texture, billboardSize, position, position + Vector3.Normalize(direction) * billboardSize.X)
			//:base(id, position, direction, speed)
			:this(id, graphicsDevice, content, position, position+Vector3.Normalize(direction), direction, speed, texture, Color.White, BlendState.AlphaBlend, billboardSize, 0)
		{
			/*IsActive = true;
			Direction.Normalize();
			this.Mode = mode;

			laserRenderer = new LaserBillboard(graphicsDevice, content, texture, billboardSize, position, position + Vector3.Normalize(direction) * billboardSize.X);*/
		}

		public LaserBillboardBullet(IFF id, GraphicsDevice graphicsDevice,
			ContentManager content, Vector3 startPosition, Vector3 endPosition, Vector3 direction, float speed, Texture2D texture, Color laserColor, BlendState laserBlendState, Vector2 billboardSize, int mode)
			//: base(graphicsDevice, content, texture, billboardSize, startPosition, endPosition, laserColor, laserBlendState)
			: base(id, startPosition, direction, speed)
		{
            MAX_DISTANCE = 2000;
			IsActive = true;
			Direction.Normalize();
			this.Mode = mode;

			laserRenderer = new LaserBillboard(graphicsDevice, content, texture, billboardSize, startPosition, endPosition);
        }


		public LaserBillboardBullet(IFF id, GraphicsDevice graphicsDevice,
			ContentManager content, Drawable user, Drawable target, Vector3 direction, float speed, Texture2D texture, Color laserColor, BlendState laserBlendState, Vector2 billboardSize, int mode)
			//: base(graphicsDevice, content, texture, billboardSize, startPosition, endPosition, laserColor, laserBlendState)
			: base(id, user.Position, direction, speed)
		{
			MAX_DISTANCE = 2000;
			IsActive = true;
			Direction.Normalize();
			this.Mode = mode;
			this.User = user;
			this.Target = target;

			//laserRenderer = new LaserBillboard(graphicsDevice, content, texture, billboardSize, startPosition, endPosition);
			laserRenderer = new LaserBillboard(graphicsDevice, content, texture, billboardSize, user.Position, target.Position, Color.White, BlendState.Additive, 1);
		}
        #endregion
    }
}
