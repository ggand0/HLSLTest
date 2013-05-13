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
				case 2:
					level.Bullets.Add(new LaserBillboardBullet(Level.device, content, Position, new Vector3(1, 0.5f, 0.3f), 1, content.Load<Texture2D>("Textures\\Mercury\\Laser"), new Vector2(30, 20)));
					break;
				case 3:
					//level.Bullets.Add(new LaserBillboardBullet(Level.device, content, Position, Position + new Vector3(100, 50, 0), new Vector3(1, 0, 0), 1, content.Load<Texture2D>("Textures\\Mercury\\Laser"), new Vector2(10, 5), 1));
					//level.Bullets.Add(new LaserBillboardBullet(Level.device, content, Position, Position + new Vector3(100, 50, 0), new Vector3(1, 0.5f, 0.3f), 1, content.Load<Texture2D>("Textures\\Mercury\\Laser"), new Vector2(10, 5), 0));

					if ((level as Level3).Asteroids.Count > 0) {
						Vector3 dir = Vector3.Normalize(new Vector3(3, 2, 1));//(level as Level3).Asteroids[r.Next(0, (level as Level3).Asteroids.Count)].Position;
						level.Bullets.Add(new LaserBillboardBullet(Level.device, content, Position, dir, 1,
							content.Load<Texture2D>("Textures\\Mercury\\Laser"), new Vector2(10, 5), 0));
					}
					
					break;
				case 4://level.Bullets.Add(new LaserBillboardBullet(Level.device, content, Position, Position + new Vector3(100, 50, 40), new Vector3(1, 0.5f, 0.3f), 1, content.Load<Texture2D>("Textures\\Laser2"), new Vector2(10, 5), 1));
					if ((level as Level3).Asteroids.Count > 0) {
						//Vector3 tmp = new Vector3(100, 60, -100);

						//Vector3 tmp = (level as Level3).Asteroids[r.Next(0, (level as Level3).Asteroids.Count)].Position;
                        Vector3 tmp = (level as Level3).Asteroids[0].Position;
//                        tmp = new Vector3(100, 50, 100);
						Vector3 dir = Vector3.Normalize(tmp - Position);
						//level.Bullets.Add(new LaserBillboardBullet(Level.device, content, Position, tmp, dir, 1,
						//	content.Load<Texture2D>("Textures\\Mercury\\Laser"), new Vector2(50, 10), 1));
						level.Bullets.Add(new LaserBillboardBullet(Level.device, content, Position, tmp, dir, 1,
							content.Load<Texture2D>("Textures\\Laser2"), new Vector2(200, 10), 1));
					}
					break;
			}
		}

		Random r = new Random();
		private int count;
		public override void Update(GameTime gameTime)
		{
			count++;
			base.Update(gameTime);
			if (JoyStick.IsOnKeyDown(2)) {
				Shoot(4);
			}

			
		}


		public ArmedSatellite(Vector3 position, float scale, string fileName)
			:base(position, scale, fileName)
		{
		}
	}
}
