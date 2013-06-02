//#define DEBUG_MODE

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
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
		private Scene currentScene;

		//public SpriteBatch spriteBatch { get; private set; }
		public SpriteFont titleFont { get; private set; }
		public SpriteFont menuFont { get; private set; }
		public Stack<Scene> scenes = new Stack<Scene>();

		public List<Level> Levels { get; private set; }
		public int LevelNum { get; private set; }
		public bool MoveNextLevel { get; set; }

		public float Width { get; private set; }
		public float Height { get; private set; }

		public Game1()
		{
			graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";

			Width = this.graphics.PreferredBackBufferWidth;
			Height = graphics.PreferredBackBufferHeight;
			//this.graphics.PreferredBackBufferWidth = 1920;
			//this.graphics.PreferredBackBufferHeight = 1080;
			//this.graphics.PreferredBackBufferWidth = 600;
			//this.graphics.PreferredBackBufferHeight = 400;
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
			KeyConfig.LoadXML("KeyConfig", "Xml\\KeyConfig.xml");
			Scene.Initialize(this, spriteBatch, Content);
			Object.game = this;
			Camera.game = this;
			
			PointLight.game = this;
			Object.content = Content;
			Bullet.content = Content;

			base.Initialize();
		}

		/// <summary>
		/// LoadContent はゲームごとに 1 回呼び出され、ここですべてのコンテンツを
		/// 読み込みます。
		/// </summary>
		protected override void LoadContent()
		{
            base.LoadContent();

			// 新規の SpriteBatch を作成します。これはテクスチャーの描画に使用できます。
			spriteBatch = new SpriteBatch(GraphicsDevice);
			Debug.game = this;
			Debug.spriteBatch = this.spriteBatch;

			Level.graphicsDevice = GraphicsDevice;
			Level.content = Content;
			//Scene.spriteBatch = this.spriteBatch;
			Scene.Initialize(this, spriteBatch, Content);
			menuFont = Content.Load<SpriteFont>("Fonts\\menuFont");
			titleFont = Content.Load<SpriteFont>("Fonts\\titleFont");

			// Create levels
			//Levels = new Level[] { new Level0(null), new Level1(null), new Level2(null) };
			Levels = new List<Level>();
			/*Levels.Add(new Level0(null));
			Levels.Add(new Level1(null));
			Levels.Add(new Level2(null));
			Levels.Add(new Level3(null));*/
			LevelNum = 3;

			// 最初に表示するシーンを追加
			PushScene(new Level4(null));
			//PushScene(new Level1(null));
			//PushScene(Levels[LevelNum]);
		}

		/// <summary>
		/// UnloadContent はゲームごとに 1 回呼び出され、ここですべてのコンテンツを
		/// アンロードします。
		/// </summary>
		protected override void UnloadContent()
		{
			// TODO: ここで ContentManager 以外のすべてのコンテンツをアンロードします。
		}
		public void PushLevel()
		{
			LevelNum++;
			//PushScene(Levels[LevelNum]);
			switch (LevelNum) {
				case 0:
					PushScene(new Level0(null));
					break;
				case 1:
					PushScene(new Level1(null));
					break;
				case 2:
					PushScene(new Level2(null));
					break;
			}
			if (LevelNum >= Levels.Count) LevelNum = 0;
		}
		public void PushScene(Scene scene)
		{
			scenes.Push(scene);//this.Window.
		}
		public void InitializeStack()
		{
			scenes.Clear();
			scenes.Push(new Level0(null));
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
            KeyInput.Update();

			// scene translation management
			currentScene = scenes.Peek();
			while (currentScene.isEndScene) {
				scenes.Pop();
				if (scenes.Count > 0) {
					currentScene = scenes.Peek();
				} else {
					if (MoveNextLevel) {
						PushLevel();
						currentScene = scenes.Peek();
						MoveNextLevel = false;
					} else {
						this.Exit();
					}
					break;
				}
			}
			if (scenes.Count > 0) {
				currentScene.Update(gameTime);
			} else this.Exit();
			

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

		bool hasSaved;
		/// <summary>
		/// ゲームが自身を描画するためのメソッドです。
		/// </summary>
		/// <param name="gameTime">ゲームの瞬間的なタイミング情報</param>
		protected override void Draw(GameTime gameTime)
		{
			currentScene.Draw(gameTime);

			base.Draw(gameTime);
		}
	}
}
