using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Audio;

namespace HLSLTest
{
	public enum FighterState
	{
		Move,
		Attack
	}
	public class Fighter : Object
	{
		public FighterState State { get; private set; }
		public Vector3 Target { get; private set; }
		public Vector3 StartPosiiton { get; private set; }

		private Vector3 currentWayPoint;
		private int currentWayPointIndex;
		private Vector3[] wayPoints0;

		BillboardStrip engineTrailEffect;
		private List<Vector3> positions;

		private void Initialize()
		{
			// Initialize waypoints
			wayPoints0 = new Vector3[] {
				Target + Vector3.Normalize(StartPosiiton - Target) * 1000,
				Target + Vector3.UnitY * 200,
				Target - Vector3.Normalize(StartPosiiton - Target) * 1000,
				Target - Vector3.UnitY * 200,
			};
		}
		private void UpdateLocus()
		{
			positions.Add(Position);
			engineTrailEffect.Positions = positions;
			if (positions.Count >= BillboardStrip.MAX_SIZE) {
				positions.RemoveAt(0);
			} else if (positions.Count > 0) {
				engineTrailEffect.AddVertices();
			}
		}
		/// <summary>
		/// WayPoint loop movement
		/// </summary>
		/// <param name="wayPoints"></param>
		private void Move(Vector3[] wayPoints)
		{
			float margin = 1.0f;
			if ((currentWayPoint - Position).Length() < Velocity.Length() + margin) {
				currentWayPointIndex++;
				if (currentWayPointIndex >= wayPoints.Length) currentWayPointIndex = 0;

				currentWayPoint = wayPoints[currentWayPointIndex];
			}
		}
		private void Shoot(int bulletType)
		{
			switch (bulletType) {
				case 0:
					level.Bullets.Add(new EntityBullet(this, 1, new Vector3(1, 0, 0)
						, this.Position, 20, "Models\\cube"));
					break;
				case 1:
					level.Bullets.Add(new BillboardBullet(Level.graphicsDevice, content, Position
						, new Vector3(1, 0, 0), 1, content.Load<Texture2D>("Textures\\Mercury\\Star"), new Vector2(10)));
					break;
				case 2:
					/*level.Bullets.Add(new LaserBillboardBullet(Level.graphicsDevice, content, Position
						, Position + Direction, Direction, 50, content.Load<Texture2D>("Textures\\Mercury\\Laser"), new Vector2(10f, 40), 0));*/
					level.Bullets.Add(new LaserBillboardBullet(Level.graphicsDevice, content, Position
						,Position + Direction * 200, Direction,50, content.Load<Texture2D>("Textures\\Mercury\\Laser"), Color.Red, BlendState.AlphaBlend, new Vector2(200f, 100), 0));
					break;
			}
		}
		int count;
		int shootRate = 120;
		protected override void UpdateWorldMatrix()
		{
			Up = Vector3.UnitY;
			Up.Normalize();
			Right = Vector3.Cross(Direction, Up);
			Up = Vector3.Cross(Right, Direction);
			RotationMatrix = Matrix.Identity;
			RotationMatrix.Forward = Direction;
			RotationMatrix.Up = Up;
			RotationMatrix.Right = Right;

			// ContentProcessorで処理した結果、デフォルトでUnitXの方向を向いているので、Directionと内積を取って角度を求めて回転させる。
			// Worldの3軸を直接変更した方が簡単だと思ったが上手くいかなかった
			_world = Matrix.Identity;
			_world = Matrix.CreateScale(Scale) * Matrix.CreateRotationY((-Vector3.Dot(Vector3.UnitX, Direction) + MathHelper.ToRadians(90))) * Matrix.CreateTranslation(Position);
			/*_world.Forward = Direction;
			_world.Up = Up;
			_world.Right = Right;*/
		}
		private void BasicAttackMove()
		{
			if ((Target - Position).Length() < 1000) State = FighterState.Attack;

			if (State == FighterState.Move) {
				float speed = 5;
				Velocity = Vector3.Normalize(Target - Position) * speed;
				Direction = Vector3.Normalize(Velocity);
			} else if (State == FighterState.Attack) {
				Velocity = Vector3.Zero;
				count++;
				if (count % shootRate == 0) {
					Shoot(2);
				}
			}
		}
		private void WayPointMove()
		{
			float speed = 5;

			Move(wayPoints0);

			Velocity = Vector3.Normalize(currentWayPoint - Position) * speed;
			Direction = Vector3.Normalize(Velocity);
		}
		public override void Update(Microsoft.Xna.Framework.GameTime gameTime)
		{
			WayPointMove();

			Position += Velocity;
			UpdateWorldMatrix();

			UpdateLocus();
			engineTrailEffect.Update(gameTime);
		}
		public override void Draw(Matrix View, Matrix Projection, Vector3 CameraPosition)
		{
			base.Draw(View, Projection, CameraPosition);

			if (!DrawingPrePass) {
				engineTrailEffect.Draw(level.camera);
			}
		}
		/*public void Draw(GameTime gameTime, Matrix View, Matrix Projection, Vector3 CameraPosition)
		{
			base.Draw(View, Projection, CameraPosition);

			engineTrailEffect.Draw(View, Projection, level.camera.Up, level.camera.Right, CameraPosition);
		}*/


		public Fighter(Vector3 position, Vector3 target, float scale, string filePath)
			:base(position, scale, filePath)
		{
			this.StartPosiiton = position;
			this.Target = target;
			this.State = FighterState.Move;

			positions = new List<Vector3>();
			engineTrailEffect = new BillboardStrip(Level.graphicsDevice, content, content.Load<Texture2D>("Textures\\Lines\\Line2T1"),
				new Vector2(10, 100), positions);

			Initialize();
		}
	}
}
