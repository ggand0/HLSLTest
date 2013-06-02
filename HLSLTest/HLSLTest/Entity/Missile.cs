using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HLSLTest
{
	/// <summary>
	/// 軌跡エフェクトを付けて動かしまくりたい！
	/// </summary>
	public class Missile : Bullet
	{
		public Object Target { get; private set; }

		// RendererとしてObjectを持たせよう
		//public Model Model { get; private set; }
		public Object Renderer { get; private set; }
		public Vector3 Velocity { get; private set; }
		private Matrix _world;
		//private float Scale;
		private BillboardStrip billboardStrip;
		private List<Vector3> positions;
		private BoundingSphere boundingSphere;
		private void UpdateLocus()
		{
			positions.Add(Position);
			billboardStrip.Positions = positions;
			if (positions.Count >= BillboardStrip.MAX_SIZE) {//120
				positions.RemoveAt(0);
			} else if (positions.Count > 0) {//positions.Count >= 2
				//billboardStrip.AddBillboard(Level.graphicsDevice, content, content.Load<Texture2D>("Textures\\Mercury\\Laser"), new Vector2(10, 40), positions[positions.Count - 2], positions[positions.Count - 1]);
				//billboardStrip.AddBillboard(Level.graphicsDevice, content, content.Load<Texture2D>("Textures\\Laser2"), new Vector2(10, 40), positions[positions.Count - 2], positions[positions.Count - 1]);
				billboardStrip.AddVertices();
			}
		}

		public override void Update(GameTime gameTime)
		{
			// とりあえず敵が追跡中に死んだら自分も殺すようにしておく
			/*if (!Target.IsAlive || !Target.IsActive) {
				Die();
				return;
			}*/

			Direction = Vector3.Normalize(Target.Position - Position);
			Velocity = Direction * Speed;

			Position += Velocity;
			UpdateWorldMatrix();
			boundingSphere = new BoundingSphere(Position, Renderer.Model.Meshes[0].BoundingSphere.Radius * Renderer.Scale);

			Renderer.Position = Position;
			Renderer.World = _world;
			UpdateLocus();
			billboardStrip.Update(gameTime);

			this.IsActive = IsActiveNow();
		}

		public override bool IsActiveNow()
		{
			distanceTravelled = Vector3.Distance(StartPosition, Position);
			if (distanceTravelled > MAX_DISTANCE || !Target.IsAlive || !Target.IsActive) {
				return false;
			} else {
				return true;
			}
		}
		protected void UpdateWorldMatrix()
		{
			_world = Matrix.Identity;
			_world *= Matrix.CreateScale(Renderer.Scale);
			_world *= Matrix.CreateTranslation(Position);
			Vector3 workVector = _world.Translation;
			workVector.Normalize();

			/*_world.Forward *= Direction;
			_world.Up *= Vector3.Normalize(Vector3.Cross(_world.Forward, workVector));
			_world.Right *= Vector3.Normalize(Vector3.Cross(_world.Forward, _world.Up));*/
		}
		public override bool IsHitWith(Object o)
		{
			//return Renderer.IsHitWith(o.transformedBoundingSphere);
			return boundingSphere.Intersects(o.transformedBoundingSphere);
		}



		public override void Draw(Camera camera)
		{
			Renderer.Draw(camera.View, camera.Projection, camera.Position);
			billboardStrip.Draw(camera.View, camera.Projection, camera.Up, camera.Right, camera.Position);
		}


		// Constructors
		public Missile(Object user, float speed)
			: this(IFF.Friend, user, null, speed, user.Direction, user.Position, 1, "Models\\pea_proj")
		{
		}
		public Missile(IFF identification, Object user, Object target, float speed, Vector3 direction, Vector3 position, float scale, string filePath)
			:base(identification, position, direction, speed)
		{
			/*this.Position = position;
			startPosition = position;
			this.Speed = speed;
			this.Direction = direction;
			Direction.Normalize();
			this.Right = user.Right;
			this.Up = user.Up;
			this.Velocity = Direction * speed;
			Load(filePath);*/

			MAX_DISTANCE = 2000;
			this.Target = target;
			Position = position;
			positions = new List<Vector3>();
			Renderer = new Object(position, scale, filePath);
			billboardStrip = new BillboardStrip(Level.graphicsDevice, content,
				content.Load<Texture2D>("Textures\\Lines\\smoke"), new Vector2(10, 30), positions, true);
		}
	}
}
