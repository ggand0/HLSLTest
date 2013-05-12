using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace HLSLTest
{
	public class Level : Scene
	{
		public static GraphicsDeviceManager graphics;
		public static GraphicsDevice device;
		//public static ContentManager content;
		public static readonly Vector3 PlayfieldSize = new Vector3(100000);
		private static readonly Vector3 initialCameraPosition = new Vector3(0.0f, 50.0f, 5000.0f);


		public ArcBallCamera camera { get; protected set; }
		protected Debug debug;
		protected EffectManager effectManager;
		protected bool displayGrid;

		public List<Object> Models { get; protected set; }
		//public List<IRenderable>  { get; protected set; }
		public SkySphere Sky { get; protected set; }

		public Vector3 LightPosition
			= new Vector3(-2000, 0, 2000);
			//= new Vector3(-200, 500, 200);

		//public List<EntityBullet> Bullets { get; protected set; }
		public List<Drawable> Bullets { get; protected set; }

		/// <summary>
		/// GraphicsDeviceのStateをデフォルトの状態に戻す。
		/// spriteBatchがDeviceにした変更を戻すのに使う
		/// </summary>
		public void ResetGraphicDevice()
		{
			device.BlendState = BlendState.Opaque;
			device.DepthStencilState = DepthStencilState.Default;
			device.SamplerStates[0] = SamplerState.LinearWrap;
		}

		protected virtual void Collide()
		{
		}
		protected virtual void Initialize()
		{
			Models = new List<Object>();
			effectManager = new EffectManager();
			Bullets = new List<Drawable>();

			EnergyShieldEffect.level = this;
			BoundingSphereRenderer.level = this;
			Object.level = this;
			PrelightingRenderer.level = this;
			Water.level = this;
			GlassEffect.level = this;
			EnergyRingEffect.level = this;
			Planet.level = this;
			Star.level = this;
		}
		protected virtual void HandleInput()
		{
			if (JoyStick.IsOnKeyDown(6)) {
				displayGrid = displayGrid ? false : true;
			}
			// Startボタンが押された時にPause Sceneにする
			if (JoyStick.IsOnKeyDown(8)) {
				//PushScene(new PauseMenu(this));
				return;
			}
			if (JoyStick.IsOnKeyDown(9)) {
				isEndScene = true;
				game.MoveNextLevel = true;
			}
		}
		public override void Load()
		{

		}
		public override void Update(GameTime gameTime)
		{
			HandleInput();
		}
		public override void Draw(GameTime gameTime)
		{
			// TODO: ここに描画コードを追加します。
			// Object継承オブジェクトの描画は各Drawで行うことにした。それ以外はDrawModelで描画する。
			//device.Clear(Color.CadetBlue);
			
		}

		public Level(Scene privousScene)
			:base(privousScene)
		{
			Initialize();
			Load();
		}
	}
}
