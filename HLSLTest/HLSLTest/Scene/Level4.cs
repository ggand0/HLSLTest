﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

namespace HLSLTest
{
	public class Level4 : Level
	{
		public Object Target { get; private set; }
		public Object Ground { get; private set; }
		public Object Teapot { get; private set; }
		public ArmedSatellite Satellite { get; private set; }

		PrelightingRenderer renderer;
		GridRenderer grid;

        // Effect test
		FlameParticleEmitter ps;
		ExplosionParticleEmitter eps;
		DiscoidParticleEmitter discoid;
		ParticleEmitter basicEmitter, beamEmitter;
		EnergyRingEffect discoidEffect;
		EnergyShieldEffect shieldEffect;
		ExplosionEffect explosionTest, smallExplosion, bigExplosion, midExplosion;
		Planet planet;
		Star star;
		public Sun sun { get; private set; }
		private Sun sunCircle;
		Effect shadowEffect;
		public List<Asteroid> Asteroids { get; private set; }
		public List<Planet> Planets { get; private set; }
		public List<DamageablePlanet> TargetPlanets { get; private set; }
		public List<Satellite> Satellites { get; private set; }
		public List<Fighter> Fighters { get; private set; }

        private Random random;
        private List<ExplosionEffect> ex = new List<ExplosionEffect>();
		private bool spawned;
		private int count;
		//ParticleSettings setting;
        private LaserBillboard lb;
		/// <summary>
		/// 小惑星の最大spawn数
		/// </summary>
		private static readonly int MAX_SPAWN_NUM = 15;

        private Vector3 tmpDirention;
        private Vector3 tmpCameraPos;
        private Matrix RotationMatrix = Matrix.Identity;
		private EnemyManager enemyManager;
		private UIManager uiManager = new UIManager();

