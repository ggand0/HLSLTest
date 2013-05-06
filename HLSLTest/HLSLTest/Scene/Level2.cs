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
		DiscoidEffect discoidEffect;
		EnergyShieldEffect shieldEffect;
		Planet planet;

		protected override void Initialize()
		{
			base.Initialize();
			Models = new List<Object>();
			Target = new Object(new Vector3(0, 20, 0), "Models\\cube");
			Target.Scale = 20;
			Models.Add(Target);

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
			WaterPlanet waterPlanet = new WaterPlanet(device, content);
			IcePlanet icePlanet = new IcePlanet(device, content);
			GasGiant gasGiant = new GasGiant(device, content);
			RockPlanet rockPlanet = new RockPlanet(device, content);
			MoltenPlanet moltenPlanet = new MoltenPlanet(device, content);

			//planet = moltenPlanet;
			//planet = gasGiant;
			//planet = icePlanet;
			planet = waterPlanet;


			DiscoidEffect.game = game;
			discoidEffect = new DiscoidEffect(content, device, new Vector3(0, 0, 0), new Vector2(300));
			EnergyShieldEffect.game = game;
			shieldEffect = new EnergyShieldEffect(content, device, new Vector3(0, 0, 0), new Vector2(300), 250);
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


			//discoidEffect.Update(gameTime);
			shieldEffect.Update(gameTime);
		}

		public override void Draw(GameTime gameTime)
		{
			base.Draw(gameTime);

			device.Clear(Color.White);

			

			// Terrain
			Sky.Draw(camera.View, camera.Projection, camera.CameraPosition);
			//planet.Draw(camera.View, Matrix.CreateScale(200) * Matrix.CreateTranslation(new Vector3(-300, 0, -200)), camera.Projection, camera.CameraPosition);
			//planet.Draw(new Vector3(-50, 0, -50), camera.View, camera.Projection, camera.CameraPosition);
			planet.Draw(new Vector3(-300, 0, -200), camera.View, camera.Projection, camera.CameraPosition);

			// Entities
			foreach (Object o in Models) {
				o.Draw(camera.View, camera.Projection, camera.CameraPosition);
			}


			//discoidEffect.Draw(gameTime, camera.View, camera.Projection, camera.CameraPosition, camera.Direction, camera.Up, camera.Right);
			shieldEffect.Draw(gameTime, camera.View, camera.Projection, camera.CameraPosition, camera.Direction, camera.Up, camera.Right);

			// Grid
			grid.ProjectionMatrix = camera.Projection;
			grid.ViewMatrix = camera.View;
			// draw the reference grid so it's easier to get our bearings
			grid.Draw();
		}


		public Level2(Scene previousScene)
			: base(previousScene)
		{
		}
	}
}
