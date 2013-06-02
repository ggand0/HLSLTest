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
	public class Turret : Object
	{
		private SoundEffect shootSound;
		private List<SoundEffectInstance> currentSounds = new List<SoundEffectInstance>();
		private EnergyShieldEffect shieldEffect;

		private List<Object> visibleEnemies;
		private BoundingSphere sensorSphere;
		private Random r = new Random();
		private int count;
		private readonly int shootRate = 60;
		private int chargeTime;
		private bool canShoot;

		private void CheckEnemies()
		{
			sensorSphere.Center = Position;

			visibleEnemies.Clear();
			foreach (Object o in level.Enemies) {
				if (o.IsHitWith(sensorSphere)) {
					visibleEnemies.Add(o);
				}
			}
		}
		private bool IsInRange()
		{
			return visibleEnemies.Count > 0;
		}
		private Vector3 SearchTarget(int tactics)
		{
			float minDis = 9999;
			Vector3 min = new Vector3(9999);

			/*foreach (Asteroid a in (level as Level4).Asteroids) {
				if ((Position - a.Position).Length() < minDis) {
					minDis = (Position - a.Position).Length();
					min = a.Position;
				}
			}*/
			switch (tactics) {
				default:
					return visibleEnemies[r.Next(0, visibleEnemies.Count)].Position;
				case 1:
					foreach (Object o in visibleEnemies) {
						if ((Position - o.Position).Length() < minDis) {
							minDis = (Position - o.Position).Length();
							min = o.Position;
						}
					}
					return min;
			}
		}
		private Object SearchTargetObj(int tactics)
		{
			float minDis = 9999;
			Vector3 min = new Vector3(9999);
			Object minObj = null;

			/*foreach (Asteroid a in (level as Level4).Asteroids) {
				if ((Position - a.Position).Length() < minDis) {
					minDis = (Position - a.Position).Length();
					min = a.Position;
				}
			}*/
			switch (tactics) {
				default:
					return visibleEnemies[r.Next(0, visibleEnemies.Count)];
				case 1:
					foreach (Object o in visibleEnemies) {
						if ((Position - o.Position).Length() < minDis) {
							minDis = (Position - o.Position).Length();
							min = o.Position;
							minObj = o;
						}
					}
					return minObj;
			}
		}
		private void Shoot(int bulletType)
		{
			switch (bulletType) {
				case 0:
					//level.Bullets.Add(new EntityBullet(this, 1, new Vector3(1, 0, 0), this.Position, 20, "Models\\cube"));
					break;
				case 1:
					//level.Bullets.Add(new BillboardBullet(Level.graphicsDevice, content, Position, new Vector3(1, 0, 0), 1, content.Load<Texture2D>("Textures\\Mercury\\Star"), new Vector2(10) ));
					break;
				case 2:
					level.Bullets.Add(new LaserBillboardBullet(IFF.Friend, Level.graphicsDevice, content, Position,
						new Vector3(1, 0.5f, 0.3f), 1, content.Load<Texture2D>("Textures\\Lines\\laser0"), new Vector2(30, 20)));//Textures\\Mercury\\Laser
					break;
				/*case 3:
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
							content.Load<Texture2D>("Textures\\Mercury\\Laser"), Color.White, BlendState.AlphaBlend, new Vector2(50, 30), 1));
						//level.Bullets.Add(new LaserBillboardBullet(Level.device, content, Position, tmp, dir, 1,
							//content.Load<Texture2D>("Textures\\Laser2"), new Vector2(200, 10), 1));
					}
					break;*/
				case 3:
					if (visibleEnemies.Count > 0) {
						Vector3 dir = visibleEnemies[r.Next(0, visibleEnemies.Count)].Position;//Vector3.Normalize(new Vector3(3, 2, 1));
						level.Bullets.Add(new LaserBillboardBullet(IFF.Friend, Level.graphicsDevice, content, Position, dir, 1,
							content.Load<Texture2D>("Textures\\Mercury\\Laser"), new Vector2(10, 5), 0));/**/
					}
					break;
				case 4:
					if (visibleEnemies.Count > 0) {
						//Vector3 tmp = SearchTarget(1);
						Drawable tmp = SearchTargetObj(1);
						Vector3 dir1 = Vector3.Normalize(tmp.Position - Position);
						level.Bullets.Add(new LaserBillboardBullet(IFF.Friend, Level.graphicsDevice, content, this, tmp, dir1, 1,
							content.Load<Texture2D>("Textures\\Lines\\laser0"), Color.White, BlendState.AlphaBlend, new Vector2(50, 30), 1));/**/
					}
					break;
			}
		}
		public override void Update(GameTime gameTime)
		{
			count++;
			base.Update(gameTime);//RotationMatrix = Matrix.CreateRotationX((float)Math.PI);

			CheckEnemies();
			/*if (!canShoot && count > chargeTime) {
				canShoot = true;
				count = 0;
			}*/
			if (!canShoot && count > chargeTime) {
				canShoot = true;
				//count = 0;
			}

			if (canShoot && IsInRange()) {//count % shootRate == 0
				Shoot(4);
				//shootSound.Play();
				//shootSoundInstance.Play();

				if (!Level.mute && shootSound != null) {
					SoundEffectInstance ls = shootSound.CreateInstance();
					ls.Volume = 0.05f;
					ls.Play();
					currentSounds.Add(ls);
				}
				canShoot = false;
				count = 0;
			}

			for (int i = currentSounds.Count - 1; i >= 0; i--) {
				if (currentSounds[i].State != SoundState.Playing) {
					currentSounds[i].Dispose();
					currentSounds.RemoveAt(i);
				}
			}
		}
		public override void Draw(Matrix View, Matrix Projection, Vector3 CameraPosition)
		{
			//base.Draw(View, Projection, CameraPosition);
			base.DrawBoundingSphere();
		}


		#region Constructors
		public Turret(Vector3 position, float scale)
			: base(scale, position)
		{
			visibleEnemies = new List<Object>();
			chargeTime = 60;
			sensorSphere = new BoundingSphere(position, 1000);
			shootSound = content.Load<SoundEffect>("SoundEffects\\laser1");
		}
		public Turret(Vector3 position, float scale, string fileName)
			: base(position, scale, fileName)
		{
		}
		#endregion
	}
}