        /// <summary>
        /// Utility専用クラスにまとめるべき
        /// </summary>
		public static float NextDouble(Random r, double min, double max)
		{
			return (float)(min + r.NextDouble() * (max - min));
		}
		private void AddAsteroids(int asteroidNum, float radius)
		{
			Asteroids = new List<Asteroid>();
			for (int i = 0; i < asteroidNum; i++) {
				//random = new Random();
				Asteroids.Add(new Asteroid(new Vector3(NextDouble(random, -radius, radius), 0, NextDouble(random, -radius, radius)), star.Position, 0.05f, "Models\\Asteroid"));
				//Asteroids[i].Scale = 0.02f;//0.1f;
				//Asteroids[i].SetModelEffect(lightingEffect, true);					// set effect to each modelmeshpart
			}
		}
		protected override void Initialize()
		{
			base.Initialize();
			enemyManager = new EnemyManager(this);
			new DebugOverlay(graphicsDevice, content);

			// Entities
			Models = new List<Object>();
			Ground = new Object(new Vector3(0, -500, 0), 1f, "Models\\ground");//-200
			Ground.RenderBoudingSphere = false;
			Models.Add(Ground);
			Target = new Object(new Vector3(0, 20, 0), 20, "Models\\cube");
			//Models.Add(Target);


			// Initializes camera
			//camera = new ArcBallCamera(Vector3.Zero);
			camera = new ArcBallCamera(Vector3.Zero);
			//camera.Initialize(game, Vector3.Zero);
			ParticleEmitter.camera = camera;

			// Set up the reference grid
			grid = new GridRenderer();
			grid.GridColor = Color.DarkSeaGreen;//Color.LimeGreen;
            grid.GridScale = 300f;//100f;
            grid.GridSize = 64;;//32;
			// Set the grid to draw on the x/z plane around the origin
			grid.WorldMatrix = Matrix.Identity;
		}	
		public override void Load()
		{
			base.Load();

			// Set up the reference grid and sample camera
			grid.LoadGraphicsContent(graphicsDevice);

			Sky = new SkySphere(content, graphicsDevice, content.Load<TextureCube>("Textures\\SkyBox\\space4"), 100);// set 11 for debug

			// Load stars
			star = new Star(new Vector3(-500, 100, 500), graphicsDevice, content, StarType.G);
			//star = new Star(-LightPosition, device, content, StarType.G);
			LightPosition = star.Position;
			//sun = new Sun(new Vector3(-500, 100, 500), graphicsDevice, content, spriteBatch);
			sun = new Sun(new Vector3(-2000, 500, 2000), graphicsDevice, content, spriteBatch);
			sunCircle = new Sun(LightPosition, graphicsDevice, content, spriteBatch);

			// Load planets
			Planets = new List<Planet>();
			TargetPlanets = new List<DamageablePlanet>();
			//WaterPlanet waterPlanet = new WaterPlanet(new Vector3(-1000, 0, -1000), -LightPosition, device, content);
			WaterPlanet waterPlanet = new WaterPlanet(new Vector3(-100, 100, -100), LightPosition, graphicsDevice, content);
			IcePlanet icePlanet = new IcePlanet(new Vector3(-100, 100, -800), LightPosition, graphicsDevice, content);
			GasGiant gasGiant = new GasGiant(new Vector3(-100, 100, -2500), LightPosition, graphicsDevice, content);
			RockPlanet rockPlanet = new RockPlanet(graphicsDevice, content);
			MoltenPlanet moltenPlanet = new MoltenPlanet(graphicsDevice, content);

			//planet = moltenPlanet;
			//planet = gasGiant;
			//planet = icePlanet;
			planet = waterPlanet;
			//Planets.Add(waterPlanet);
			TargetPlanets.Add(waterPlanet);
			Planets.Add(icePlanet);
			Planets.Add(gasGiant);

			// Load asteroids
			random = new Random();
			Asteroids = new List<Asteroid>();
			AddAsteroids(5, 2000);
			/*foreach (Asteroid o in Asteroids) {
				Models.Add(o);
			}*/
			foreach (Object o in Asteroids) {
				Enemies.Add(o);
			}
			spawned = true;


			// Load satellites
			//Satellite = new ArmedSatellite(new Vector3(300, 50, 300), star.Position, 5, "Models\\ISS", "SoundEffects\\laser1");
			Satellite = new ArmedSatellite(new Vector3(300, 50, 300), sun.Position, 5, "Models\\ISS", "SoundEffects\\laser0");
			Models.Add(Satellite);
			//Models.Add(new Satellite(false, waterPlanet.Position + new Vector3(400, 100, 600), waterPlanet.Position, 100f, "Models\\spacestation4"));
			Models.Add(new SpaceStation(false, waterPlanet.Position + new Vector3(400, 100, 600), waterPlanet.Position, 100f, "Models\\spacestation4"));

			//Models.Add(new ArmedSatellite(waterPlanet.Position + new Vector3(400, 50, 0), waterPlanet.Position, 0.01f, "Models\\TDRS", "SoundEffects\\License\\LAAT0"));
			Models.Add(new ArmedSatellite(SatelliteWeapon.Missile, waterPlanet.Position + new Vector3(400, 50, 0), waterPlanet.Position, 10f, "Models\\Dawn", "SoundEffects\\missile0"));// deepspace,10 / rosetta,10

			
			//Models.Add(new ArmedSatellite(waterPlanet.Position + new Vector3(400, 50, 0), waterPlanet.Position, 0.01f, "Models\\TDRS", "SoundEffects\\laser0"));

			// Load fighters
			//Models.Add(new Fighter(new Vector3(2000, 50, 1000), waterPlanet.Position, 20f, "Models\\fighter0"));
			//Enemies.Add(Models[Models.Count - 1]);
			Fighters = new List<Fighter>();
			Fighters.Add(new Fighter(new Vector3(2000, 50, 1000), waterPlanet.Position, 20f, "Models\\fighter0"));
			Fighters.Add(new Fighter(new Vector3(100, 1000, 100), waterPlanet.Position, 20f, "Models\\fighter0"));
			Fighters.Add(new Fighter(new Vector3(-100, -1000, -100), waterPlanet.Position, 20f, "Models\\fighter0"));
			Fighters.Add(new Fighter(new Vector3(-2000, 50, -1000), waterPlanet.Position, 20f, "Models\\fighter0"));
			foreach (Fighter f in Fighters) {
				Enemies.Add(f);
			}
			foreach (Object o in Enemies) {
				Models.Add(o);
			}


			// Set up light effects !!
			shadowEffect = content.Load<Effect>("ProjectShadowDepthEffectV4");
			Effect lightingEffect = content.Load<Effect>("PPModel");	// load Prelighting Effect
			foreach (Object o in Models) {
				o.RenderBoudingSphere = false;
				if (!(o is Asteroid)) {
					o.SetModelEffect(shadowEffect, true);
				}
			}
			foreach (Planet p in Planets) {
				p.RenderBoudingSphere = false;
				//p.SetModelEffect(shadowEffect, true);
				Models.Add(p);
			}
			foreach (DamageablePlanet p in TargetPlanets) {
				p.RenderBoudingSphere = false;
				//p.SetModelEffect(shadowEffect, true);
				Models.Add(p);
			}


			renderer = new PrelightingRenderer(graphicsDevice, content);
			renderer.Models = Models;
			renderer.Camera = camera;
			renderer.Lights = new List<PointLight>() {
				//new PointLightCircle(new Vector3(0, 1000, 0), 2000, Color.White, 2000),	// 影のデバッグ用
				//new PointLight(new Vector3(0, 500, 0), Color.White, 2000),				// 太陽の光源
                new PointLight(sun.Position, Color.White, 5000),							// 太陽の光源
				new PointLight(new Vector3(0, 10000, 0), Color.White * .85f, 100000),		// シーン全体を照らす巨大なライトにする
			};
			renderer.ShadowLightPosition = new Vector3(300, 500, 300);//LightPosition;
			renderer.ShadowLightTarget = new Vector3(0, 0, 0);
			renderer.DoShadowMapping = true;
			renderer.ShadowMult = 0.3f;//0.01f;//0.3f;
			LightPosition = renderer.Lights[0].Position;


			// Special effects
			EnergyRingEffect.game = game;
			discoidEffect = new EnergyRingEffect(content, graphicsDevice, new Vector3(0, 0, 0), new Vector2(300));
			EnergyShieldEffect.game = game;
			shieldEffect = new EnergyShieldEffect(content, graphicsDevice, Satellite.Position, new Vector2(300), 250);
			explosionTest = new ExplosionEffect(content, graphicsDevice, new Vector3(0, 50, 0), Vector2.One, true, "Xml\\Particle\\particleExplosion0.xml", true);
			smallExplosion = new ExplosionEffect(content, graphicsDevice, new Vector3(0, 50, 0), Vector2.One, false, "Xml\\Particle\\particleExplosion0.xml", false);
			//smallExplosion = new ExplosionEffect(content, device, new Vector3(0, 50, 0), Vector2.One, false, "Xml\\Particle\\particleExplosion0.xml", true);
			bigExplosion = new ExplosionEffect(content, graphicsDevice, new Vector3(0, 50, 0), Vector2.One, false, "Xml\\Particle\\particleExplosion1.xml", false);
			midExplosion = new ExplosionEffect(content, graphicsDevice, new Vector3(0, 50, 0), Vector2.One, false, "Xml\\Particle\\particleExplosion2.xml", false);

			lb = new LaserBillboard(graphicsDevice, content, content.Load<Texture2D>("Textures\\Laser2"), new Vector2(300, 50), new Vector3(0, 50, 0), new Vector3(100, 60, -100));
		}

