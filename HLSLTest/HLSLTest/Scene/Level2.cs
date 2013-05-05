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

		FlameParticleEmitter ps;
		ExplosionParticleEmitter eps;
		DiscoidParticleEmitter discoid;
		ParticleEmitter basicEmitter, beamEmitter;
		DiscoidEffect discoidEffect;
		Planet planet;

		protected override void Initialize()
		{
			base.Initialize();
			Models = new List<Object>();
			Target = new Object(new Vector3(0, 20, 0), "Models\\cube");
			Target.Scale = 20;
			Models.Add(Target);

			camera = new ArcBallCamera();
			camera.Initialize(game, Vector3.Zero);
			ParticleEmitter.camera = camera;
		}
		public override void Load()
		{
			base.Load();

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


			discoid.Update();
		}

		public override void Draw(GameTime gameTime)
		{
			base.Draw(gameTime);

			device.Clear(Color.White);

			// Terrain
			Sky.Draw(camera.View, camera.Projection, camera.CameraPosition);
			//planet.Draw(camera.View, Matrix.CreateScale(200) * Matrix.CreateTranslation(new Vector3(-300, 0, -200)), camera.Projection, camera.CameraPosition);
			planet.Draw(new Vector3(-300, 0, -200), camera.View, camera.Projection, camera.CameraPosition);

			// Entities
			foreach (Object o in Models) {
				o.Draw(camera.View, camera.Projection, camera.CameraPosition);
			}

		}


		public Level2(Scene previousScene)
			: base(previousScene)
		{
		}
	}
}
