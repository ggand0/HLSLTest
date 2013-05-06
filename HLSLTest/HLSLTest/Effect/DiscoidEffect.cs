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
		public static Level level;

		Effect discoidEffect;
		ContentManager content;
		GraphicsDevice graphics;
		RenderTarget2D reflectionTarg;
		public List<IRenderable> Objects = new List<IRenderable>();

		private Object discoidMesh;
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
			currentRadius += speed * 0.05f * 5;
			if (currentRadius >= MAX_RADIUS) {
				currentRadius = DEF_RADIUS;
			}
			Scale = Matrix.CreateScale(currentRadius);
			//discoidMesh.ScaleVector = new Vector3(currentRadius, currentRadius, 50);
			//discoidMesh.ScaleVector = new Vector3(currentRadius, 10, currentRadius);
		}
		public void Draw(GameTime gameTime, Matrix View, Matrix Projection, Vector3 CameraPosition, Vector3 CameraDirection, Vector3 Up, Vector3 Right)
		{
			//discoidEffect.Parameters["Time"].SetValue((float)gameTime.TotalGameTime.TotalSeconds);

			// 本文にはDraw関数の記述は無かったが、恐らくSkySphere.Drawと同様だろう
			graphics.BlendState = BlendState.AlphaBlend;
			// スケールにベクトルを使用していることに注意
			/*discoidMesh.World = Matrix.CreateScale(discoidMesh.ScaleVector)
				* discoidMesh.RotationMatrix * Matrix.CreateTranslation(discoidMesh.Position);*/
			discoidMesh.World = Scale * discoidMesh.RotationMatrix * Matrix.CreateTranslation(discoidMesh.Position);
			SetEffectParameters(CameraPosition, CameraDirection);


			// 両面を描画する
			graphics.RasterizerState = RasterizerState.CullCounterClockwise;
			discoidMesh.Draw(View, Projection, CameraPosition);

			Vector3 reflectedCameraPosition = CameraPosition;
			reflectedCameraPosition.Y = -reflectedCameraPosition.Y + discoidMesh.Position.Y * 2;
			Vector3 reflectedCameraTarget = level.camera.ChasePosition;
			reflectedCameraTarget.Y = -reflectedCameraTarget.Y - level.camera.LookAtOffset.Y + discoidMesh.Position.Y * 2;
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
			/*float4 AmbientColor;
            float4 DiffuseColor0;// 例外対策
            float4 SpecularColor;
            float4 RimColor;
 
            float AmbientIntensity;
            float DiffuseIntensity;
            float SpecularIntensity;
            float RimIntensity;         // Intensity of the rim light
 
            float3 DiffuseLightDirection;
            float3 CameraPosition;
            float3 CameraDirection;
            float Shinniness;*/



			//discoidEffect.Parameters["ParticleTexture"].SetValue(Texture);
			discoidEffect.Parameters["CameraPosition"].SetValue(CameraPosition);
			discoidEffect.Parameters["CameraDirection"].SetValue(CameraDirection);
			//discoidEffect.Parameters["CenterToCamera"].SetValue(new Vector4(Vector3.Normalize(CameraPosition - discoidMesh.Position), 1));
			discoidEffect.Parameters["CenterToCamera"].SetValue(new Vector4(Vector3.Up, 1));
		}


		public DiscoidEffect(ContentManager content, GraphicsDevice graphics,
			Vector3 position, Vector2 size)
		{
			this.content = content;
			this.graphics = graphics;
			//discoidMesh = new Object(content.Load<Model>("plane"), position, Vector3.Zero, new Vector3(size.X, 1, size.Y), graphics);
			//discoidMesh = new Object(position, "Models\\DiscoidMesh");
			discoidMesh = new Object(position, "Models\\Disk");
			

			//discoidMesh.ScaleVector = new Vector3(size.X, 1, size.Y);
			discoidMesh.ScaleVector = new Vector3(size.X, size.Y, 1);// 元々Blenderで作成した素材なので傾いている。それに合わせて、rescaleする成分を調整
			discoidMesh.RotationMatrix = Matrix.Identity * Matrix.CreateRotationZ(MathHelper.ToRadians(90))
				* Matrix.CreateRotationX(MathHelper.ToRadians(-90));

			/*discoidEffect = content.Load<Effect>("Billboard\\DiscoidEffect");
			discoidMesh.SetModelEffect(discoidEffect, false);
			//discoidEffect.Parameters["viewportWidth"].SetValue(graphics.Viewport.Width);
			//discoidEffect.Parameters["viewportHeight"].SetValue(graphics.Viewport.Height);
			discoidEffect.Parameters["Texture"].SetValue(content.Load<Texture2D>("Textures\\Plasma_0"));//Mask1"));
			discoidEffect.Parameters["Color"].SetValue(Color.LightGreen.ToVector4());*/


			Texture2D tex = content.Load<Texture2D>("Textures\\Plasma_0");
			//discoidEffect = content.Load<Effect>("Lights\\RimLightingEffectV2");
			//discoidEffect = content.Load<Effect>("Lights\\EnergyShieldEffect");
			discoidEffect = content.Load<Effect>("Lights\\EnergyRingEffect");
            //discoidEffect = content.Load<Effect>("Lights\\SimpleEffect");
			//discoidEffect.Parameters["RimColor"].SetValue(Color.LightGreen.ToVector4());
			discoidEffect.Parameters["RimColor"].SetValue(new Vector4(Color.LightGreen.ToVector3(), 0.05f));
			discoidEffect.Parameters["BaseTexture"].SetValue(tex);
			discoidEffect.Parameters["RefractionMap"].SetValue(level.Sky.TextureCube);
			discoidEffect.Parameters["NormalMap"].SetValue(content.Load<Texture2D>("waterbump"));
			discoidEffect.Parameters["AlphaTest"].SetValue(true);
			discoidEffect.Parameters["WaveSpeed"].SetValue(0.1f);
			discoidMesh.SetModelEffect(discoidEffect, false);
            

			reflectionTarg = new RenderTarget2D(graphics, graphics.Viewport.Width, graphics.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth24);

			//eps = new ExplosionParticleEmitter(graphics, content, content.Load<Texture2D>("Textures\\nova_2"), position + new Vector3(0, 10, 0), 1000, new Vector2(10), 20, 5f);
			eps = new ExplosionParticleEmitter(graphics, content, content.Load<Texture2D>("Textures\\Particle\\nova_2"), position, 2000, new Vector2(10), 20, 5f);
			currentRadius = DEF_RADIUS;
			//currentRadius = 100;
			speed = eps.Velocity.Length();
		}
	}
}