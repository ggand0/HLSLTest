using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace HLSLTest
{
	public class DiscoidEffect
	{
		public static Game1 game;
		Effect discoidEffect;
		ContentManager content;
		GraphicsDevice graphics;
		RenderTarget2D reflectionTarg;
		public List<IRenderable> Objects = new List<IRenderable>();

		private Object discoidMesh;
		private float currentRadius;
		private readonly float MAX_RADIUS = 1000;
		private Matrix Scale;
		private ExplosionParticleEmitter eps;

		public void Update()
		{
			eps.Update();
			currentRadius++;
			if (currentRadius >= MAX_RADIUS) {
				currentRadius = 1;
			}

			Scale = Matrix.CreateScale(currentRadius);
		}
		public void Draw(Matrix View, Matrix Projection, Vector3 CameraPosition, Vector3 Up, Vector3 Right)
		{
			// 本文にはDraw関数の記述は無かったが、恐らくSkySphere.Drawと同様だろう
			graphics.BlendState = BlendState.AlphaBlend;
			// スケールにベクトルを使用していることに注意
			//discoidMesh.World = Matrix.CreateScale(discoidMesh.ScaleVector) * discoidMesh.RotationMatrix * Matrix.CreateTranslation(discoidMesh.Position);
			discoidMesh.World = Scale * discoidMesh.RotationMatrix * Matrix.CreateTranslation(discoidMesh.Position);

			// 両面を描画する
			graphics.RasterizerState = RasterizerState.CullCounterClockwise;
			//discoidMesh.Draw(View, Projection, CameraPosition);

			Vector3 reflectedCameraPosition = CameraPosition;
			reflectedCameraPosition.Y = -reflectedCameraPosition.Y + discoidMesh.Position.Y * 2;
			Vector3 reflectedCameraTarget = game.camera.ChasePosition;
			reflectedCameraTarget.Y = -reflectedCameraTarget.Y - game.camera.LookAtOffset.Y + discoidMesh.Position.Y * 2;
			TargetCamera reflectionCamera = new TargetCamera(reflectedCameraPosition, reflectedCameraTarget, graphics);
			reflectionCamera.Update();// 上方ベクトルは-Yになってた
			
			graphics.BlendState = BlendState.Opaque;

			eps.Draw(View, Projection, Up, Right);
		}


		public DiscoidEffect(ContentManager content, GraphicsDevice graphics,
			Vector3 position, Vector2 size)
		{
			this.content = content;
			this.graphics = graphics;
			//discoidMesh = new Object(content.Load<Model>("plane"), position, Vector3.Zero, new Vector3(size.X, 1, size.Y), graphics);
			discoidMesh = new Object(position, "Models\\DiscoidMesh");
			currentRadius = 1;

			//discoidMesh.ScaleVector = new Vector3(size.X, 1, size.Y);
			discoidMesh.ScaleVector = new Vector3(size.X, size.Y, 1);// 元々Blenderで作成した素材なので傾いている。それに合わせて、rescaleする成分を調整
			discoidMesh.RotationMatrix = Matrix.Identity * Matrix.CreateRotationZ(MathHelper.ToRadians(90))
				* Matrix.CreateRotationX(MathHelper.ToRadians(-90));

			discoidEffect = content.Load<Effect>("Billboard\\DiscoidEffect");
			discoidMesh.SetModelEffect(discoidEffect, false);
			//discoidEffect.Parameters["viewportWidth"].SetValue(graphics.Viewport.Width);
			//discoidEffect.Parameters["viewportHeight"].SetValue(graphics.Viewport.Height);


			reflectionTarg = new RenderTarget2D(graphics, graphics.Viewport.Width, graphics.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth24);
			discoidEffect.Parameters["Texture"].SetValue(content.Load<Texture2D>("Textures\\Mask1"));
			discoidEffect.Parameters["Color"].SetValue(Color.LightGreen.ToVector4());
			//eps = new ExplosionParticleEmitter(graphics, content, content.Load<Texture2D>("Textures\\nova_2"), position + new Vector3(0, 10, 0), 1000, new Vector2(10), 20, 5f);
			eps = new ExplosionParticleEmitter(graphics, content, content.Load<Texture2D>("Textures\\nova_2"), Vector3.Zero, 2000, new Vector2(10), 20, 5f);
		}
	}
}
