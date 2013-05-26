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
		public override void Update(Microsoft.Xna.Framework.GameTime gameTime)
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
			Position += Velocity;

			UpdateWorldMatrix();
		}


		public Fighter(Vector3 position, Vector3 target, float scale, string filePath)
			:base(position, scale, filePath)
		{
			this.Target = target;
			this.State = FighterState.Move;
		}
	}
}
