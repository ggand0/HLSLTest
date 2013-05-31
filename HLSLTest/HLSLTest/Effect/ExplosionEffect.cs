using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;


namespace HLSLTest
{
	/// <summary>
	/// 爆発エフェクトを描画するクラス。複数のパーティクルエミッタを使用する。
	/// </summary>
	public class ExplosionEffect : SpecialEffect, ICloneable
	{
		public static Level level;
		//private static bool initialized;
		public bool Available { get; private set; }

		private static readonly int FREQUENCY = 300;
		private ContentManager content;
		private GraphicsDevice graphicsDevice;

		private Matrix Scale;
		private ExplosionParticleEmitter explosion, spark, flare;
		//private List<ExplosionParticleEmitter> emitters;
		public List<ExplosionParticleEmitter> emitters;
		private EnergyRingEffect shockWaveEffect;
		private bool renderBoundingSphere;

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
			if (shockWaveEffect != null) {
				shockWaveEffect.Update(gameTime);
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
				 e.Draw(camera.View, camera.Projection, camera.Position, camera.Up, camera.Right);
			 }
			if (shockWaveEffect != null) {
				//shockWaveEffect.Draw(gameTime, camera.View, camera.Projection, camera.Direction, camera.Up, camera.Right);
			}

			// Draw BoundingBox for debug
			if (renderBoundingSphere) {
				int size = 25;
				BoundingBoxRenderer.Render(new BoundingBox(new Vector3(-size / 2.0f, -size / 2.0f, -size / 2.0f) + Position, new Vector3(size / 2.0f, size / 2.0f, size / 2.0f) + Position)
					, graphicsDevice, camera.View, camera.Projection, Color.White);
			}
		}

		public void Run()
		{
			foreach (ExplosionParticleEmitter e in emitters) {
				e.Initialize();
				e.Position = this.Position;
				e.Run();
			}
			Available = false;
		}
		public object Clone()
		{
			ExplosionEffect cloned = (ExplosionEffect)MemberwiseClone();

			// 参照型フィールドの複製を作成する
			if (this.emitters != null) {
				// Listを直接Clone出来ないのでこのような操作を行っている
				cloned.emitters = new List<ExplosionParticleEmitter>();
				for (int i = 0; i < emitters.Count; i++) {
					cloned.emitters.Add((ExplosionParticleEmitter)this.emitters[i].Clone());
				}
			}

			return cloned;
		}
		#region Constructors
		public ExplosionEffect(ContentManager content, GraphicsDevice graphics,
			Vector3 position, Vector2 size)
			:this (content, graphics, position, size, false)
		{
		}
		public ExplosionEffect(ContentManager content, GraphicsDevice graphics,
			Vector3 position, Vector2 size, bool repeat)
		{
			this.content = content;
			this.graphicsDevice = graphics;
			this.Position = position;
			this.Repeat = repeat;

			explosion = new ExplosionParticleEmitter(graphics, content,
					position, content.Load<Texture2D>("Textures\\Particle\\explosion"), 200, new Vector2(50), 3, 0);
			spark = new ExplosionParticleEmitter(graphics, content,
				position, content.Load<Texture2D>("Textures\\Mercury\\FlowerBurst"), 20, new Vector2(10), 3, 0);
			flare = new ExplosionParticleEmitter(graphics, content,
				position, content.Load<Texture2D>("Textures\\Mercury\\LensFlare"), 10, new Vector2(80), 0.2f, 0);
			speed = explosion.Velocity.Length();

			spark.Speed = 2;
			explosion.Speed = 0.0f;
			flare.Speed = 0.1f;
			emitters.Add(flare);
			emitters.Add(explosion);
			emitters.Add(spark);
		}


		/// <summary>
		/// ファイルから情報をロードしてエミッタを生成する。
		/// runをfalseにする場合は、実行メソッドを後で手動で呼ぶ必要がある。
		/// </summary>
		/// <param name="content"></param>
		/// <param name="graphics"></param>
		/// <param name="position"></param>
		/// <param name="size"></param>
		/// <param name="repeat"></param>
		/// <param name="filePath">ロードするファイルの相対パス</param>
		/// <param name="run">すぐ動作させるかどうか。</param>
		public ExplosionEffect(ContentManager content, GraphicsDevice graphics,
			Vector3 position, Vector2 size, bool repeat, string filePath, bool run)
		{
			this.content = content;
			this.graphicsDevice = graphics;
			this.Position = position;
			this.Repeat = repeat;
			emitters = new List<ExplosionParticleEmitter>();

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


			// Load emitters from XML the file.
			LoadParticleSettings load = new LoadParticleSettings();
			emitters = (List<ExplosionParticleEmitter>)load.Load(graphics, content, position, filePath);
			//emitters = load.Load(graphics, content, position, fileName);


			// 大量に使用する時など、すぐに走らせたくないときは、
			// falseを引数に与えておく
			if (run) {
				Run();
			}
		}
		#endregion
	}
}

