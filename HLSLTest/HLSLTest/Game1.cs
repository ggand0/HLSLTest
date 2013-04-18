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

		Effect effect;
		
		/*VertexPositionColor[] vertices = {
			new VertexPositionColor(new Vector3(0, 1, 0), Color.White),
			new VertexPositionColor(new Vector3(1, 0, 0), Color.Blue),
			new VertexPositionColor(new Vector3(-1, 0, 0), Color.Red),
		};*/
		//Viewport viewport = GraphicsDevice.Viewport;
		VertexPositionTexture[] vertices = {
			new VertexPositionTexture(new Vector3(0, 1, 0), new Vector2(0.5f, 0)),
			new VertexPositionTexture(new Vector3(1, 0, 0), new Vector2(1, 1)),
			new VertexPositionTexture(new Vector3(-1, 0, 0), new Vector2(0, 1))
		};
		Matrix view;
		Matrix projection;
		public Object Target { get; private set; }
		public Object Ground { get; private set; }
		public ArcBallCamera camera { get; private set; }
		Debug debug;
		PrelightingRenderer renderer;
		List<Object> models;

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
			//Target.Scale = 10f;
			Target.Scale = 0.1f;
			Ground.Scale = 0.05f;
			models = new List<Object>();
			models.Add(Target);
			models.Add(Ground);
			camera.Initialize(this, Target);

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
			debug = new Debug();

			// projection
			Effect effect = Content.Load<Effect>("TextureProjectionEffect");
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
			models[1].Material = mat;

			// light map
			Effect lightingEffect = Content.Load<Effect>("PPModel");// load Prelighting Effect
			//models[0].SetModelEffect(lightingEffect, true);			// set effect to each modelmeshpart
			//models[1].SetModelEffect(lightingEffect, true);
			renderer = new PrelightingRenderer(GraphicsDevice, Content);
			renderer.Models = models;
			renderer.Camera = camera;
			renderer.Lights = new List<PPPointLight>() {
				new PPPointLight(new Vector3(-100, 100, 0), Color.Red * .85f,
				200),
				new PPPointLight(new Vector3(100, 100, 0), Color.Blue * .85f,
				200),
				new PPPointLight(new Vector3(0, 100, 100), Color.Green * .85f,
				200),
				new PPPointLight(new Vector3(0, 100, -100), Color.White * .85f,
				200)
			};

			// TODO: this.Content クラスを使用して、ゲームのコンテンツを読み込みます。
			/*effect = Content.Load<Effect>("textureEffect");

			view = Matrix.CreateLookAt(
				new Vector3(1, 0, 1),//new Vector3(2, 0, 3),
				new Vector3(),
				new Vector3(0, 1, 0)
			);
			projection = Matrix.CreatePerspectiveFieldOfView(
				MathHelper.ToRadians(90),
				GraphicsDevice.Viewport.AspectRatio,
				0.1f,
				100
			);
			//effect.Parameters["Transform"].SetValue(view * projection);
			effect.Parameters["View"].SetValue(view);
			effect.Parameters["Projection"].SetValue(projection);
			effect.Parameters["testTexture"].SetValue(Content.Load<Texture2D>("Xnalogo"));*/
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

			// TODO: ここにゲームのアップデート ロジックを追加します。
			// 回転の中心はウィンドウの中心。
			//エフェクトファイルで三角形のオフセットを表すグローバル変数を宣言して、
			// それをC#側から連続して変更していき、回転させている
			/*double angle = gameTime.TotalGameTime.TotalSeconds;
			effect.Parameters["offset"].SetValue(
				new Vector3((float)Math.Cos(angle), (float)Math.Sin(angle), 0)
			);*/
			JoyStick.Update(1);
			Target.Update(gameTime);
			Ground.Update(gameTime);
			camera.UpdateChaseTarget(Target);
			camera.Update(gameTime);

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
			renderer.Draw();

			GraphicsDevice.Clear(Color.CadetBlue);
			foreach (Object o in models) {
				//if (camera.BoundingVolumeIsInView(model.BoundingSphere)) {
					o.Draw();
				
			}

			// TODO: ここに描画コードを追加します。
			/*foreach (var pass in effect.CurrentTechnique.Passes) {
				pass.Apply();
				// ReachだとNotSupportedException
				GraphicsDevice.DrawUserPrimitives<VertexPositionTexture>(PrimitiveType.TriangleList, vertices, 0, vertices.Length / 3);// <VertexPositionColor>
			}*/
			// モデルの描画
			/*Matrix[] transforms = new Matrix[model.Bones.Count];
			model.CopyAbsoluteBoneTransformsTo(transforms);
			foreach (ModelMesh mesh in model.Meshes) {
				foreach (BasicEffect effect in mesh.Effects) {
					effect.EnableDefaultLighting();
					// "ワールド座標を使用してモデルの位置を変更する場合に、この行列を使用します。"
					effect.World = transforms[mesh.ParentBone.Index] * Matrix.Identity * Matrix.CreateScale(0.003f);
					// 追尾カメラによって提供される行列を使用します : ビュー変換と射影変換はcameraに任せる
					effect.View = camera.View;
					effect.Projection = camera.Projection;
				}
				mesh.Draw();
			}*/


			/*ResetGraphicDevice();
			Ground.Draw();
			//ResetGraphicDevice();
			//DrawModel(groundModel, Matrix.Identity * Matrix.CreateScale(0.05f));
			//debug.Draw(gameTime);
			ResetGraphicDevice();
			//Target.World *= Matrix.CreateRotationZ(MathHelper.ToRadians(-90));
			Target.Draw();*/
			/*ResetGraphicDevice();
			spriteBatch.Begin();
			spriteBatch.Draw(t, Vector2.Zero, Color.White);
			spriteBatch.End();
			ResetGraphicDevice();*/

			base.Draw(gameTime);
		}
	}
}
