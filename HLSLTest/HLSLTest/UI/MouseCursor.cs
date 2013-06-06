using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace HLSLTest
{
	public class MouseCursor : UIObject
	{
		public override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
		{
			base.Draw(gameTime);

			spriteBatch.Begin();
			spriteBatch.Draw(texture, MouseInput.GetMousePositiion(), Color.White);
			spriteBatch.End();
		}

		public MouseCursor()
		{
			texture = content.Load<Texture2D>("Textures\\UI\\cursor");
		}
	}
}
