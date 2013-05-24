using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Audio;

namespace HLSLTest
{
	public class ArmedSatellite : Satellite
	{
		private SoundEffect shootSound;
		private List<SoundEffectInstance> currentSounds = new List<SoundEffectInstance>();
		EnergyShieldEffect shieldEffect;

        //private List<Enemy> currentEnemies;

		private Vector3 SearchTarget()
		{
			float minDis = 9999;
			Vector3 min = new Vector3(9999);

			foreach (Asteroid a in (level as Level4).Asteroids) {
				if ((Position - a.Position).Length() < minDis) {
					minDis = (Position - a.Position).Length();
					min = a.Position;
				}
			}
			return min;
		}
		private void Shoot(int bulletType)
		{
			switch (bulletType) {
				case 0:
					level.Bullets.Add(new EntityBullet(this, 1, new Vector3(1, 0, 0), this.Position, 20, "Models\\cube"));
					break;
				case 1:
					level.Bullets.Add(new BillboardBullet(Level.graphicsDevice, content, Position, new Vector3(1, 0, 0), 1, content.Load<Texture2D>("Textures\\Mercury\\Star"), new Vector2(10) ));
					break;
				case 2:
					level.Bullets.Add(new LaserBillboardBullet(Level.graphicsDevice, content, Position, new Vector3(1, 0.5f, 0.3f), 1, content.Load<Texture2D>("Textures\\Mercury\\Laser"), new Vector2(30, 20)));
					break;
				case 3:
					//level.Bullets.Add(new LaserBillboardBullet(Level.device, content, Position, Position + new Vector3(100, 50, 0), new Vector3(1, 0, 0), 1, content.Load<Texture2D>("Textures\\Mercury\\Laser"), new Vector2(10, 5), 1));
					//level.Bullets.Add(new LaserBillboardBullet(Level.device, content, Position, Position + new Vector3(100, 50, 0), new Vector3(1, 0.5f, 0.3f), 1, content.Load<Texture2D>("Textures\\Mercury\\Laser"), new Vector2(10, 5), 0));

					if ((level as Level4).Asteroids.Count > 0) {
						Vector3 dir = Vector3.Normalize(new Vector3(3, 2, 1));//(level as Level3).Asteroids[r.Next(0, (level as Level3).Asteroids.Count)].Position;
						level.Bullets.Add(new LaserBillboardBullet(Level.graphicsDevice, content, Position, dir, 1,
							content.Load<Texture2D>("Textures\\Mercury\\Laser"), new Vector2(10, 5), 0));
					}
					
					break;
				case 4://level.Bullets.Add(new LaserBillboardBullet(Level.device, content, Position, Position + new Vector3(100, 50, 40), new Vector3(1, 0.5f, 0.3f), 1, content.Load<Texture2D>("Textures\\Laser2"), new Vector2(10, 5), 1));
					if ((level as Level4).Asteroids.Count > 0) {
						//Vector3 tmp = new Vector3(100, 60, -100);

						//Vector3 tmp = (level as Level3).Asteroids[r.Next(0, (level as Level3).Asteroids.Count)].Position;
                        //Vector3 tmp = (level as Level3).Asteroids[0].Position;
						Vector3 tmp = SearchTarget();
						

						Vector3 dir = Vector3.Normalize(tmp - Position);
						level.Bullets.Add(new LaserBillboardBullet(Level.graphicsDevice, content, Position, tmp, dir, 1,
							content.Load<Texture2D>("Textures\\Mercury\\Laser"), new Vector2(50, 30), 1));
						/*level.Bullets.Add(new LaserBillboardBullet(Level.device, content, Position, tmp, dir, 1,
							content.Load<Texture2D>("Textures\\Laser2"), new Vector2(200, 10), 1));*/
					}
					break;
			}
		}

		private Random r = new Random();
		private int count;
		private readonly int shootRate = 60;
		private int chargeTime;
		private bool canShoot;
		public override void Update(GameTime gameTime)
		{
			count++;
			base.Update(gameTime);

			if (!canShoot && count > chargeTime) {
				canShoot = true;
				count = 0;
			}

			if (canShoot && JoyStick.IsOnKeyDown(2) || count % shootRate == 0) {
				Shoot(4);
				//shootSound.Play();
				//shootSoundInstance.Play();

				if (!Level.mute) {
				    SoundEffectInstance ls = shootSound.CreateInstance();
				    ls.Volume = 0.1f;
				    ls.Play();
				    currentSounds.Add(ls);
				}
			}

			for (int i = currentSounds.Count - 1; i >= 0; i--) {
				if (currentSounds[i].State != SoundState.Playing) {
					currentSounds[i].Dispose();
					currentSounds.RemoveAt(i);
				}
			}

			shieldEffect.Position = Position;
			shieldEffect.Update(gameTime);
		}

		public void Draw(GameTime gameTime, Matrix View, Matrix Projection, Vector3 CameraPosition)
		{
			base.Draw(View, Projection, CameraPosition);

			shieldEffect.Draw(gameTime, View, Projection, CameraPosition, level.camera.Direction, level.camera.Up, level.camera.Right);
		}

		#region Constructors
		public ArmedSatellite(Vector3 position, float scale, string fileName)
			:base(position, scale, fileName)
		{
			shootSound = content.Load<SoundEffect>("SoundEffects\\laser0");
		}

		public ArmedSatellite(Vector3 position, Vector3 center, float scale, string fileName)
			: this(position, center, scale, fileName, "SoundEffects\\laser0")
		{
		}
		public ArmedSatellite(Vector3 position, Vector3 center, float scale, string fileName, string SEPath)
			: base(position, center, scale, fileName)
		{
			//random = new Random();
			chargeTime = random.Next(10, 70);
			shootSound = content.Load<SoundEffect>(SEPath);
			shieldEffect = new EnergyShieldEffect(content, game.GraphicsDevice, Position, new Vector2(150), 100);//300,250
		}
		#endregion
	}
}
