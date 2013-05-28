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
		private Vector3[] wayPoints0, yawDebug, pitchDebug, rollDebug;
		private List<Vector3> changeDirWayPoints;

		private List<BoundingSphere> obstacles;
		private BoundingSphere viewSphere;

		BillboardStrip engineTrailEffect;
		private List<Vector3> positions;

		protected Vector3 upPosition;

		private void Initialize()
		{
			// Initialize default up position. this value is used for calculating Up vector later.
			upPosition = Position + Vector3.UnitY;

			viewSphere = new BoundingSphere(Position, 100);// 2000
			obstacles = new List<BoundingSphere>();

			// Initialize waypoints
			changeDirWayPoints = new List<Vector3>();
			wayPoints0 = new Vector3[] {
				Target + Vector3.Normalize(StartPosiiton - Target) * 500,
				Target + Vector3.UnitY * 200,
				Target - Vector3.Normalize(StartPosiiton - Target) * 500,
				Target - Vector3.UnitY * 200,
			};
			yawDebug = new Vector3[] {
				Target + Vector3.Normalize(StartPosiiton - Target) * 500,
				Target + Vector3.UnitX * 500,
				Target - Vector3.Normalize(StartPosiiton - Target) * 500,
				Target - Vector3.UnitX * 500,
			};
			currentWayPoint = wayPoints0[0];
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

				if (currentWayPointIndex == 1 || currentWayPointIndex == 3) Shoot(2);

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
			Direction.Normalize();
			//Up = Vector3.UnitY; // Upが誤っているので表示されない
			Up = Vector3.Normalize(upPosition - Position);
			Up.Normalize();
			Right = Vector3.Cross(Direction, Up);
			Right.Normalize();

			Vector3 projectedDirection = Direction - Vector3.Dot(Direction, Vector3.UnitY) * Vector3.UnitY;
			float angleX = (float)Math.Acos((double)(Vector3.Dot(Direction, projectedDirection)
				/ (Direction.Length() * projectedDirection.Length())));
			//float angleY = Math.Atan();
			//float angleZ = Math.Atan();

			// ContentProcessorで処理した結果、デフォルトでUnitXの方向を向いているので、Directionと内積を取って角度を求めて回転させる。
			// Worldの3軸を直接変更した方が簡単だと思ったがUpが定まらないので無理
			/*_world = Matrix.Identity;
			_world = Matrix.CreateScale(Scale)
				//* Matrix.CreateRotationY((-Vector3.Dot(Vector3.UnitX, Direction) + MathHelper.ToRadians(90)))
				* Matrix.CreateFromYawPitchRoll(angleX//Vector3.Dot(Vector3.UnitZ, Direction
					, 0//-Vector3.Dot(Vector3.UnitY, Direction) + MathHelper.ToRadians(90)
					, 0)
				* Matrix.CreateTranslation(Position);*/

			/*_world = Matrix.Identity;
			_world *= Matrix.CreateScale(Scale);
			_world.Forward *= Direction;
			_world.Up *= Up;
			_world.Right *= Right;
			_world *= Matrix.CreateTranslation(Position);*/
			_world = Matrix.Identity;
			_world.Forward = Direction;
			_world.Up = Up; ;
			_world.Right = Right;/**/
			_world *= Matrix.CreateScale(Scale);
			_world *= Matrix.CreateTranslation(Position);

			/*_world = Matrix.CreateWorld(Position, Direction, Vector3.Up);
			_world *= Matrix.CreateScale(Scale);*/
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
			Move(wayPoints0);

			// 速度の決定
			float speed = 1;//5
			//Velocity = Vector3.Normalize(currentWayPoint - Position) * speed;
			Vector3 dest = Vector3.Normalize(currentWayPoint - Position);
			/*float maxAnglePerFrame = (float)Math.PI / 2.0f / 20f;
			float angle = Vector3.Dot(Velocity, dest);
			Velocity += Vector3.Lerp(Velocity, dest, 0.2f);*/
			Velocity += dest * speed;
			Velocity *= 0.9f;

			Direction = Vector3.Normalize(Velocity);
		}
		private void BuildDirectionChangeWayPoints()
		{
			// とりあえず今乗っている平面内での円形ターンするルートで
			Matrix rotation = Matrix.CreateFromAxisAngle(Up, MathHelper.ToRadians(90));
			Vector3 optionalUnitVector = Vector3.Transform(Direction, rotation);
			changeDirWayPoints.Add(Position + Vector3.Normalize(Velocity) * 100);
			changeDirWayPoints.Add(changeDirWayPoints[0] + Direction * 50 + optionalUnitVector * 50);
			changeDirWayPoints.Add(changeDirWayPoints[0] + optionalUnitVector * 100);
			changeDirWayPoints.Add(Position + optionalUnitVector * 100);
		}
		private bool ChangeDirection()
		{
			float margin = 1.0f;
			if ((currentWayPoint - Position).Length() < Velocity.Length() + margin) {
				currentWayPointIndex++;
				if (currentWayPointIndex >= changeDirWayPoints.Count) return true;//currentWayPointIndex = 0;

				currentWayPoint = changeDirWayPoints[currentWayPointIndex];
			}
			return false;
		}
		private void CheckObstacles()
		{
			obstacles.Clear();
			viewSphere.Center = Position;
			foreach (Object o in level.Models) {
				if (viewSphere.Intersects(o.transformedBoundingSphere)) {
					obstacles.Add(o.transformedBoundingSphere);
				}
			}
		}
		private bool IsGoingToCollide(BoundingSphere target)
		{
			Vector3 v = Direction * 100;
			Vector3 dir = target.Center - Position;
			Vector3 dirProjected = Vector3.Dot(v, dir) * v / (v.Length() * v.Length());
			Vector3 b = dirProjected - dir;

			return target.Radius > b.Length();
		}
		private bool IsLeft(Vector3 vector, Vector3 targetPoint)
		{
			return Math.Sin(vector.X * (targetPoint.Y - 0) - vector.Y * (targetPoint.X - 0)) > 0;
		}
		private void AddAvoidanceVelocity()
		{
			float avoidSpeed = 0.5f;
			Vector3 projectedDirection = Direction - Vector3.Dot(Direction, Up) * Up;

			foreach (BoundingSphere bs in obstacles) {
				if (IsGoingToCollide(bs)) {
					Vector3 planeToCenter = bs.Center - Position;
					float distancePlaneToCenter = Vector3.Dot(planeToCenter, Up);
					Vector3 projectedCenter =  bs.Center - distancePlaneToCenter * Up;

					Vector3 avoidVelocity = IsLeft(projectedDirection, projectedCenter) ? _world.Right : _world.Left;

					Velocity += avoidVelocity * avoidSpeed;
					//Velocity += Vector3.UnitX * avoidSpeed;
				}
			}
		}
		public override void Update(Microsoft.Xna.Framework.GameTime gameTime)
		{
			CheckObstacles();
			AddAvoidanceVelocity();
			WayPointMove();


			Position += Velocity;
			upPosition += Velocity;
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
