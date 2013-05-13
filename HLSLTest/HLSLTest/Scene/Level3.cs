using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace HLSLTest
{
	public class Level3 : Level
	{
		public Object Target { get; private set; }
		public Object Ground { get; private set; }
		public Object Teapot { get; private set; }
		public ArmedSatellite Satellite { get; private set; }

		PrelightingRenderer renderer;
		GridRenderer grid;

		FlameParticleEmitter ps;
		ExplosionParticleEmitter eps;
		DiscoidParticleEmitter discoid;
		ParticleEmitter basicEmitter, beamEmitter;
		EnergyRingEffect discoidEffect;
		EnergyShieldEffect shieldEffect;
		ExplosionEffect explosionTest, smallExplosion, bigExplosion;
		Planet planet;
		Star star;
		public List<Object> Asteroids { get; private set; }
		
		Random random;
		List<ExplosionEffect> ex = new List<ExplosionEffect>();
		private bool spawned;
		private int count;
		//ParticleSettings setting;
		LaserBillboard lb;
		public static float NextDouble(Random r, double min, double max)
		{
			return (float)(min + r.NextDouble() * (max - min));
		}
		private void AddAsteroids(int asteroidNum, float radius)
		{
			Asteroids = new List<Object>();
			Effect lightingEffect = content.Load<Effect>("Lights\\AsteroidLightingEffect");	// load Prelighting Effect
			Object.SetEffectParameter(lightingEffect, "LightDirection", -LightPosition);
			Object.SetEffectParameter(lightingEffect, "SpecularPower", 200);
			//Object.SetEffectParameter(lightingEffect, "AmbientColor", new Vector3(0.5f));

			/*asteroids.Add(new Object(new Vector3(-100,0,100), "Models\\Asteroid"));
			asteroids.Add(new Object(new Vector3(-500, 0, 500), "Models\\Asteroid"));
			asteroids[0].Scale = 0.02f;
			asteroids[0].SetModelEffect(lightingEffect, true);
			asteroids[1].Scale = 0.02f;
			asteroids[1].SetModelEffect(lightingEffect, true);*/
            for (int i = 0; i < asteroidNum; i++) {
				//random = new Random();
                Asteroids.Add(new Object(new Vector3(NextDouble(random, -radius, radius), 0, NextDouble(random, -radius, radius)), "Models\\Asteroid"));
				Asteroids[i].Scale = 0.02f;//0.1f;
				Asteroids[i].SetModelEffect(lightingEffect, true);					// set effect to each modelmeshpart
			}/**/
		}
		protected override void Initialize()
		{
			base.Initialize();
			Models = new List<Object>();
			Ground = new Object(new Vector3(0, -50, 0), 0.02f, "Models\\ground");
			Models.Add(Ground);


			Target = new Object(new Vector3(0, 20, 0), 20, "Models\\cube");
			//Models.Add(Target);
			Satellite = new ArmedSatellite(new Vector3(0, 50, 0), 1, "Models\\ISS");
			Models.Add(Satellite);

			random = new Random();
			Asteroids = new List<Object>();
			AddAsteroids(50, 500);
			spawned = true;


			// Initializes camera
			camera = new ArcBallCamera();
			camera.Initialize(game, Vector3.Zero);
			ParticleEmitter.camera = camera;

			// Set up the reference grid
			grid = new GridRenderer();
			grid.GridColor = Color.DarkSeaGreen;//Color.LimeGreen;
			grid.GridScale = 100f;
			grid.GridSize = 32;//32;
			// Set the grid to draw on the x/z plane around the origin
			grid.WorldMatrix = Matrix.Identity;
		}
		public override void Load()
		{
			base.Load();

			// Set up the reference grid and sample camera
			grid.LoadGraphicsContent(device);

			Sky = new SkySphere(content, device, content.Load<TextureCube>("Textures\\SkyBox\\space4"), 100);// set 11 for debug

			// Set up light effects !!
			Effect shadowEffect = content.Load<Effect>("ProjectShadowDepthEffectV4");
			Effect lightingEffect = content.Load<Effect>("PPModel");	// load Prelighting Effect
			foreach (Object o in Models) {
				o.RenderBoudingSphere = false;
				o.SetModelEffect(shadowEffect, true);
			}
			renderer = new PrelightingRenderer(device, content);
			renderer.Models = Models;
			renderer.Camera = camera;
			renderer.Lights = new List<PointLight>() {
				//new PointLightCircle(new Vector3(0, 200, 0), 200, Color.White, 2000),
				new PointLight(LightPosition, Color.White * .85f, 200000),
				new PointLight(new Vector3(0, 200, 0), Color.White * .85f, 20000),
			};
			renderer.ShadowLightPosition = LightPosition;
			renderer.ShadowLightTarget = new Vector3(0, 0, 0);
			renderer.DoShadowMapping = true;
			renderer.ShadowMult = 0.3f;//0.01f;//0.3f;


			// Load planets
			WaterPlanet waterPlanet = new WaterPlanet(new Vector3(-1000, 0, -1000), -LightPosition, device, content);
			IcePlanet icePlanet = new IcePlanet(device, content);
			GasGiant gasGiant = new GasGiant(device, content);
			RockPlanet rockPlanet = new RockPlanet(device, content);
			MoltenPlanet moltenPlanet = new MoltenPlanet(device, content);

			//planet = moltenPlanet;
			//planet = gasGiant;
			//planet = icePlanet;
			planet = waterPlanet;
			star = new Star(device, content, StarType.G);


			EnergyRingEffect.game = game;
			discoidEffect = new EnergyRingEffect(content, device, new Vector3(0, 0, 0), new Vector2(300));
			EnergyShieldEffect.game = game;
			shieldEffect = new EnergyShieldEffect(content, device, new Vector3(0, 0, 0), new Vector2(300), 250);
			explosionTest = new ExplosionEffect(content, device, new Vector3(0, 50, 0), Vector2.One, true, "Xml\\Particle\\particleExplosion0.xml", true);
			smallExplosion = new ExplosionEffect(content, device, new Vector3(0, 50, 0), Vector2.One, false, "Xml\\Particle\\particleExplosion0.xml", false);
			//smallExplosion = new ExplosionEffect(content, device, new Vector3(0, 50, 0), Vector2.One, false, "Xml\\Particle\\particleExplosion0.xml", true);
			bigExplosion = new ExplosionEffect(content, device, new Vector3(0, 50, 0), Vector2.One, false, "Xml\\Particle\\particleExplosion1.xml", false);

			// pre-load
			//setting = new ParticleSettings("Xml\\Particle\\particleExplosion0");


			lb = new LaserBillboard(device, content, content.Load<Texture2D>("Textures\\Laser2"), new Vector2(300, 50), new Vector3(0, 50, 0), new Vector3(100, 60, -100));
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
			foreach (Drawable b in Bullets) {
				foreach (Object a in Asteroids) {
					if (b.IsActive && a.IsActive && b.IsHitWith(a)) {
						a.IsActive = false;
						//b.IsActive = false;

						ExplosionEffect e = (random.Next(0, 2) == 0) ? (ExplosionEffect)smallExplosion.Clone() : (ExplosionEffect)bigExplosion.Clone();
						//ExplosionEffect e = (ExplosionEffect)bigExplosion.Clone();

						e.Position = a.Position;
						/*foreach (ExplosionParticleEmitter ep in e.emitters) {
							ep.Position = e.Position;// もう既にparticlesは初期化されてしまってるので手遅れ！
						}*/
						e.Run();
						effectManager.Add(e);
					}/**/
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
					if (!Asteroids[j].IsActive) {
						Asteroids.RemoveAt(j);
					}
				}/**/
			}
			if (Bullets.Count > 0) {
				for (int j = 0; j < Bullets.Count; j++) {
					if (!Bullets[j].IsActive) {
						Bullets.RemoveAt(j);
					}
				}
			}
		}
		public override void Update(GameTime gameTime)
		{
			float elapsed = (float)gameTime.TotalGameTime.TotalSeconds;
			/*count++;
			if (count % 1001 == 0) {
				spawned = false;
			}
			if (!spawned) {
				//AddAsteroids();
				for (int i = 0; i < asteroids.Count; i++) {
					asteroids[i].IsActive = true;
				}
				spawned = true;
			}*/

			base.Update(gameTime);


			camera.UpdateChaseTarget(Vector3.Zero);
			camera.Update(gameTime);
			renderer.Update(gameTime);
			Sky.Update(gameTime);

			foreach (Object o in Models) {
				o.Update(gameTime);
			}
			foreach (Object a in Asteroids) {
				if (a.IsActive) a.Update(gameTime);
			}
			foreach (Drawable b in Bullets) {
				if (b.IsActive) b.Update(gameTime);
			}

			if (planet.IsActive) planet.Update(gameTime);


			//discoidEffect.Update(gameTime);
			//shieldEffect.Update(gameTime);
			//explosionTest.Update(gameTime);
			//bigExplosion.Update(gameTime);
			lb.Update(gameTime);

			Collide();

			effectManager.Update(gameTime);
		}

		public override void Draw(GameTime gameTime)
		{
			base.Draw(gameTime);

			renderer.PreDraw();
			device.Clear(Color.White);

			// Environment
			Sky.Draw(camera.View, camera.Projection, camera.CameraPosition);
			//planet.Draw(camera.View, Matrix.CreateScale(200) * Matrix.CreateTranslation(new Vector3(-300, 0, -200)), camera.Projection, camera.CameraPosition);
			//planet.Draw(new Vector3(-300, 0, -200), camera.View, camera.Projection, camera.CameraPosition);

			if (planet.IsActive) planet.Draw(camera.View, camera.Projection, camera.CameraPosition);
			star.Draw(camera.View, camera.Projection);

			// Entities
			foreach (Object o in Models) {
				o.Draw(camera.View, camera.Projection, camera.CameraPosition);
			}
			foreach (Object a in Asteroids) {
				if (a.IsActive) a.Draw(camera.View, camera.Projection, camera.CameraPosition);
			}
			/*foreach (Drawable b in Bullets) {
				if (b.IsActive) b.Draw(camera.View, camera.Projection, camera.CameraPosition);
			}*/
			foreach (Drawable b in Bullets) {
				if (b.IsActive) b.Draw(camera);
			}
			//lb.Draw(camera.View, camera.Projection, camera.Up, camera.Right, camera.CameraPosition);

			//discoidEffect.Draw(gameTime, camera.View, camera.Projection, camera.CameraPosition, camera.Direction, camera.Up, camera.Right);
			//shieldEffect.Draw(gameTime, camera.View, camera.Projection, camera.CameraPosition, camera.Direction, camera.Up, camera.Right);
			//explosionTest.Draw(gameTime, camera);
			//bigExplosion.Draw(gameTime, camera);

			renderer.Draw(gameTime);
			// Grid
			if (displayGrid) {
				grid.ProjectionMatrix = camera.Projection;
				grid.ViewMatrix = camera.View;
				// draw the reference grid so it's easier to get our bearings
				//grid.Draw();
			}


			effectManager.Draw(gameTime, camera);
		}


		public Level3(Scene previousScene)
			: base(previousScene)
		{
			displayGrid = true;
		}
	}
}
