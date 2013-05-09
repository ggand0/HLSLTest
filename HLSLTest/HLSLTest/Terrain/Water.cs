using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace HLSLTest
{
	public class Water
	{
		public static Game1 game;
		public static Level level;

		Object waterMesh;
		Effect waterEffect;
		ContentManager content;
		GraphicsDevice graphics;
		RenderTarget2D reflectionTarg;
		public List<IRenderable> Objects = new List<IRenderable>();
		bool hasSaved;

		PrelightingRenderer renderer;
		List<Object> models;

		public void Initialize()
		{
			// Objectsのうち、Objectクラスのみのリストを作っておく
			// 厳密に言うと「Prelighting及びShadowMappingを使うエフェクトで描画するオブジェクト」。
			models = new List<Object>();
			foreach (IRenderable renderable in Objects) {
				if (renderable is Object) {
					models.Add(renderable as Object);
				}
			}
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="camera">main camera instance</param>
		public void renderReflection(ArcBallCamera camera)
		{
			// Reflect the camera's properties across the water plane
			Vector3 reflectedCameraPosition = camera.Position;
			reflectedCameraPosition.Y = -reflectedCameraPosition.Y + waterMesh.Position.Y * 2;
			Vector3 reflectedCameraTarget = camera.ChasePosition;
			reflectedCameraTarget.Y = -reflectedCameraTarget.Y - camera.LookAtOffset.Y/**/ + waterMesh.Position.Y * 2;

			// Create a temporary camera to render the reflected scene
			// ArcBallCameraのLookAtOffsetの関係でずれたりするから、出来れば同じ型のクラスのほうがいいだろう
			TargetCamera reflectionCamera = new TargetCamera(reflectedCameraPosition, reflectedCameraTarget, graphics);
			reflectionCamera.Update();// 上方ベクトルは-Yになってた
			//ArcBallCamera reflectionCamera = new ArcBallCamera(reflectedCameraPosition, reflectedCameraTarget, Vector3.Down);
			// reflectionのviewがおかしいのか...??

			// これ、ネットから拾ってきたmatrixだったのを忘れてた
			/*Matrix reflectionMatrix = Matrix.Identity
				//Matrix.CreateTranslation(0f, -waterMesh.Position.Y, 0f) * 
				//Matrix.CreateScale(1f, -1f, 1f)	 
				//Matrix.CreateTranslation(0f, -waterMesh.Position.Y, 0f)
				;
			Matrix reflectedViewMatrix = reflectionMatrix * camera.View;*/


			// Set the reflection camera's view matrix to the water effect
			waterEffect.Parameters["ReflectedView"].SetValue(reflectionCamera.View);
			//waterEffect.Parameters["ReflectedView"].SetValue(reflectedViewMatrix);

			// Create the clip plane
			Vector4 clipPlane = new Vector4(0, 1, 0, -waterMesh.Position.Y);// w成分は法線方向を表す？
			

			// lt, stを使うmodelsのために初期化:light map, shadow mapを作り直す。（めんどくさい）
			//models[1].RotationMatrix = Matrix.Identity * Matrix.CreateRotationZ(MathHelper.ToRadians(90))* Matrix.CreateRotationX(MathHelper.ToRadians(-90));
			// あー分かった、モデルの回転が異常なのではなくて、BlendStateなどがおかしい！（だから透過して見えるので、表面が見えて回転しているようにみえる）
			var depthNormal = renderer.drawDepthNormalMap(models
						//, reflectedViewMatrix, reflectionCamera.Projection, reflectionCamera.Position);
						, reflectionCamera.View, reflectionCamera.Projection, reflectionCamera.Position);
			RenderTarget2D lt = renderer.drawLightMap(models, depthNormal.dt, depthNormal.nt
				, reflectionCamera.View, reflectionCamera.Projection, reflectionCamera.Position);
				//, reflectedViewMatrix, reflectionCamera.Projection, reflectionCamera.Position);
			//RenderTarget2D st = renderer.drawShadowDepthMap(models);
			renderer.drawShadowDepthMap();
			renderer.prepareMainPass(models, lt);

			// Set the render target
			graphics.SetRenderTarget(reflectionTarg);
			graphics.Clear(Color.Black);

			// Draw all objects with clip plane
			// ここを弄ると面白い演出が出来るかも
			foreach (IRenderable renderable in Objects) {
				renderable.SetClipPlane(clipPlane);
				string cullState = graphics.RasterizerState.ToString();

				if (renderable is Object) {
					(renderable as Object).Update(new GameTime());
				}
				// ここでreflectionCameraを設定しているはずなのだが... 
				//renderable.Draw(reflectedViewMatrix, reflectionCamera.Projection, reflectedCameraPosition);
				//renderable.Draw(camera.View, camera.Projection, camera.CameraPosition);// ここでreflectionCameraを設定しているはずなのだが... 
				renderable.Draw(reflectionCamera.View, reflectionCamera.Projection, reflectedCameraPosition);

				renderable.SetClipPlane(null);
			}
			graphics.SetRenderTarget(null);
			graphics.RasterizerState = RasterizerState.CullCounterClockwise;


			/*// Set the render target
			//graphics.SetRenderTarget(reflectionTarg);
			graphics.Clear(Color.Black);
			foreach (IRenderable renderable in Objects) {
				renderable.SetClipPlane(clipPlane);

				if (renderable is Object) {
					(renderable as Object).Update(new GameTime());
					//continue;
				}
				// ここでreflectionCameraを設定しているはずなのだが... 
				//renderable.Draw(reflectedViewMatrix, reflectionCamera.Projection, reflectedCameraPosition);
				//renderable.Draw(camera.View, camera.Projection, camera.CameraPosition);// ここでreflectionCameraを設定しているはずなのだが... 
				renderable.Draw(reflectionCamera.View, reflectionCamera.Projection, reflectedCameraPosition);

				renderable.SetClipPlane(null);
			}*/


			/*if (!hasSaved) {
				using (Stream stream = File.OpenWrite("reflection_map.png")) {
					reflectionTarg.SaveAsPng(stream, reflectionTarg.Width, reflectionTarg.Height);
					stream.Position = 0;
					//MediaLibrary media = new MediaLibrary();
					//media.SavePicture("shadowDepth.jpg", stream);
					hasSaved = true; // 下でfalseに
				}
			}*/

			// Set the reflected scene to its effect parameter in
			// the water effect
			waterEffect.Parameters["ReflectionMap"].SetValue(reflectionTarg);
		}


		public void Update()
		{
			if (JoyStick.onStickDirectionChanged) {
				hasSaved = false;
			}
			renderer.Update();
		}
		public void PreDraw(ArcBallCamera camera, GameTime gameTime)
		{
			renderReflection(camera);
			waterEffect.Parameters["Time"].SetValue((float)gameTime.TotalGameTime.TotalSeconds);
		}
		/// <summary>
		/// カスタムエフェクトを使用したDraw
		/// </summary>
		public void Draw(Matrix View, Matrix Projection, Vector3 CameraPosition)
		{
			graphics.BlendState = BlendState.AlphaBlend;
			//graphics.DepthStencilState = DepthStencilState.None;
			// 本文にはDraw関数の記述は無かったが、恐らくSkySphere.Drawと同様だろう
			waterMesh.World = Matrix.CreateScale(waterMesh.ScaleVector) * waterMesh.RotationMatrix * Matrix.CreateTranslation(waterMesh.Position);// スケールにベクトルを使用していることに注意
			graphics.RasterizerState = RasterizerState.CullClockwise;
			waterMesh.Draw(View, Projection, CameraPosition);
			graphics.RasterizerState = RasterizerState.CullCounterClockwise;
			waterMesh.Draw(View, Projection, CameraPosition);

			graphics.BlendState = BlendState.Opaque;
			graphics.DepthStencilState = DepthStencilState.Default;
		}

		public Water(ContentManager content, GraphicsDevice graphics,
			Vector3 position, Vector2 size)
		{
			this.content = content;
			this.graphics = graphics;
			//waterMesh = new Object(content.Load<Model>("plane"), position, Vector3.Zero, new Vector3(size.X, 1, size.Y), graphics);
			waterMesh = new Object(position, "Models\\WaterMesh");
			//waterMesh.ScaleVector = new Vector3(size.X, 1, size.Y);
			waterMesh.ScaleVector = new Vector3(size.X, size.Y, 1);// 元々Blenderで作成した素材なので傾いている。それに合わせて、rescaleする成分を調整
			waterMesh.RotationMatrix = Matrix.Identity * Matrix.CreateRotationZ(MathHelper.ToRadians(90))
				* Matrix.CreateRotationX(MathHelper.ToRadians(-90));

			waterEffect = content.Load<Effect>("WaterEffectV3");
			waterMesh.SetModelEffect(waterEffect, false);
			waterEffect.Parameters["viewportWidth"].SetValue(graphics.Viewport.Width);
			waterEffect.Parameters["viewportHeight"].SetValue(graphics.Viewport.Height);


			//reflectionTarg = new RenderTarget2D(graphics, graphics.Viewport.Width, graphics.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth24);
			reflectionTarg = new RenderTarget2D(graphics, graphics.Viewport.Width, graphics.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth24);
			//PresentationParameters pp = graphics.PresentationParameters;
			//reflectionTarg = new RenderTarget2D(graphics, pp.BackBufferWidth, pp.BackBufferHeight, false, SurfaceFormat.Color, DepthFormat.Depth24);
			//reflectionTarg = new RenderTarget2D(graphics, 2048, 2048, false, SurfaceFormat.Color, DepthFormat.Depth24);

			waterEffect.Parameters["WaterNormalMap"].SetValue(content.Load<Texture2D>("waterbump"));
			waterEffect.Parameters["Mask"].SetValue(content.Load<Texture2D>("Textures\\Billboard\\cloud_mask2"));


			/**/if (renderer == null) {
				renderer = new PrelightingRenderer(graphics, content);
			}
			renderer = new PrelightingRenderer(graphics, content);
			renderer.Models = level.Models;
			renderer.Lights = new List<PPPointLight>() {
				new PPPointLight(new Vector3(0, 200, 0), Color.White * .85f,//ew Vector3(0, 100, -100),
				20000),
				new PPPointLight(new Vector3(0, -200, 0), Color.White * .85f,//ew Vector3(0, 100, -100),
				20000)/**/
			};
			// setup shadows
			renderer.ShadowLightPosition = new Vector3(500, 500, 0);//new Vector3(1500, 1500, 2000);
			renderer.ShadowLightTarget = new Vector3(0, 300, 0);//new Vector3(0, 150, 0)


			renderer.DoShadowMapping = true;
			renderer.ShadowMult = 0.3f;//0.01f;//0.3f;
		}
		public Water(ContentManager content, GraphicsDevice graphics,
			Vector3 position, Vector2 size, PrelightingRenderer renderer)
			:this(content, graphics, position, size)
		{
			this.renderer = renderer;
		}
	}
	
}
