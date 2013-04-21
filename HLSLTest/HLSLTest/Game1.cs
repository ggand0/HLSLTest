//#define DEBUG_MODE

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace HLSLTest
{
	/// <summary>
	/// 基底 Game クラスから派生した、ゲームのメイン クラスです。
	/// </summary>
	public class Game1 : Microsoft.Xna.Framework.Game
	{
		GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;
		public Object Target { get; private set; }
		public Object Ground { get; private set; }
		public Object Teapot { get; private set; }
		public ArcBallCamera camera { get; private set; }
		Debug debug;
		PrelightingRenderer renderer;
		List<Object> models;
		SkySphere sky;
		Water water;


		public Game1()
		{
			graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
		}

		/// <summary>
		/// ゲームの開始前に実行する必要がある初期化を実行できるようにします。
		/// ここで、要求されたサービスを問い合わせて、非グラフィック関連のコンテンツを読み込むことができます。
		/// base.Initialize を呼び出すと、任意のコンポーネントが列挙され、
		/// 初期化もされます。
		/// </summary>
		protected override void Initialize()
		{
			// TODO: ここに初期化ロジックを追加します。
			camera = new ArcBallCamera();
			Object.game = this;
			Object.content = Content;


			//Target = new Object("Models\\UtahTeapotDef");
			Target = new Object("Models\\tank");
			Ground = new Object("Models\\ground");
			Teapot = new Object(new Vector3(-100, 0, 0), "Models\\UtahTeapotDef");
			//Target.Scale = 10f;
			Target.Scale = 0.1f;
			Target.Direction = Vector3.UnitX;
			//Target.RotationMatrix.Forward = Vector3.UnitX;
			Ground.Scale = 0.05f;
			Teapot.Scale = 10;
			//Teapot.RotationMatrix = Matrix.CreateRotationZ(MathHelper.ToRadians(-180));
			Teapot.RotationMatrix = Matrix.Identity * Matrix.CreateRotationZ(MathHelper.ToRadians(90))
				* Matrix.CreateRotationX(MathHelper.ToRadians(-90));
			models = new List<Object>();
			//models.Add(Ground);
			models.Add(Target);
			models.Add(Teapot);
			camera.Initialize(this, Target);

			

			//foreach (Object o in models) o.Up = Vector3.Up;
			/**/

			base.Initialize();
		}
		Texture2D t;
		/// <summary>
		/// LoadContent はゲームごとに 1 回呼び出され、ここですべてのコンテンツを
		/// 読み込みます。
		/// </summary>
		protected override void LoadContent()
		{
			// 新規の SpriteBatch を作成します。これはテクスチャーの描画に使用できます。
			spriteBatch = new SpriteBatch(GraphicsDevice);
			Debug.game = this;
			Debug.spriteBatch = this.spriteBatch;
			PrelightingRenderer._gameInstance = this;
			debug = new Debug();

			// skymap reflect
			Effect cubeMapEffect = Content.Load<Effect>("CubeMapReflect");
			CubeMapReflectMaterial cubeMat = new CubeMapReflectMaterial(Content.Load<TextureCube>("Cross"));
			Teapot.SetModelEffect(cubeMapEffect, false);
			Teapot.Material = cubeMat;/**/
			//models[2].SetModelEffect(cubeMapEffect, false);
			//models[2].Material = cubeMat;

			// projection
			/*Effect effect = Content.Load<Effect>("TextureProjectionEffect");
			models[0].SetModelEffect(effect, true);
			models[1].SetModelEffect(effect, true);
			//t = Content.Load<Texture2D>("Checker");//("projectedTexture");
			t = Content.Load<Texture2D>("projectedTexture");
			ProjectedTextureMaterial mat = new ProjectedTextureMaterial(Content.Load<Texture2D>("projectedTexture")
				, GraphicsDevice);
			mat.ProjectorPosition = new Vector3(0, 100, 0);
			mat.ProjectorTarget = new Vector3(0, 0, 0);
			mat.Scale = 0.05f;//2
			models[0].Material = mat;
			models[1].Material = mat;*/

			// light map
			Effect shadowEffect = Content.Load<Effect>("ProjectShadowDepthEffectV4");
			Effect lightingEffect = Content.Load<Effect>("PPModel");	// load Prelighting Effect
			//models[0].SetModelEffect(lightingEffect, true);			// set effect to each modelmeshpart
			//models[1].SetModelEffect(lightingEffect, true);
			models[0].SetModelEffect(shadowEffect, true);				// set effect to each modelmeshpart
			models[1].SetModelEffect(shadowEffect, true);

			renderer = new PrelightingRenderer(GraphicsDevice, Content);
			renderer.Models = models;
			renderer.Camera = camera;
			renderer.Lights = new List<PPPointLight>() {
				/*new PPPointLight(new Vector3(-100, 100, 0), Color.Red * .85f,
				200),
				new PPPointLight(new Vector3(100, 100, 0), Color.Blue * .85f,
				200),
				new PPPointLight(new Vector3(0, 100, 100), Color.Green * .85f,
				200),*/
				new PPPointLight(new Vector3(0, 200, 0), Color.White * .85f,//ew Vector3(0, 100, -100),
				20000),
				new PPPointLight(new Vector3(0, -200, 0), Color.White * .85f,//ew Vector3(0, 100, -100),
				20000)
			};
			// setup shadows
			renderer.ShadowLightPosition = new Vector3(500, 500, 0);//new Vector3(1500, 1500, 2000);
			renderer.ShadowLightTarget = new Vector3(0, 300, 0);//new Vector3(0, 150, 0)
			//renderer.ShadowLightPosition = new Vector3(200, 100, 0);//new Vector3(1500, 1500, 2000);
			//renderer.ShadowLightTarget = new Vector3(-50, -50, 0);//new Vector3(0, 150, 0)
			//renderer.ShadowLightPosition = new Vector3(0, 150, 0);//new Vector3(1500, 1500, 2000);
			//renderer.ShadowLightTarget = new Vector3(0, 0, 0);//new Vector3(0, 150, 0)
			
			renderer.DoShadowMapping = true;
			renderer.ShadowMult = 0.3f;//0.01f;//0.3f;


			//sky = new SkySphere(Content, GraphicsDevice, Content.Load<TextureCube>("OutputCube0"));//("OutputCube0"));
			sky = new SkySphere(Content, GraphicsDevice, Content.Load<TextureCube>("Cross"));//("OutputCube0"));

			//water = new Water(Content, GraphicsDevice, new Vector3(0, 0, 0), new Vector2(1000, 1000));
			water = new Water(Content, GraphicsDevice, new Vector3(0, 0, 0), new Vector2(1000, 1000), renderer);
			water.Objects.Add(sky);
			water.Objects.Add(models[0]);
			water.Objects.Add(models[1]);
			water.Initialize();
		}

		/// <summary>
		/// UnloadContent はゲームごとに 1 回呼び出され、ここですべてのコンテンツを
		/// アンロードします。
		/// </summary>
		protected override void UnloadContent()
		{
			// TODO: ここで ContentManager 以外のすべてのコンテンツをアンロードします。
		}

		/// <summary>
		/// ワールドの更新、衝突判定、入力値の取得、オーディオの再生などの
		/// ゲーム ロジックを、実行します。
		/// </summary>
		/// <param name="gameTime">ゲームの瞬間的なタイミング情報</param>
		protected override void Update(GameTime gameTime)
		{
			// ゲームの終了条件をチェックします。
			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
				this.Exit();

			JoyStick.Update(1);
			sky.Update(gameTime);

			foreach (Object o in models) {
				o.Update(gameTime);
			}

			renderer.Update();
			camera.UpdateChaseTarget(Target);
			camera.Update(gameTime);
			water.Update();

			base.Update(gameTime);
		}

		/// <summary>
		/// GraphicsDeviceのStateをデフォルトの状態に戻す。
		/// spriteBatchがDeviceにした変更を戻すのに使う
		/// </summary>
		public void ResetGraphicDevice()
		{
			GraphicsDevice.BlendState = BlendState.Opaque;
			GraphicsDevice.DepthStencilState = DepthStencilState.Default;
			GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
		}
		/// <summary>
		/// 単純なモデル描画メソッド。ここで興味深い部分は、
		/// ビュー行列と射影行列がカメラ オブジェクトから取得されることです。
		/// </summary>        
		private void DrawModel(Model model, Matrix world)
		{
			Matrix[] transforms = new Matrix[model.Bones.Count];
			model.CopyAbsoluteBoneTransformsTo(transforms);

			foreach (ModelMesh mesh in model.Meshes) {
				foreach (BasicEffect effect in mesh.Effects) {
					effect.EnableDefaultLighting();
					effect.World = transforms[mesh.ParentBone.Index] * world;

					// 追尾カメラによって提供される行列を使用します
					effect.View = camera.View;
					effect.Projection = camera.Projection;
				}
				mesh.Draw();
			}
		}
		/// <summary>
		/// ゲームが自身を描画するためのメソッドです。
		/// </summary>
		/// <param name="gameTime">ゲームの瞬間的なタイミング情報</param>
		protected override void Draw(GameTime gameTime)
		{
#if DEBUG_MODE			
			GraphicsDevice.Clear(Color.CornflowerBlue);
			renderer.Draw();
			
#else
			
			renderer.Draw();
			water.PreDraw(camera, gameTime);
			//GraphicsDevice.Clear(Color.CornflowerBlue);
			//sky.Draw(camera.View, camera.Projection, camera.CameraPosition);
			//water.Draw(camera.View, camera.Projection, (camera).Position);
			
			foreach (Object o in models) {
				//if (camera.BoundingVolumeIsInView(model.BoundingSphere)) {
				string s = o.Scale.ToString();
				//o.Draw(camera.View, camera.Projection, camera.CameraPosition);
			}
#endif

			/*ResetGraphicDevice();
			Ground.Draw();
			//ResetGraphicDevice();
			//DrawModel(groundModel, Matrix.Identity * Matrix.CreateScale(0.05f));
			//debug.Draw(gameTime);
			ResetGraphicDevice();
			//Target.World *= Matrix.CreateRotationZ(MathHelper.ToRadians(-90));
			Target.Draw();*/

			base.Draw(gameTime);
		}
	}
}
