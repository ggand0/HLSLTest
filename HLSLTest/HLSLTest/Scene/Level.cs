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

		public List<Object> Models { get; protected set; }
		public SkySphere Sky { get; protected set; }

		/// <summary>
		/// GraphicsDeviceのStateをデフォルトの状態に戻す。
		/// spriteBatchがDeviceにした変更を戻すのに使う
		/// </summary>
		public void ResetGraphicDevice()
		{
			graphics.GraphicsDevice.BlendState = BlendState.Opaque;
			graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
			graphics.GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
		}

		protected virtual void Initialize()
		{
			EnergyShieldEffect.level = this;
			BoundingSphereRenderer.level = this;
			Object.level = this;
			PrelightingRenderer.level = this;
			Water.level = this;
			GlassEffect.level = this;
			DiscoidEffect.level = this;
		}
		public override void Load()
		{

		}
		public override void Update(GameTime gameTime)
		{

			// Startボタンが押された時にPause Sceneにする
			if (JoyStick.IsOnKeyDown(8)) {
				//PushScene(new PauseMenu(this));
				return;
			}
		}
		public override void Draw(GameTime gameTime)
		{
			// TODO: ここに描画コードを追加します。
			// Object継承オブジェクトの描画は各Drawで行うことにした。それ以外はDrawModelで描画する。
			//graphics.GraphicsDevice.Clear(Color.CadetBlue);
			
		}

		public Level(Scene privousScene)
			:base(privousScene)
		{
			Initialize();
			Load();
		}
	}
}
