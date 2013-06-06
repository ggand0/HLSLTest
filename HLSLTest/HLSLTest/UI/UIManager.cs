using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace HLSLTest
{
	public class UIManager : Drawable
	{
		public List<UIObject> interfaces = new List<UIObject>();

		public override void Draw(GameTime gameTime)
		{
			base.Draw(gameTime);

			foreach (UIObject ui in interfaces) {
				ui.Draw(gameTime);
			}
		}
		private void Initialize()
		{
			interfaces.Add(new MouseCursor());
		}

		public UIManager()
		{
			Initialize();
		}
	}
}
