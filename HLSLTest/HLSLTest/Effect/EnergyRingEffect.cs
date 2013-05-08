using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace HLSLTest
{
	public class EnergyRingEffect
	{
		public static Game1 game;
		public static Level level;

		Effect effect;
		ContentManager content;
		GraphicsDevice graphics;
		RenderTarget2D reflectionTarg;
		public List<IRenderable> Objects = new List<IRenderable>();

		private Object mesh;
		private float currentRadius;
		private readonly float MAX_RADIUS = 1000;
		private readonly int DEF_RADIUS = 10;
		private Matrix Scale;
		private ExplosionParticleEmitter eps;
		private float speed;
		private Vector3 Position;
		private int count;
		private readonly int FREQUENCY = 1000;
		private DateTime start;

		public void Update(GameTime gameTime)
		{
			float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

			//currentRadius += speed * 0.05f * 5;
			currentRadius += speed * 0.005f * 20;

			//if (currentRadius >= MAX_RADIUS) {
			if (count > FREQUENCY) {
				currentRadius = DEF_RADIUS;
				eps.Reset = true;
				count = 0;
				start = DateTime.Now;
			}
			eps.Update();

			Scale = Matrix.CreateScale(currentRadius);
			//discoidMesh.ScaleVector = new Vector3(currentRadius, currentRadius, 50);
			//discoidMesh.ScaleVector = new Vector3(currentRadius, 10, currentRadius);

			mesh.Position = Position;
			count++;
		}
		public bool IsHitWith(Object o)
		{
			//BoundingSphere bs = o.Model.Meshes[0].BoundingSphere;
			BoundingSphere bs = o.transformedBoundingSphere;
			Vector3 normal = Vector3.UnitY;

			Vector3 projectedPoint = Vector3.Zero;
			float distance = 0;
			float projectedRadius = 0;
			// Position, currentRadius;

			/*点pt
			平面p0 = 法線ベクトルpn0、平面上の点pp0
			距離d = (pt-pp0)・pn0*/
			distance = Vector3.Dot((bs.Center - Position), normal);
			//projectedRadius = (float)Math.Sqrt(currentRadius * currentRadius + distance * distance);
			projectedRadius = (float)Math.Sqrt(bs.Radius * bs.Radius + distance * distance);
			projectedPoint = bs.Center + (-normal) * distance;

			float value1 = (float)Math.Abs((float)(projectedPoint - Position).Length());
			float value2 = projectedRadius + currentRadius;
			return value1 < value2;
		}
		public bool IsHitWith(BoundingSphere o)
		{
			//BoundingSphere bs = o.Model.Meshes[0].BoundingSphere;
			BoundingSphere bs = o;
			Vector3 normal = Vector3.UnitY;

			Vector3 projectedPoint = Vector3.Zero;
			float distance = 0;
			float projectedRadius = 0;
			// Position, currentRadius;

			/*点pt
			平面p0 = 法線ベクトルpn0、平面上の点pp0
			距離d = (pt-pp0)・pn0*/
			distance = Vector3.Dot((bs.Center - Position), normal);
			//projectedRadius = (float)Math.Sqrt(currentRadius * currentRadius + distance * distance);
			projectedRadius = (float)Math.Sqrt(bs.Radius * bs.Radius + distance * distance);
			projectedPoint = bs.Center + (-normal) * distance;

			float value1 = (float)Math.Abs((float)(projectedPoint - Position).Length());
			float value2 = projectedRadius + currentRadius;
			return value1 < value2;
		}

		public void Draw(GameTime gameTime, Matrix View, Matrix Projection, Vector3 CameraPosition, Vector3 CameraDirection, Vector3 Up, Vector3 Right)
		{
			//effect.Parameters["Time"].SetValue((float)gameTime.TotalGameTime.TotalSeconds);
			effect.Parameters["Time"].SetValue((float)(DateTime.Now - start).TotalSeconds);

			// 本文にはDraw関数の記述は無かったが、恐らくSkySphere.Drawと同様だろう
			graphics.BlendState = BlendState.AlphaBlend;
			// スケールにベクトルを使用していることに注意
			/*discoidMesh.World = Matrix.CreateScale(discoidMesh.ScaleVector)
				* discoidMesh.RotationMatrix * Matrix.CreateTranslation(discoidMesh.Position);*/
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
			effect.Parameters["CameraPosition"].SetValue(CameraPosition);
			effect.Parameters["CameraDirection"].SetValue(CameraDirection);
			//discoidEffect.Parameters["CenterToCamera"].SetValue(new Vector4(Vector3.Normalize(CameraPosition - discoidMesh.Position), 1));
			effect.Parameters["CenterToCamera"].SetValue(new Vector4(Vector3.Up, 1));
		}


		public EnergyRingEffect(ContentManager content, GraphicsDevice graphics,
			Vector3 position, Vector2 size)
		{
			this.content = content;
			this.graphics = graphics;
			//discoidMesh = new Object(content.Load<Model>("plane"), position, Vector3.Zero, new Vector3(size.X, 1, size.Y), graphics);
			//discoidMesh = new Object(position, "Models\\DiscoidMesh");
			mesh = new Object(position, "Models\\Disk");
			this.Position = position;
			

			//discoidMesh.ScaleVector = new Vector3(size.X, 1, size.Y);
			mesh.ScaleVector = new Vector3(size.X, size.Y, 1);// 元々Blenderで作成した素材なので傾いている。それに合わせて、rescaleする成分を調整
			mesh.RotationMatrix = Matrix.Identity * Matrix.CreateRotationZ(MathHelper.ToRadians(90))
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
			effect = content.Load<Effect>("Lights\\EnergyRingEffect");
            //discoidEffect = content.Load<Effect>("Lights\\SimpleEffect");


			//effect.Parameters["RimColor"].SetValue(new Vector4(Color.LightGreen.ToVector3(), 0.05f));
			effect.Parameters["RimColor"].SetValue(new Vector4(Color.LightBlue.ToVector3(), 0.05f));
			effect.Parameters["BaseTexture"].SetValue(tex);
			effect.Parameters["RefractionMap"].SetValue(level.Sky.TextureCube);
			effect.Parameters["NormalMap"].SetValue(content.Load<Texture2D>("waterbump"));
			effect.Parameters["AlphaTest"].SetValue(true);
			effect.Parameters["WaveSpeed"].SetValue(0.1f);

			// Determine the start time
			start = DateTime.Now;
			float startTime = (float)(DateTime.Now - start).TotalSeconds;
			effect.Parameters["StartTime"].SetValue(startTime);
			effect.Parameters["LifeSpan"].SetValue(FREQUENCY / 60.0f);
			mesh.SetModelEffect(effect, false);
			mesh.RenderBoudingSphere = false;
            

			reflectionTarg = new RenderTarget2D(graphics, graphics.Viewport.Width, graphics.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth24);

			//eps = new ExplosionParticleEmitter(graphics, content, content.Load<Texture2D>("Textures\\nova_2"), position + new Vector3(0, 10, 0), 1000, new Vector2(10), 20, 5f);
			//eps = new ExplosionParticleEmitter(graphics, content, content.Load<Texture2D>("Textures\\Particle\\nova_2"), position, 2000, new Vector2(10), 20, 5f);
			eps = new ExplosionParticleEmitter(graphics, content, content.Load<Texture2D>("Textures\\Particle\\nova_2"), position, 2000, new Vector2(10), FREQUENCY / 60.0f, 5f);
			currentRadius = DEF_RADIUS;
			//currentRadius = 100;
			speed = eps.Velocity.Length();
		}
	}
}