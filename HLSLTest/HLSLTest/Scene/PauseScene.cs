using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HLSLTest
{
	public class PauseMenu : MenuScene
	{
		Camera camera;

		public PauseMenu(Level privousScene)
			: base(privousScene)
		{
			sceneTitle = "Pause";
			drawBackGround = false;
			menuString = new string[] {
				"Resume",
			};
			buttonNum = menuString.Length;
			button = new Button[buttonNum];
			for (int i = 0; i < buttonNum; i++) {
				button[i].color = Color.Blue;
				button[i].name = menuString[i];
			}
			//SoundControl.Pause();
			this.camera = privousScene.camera;

			Load();
		}

		protected override void HandleInput()
		{
			if (JoyStick.IsOnKeyDown(3) || JoyStick.IsOnKeyDown(8)) {
				isEndScene = true;
				//if (!game.isMuted) cancel.Play(SoundControl.volumeAll, 0f, 0f);
			}
		}
		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);
			camera.Update(gameTime);
		}

		public override void Draw(GameTime gameTime)
		{
			higherScene.Draw(gameTime);

			spriteBatch.Begin();
			spriteBatch.Draw(mask, new Rectangle(0, 0, (int)game.Width, (int)game.Height), new Color(0, 0, 0, 100));
			spriteBatch.End();
			base.Draw(gameTime);
		}
	}
}
