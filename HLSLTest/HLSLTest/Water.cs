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
		Object waterMesh;
		Effect waterEffect;
		ContentManager content;
		GraphicsDevice graphics;
		RenderTarget2D reflectionTarg;
		public List<IRenderable> Objects = new List<IRenderable>();
		bool hasSaved;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="camera">main camera instance</param>
		public void renderReflection(ArcBallCamera camera)
		{
			// Reflect the camera's properties across the water plane
			Vector3 reflectedCameraPosition = camera.Position;
			reflectedCameraPosition.Y = -reflectedCameraPosition.Y + waterMesh.Position.Y * 2;
			//Vector3 reflectedCameraTarget = (camera).Target;
			Vector3 reflectedCameraTarget = camera.ChasePosition;
			reflectedCameraTarget.Y = -reflectedCameraTarget.Y + waterMesh.Position.Y * 2;

			// Create a temporary camera to render the reflected scene
			TargetCamera reflectionCamera = new TargetCamera(reflectedCameraPosition, reflectedCameraTarget, graphics);
			reflectionCamera.Update();// 上方ベクトルは-Yになってた/**/
			//ArcBallCamera reflectionCamera = new ArcBallCamera(reflectedCameraPosition, reflectedCameraTarget, Vector3.Down);
			// reflectionのviewがおかしいのか...??
			Matrix reflectionMatrix = Matrix.CreateTranslation(0f, -waterMesh.Position.Y, 0f)
				* Matrix.CreateScale(1f, -1f, 1f) * Matrix.CreateTranslation(0f, -waterMesh.Position.Y, 0f);
			Matrix reflectedViewMatrix = reflectionMatrix * camera.View;

			// Set the reflection camera's view matrix to the water effect
			//waterEffect.Parameters["ReflectedView"].SetValue(reflectionCamera.View);
			waterEffect.Parameters["ReflectedView"].SetValue(reflectedViewMatrix);

			// Create the clip plane
			Vector4 clipPlane = new Vector4(0, 1, 0, -waterMesh.Position.Y);// w成分は法線方向を表す？


			// Set the render target
			graphics.SetRenderTarget(reflectionTarg);
			graphics.Clear(Color.Black);
			// Draw all objects with clip plane
			// ここを弄ると面白い演出が出来るかも
			foreach (IRenderable renderable in Objects) {
				renderable.SetClipPlane(clipPlane);

				if (renderable is Object) {
					(renderable as Object).Update(new GameTime());
					
					// light map, shadow mapを作り直す。（めんどくさい）

				}
				// ここでreflectionCameraを設定しているはずなのだが... 
				renderable.Draw(reflectionCamera.View, reflectionCamera.Projection, reflectedCameraPosition);
				//renderable.Draw(camera.View, camera.Projection, camera.CameraPosition);// ここでreflectionCameraを設定しているはずなのだが... 
				//renderable.Draw(reflectedViewMatrix, camera.Projection, reflectedCameraPosition);

				renderable.SetClipPlane(null);
			}
			graphics.SetRenderTarget(null);

			// Set the render target
			//graphics.SetRenderTarget(reflectionTarg);
			graphics.Clear(Color.Black);
			foreach (IRenderable renderable in Objects) {
				renderable.SetClipPlane(clipPlane);

				if (renderable is Object) {
					(renderable as Object).Update(new GameTime());
					//continue;
				}
				// ここでreflectionCameraを設定しているはずなのだが... 
				renderable.Draw(reflectionCamera.View, reflectionCamera.Projection, reflectedCameraPosition);
				//renderable.Draw(camera.View, camera.Projection, camera.CameraPosition);// ここでreflectionCameraを設定しているはずなのだが... 
				//renderable.Draw(reflectedViewMatrix, camera.Projection, reflectedCameraPosition);

				renderable.SetClipPlane(null);
			}
			graphics.SetRenderTarget(null);/**/

			if (!hasSaved) {
				using (Stream stream = File.OpenWrite("reflection_map.png")) {
					reflectionTarg.SaveAsPng(stream, reflectionTarg.Width, reflectionTarg.Height);
					stream.Position = 0;
					//MediaLibrary media = new MediaLibrary();
					//media.SavePicture("shadowDepth.jpg", stream);
					hasSaved = true; // 下でfalseに
				}
			}

			// Set the reflected scene to its effect parameter in
			// the water effect
			waterEffect.Parameters["ReflectionMap"].SetValue(reflectionTarg);
		}


		public void Update()
		{
			if (JoyStick.onStickDirectionChanged) {
				hasSaved = false;
			}
		}
		public void PreDraw(ArcBallCamera camera, GameTime gameTime)
		{
			renderReflection(camera);
		}
		/// <summary>
		/// カスタムエフェクトを使用したDraw
		/// </summary>
		public void Draw(Matrix View, Matrix Projection, Vector3 CameraPosition)
		{
			// 本文にはDraw関数の記述は無かったが、恐らくSkySphere.Drawと同様だろう
			waterMesh.World = Matrix.CreateScale(waterMesh.ScaleVector) * waterMesh.RotationMatrix * Matrix.CreateTranslation(waterMesh.Position);// スケールにベクトルを使用していることに注意
			waterMesh.Draw(View, Projection, CameraPosition);
		}

		public Water(ContentManager content, GraphicsDevice graphics,
			Vector3 position, Vector2 size)
		{
			this.content = content;
			this.graphics = graphics;
			//waterMesh = new Object(content.Load<Model>("plane"), position, Vector3.Zero, new Vector3(size.X, 1, size.Y), graphics);
			waterMesh = new Object(position, "Models\\WaterMesh");
			//Vector3.Zero
			//waterMesh.ScaleVector = new Vector3(size.X, 1, size.Y);
			waterMesh.ScaleVector = new Vector3(size.X, size.Y, 1);// 元々Blenderで作成した素材なので傾いている。それに合わせて、rescaleする成分を調整
			waterMesh.RotationMatrix = Matrix.Identity * Matrix.CreateRotationZ(MathHelper.ToRadians(90))
				* Matrix.CreateRotationX(MathHelper.ToRadians(-90));

			waterEffect = content.Load<Effect>("WaterEffect");
			waterMesh.SetModelEffect(waterEffect, false);
			waterEffect.Parameters["viewportWidth"].SetValue(graphics.Viewport.Width);
			waterEffect.Parameters["viewportHeight"].SetValue(graphics.Viewport.Height);


			reflectionTarg = new RenderTarget2D(graphics, graphics.Viewport.Width,
				graphics.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth24);
		}
	}
	
}
