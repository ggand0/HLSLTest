using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace HLSLTest
{
	public class Level1 : Level
	{
		Water water;
		BillboardSystem trees;
		BillboardSystem clouds;
		Terrain terrain;
		//SphericalTerrain2 sphericalTerrain;

		protected override void Initialize()
		{
			base.Initialize();

			camera = new ArcBallCamera();
			camera.Initialize(game, Vector3.Zero);
			ParticleEmitter.camera = camera;
		}
		public override void Load()
		{
			base.Load();

			PrelightingRenderer.game = game;
			debug = new Debug();

			Sky = new SkySphere(content, device, content.Load<TextureCube>("Textures\\SkyBox\\SkyBoxTex"), 10000);


			terrain = new Terrain(content.Load<Texture2D>("Textures\\Terrain\\terrain"), 100, 8000, -6000,
				content.Load<Texture2D>("Textures\\Terrain\\grass"), 6, new Vector3(1, -1, 0), device, content);
			terrain.WeightMap = content.Load<Texture2D>("Textures\\color1");
			terrain.RTexture = content.Load<Texture2D>("Textures\\Terrain\\sand");
			terrain.GTexture = content.Load<Texture2D>("Textures\\Terrain\\grass");
			terrain.BTexture = content.Load<Texture2D>("Textures\\Terrain\\stone");
			terrain.DetailTexture = content.Load<Texture2D>("Textures\\detail0");


			Water.game = game;
			water = new Water(content, device, new Vector3(0, -1800, -4000), new Vector2(5000, 5000));
			//water = new Water(content, device, new Vector3(0, 0, 0), new Vector2(1000, 1000), renderer);
			water.Objects.Add(Sky);
			water.Objects.Add(terrain);
			//water.Objects.Add(models[0]); water.Objects.Add(models[1]);
			foreach (Object o in Models) {
				water.Objects.Add(o);
			}
			water.Initialize();


			
		}
		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			Sky.Update(gameTime);

			foreach (Object o in Models) {
				o.Update(gameTime);
			}

			//renderer.Update();
			camera.UpdateChaseTarget(Vector3.Zero);
			camera.Update(gameTime);
			water.Update(gameTime);
			//Ground.Update(gameTime);
		}
		public override void Draw(GameTime gameTime)
		{
			base.Draw(gameTime);

			//softParticle.DrawDepth(camera.View, camera.Projection, camera.CameraPosition);
			water.PreDraw(camera, gameTime);// renderer.Drawとの順番に注意　前に行わないとrendererのパラメータを汚してしまう?
			//renderer.Draw();
			device.Clear(Color.Black);

			// Draw terrain
			Sky.Draw(camera.View, camera.Projection, camera.CameraPosition);
			terrain.Draw(false, camera.View, camera.Projection);
			water.Draw(camera.View, camera.Projection, camera.CameraPosition);
			
			//sphericalTerrain.Draw(false, camera.View, camera.Projection);


			//Ground.Model.Draw(Ground.World, camera.View, camera.Projection);
			foreach (Object o in Models) {
				//if (camera.BoundingVolumeIsInView(model.BoundingSphere)) {
				o.Draw(camera.View, camera.Projection, camera.CameraPosition);
			}

		}

		public Level1(Scene previousScene)
			:base(previousScene)
		{
		}
	}
}