		Vector3 GetRayPlaneIntersectionPoint(Ray ray, Plane plane)//Nullable<Vector3>
		{
			float? distance = ray.Intersects(plane);
			//return distance.HasValue ? ray.Position + ray.Direction * distance.Value : null;
			return distance.HasValue ? ray.Position + ray.Direction * distance.Value : Vector3.Zero;
		}
		Vector2 mouseOrgPos, curPos, prevPos;
		float leftrightRot, updownRot;
		protected override void HandleInput()
		{
			base.HandleInput();

			MouseState ms = Mouse.GetState();
			ButtonState bs = ms.LeftButton;
			if (MouseInput.IsOnButtonDownL()) {
				Vector2 mousePos = MouseInput.GetMousePosition();

				// rayとplaneのintersection pointを計算する
				Vector3 nearsource = new Vector3((float)mousePos.X, (float)mousePos.Y, 0f);
				Vector3 farsource = new Vector3((float)mousePos.X, (float)mousePos.Y, 1f);

				Matrix world = Matrix.CreateTranslation(0, 0, 0);
				Vector3 nearPoint = graphicsDevice.Viewport.Unproject(nearsource, camera.Projection, camera.View, world);
				Vector3 farPoint = graphicsDevice.Viewport.Unproject(farsource, camera.Projection, camera.View, world);
				// Create a ray from the near clip plane to the far clip plane.
				Vector3 direction = farPoint - nearPoint;
				direction.Normalize();
				Ray pickRay = new Ray(nearPoint, direction);
				Plane planeXZ = new Plane(Vector3.Up, 0);
				float spawnHeight = -200;
				Plane planeGround = new Plane(Vector3.Up, -spawnHeight);

				Vector3 intersectionPoint = GetRayPlaneIntersectionPoint(pickRay, planeGround);
				//Models.Add(new ArmedSatellite(intersectionPoint, 10, "Models\\ISS"));
				Models.Add(new Object(intersectionPoint, 20, "Models\\cube"));
				Models[Models.Count - 1].RenderBoudingSphere = false;
			}

			/*if (MouseInput.IsOnButtonDownR()) {
				mouseOrgPos = MouseInput.GetMousePositiion();
				leftrightRot = updownRot = 0;
			}
			if (MouseInput.BUTTONR()) {
				float rotationSpeed = 5;
				Vector2 mousePos = MouseInput.GetMousePositiion();

				// rayとplaneのintersection pointを計算する
				Vector3 nearsource = new Vector3((float)mousePos.X, (float)mousePos.Y, 0f);
				Vector3 farsource = new Vector3((float)mousePos.X, (float)mousePos.Y, 1f);

				Matrix world = Matrix.CreateTranslation(0, 0, 0);
				Vector3 nearPoint = graphicsDevice.Viewport.Unproject(nearsource, camera.Projection, camera.View, world);
				Vector3 farPoint = graphicsDevice.Viewport.Unproject(farsource, camera.Projection, camera.View, world);
				// Create a ray from the near clip plane to the far clip plane.
				Vector3 direction = farPoint - nearPoint;
				direction.Normalize();

				float xDifference = curPos.X - mouseOrgPos.X;
				float yDifference = curPos.Y - mouseOrgPos.Y;
				leftrightRot -= rotationSpeed * xDifference;// * amount
				updownRot -= rotationSpeed * yDifference;
				Mouse.SetPosition((int)mouseOrgPos.X, (int)mouseOrgPos.Y);
			}
*/
			float stickSensitivity = 0.2f;
			//  スティックが倒されていればDirectionを再計算する
			if (JoyStick.Vector.Length() > stickSensitivity) {
				/*double analogAngle = Math.Atan2(JoyStick.Vector.Y, JoyStick.Vector.X);
				float speed = JoyStick.Vector.Length() * 30;
				analogAngle += MathHelper.ToRadians(-90);
                
				Vector3 tmpVelocity = Vector3.Zero;
				tmpDirention = tmpCameraPos - camera.Position;
				tmpDirention = new Vector3(tmpDirention.X, 0, tmpDirention.Z);
				RotationMatrix = Matrix.CreateRotationY((float)analogAngle);
				// 面白い動き : //RotationMatrix = Matrix.CreateRotationY(MathHelper.ToRadians(JoyStick.Vector.Y)) * Matrix.CreateRotationX(MathHelper.ToRadians(-JoyStick.Vector.X));
				tmpDirention = Vector3.TransformNormal(tmpDirention, RotationMatrix);
				tmpDirention = Vector3.Normalize(tmpDirention);// プロパティなので代入しないと反映されないことに注意
				tmpVelocity = new Vector3(tmpDirention.X * speed, tmpVelocity.Y, tmpDirention.Z * speed);

				tmpCameraPos += tmpVelocity;*/
			}
			/*if (JoyStick.stickDirection == Direction.LEFT) {
				tmpCameraPos += new Vector3(-10, 0, 0);
			} else if (JoyStick.stickDirection == Direction.RIGHT) {
				tmpCameraPos += new Vector3(10, 0, 0);
			}
			if (JoyStick.stickDirection == Direction.UP) {
				tmpCameraPos += new Vector3(0, 0, 10);
			} else if (JoyStick.stickDirection == Direction.DOWN) {
				tmpCameraPos += new Vector3(0, 0, -10);
			}*/
		}
		protected override void Collide()
		{
			base.Collide();

			/*foreach (Object o in asteroids) {
				if (o.IsActive && discoidEffect.IsHitWith(o.transformedBoundingSphere)) {
					o.IsActive = false;

					ExplosionEffect e = (ExplosionEffect)smallExplosion.Clone();// positionは与えなおさないとｗ
					e.Position = o.Position;
					foreach (ExplosionParticleEmitter ep in e.emitters) {
						ep.Position = e.Position;// もう既にparticlesは初期化されてしまってるので手遅れｗｗ
					}
					//e.Run();
					//effectManager.Add(e);
				}
			}*/
			foreach (Bullet b in Bullets) {
				//foreach (Object a in Asteroids) {
				foreach (Object o in Enemies) {
					if (o is Asteroid && b.IsActive && o.IsActive && b.IsHitWith(o)) {
						//a.IsActive = false;
						//b.IsActive = false;
						if (!(b is LaserBillboardBullet && (b as LaserBillboardBullet).Mode == 1)) b.Die();
						o.Die();
						

						//ExplosionEffect e = (random.Next(0, 2) == 0) ? (ExplosionEffect)smallExplosion.Clone() : (ExplosionEffect)bigExplosion.Clone();
						//ExplosionEffect e = (ExplosionEffect)bigExplosion.Clone();
						ExplosionEffect e = null;
						/*double p = random.NextDouble();
						if (p <= 0.5) {
							e = (ExplosionEffect)smallExplosion.Clone();
						} else if (p <= 0.9) {
							e = (ExplosionEffect)midExplosion.Clone();
						} else {
							e = (ExplosionEffect)bigExplosion.Clone();
						}*/
						e = (ExplosionEffect)smallExplosion.Clone();

						e.Position = o.Position;
						/*foreach (ExplosionParticleEmitter ep in e.emitters) {
							ep.Position = e.Position;// もう既にparticlesは初期化されてしまってるので手遅れ！
						}*/
						e.Run();
						effectManager.Add(e);
					}

				

				//foreach (Object o in Enemies) {
					if (o is Fighter && (b is Missile) && b.IsActive && b.Identification == IFF.Friend && b.IsHitWith(o)) {
						 //b.IsActive = false;
						if (!(b is LaserBillboardBullet && (b as LaserBillboardBullet).Mode == 1)) b.Die();
						o.Die();

						ExplosionEffect e = (ExplosionEffect)smallExplosion.Clone();
						e.Position = o.Position;
						/*foreach (ExplosionParticleEmitter ep in e.emitters) {
							ep.Position = e.Position;// もう既にparticlesは初期化されてしまってるので手遅れ！
						}*/
						e.Run();
						effectManager.Add(e);
					}
				}

				// Collide with waterPlanet
				foreach (DamageablePlanet p in TargetPlanets) {
					if (b.Identification == IFF.Foe && b.IsHitWith(p)) {
						//b.IsActive = false;
						b.Die();
						p.Damage();

						// 重い！数が多いのでもっと軽いエフェクトを作ろう
						/*ExplosionEffect e = (ExplosionEffect)smallExplosion.Clone();
						e.Position = b.Position;
						e.Run();
						effectManager.Add(e);*/
					}
				}
			}
			BoundingSphere bs = new BoundingSphere(planet.Position, 200);
			if (discoidEffect.IsHitWith(bs)) {
				//planet.IsActive = false;
				//effectManager.Add(new ExplosionEffect(content, device, planet.Position, Vector2.One));
			}


			// Remove dead objects
			if (Asteroids.Count > 0) {
				for (int j = 0; j < Asteroids.Count; j++) {
					if (!Asteroids[j].IsAlive) {
						Asteroids.RemoveAt(j);
					}
				}
			}
			if (Bullets.Count > 0) {
				for (int j = 0; j < Bullets.Count; j++) {
					if (!Bullets[j].IsActive) {
						Bullets.RemoveAt(j);
					}
				}
			}
			for (int j = 0; j < Models.Count; j++) {
				if (!Models[j].IsAlive) {
					Models.RemoveAt(j);
				}
			}
			for (int j = 0; j < Enemies.Count; j++) {
				if (!Enemies[j].IsAlive) {
					Enemies.RemoveAt(j);
				}
			}
		}
		public override void Update(GameTime gameTime)
		{
			float elapsed = (float)gameTime.TotalGameTime.TotalSeconds;
			count++;
			/*if (count % 1001 >= 0 && Asteroids.Count == 0) {
				spawned = false;
				count = 0;
			}
			if (!spawned) {
				AddAsteroids(15, 2000);
				for (int i = 0; i < Asteroids.Count; i++) {
					Asteroids[i].IsActive = true;
					Asteroids[i].RenderBoudingSphere = false;
					Asteroids[i].SetModelEffect(shadowEffect, true);
					Models.Add(Asteroids[i]);
				}
				spawned = true;
			}*/
			/*if (Asteroids.Count < MAX_SPAWN_NUM) {// = 15 -5
				float radius = 3000;
				Asteroid a = new Asteroid(new Vector3(NextDouble(random, -radius, radius), 0, NextDouble(random, -radius, radius)), star.Position, 0.05f, "Models\\Asteroid");
				//Asteroids[i].Scale = 0.02f;//0.1f;
				//a.SetModelEffect(shadowEffect, true);					// set effect to each modelmeshpart
				a.IsActive = true;
				a.RenderBoudingSphere = false;
				Asteroids.Add(a);
				Models.Add(a);
			}*/
			enemyManager.Update(gameTime);

			base.Update(gameTime);

			HandleInput();
			camera.Update(gameTime);
			renderer.Update(gameTime);
			LightPosition = renderer.Lights[0].Position;

			Sky.Update(gameTime);
			sun.Update(gameTime);
			sunCircle.Position = renderer.Lights[0].Position;
			sunCircle.Update(gameTime);

			foreach (Object o in Models) {
				if (o.IsAlive) o.Update(gameTime);
			}
			foreach (Bullet b in Bullets) {
				if (b.IsAlive) b.Update(gameTime);
			}

			if (planet.IsAlive) planet.Update(gameTime);


			//discoidEffect.Update(gameTime);

			//shieldEffect.Position = Satellite.Position;
			//shieldEffect.Update(gameTime);

			//explosionTest.Update(gameTime);
			//bigExplosion.Update(gameTime);
			lb.Update(gameTime);

			Collide();

			effectManager.Update(gameTime);
			uiManager.Update(gameTime);
		}
		public override void Draw(GameTime gameTime)
		{
			base.Draw(gameTime);

			// Initialize values (for debug)
			ResetGraphicDevice();
			float sunDepth = Vector3.Transform(sun.Position, camera.View).Z;
			//sunDepth =   Vector3.Transform(Vector3.Transform(Vector3.Transform(sun.Position, sun.World), camera.View), camera.Projection).Z;
			//float sunFrontDepth = Vector3.Transform(Vector3.Transform(sun.Position + (Vector3.Normalize(sun.Position - camera.CameraPosition) * 200), sun.world), camera.View).Z;
			camera.FarPlaneDistance = 10000000;


			// Draw pre-passes
			/*graphicsDevice.SetRenderTarget(maskLayer);
			graphicsDevice.Clear(Color.White);
			foreach (Object o in Models) {
				//o.DrawMask(camera.View, camera.Projection, camera.Position, ref maskLayer, sunDepth);
				if (camera.BoundingVolumeIsInView(o.transformedBoundingSphere)) {
					o.DrawMask(camera.View, camera.Projection, camera.Position, ref maskLayer, sunDepth);
				}
			}
			graphicsDevice.SetRenderTarget(null);*/
			graphicsDevice.SetRenderTarget(maskLayer);
			graphicsDevice.Clear(Color.White);
			graphicsDevice.SetRenderTarget(null);
			renderer.PreDraw();
			sun.PreDraw(camera.View, camera.Projection);
			graphicsDevice.Clear(Color.White);


			// Draw environment
			ResetGraphicDevice();
			Sky.Draw(camera.View, camera.Projection, camera.Position);
			ResetGraphicDevice();
			//sun.Draw(true, camera.View, camera.Projection, maskLayer);
			ResetGraphicDevice();
			sunCircle.Draw(false, camera.View, camera.Projection);
			//planet.Draw(camera.View, Matrix.CreateScale(200) * Matrix.CreateTranslation(new Vector3(-300, 0, -200)), camera.Projection, camera.Position);
			//planet.Draw(new Vector3(-300, 0, -200), camera.View, camera.Projection, camera.Position);
			//if (planet.IsActive) planet.Draw(camera.View, camera.Projection, camera.Position);
			//star.Draw(camera.View, camera.Projection);


			// Draw objects
			foreach (Object o in Models) {
				if (o.IsAlive && camera.BoundingVolumeIsInView(o.transformedBoundingSphere)) {
					if (o is ArmedSatellite) {
						(o as ArmedSatellite).Draw(gameTime, camera.View, camera.Projection, camera.Position);
					} else {
						o.Draw(camera.View, camera.Projection, camera.Position);
					}
				}
			}
			foreach (Bullet b in Bullets) {
				if (b.IsActive) b.Draw(camera);
			}

			// Draw debug overlays
			renderer.Draw(gameTime);
			if (displayGrid) {
				grid.ProjectionMatrix = camera.Projection;
				grid.ViewMatrix = camera.View;
				// draw the reference grid so it's easier to get our bearings
				//grid.Draw();
			}
			DebugOverlay.Arrow(Vector3.Zero, Vector3.UnitX * 1000, 1, Color.Red);
			DebugOverlay.Arrow(Vector3.Zero, Vector3.UnitY * 1000, 1, Color.Green);
			DebugOverlay.Arrow(Vector3.Zero, Vector3.UnitZ * 1000, 1, Color.Blue);
			DebugOverlay.Singleton.Draw(camera.Projection, camera.View);

			// Draw effects
			effectManager.Draw(gameTime, camera);

			foreach (EnergyShieldEffect ese in transparentEffects) {
				//ese.Draw(gameTime, camera.View, camera.Projection, camera.Position, camera.Direction, camera.Up, camera.Right);
				ese.Draw(gameTime, camera.View, camera.Projection, camera.Position, camera.Direction, camera.Up, camera.Right);
			}


			// Draw UIs
			uiManager.Draw(gameTime);
		}


		public Level4(Scene previousScene)
			: base(previousScene)
		{
			displayGrid = true;
		}
	}
}
