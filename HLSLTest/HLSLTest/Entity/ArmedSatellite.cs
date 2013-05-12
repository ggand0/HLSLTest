using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace HLSLTest
{
	public class ArmedSatellite : Object
	{
		private void Shoot(int bulletType)
		{
			switch (bulletType) {
				case 0:
					level.Bullets.Add(new EntityBullet(this, 1, new Vector3(1, 0, 0), this.Position, 20, "Models\\cube"));
					break;
				case 1:
					level.Bullets.Add(new BillboardBullet(Level.device, content, Position, new Vector3(1, 0, 0), 1, content.Load<Texture2D>("Textures\\Mercury\\Star"), new Vector2(10) ));
					break;
			}
		}
		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);
			if (JoyStick.IsOnKeyDown(2)) {
				Shoot(1);
			}
		}


		public ArmedSatellite(Vector3 position, float scale, string fileName)
			:base(position, scale, fileName)
		{
		}
	}
}
