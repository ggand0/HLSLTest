using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace HLSLTest
{
	public class UIObject : Drawable
	{
		public static SpriteBatch spriteBatch;
		public static ContentManager content;
		protected Texture2D texture;

		protected virtual void HandleInput()
		{
		}
		public override void Update(Microsoft.Xna.Framework.GameTime gameTime)
		{
			HandleInput();
			base.Update(gameTime);
		}

		public UIObject()
		{
		}
	}
}
