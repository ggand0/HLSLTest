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
		//private static bool initialized;
		public bool Available { get; private set; }

		private static readonly int FREQUENCY = 300;
		private ContentManager content;
		private GraphicsDevice graphics;

		private Matrix Scale;
		private ExplosionParticleEmitter explosion, spark, flare;
		//private List<ExplosionParticleEmitter> emitters;
		private List<ParticleEmitter> emitters;

		private float speed;
		public bool Repeat { get; private set; }
		

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);
			float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

			//explosion.Update();
			foreach (ExplosionParticleEmitter e in emitters) {
				e.Update();
			}

			if (count > FREQUENCY) {
				if (!Repeat) {
					Removable = true;
				} else {
					count = 0;
					//explosion.Reset = true;
					foreach (ExplosionParticleEmitter e in emitters) {
						e.Reset = true;
					}
					Available = true;
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
			 //explosion.Draw(camera.View, camera.Projection, camera.Up, camera.Right);
			 foreach (ExplosionParticleEmitter e in emitters) {
				 e.Draw(camera.View, camera.Projection, camera.Up, camera.Right);
			 }
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
			: this(content, graphics, position, size, repeat, false, "particle\\defExplostionParticleSettings")
		{
			this.content = content;
			this.graphics = graphics;
			this.Position = position;
			this.Repeat = repeat;
		}

		// lua scripting test
		public ExplosionEffect(ContentManager content, GraphicsDevice graphics,
			Vector3 position, Vector2 size, bool repeat, bool enableXML, string fileName)
		{
			this.content = content;
			this.graphics = graphics;
			this.Position = position;
			this.Repeat = repeat;

			//emitters = new List<ExplosionParticleEmitter>();
			emitters = new List<ParticleEmitter>();
			if (!enableXML) {
				// XML/スクリプトから読みこむようにしたい！
				explosion = new ExplosionParticleEmitter(graphics, content,
					content.Load<Texture2D>("Textures\\Particle\\explosion"), position, 200, new Vector2(50), 3, 0);
				spark = new ExplosionParticleEmitter(graphics, content,
					content.Load<Texture2D>("Textures\\Mercury\\FlowerBurst"), position, 20, new Vector2(10), 3, 0);
				flare = new ExplosionParticleEmitter(graphics, content,
					content.Load<Texture2D>("Textures\\Mercury\\LensFlare"), position, 10, new Vector2(80), 0.2f, 0);
				speed = explosion.Velocity.Length();

				spark.Speed = 2;
				explosion.Speed = 0.0f;
				flare.Speed = 0.1f;
				emitters.Add(flare);
				emitters.Add(explosion);
				emitters.Add(spark);
			} else {
				// lua test
				/*explosion = new ExplosionParticleEmitter(graphics, content,
					content.Load<Texture2D>("Textures\\Particle\\explosion"), position, 50, new Vector2(20), 3, 0, "Script\\explosion0.lua");
				spark = new ExplosionParticleEmitter(graphics, content,
					content.Load<Texture2D>("Textures\\Mercury\\FlowerBurst"), position, 20, new Vector2(10), 3, 0, "Script\\explosion1.lua");
				flare = new ExplosionParticleEmitter(graphics, content,
					content.Load<Texture2D>("Textures\\Mercury\\LensFlare"), position, 10, new Vector2(80), 0.2f, 0, "Script\\explosion2.lua");
				//speed = explosion.Velocity.Length();
				
				emitters.Add(flare);
				emitters.Add(explosion);
				emitters.Add(spark);
				Available = true;*/

				LoadParticleSettings load = new LoadParticleSettings();
				//emitters = (List<ExplosionParticleEmitter>)load.Load(fileName);
				emitters = load.Load(graphics, content, position, fileName);
			}
			Run();
		}
		public void Run()
		{
			foreach (ExplosionParticleEmitter e in emitters) {
				e.Run();
			}
			Available = false;
		}
	}
}

