using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace HLSLTest
{
	public enum CursorState
	{
		Normal,
		CameraRotation
	}
	public class MouseCursor : UIObject
	{
		public CursorState State { get; private set; }

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			/*if (MouseInput.BUTTONR()) {
				State = CursorState.CameraRotation;
			} else {
				State = CursorState.Normal;
			}*/
		}

		public override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
		{
			base.Draw(gameTime);

			if (MouseInput.BUTTONR()) {
				State = CursorState.CameraRotation;
			} else {
				State = CursorState.Normal;
			}

			Vector2 pos = MouseInput.GetMousePosition();
			spriteBatch.Begin();
			spriteBatch.Draw(texture, State == CursorState.Normal ? MouseInput.GetMousePosition() : MouseInput.GetCachedPosition(), Color.White);
			spriteBatch.End();
		}

		public MouseCursor()
		{
			texture = content.Load<Texture2D>("Textures\\UI\\cursor");
			State = CursorState.Normal;
		}
	}
}
