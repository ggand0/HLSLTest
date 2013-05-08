using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace HLSLTest
{
	/// <summary>
	/// 爆発エフェクトを描画するクラス。複数のパーティクルエミッタを使用する。
	/// </summary>
	public class ExplosionEffect : SpecialEffect
	{
		public static Level level;
		private static readonly int FREQUENCY = 300;
		private ContentManager content;
		private GraphicsDevice graphics;

		private Matrix Scale;
		private ExplosionParticleEmitter explosion;
		private float speed;
		public bool Repeat { get; private set; }
		

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);
			float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

			explosion.Update();
			if (count > FREQUENCY) {
				if (!Repeat) {
					Removable = true;
				} else {
					count = 0;
					explosion.Reset = true;
				}
			}
		}

		/*public override void Draw(GameTime gameTime, Matrix View, Matrix Projection, Vector3 CameraPosition, Vector3 CameraDirection, Vector3 Up, Vector3 Right)
		{
			eps.Draw(View, Projection, Up, Right);
		}*/
		public override void  Draw(GameTime gameTime, Camera camera)
		{
 			 base.Draw(gameTime, camera);
			 explosion.Draw(camera.View, camera.Projection, camera.Up, camera.Right);
		}
        /// <summary>
        /// エフェクトファイル内のパラメータを設定する
        /// </summary>
		private void SetEffectParameters(Vector3 CameraPosition, Vector3 CameraDirection)
		{
		}


		public ExplosionEffect(ContentManager content, GraphicsDevice graphics,
			Vector3 position, Vector2 size)
			:this (content, graphics, position, size, false)
		{
		}
		public ExplosionEffect(ContentManager content, GraphicsDevice graphics,
			Vector3 position, Vector2 size, bool repeat)
		{
			this.content = content;
			this.graphics = graphics;
			this.Position = position;
			this.Repeat = repeat;

			// XML/スクリプトから読みこむようにしたい！
			explosion = new ExplosionParticleEmitter(graphics, content,
				content.Load<Texture2D>("Textures\\Particle\\explosion"), position, 200, new Vector2(30), 3, 0, 1);
			speed = explosion.Velocity.Length();
		}
	}
}
