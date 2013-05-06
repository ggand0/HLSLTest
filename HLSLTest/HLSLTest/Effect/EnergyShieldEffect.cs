using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace HLSLTest
{
	public class EnergyShieldEffect
	{
		public static Game1 game;
		public static Level level;

		private Effect effect;
		private ContentManager content;
		private GraphicsDevice graphics;
		private RenderTarget2D reflectionTarg;
		public List<IRenderable> Objects = new List<IRenderable>();

		private Object mesh;
		private float currentRadius;
		private readonly float MAX_RADIUS = 1000;
		private readonly int DEF_RADIUS = 10;
		private Matrix Scale;
		private ExplosionParticleEmitter eps;
		private float speed;

		public void Update(GameTime gameTime)
		{
			float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

			eps.Update();
			//currentRadius += speed * 0.05f;
			if (currentRadius >= MAX_RADIUS) {
				currentRadius = DEF_RADIUS;
			}
			Scale = Matrix.CreateScale(currentRadius);
		}
		public void Draw(GameTime gameTime, Matrix View, Matrix Projection, Vector3 CameraPosition, Vector3 CameraDirection, Vector3 Up, Vector3 Right)
		{
			effect.Parameters["Time"].SetValue((float)gameTime.TotalGameTime.TotalSeconds);
			// 本文にはDraw関数の記述は無かったが、恐らくSkySphere.Drawと同様だろう
			graphics.BlendState = BlendState.AlphaBlend;
			// スケールにベクトルを使用していることに注意
			//discoidMesh.World = Matrix.CreateScale(discoidMesh.ScaleVector) * discoidMesh.RotationMatrix * Matrix.CreateTranslation(discoidMesh.Position);
			mesh.World = Scale * mesh.RotationMatrix * Matrix.CreateTranslation(mesh.Position);
			SetEffectParameters(CameraPosition, CameraDirection);

			// 両面を描画する
			graphics.RasterizerState = RasterizerState.CullCounterClockwise;
			mesh.Draw(View, Projection, CameraPosition);

			Vector3 reflectedCameraPosition = CameraPosition;
			reflectedCameraPosition.Y = -reflectedCameraPosition.Y + mesh.Position.Y * 2;
			Vector3 reflectedCameraTarget = level.camera.ChasePosition;
			reflectedCameraTarget.Y = -reflectedCameraTarget.Y - level.camera.LookAtOffset.Y + mesh.Position.Y * 2;
			TargetCamera reflectionCamera = new TargetCamera(reflectedCameraPosition, reflectedCameraTarget, graphics);
			reflectionCamera.Update();// 上方ベクトルは-Yになってた

			graphics.BlendState = BlendState.Opaque;
			eps.Draw(View, Projection, Up, Right);
		}
		/// <summary>
		/// エフェクトファイル内のパラメータを設定する
		/// </summary>
		/// <param name="CameraPosition"></param>
		/// <param name="CameraDirection"></param>
		private void SetEffectParameters(Vector3 CameraPosition, Vector3 CameraDirection)
		{
			effect.Parameters["CameraPosition"].SetValue(CameraPosition);
			effect.Parameters["CameraDirection"].SetValue(CameraDirection);
			effect.Parameters["CenterToCamera"].SetValue(
				new Vector4(Vector3.Normalize(CameraPosition - mesh.Position), 1));
		}


		public EnergyShieldEffect(ContentManager content, GraphicsDevice graphics,
			Vector3 position, Vector2 size, float scale)
		{
			this.content = content;
			this.graphics = graphics;
			this.Scale = Matrix.CreateScale(scale);
			mesh = new Object(position, "Models\\SkySphereMesh");

			mesh.ScaleVector = new Vector3(size.X, size.Y, 1);// 元々Blenderで作成した素材なので傾いている。それに合わせて、rescaleする成分を調整
			mesh.RotationMatrix = Matrix.Identity * Matrix.CreateRotationZ(MathHelper.ToRadians(90))
				* Matrix.CreateRotationX(MathHelper.ToRadians(-90));

			Texture2D tex = content.Load<Texture2D>("Textures\\Plasma_0");
			//discoidEffect = content.Load<Effect>("Lights\\RimLightingEffectV2");
			effect = content.Load<Effect>("Lights\\EnergyShieldEffect");
			effect.Parameters["RimColor"].SetValue(new Vector4(Color.LightGreen.ToVector3(), 0.05f));
			effect.Parameters["BaseTexture"].SetValue(tex);
			effect.Parameters["RefractionMap"].SetValue(level.Sky.TextureCube);
			effect.Parameters["NormalMap"].SetValue(content.Load<Texture2D>("waterbump"));
			mesh.SetModelEffect(effect, false);


			reflectionTarg = new RenderTarget2D(graphics, graphics.Viewport.Width, graphics.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth24);
			//eps = new ExplosionParticleEmitter(graphics, content, content.Load<Texture2D>("Textures\\nova_2"), position + new Vector3(0, 10, 0), 1000, new Vector2(10), 20, 5f);
			eps = new ExplosionParticleEmitter(graphics, content, content.Load<Texture2D>("Textures\\nova_2"), position, 2000, new Vector2(10), 20, 5f);
			currentRadius = DEF_RADIUS;
			currentRadius = 100;
			speed = eps.Velocity.Length();
		}
	}
}