using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace HLSLTest
{
	public class Level2 : Level
	{
		public Object Target { get; private set; }
		public Object Ground { get; private set; }
		public Object Teapot { get; private set; }

		PrelightingRenderer renderer;
		GridRenderer grid;

		FlameParticleEmitter ps;
		ExplosionParticleEmitter eps;
		DiscoidParticleEmitter discoid;
		ParticleEmitter basicEmitter, beamEmitter;
		EnergyRingEffect discoidEffect;
		EnergyShieldEffect shieldEffect;
		ExplosionEffect explosionTest;
		Planet planet;
		Star star;
		List<Object> asteroids;
		Random random;

		public static float NextDouble(Random r, double min, double max)
		{
			return (float)(min + r.NextDouble() * (max - min));
		}
		protected override void Initialize()
		{
			base.Initialize();
			Models = new List<Object>();
			Target = new Object(new Vector3(0, 20, 0), "Models\\cube");
			Target.Scale = 20;
			Models.Add(Target);

			random = new Random();
			asteroids = new List<Object>();
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
			for (int i = 0; i < 100; i++) {
				//random = new Random();
				asteroids.Add(new Object(new Vector3(NextDouble(random, -1000, 1000), 0, NextDouble(random, -1000, 1000)), "Models\\Asteroid"));
				asteroids[i].Scale = 0.02f;//0.1f;
				asteroids[i].SetModelEffect(lightingEffect, true);					// set effect to each modelmeshpart
			}/**/

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
			explosionTest = new ExplosionEffect(content, device, new Vector3(0, 50, 0), Vector2.One, true);
		}

		protected override void Collide()
		{
			base.Collide();

			foreach (Object o in asteroids) {
				if (discoidEffect.IsHitWith(o.transformedBoundingSphere)) {
					o.IsActive = false;
					effectManager.Add(new ExplosionEffect(content, device, o.Position, Vector2.One));
				}
			}

			BoundingSphere bs = new BoundingSphere(planet.Position, 200);
			if (discoidEffect.IsHitWith(bs)) {
				planet.IsActive = false;
				//effectManager.Add(new ExplosionEffect(content, device, planet.Position, Vector2.One));
			}
			
			// Remove dead objects
			if (asteroids.Count > 0) {
				for (int j = 0; j < asteroids.Count; j++) {
					if (!asteroids[j].IsActive) {
						asteroids.RemoveAt(j);
					}
				}
			}
		}
		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			camera.UpdateChaseTarget(Vector3.Zero);
			camera.Update(gameTime);

			Sky.Update(gameTime);

			foreach (Object o in Models) {
				o.Update(gameTime);
			}
			foreach (Object a in asteroids) {
				if (a.IsActive) a.Update(gameTime);
			}

			if (planet.IsActive) planet.Update(gameTime);


			discoidEffect.Update(gameTime);
			//shieldEffect.Update(gameTime);
			explosionTest.Update(gameTime);

			Collide();

			effectManager.Update(gameTime);
		}

		public override void Draw(GameTime gameTime)
		{
			base.Draw(gameTime);

			device.Clear(Color.White);

			

			// Environment
			Sky.Draw(camera.View, camera.Projection, camera.CameraPosition);
			//planet.Draw(camera.View, Matrix.CreateScale(200) * Matrix.CreateTranslation(new Vector3(-300, 0, -200)), camera.Projection, camera.CameraPosition);
			//planet.Draw(new Vector3(-300, 0, -200), camera.View, camera.Projection, camera.CameraPosition);

			if (planet.IsActive) planet.Draw(camera.View, camera.Projection, camera.CameraPosition);
			star.Draw(camera.View, camera.Projection);

			// Entities
			foreach (Object o in Models) {
				//o.Draw(camera.View, camera.Projection, camera.CameraPosition);
			}
			foreach (Object a in asteroids) {
				if (a.IsActive) a.Draw(camera.View, camera.Projection, camera.CameraPosition);
			}


			discoidEffect.Draw(gameTime, camera.View, camera.Projection, camera.CameraPosition, camera.Direction, camera.Up, camera.Right);
			//shieldEffect.Draw(gameTime, camera.View, camera.Projection, camera.CameraPosition, camera.Direction, camera.Up, camera.Right);
			explosionTest.Draw(gameTime, camera);

			// Grid
			if (displayGrid) {
				grid.ProjectionMatrix = camera.Projection;
				grid.ViewMatrix = camera.View;
				// draw the reference grid so it's easier to get our bearings
				grid.Draw();
			}


			effectManager.Draw(gameTime, camera);
		}


		public Level2(Scene previousScene)
			: base(previousScene)
		{
			displayGrid = true;
		}
	}
}
