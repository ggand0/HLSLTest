//#define DEBUG_MODE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;

namespace HLSLTest
{
	public class PrelightingRenderer
	{
		public static Game1 game;
		public static Level level;

		// Normal, depth, and light map render targets
		RenderTarget2D depthTarg;
		RenderTarget2D normalTarg;
		RenderTarget2D lightTarg;

		// Depth/normal effect and light mapping effect
		Effect depthNormalEffect;
		Effect lightingEffect;

		/// <summary>
		/// Point light (sphere) mesh
		/// </summary>
		Model lightMesh;

		// List of models, lights, and the camera
		public List<Object> Models { get; set; }
		public List<PointLight> Lights { get; set; }
		public ArcBallCamera Camera { get; set; }
		GraphicsDevice graphicsDevice;
		int viewWidth = 0, viewHeight = 0;


		// shadow関係
		// Position and target of the shadowing light
		public Vector3 ShadowLightPosition { get; set; }
		public Vector3 ShadowLightTarget { get; set; }
		// Shadow depth target and depth-texture effect
		RenderTarget2D shadowDepthTarg;
		Effect shadowDepthEffect;
		// Depth texture parameters
		int shadowMapSize = 2048;
		//int shadowFarPlane = 10000;//10000;
		int shadowFarPlane = 2000;//10000;
		// Shadow light view and projection
		Matrix shadowView, shadowProjection;
		// Shadow properties
		public bool DoShadowMapping { get; set; }
		public float ShadowMult { get; set; }

		// VSM関係
		SpriteBatch spriteBatch;
		RenderTarget2D shadowBlurTarg;
		Effect shadowBlurEffect;



		QuadRenderer quadRenderer;
		/// <summary>
		/// Our frustum corners in world space
		/// </summary>
		private Vector3[] _cornersWorldSpace = new Vector3[8];
		/// <summary>
		/// Our frustum corners in view space
		/// </summary>
		private Vector3[] _cornersViewSpace = new Vector3[8];
		/// <summary>
		/// Compute the frustum corners for a camera.
		/// Its used to reconstruct the pixel position using only the depth value.
		/// Read here for more information
		/// http://mynameismjp.wordpress.com/2009/03/10/reconstructing-position-from-depth/
		/// </summary>
		/// <param name="camera"> Current rendering camera </param>
		private void ComputeFrustumCorners(ArcBallCamera camera)
		{
			camera.Frustum.GetCorners(_cornersWorldSpace);
			//Matrix matView = camera.EyeTransform; //this is the inverse of our camera transform
			Matrix matView = camera.View; //this is the inverse of our camera transform
			Vector3.Transform(_cornersWorldSpace, ref matView, _cornersViewSpace); //put the frustum into view space
			for (int i = 0; i < 4; i++) //take only the 4 farthest points
            {
				_currentFrustumCorners[i] = _cornersViewSpace[i + 4];
			}
			Vector3 temp = _currentFrustumCorners[3];
			_currentFrustumCorners[3] = _currentFrustumCorners[2];
			_currentFrustumCorners[2] = temp;
		}
		/// <summary>
		/// Our final corners, the 4 farthest points on the view space frustum
		/// </summary>
		private Vector3[] _currentFrustumCorners = new Vector3[4];
		/// <summary>
		/// This method computes the frustum corners applied to a quad that can be smaller than
		/// our screen. This is useful because instead of drawing a full-screen quad for each
		/// point light, we can draw smaller quads that fit the light's bounding sphere in screen-space,
		/// avoiding unecessary pixel shader operations
		/// </summary>
		/// <param name="effect">The effect we want to apply those corners</param>
		/// <param name="topLeftVertex"> The top left vertex, in screen space [-1..1]</param>
		/// <param name="bottomRightVertex">The bottom right vertex, in screen space [-1..1]</param>
		private void ApplyFrustumCorners(Effect effect, Vector2 topLeftVertex, Vector2 bottomRightVertex)
		{
			float dx = _currentFrustumCorners[1].X - _currentFrustumCorners[0].X;
			float dy = _currentFrustumCorners[0].Y - _currentFrustumCorners[2].Y;

			Vector3[] _localFrustumCorners = new Vector3[4];
			_localFrustumCorners[0] = _currentFrustumCorners[2];
			_localFrustumCorners[0].X += dx * (topLeftVertex.X * 0.5f + 0.5f);
			_localFrustumCorners[0].Y += dy * (bottomRightVertex.Y * 0.5f + 0.5f);

			_localFrustumCorners[1] = _currentFrustumCorners[2];
			_localFrustumCorners[1].X += dx * (bottomRightVertex.X * 0.5f + 0.5f);
			_localFrustumCorners[1].Y += dy * (bottomRightVertex.Y * 0.5f + 0.5f);

			_localFrustumCorners[2] = _currentFrustumCorners[2];
			_localFrustumCorners[2].X += dx * (topLeftVertex.X * 0.5f + 0.5f);
			_localFrustumCorners[2].Y += dy * (topLeftVertex.Y * 0.5f + 0.5f);

			_localFrustumCorners[3] = _currentFrustumCorners[2];
			_localFrustumCorners[3].X += dx * (bottomRightVertex.X * 0.5f + 0.5f);
			_localFrustumCorners[3].Y += dy * (topLeftVertex.Y * 0.5f + 0.5f);

			effect.Parameters["FrustumCorners"].SetValue(_localFrustumCorners);
		}

		private void DrawDepthNormalMap()
		{
			// Set the render targets to 'slots' 1 and 2
			graphicsDevice.SetRenderTargets(normalTarg, depthTarg);

			// Clear the render target to 1 (infinite depth)
			graphicsDevice.Clear(Color.White);
			graphicsDevice.BlendState = BlendState.Opaque;

			// Draw each model with the PPDepthNormal effect
			// 法線マップと深度マップをDrawするエフェクトをセットしてそれぞれ書き込む
			foreach (Object o in Models) {
				o.CacheEffects();// すでにあるエフェクトを上書きさせないために退避させておく
				o.SetModelEffect(depthNormalEffect, false);// 空いたスペースで法線マップをDrawする
				o.Draw(level.camera.View, level.camera.Projection,
					level.camera.CameraPosition);
				o.RestoreEffects();// 退避させておいたエフェクトを戻す
			}

			// Un-set the render targets
			graphicsDevice.SetRenderTargets(null);

			if (JoyStick.IsOnKeyDown(8)) {
				using (Stream stream = File.OpenWrite("lighting_normalmap.png")) {
					normalTarg.SaveAsPng(stream, normalTarg.Width, normalTarg.Height);
					stream.Position = 0;
				}
				using (Stream stream = File.OpenWrite("lighting_depthmap.png")) {
					depthTarg.SaveAsPng(stream, depthTarg.Width, depthTarg.Height);
					stream.Position = 0;
				}
			}
		}
		private void DrawLightMap()
		{
			// Set the depth and normal map info to the effect
			lightingEffect.Parameters["DepthTexture"].SetValue(depthTarg);
			lightingEffect.Parameters["NormalTexture"].SetValue(normalTarg);

			// Calculate the view * projection matrix
			Matrix viewProjection = Camera.View * Camera.Projection;

			// Set the inverse of the view * projection matrix to the effect
			Matrix invViewProjection = Matrix.Invert(viewProjection);
			lightingEffect.Parameters["InvViewProjection"].SetValue(invViewProjection);

			// Set the render target to the graphics device
			graphicsDevice.SetRenderTarget(lightTarg);

			// Clear the render target to black (no light)
			graphicsDevice.Clear(Color.Transparent);// transparent black!

			// Set the render target to the graphics device
			/*graphicsDevice.SetRenderTarget(lightTarg);

			// Clear the render target to black (no light)
			graphicsDevice.Clear(Color.Black);*/

			// Set render states to additive (lights will add their influences)
			graphicsDevice.BlendState = BlendState.Additive;
			graphicsDevice.DepthStencilState = DepthStencilState.None;


			//ComputeFrustumCorners(level.camera);
			foreach (PointLight light in Lights) {
				// Set the light's parameters to the effect
				light.SetEffectParameters(lightingEffect);

				// Calculate the world * view * projection matrix and set it to the effect
				Matrix wvp = (Matrix.CreateScale(light.Attenuation)
					* Matrix.CreateTranslation(light.Position)) * viewProjection;
				lightingEffect.Parameters["WorldViewProjection"].SetValue(wvp);

				// Determine the distance between the light and camera
				//float dist = Vector3.Distance(((FreeCamera)Camera).Position,
				float dist = Vector3.Distance(Camera.Position, light.Position);

				// If the camera is inside the light-sphere, invert the cull mode
				// to draw the inside of the sphere instead of the outside
				if (dist < light.Attenuation) {
					graphicsDevice.RasterizerState = RasterizerState.CullClockwise;
					//graphicsDevice.RasterizerState = RasterizerState.CullNone;
				} else {
					string debug = "";
				}


				// Draw the point-light-sphere
				// Additiveにしてるから個々の光源の色が加算される？
				lightMesh.Meshes[0].Draw();// ここでライトマップを作成する（重要）
				//lightingEffect.CurrentTechnique.Passes[0].Apply();
				//quadRenderer.RenderQuad(graphicsDevice, -Vector2.One, Vector2.One);


				// Revert the cull mode
				graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
			}
			// Un-set the render target
			graphicsDevice.SetRenderTarget(null);

			// debug
			/*foreach (PointLight light in Lights) {
				// Set the light's parameters to the effect
				light.SetEffectParameters(lightingEffect);
				// Calculate the world * view * projection matrix and set it to the effect
				Matrix wvp = (Matrix.CreateScale(light.Attenuation)
					* Matrix.CreateTranslation(light.Position)) * viewProjection;
				lightingEffect.Parameters["WorldViewProjection"].SetValue(wvp);
				// Determine the distance between the light and camera
				//float dist = Vector3.Distance(((FreeCamera)Camera).Position,
				float dist = Vector3.Distance(Camera.Position, light.Position);

				// If the camera is inside the light-sphere, invert the cull mode
				// to draw the inside of the sphere instead of the outside
				if (dist < light.Attenuation) {
					graphicsDevice.RasterizerState = RasterizerState.CullClockwise;
					//graphicsDevice.RasterizerState = RasterizerState.CullNone;
				}
				// Draw the point-light-sphere
				// Additiveにしてるから個々の光源の色が加算される？
				lightMesh.Meshes[0].Draw();// ここでライトマップを作成する（重要）
				//lightingEffect.CurrentTechnique.Passes[0].Apply();
				//quadRenderer.RenderQuad(graphicsDevice, -Vector2.One, Vector2.One);
				graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
			}*/

			/*graphicsDevice.BlendState = BlendState.Opaque;
			graphicsDevice.DepthStencilState = DepthStencilState.None;
			graphicsDevice.RasterizerState = RasterizerState.CullNone;
			foreach (PointLight light in Lights) {
				// Set the light's parameters to the effect
				light.SetEffectParameters(lightingEffect);

				// Calculate the world * view * projection matrix and set it to the effect
				Matrix wvp = (Matrix.CreateScale(light.Attenuation)
					* Matrix.CreateTranslation(light.Position)) * viewProjection;
				lightingEffect.Parameters["WorldViewProjection"].SetValue(wvp);

				// Determine the distance between the light and camera
				//float dist = Vector3.Distance(((FreeCamera)Camera).Position,
				float dist = Vector3.Distance(Camera.Position, light.Position);


				// Draw the point-light-sphere
				// Additiveにしてるから個々の光源の色が加算される？
				//lightMesh.Meshes[0].Draw();// ここでライトマップを作成する（重要）
				//lightingEffect.CurrentTechnique.Passes[0].Apply();
				//quadRenderer.RenderQuad(graphicsDevice, -Vector2.One, Vector2.One);

				Vector2 bottomLeftCorner, topRightCorner, size;
				//compute a screen-space quad that fits the light's bounding sphere
				level.camera.ProjectBoundingSphereOnScreen(lightMesh.Meshes[0].BoundingSphere, out bottomLeftCorner, out size);
				bottomLeftCorner.Y = -bottomLeftCorner.Y - size.Y;
				topRightCorner = bottomLeftCorner + size;
				//clamp them
				bottomLeftCorner.X = Math.Max(bottomLeftCorner.X, -1);
				bottomLeftCorner.Y = Math.Max(bottomLeftCorner.Y, -1);
				topRightCorner.X = Math.Min(topRightCorner.X, 1);
				topRightCorner.Y = Math.Min(topRightCorner.Y, 1);
				//apply our frustum corners to this effect. Use the computed view-space rect
				ApplyFrustumCorners(lightingEffect, bottomLeftCorner, topRightCorner);
				lightingEffect.CurrentTechnique.Passes[0].Apply();
				//render the computed view-space quad
				quadRenderer.RenderQuad(graphicsDevice, bottomLeftCorner, topRightCorner);


				// Revert the cull mode
				graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
			}*/

			if (JoyStick.IsOnKeyDown(8)) {
				using (Stream stream = File.OpenWrite("lightmap.png")) {
					lightTarg.SaveAsPng(stream, lightTarg.Width, lightTarg.Height);
					stream.Position = 0;
				}
			}/**/

			// Revert the blending and depth render states
			graphicsDevice.BlendState = BlendState.Opaque;
			graphicsDevice.DepthStencilState = DepthStencilState.Default;
		}
		private void PrepareMainPass()
		{
			foreach (Object o in Models)
				foreach (ModelMesh mesh in o.Model.Meshes)
					foreach (ModelMeshPart part in mesh.MeshParts) {
						// Set the light map and viewport parameters to each model's effect
						if (part.Effect.Parameters["LightTexture"] != null)
							part.Effect.Parameters["LightTexture"].SetValue(lightTarg);// ライトマップをセット
						if (part.Effect.Parameters["viewportWidth"] != null)
							part.Effect.Parameters["viewportWidth"].SetValue(viewWidth);
						if (part.Effect.Parameters["viewportHeight"] != null)
							part.Effect.Parameters["viewportHeight"].
							SetValue(viewHeight);

						// shadow関係
						if (part.Effect.Parameters["DoShadowMapping"] != null)
							part.Effect.Parameters["DoShadowMapping"].SetValue(DoShadowMapping);
						if (!DoShadowMapping) continue;
						if (part.Effect.Parameters["ShadowMap"] != null)
							part.Effect.Parameters["ShadowMap"].SetValue(shadowDepthTarg);// シャドウマップをセット
						if (part.Effect.Parameters["ShadowView"] != null)
							part.Effect.Parameters["ShadowView"].SetValue(shadowView);
						if (part.Effect.Parameters["ShadowProjection"] != null)
							part.Effect.Parameters["ShadowProjection"].SetValue(shadowProjection);

						// shadow bias関係
						if (part.Effect.Parameters["ShadowLightPosition"] != null)
							part.Effect.Parameters["ShadowLightPosition"].
							SetValue(ShadowLightPosition);
						if (part.Effect.Parameters["ShadowFarPlane"] != null)
							part.Effect.Parameters["ShadowFarPlane"].SetValue(shadowFarPlane);
						if (part.Effect.Parameters["ShadowMult"] != null)
							part.Effect.Parameters["ShadowMult"].SetValue(ShadowMult);
					}
		}
		public void DrawShadowDepthMap()
		{
			// Calculate view and projection matrices for the "light"
			// shadows are being calculated for
			//shadowView = Matrix.CreateLookAt(ShadowLightPosition, ShadowLightTarget, Vector3.Up);
			//ShadowLightPosition = new Vector3(150, 15, 0);
			shadowView = Matrix.CreateLookAt(ShadowLightPosition, ShadowLightTarget, Vector3.Up);//Vector3.UnitX);

			shadowProjection = Matrix.CreatePerspectiveFieldOfView(
				MathHelper.ToRadians(90), 1, 1, shadowFarPlane);//45

#if DEBUG_MODE
			// Set render target
			graphicsDevice.SetRenderTarget(shadowDepthTarg);
			// Clear the render target to 1 (infinite depth)
			graphicsDevice.Clear(Color.White);
			// Draw each model with the ShadowDepthEffect effect
			foreach (Object o in Models) {
				o.CacheEffects();
				o.SetModelEffect(shadowDepthEffect, false);
				o.Draw(shadowView, shadowProjection, ShadowLightPosition);
				//o.Draw();
				o.RestoreEffects();
			}
			// Un-set the render targets
			graphicsDevice.SetRenderTarget(null);

			// シャドウマップを直で描画してデバッグできるようni
			// Clear the render target to 1 (infinite depth)
			graphicsDevice.Clear(Color.White);
			// Draw each model with the ShadowDepthEffect effect
			foreach (Object o in Models) {
				o.CacheEffects();
				o.SetModelEffect(shadowDepthEffect, false);
				o.Draw(shadowView, shadowProjection, ShadowLightPosition);
				//o.Draw();
				o.RestoreEffects();
			}
#else
			// Set render target
			graphicsDevice.SetRenderTarget(shadowDepthTarg);

			// Clear the render target to 1 (infinite depth)
			graphicsDevice.Clear(Color.White);

			// Draw each model with the ShadowDepthEffect effect
			foreach (Object o in Models) {
				o.CacheEffects();
				o.SetModelEffect(shadowDepthEffect, false);
				o.Draw(shadowView, shadowProjection, ShadowLightPosition);// これじゃーーーん
				//o.Draw();
				o.RestoreEffects();
			}

			// Un-set the render targets
			graphicsDevice.SetRenderTarget(null);
#endif

			/*if (JoyStick.IsOnKeyDown(8)) {
				using (Stream stream = File.OpenWrite("shadowDepth.png")) {
					shadowDepthTarg.SaveAsPng(stream, shadowDepthTarg.Width, shadowDepthTarg.Height);
					stream.Position = 0;
					hasSaved = true;
				}
			}*/
		}
		private void BlurShadow(RenderTarget2D to, RenderTarget2D from, int dir)
		{
			// Set the target render target
			graphicsDevice.SetRenderTarget(to);
			graphicsDevice.Clear(Color.Black);

			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
			// Start the Gaussian blur effect
			shadowBlurEffect.CurrentTechnique.Passes[dir].Apply();
			// Draw the contents of the source render target so they can
			// be blurred by the gaussian blur pixel shader
			spriteBatch.Draw(from, Vector2.Zero, Color.White);
			spriteBatch.End();

			// Clean up after the sprite batch
			graphicsDevice.BlendState = BlendState.Opaque;
			graphicsDevice.DepthStencilState = DepthStencilState.Default;
			graphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

			// Remove the render target
			graphicsDevice.SetRenderTarget(null);

			/*if (dir == 1 && JoyStick.IsOnKeyDown(8)) {// 2パス目時にだけ出力
				using (Stream stream = File.OpenWrite("bluredShadowDepth.png")) {
					shadowDepthTarg.SaveAsPng(stream, shadowDepthTarg.Width, shadowDepthTarg.Height);
					stream.Position = 0;
					hasSaved = true;
				}
			}*/
		}

		// 外部からLMやSMを、指定のビュー行列を使って作成したい場合に使う
		#region For building reflection map
		// dynamic→voidへ変更。面倒でも参照渡しさせた方が良い。
		public void drawDepthNormalMap(List<Object> models, RenderTarget2D nt, RenderTarget2D dt/**/
			, Matrix view, Matrix projection, Vector3 cameraPos)
		{
			//RenderTarget2D nt = new RenderTarget2D(graphicsDevice, viewWidth, viewHeight, false, SurfaceFormat.Color, DepthFormat.Depth24);
			//RenderTarget2D dt = new RenderTarget2D(graphicsDevice, viewWidth, viewHeight, false, SurfaceFormat.Single, DepthFormat.Depth24);
			//nt = new RenderTarget2D(graphicsDevice, viewWidth, viewHeight, false, SurfaceFormat.Color, DepthFormat.Depth24);
			//dt = new RenderTarget2D(graphicsDevice, viewWidth, viewHeight, false, SurfaceFormat.Single, DepthFormat.Depth24);

			graphicsDevice.DepthStencilState = DepthStencilState.Default;
			// Set the render targets to 'slots' 1 and 2
			graphicsDevice.SetRenderTargets(nt, dt);
			
			// Clear the render target to 1 (infinite depth)
			graphicsDevice.Clear(Color.White);
			// Draw each model with the PPDepthNormal effect
			// 法線マップと深度マップをDrawするエフェクトをセットしてそれぞれ書き込む
			foreach (Object o in models) {
				o.CacheEffects();// すでにあるエフェクトを上書きさせないために退避させておく
				o.SetModelEffect(depthNormalEffect, false);// 空いたエフェクトのスペースで法線マップをDrawする
				//o.Draw(_gameInstance.camera.View, _gameInstance.camera.Projection, _gameInstance.camera.CameraPosition);
				o.Draw(view, projection, cameraPos);
				o.RestoreEffects();// 退避させておいたエフェクトを戻す
			}
			// Un-set the render targets
			graphicsDevice.SetRenderTargets(null);
			
			/*if (!hasSaved) {
				using (Stream stream = File.OpenWrite("reflected lighdepthtmap.png")) {
					dt.SaveAsPng(stream, dt.Width, dt.Height);
					stream.Position = 0;
					hasSaved = true;
				}
			}*/

			/*return new {
				dt, nt
			};*/
		}
		public RenderTarget2D drawLightMap(List<Object> models, RenderTarget2D dt, RenderTarget2D nt
			, Matrix view, Matrix projection, Vector3 cameraPos)
		{
			RenderTarget2D lt = new RenderTarget2D(graphicsDevice, viewWidth, viewHeight, false, SurfaceFormat.Color, DepthFormat.Depth24);

			// Set the depth and normal map info to the effect
			lightingEffect.Parameters["DepthTexture"].SetValue(dt);
			lightingEffect.Parameters["NormalTexture"].SetValue(nt);
			// Calculate the view * projection matrix
			//Matrix viewProjection = Camera.View * Camera.Projection;
			Matrix viewProjection = view * projection;
			// Set the inverse of the view * projection matrix to the effect
			Matrix invViewProjection = Matrix.Invert(viewProjection);
			lightingEffect.Parameters["InvViewProjection"].SetValue(invViewProjection);

			// Set the render target to the graphics device
			graphicsDevice.SetRenderTarget(lt);
			// Clear the render target to black (no light)
			graphicsDevice.Clear(Color.Black);
			// Set render states to additive (lights will add their influences)
			graphicsDevice.BlendState = BlendState.Additive;
			graphicsDevice.DepthStencilState = DepthStencilState.None;
			
			foreach (PointLight light in Lights) {
				// Set the light's parameters to the effect
				light.SetEffectParameters(lightingEffect);
				// Calculate the world * view * projection matrix and set it to the effect
				Matrix wvp = (Matrix.CreateScale(light.Attenuation) * Matrix.CreateTranslation(light.Position)) * viewProjection;
					lightingEffect.Parameters["WorldViewProjection"].SetValue(wvp);
				// Determine the distance between the light and camera
				//float dist = Vector3.Distance(Camera.Position, light.Position);
				float dist = Vector3.Distance(cameraPos, light.Position);

				// If the camera is inside the light-sphere, invert the cull mode
				// to draw the inside of the sphere instead of the outside
				if (dist < light.Attenuation) {
					graphicsDevice.RasterizerState = RasterizerState.CullClockwise;
					//graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
				}
				// Draw the point-light-sphere
				// Additiveにしてるから個々の光源の色が加算される？
				lightMesh.Meshes[0].Draw();// ここでライトマップを作成する（重要）
				// Revert the cull mode
				graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
				//graphicsDevice.RasterizerState = RasterizerState.CullClockwise;
			}

			// Revert the blending and depth render states
			graphicsDevice.BlendState = BlendState.Opaque;
			graphicsDevice.DepthStencilState = DepthStencilState.Default;
			//graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
			// Un-set the render target
			graphicsDevice.SetRenderTarget(null);

			/*if (!hasSaved) {
				using (Stream stream = File.OpenWrite("reflected lightmap.png")) {
					lt.SaveAsPng(stream, lt.Width, lt.Height);
					stream.Position = 0;
					//hasSaved = true;
				}
			}*/

			return lt;
		}
		public RenderTarget2D drawShadowDepthMap(List<Object> models)
		{
			RenderTarget2D st = new RenderTarget2D(graphicsDevice, shadowMapSize, shadowMapSize, false, SurfaceFormat.HalfVector2, DepthFormat.Depth24);
			shadowView = Matrix.CreateLookAt(ShadowLightPosition, ShadowLightTarget, Vector3.Up);
			shadowProjection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(90), 1, 1, shadowFarPlane);//45
			// Set render target
			graphicsDevice.SetRenderTarget(st);
			// Clear the render target to 1 (infinite depth)
			graphicsDevice.Clear(Color.White);
			// Draw each model with the ShadowDepthEffect effect
			foreach (Object o in models) {
				o.CacheEffects();
				o.SetModelEffect(shadowDepthEffect, false);
				o.Draw(shadowView, shadowProjection, ShadowLightPosition);
				o.RestoreEffects();
			}
			// Un-set the render targets
			graphicsDevice.SetRenderTarget(null);

			/*if (!hasSaved) {
				using (Stream stream = File.OpenWrite("reflected shadowmap.png")) {
					st.SaveAsPng(stream, st.Width, st.Height);
					stream.Position = 0;
					hasSaved = true;
				}
			}*/

			return st;
		}
		public void prepareMainPass(List<Object> models, RenderTarget2D lt)//, RenderTarget2D st)
		{
			foreach (Object o in models)
				foreach (ModelMesh mesh in o.Model.Meshes)
					foreach (ModelMeshPart part in mesh.MeshParts) {
						// Set the light map and viewport parameters to each model's effect
						if (part.Effect.Parameters["LightTexture"] != null)
							part.Effect.Parameters["LightTexture"].SetValue(lt);// ライトマップをセット
						if (part.Effect.Parameters["viewportWidth"] != null)
							part.Effect.Parameters["viewportWidth"].SetValue(viewWidth);
						if (part.Effect.Parameters["viewportHeight"] != null)
							part.Effect.Parameters["viewportHeight"].
							SetValue(viewHeight);

						// shadow関係
						if (part.Effect.Parameters["DoShadowMapping"] != null)
							part.Effect.Parameters["DoShadowMapping"].SetValue(DoShadowMapping);
						if (!DoShadowMapping) continue;
						if (part.Effect.Parameters["ShadowMap"] != null)
							part.Effect.Parameters["ShadowMap"].SetValue(shadowDepthTarg);
						if (part.Effect.Parameters["ShadowView"] != null)
							part.Effect.Parameters["ShadowView"].SetValue(shadowView);
						if (part.Effect.Parameters["ShadowProjection"] != null)
							part.Effect.Parameters["ShadowProjection"].SetValue(shadowProjection);

						// shadow bias関係
						if (part.Effect.Parameters["ShadowLightPosition"] != null)
							part.Effect.Parameters["ShadowLightPosition"].
							SetValue(ShadowLightPosition);
						if (part.Effect.Parameters["ShadowFarPlane"] != null)
							part.Effect.Parameters["ShadowFarPlane"].SetValue(shadowFarPlane);
						if (part.Effect.Parameters["ShadowMult"] != null)
							part.Effect.Parameters["ShadowMult"].SetValue(ShadowMult);
					}
		}
		#endregion

		public void Update(GameTime gameTime)
		{
			foreach (PointLight light in Lights) {
				light.Update(gameTime);
			}

			ShadowLightPosition = Lights[0].Position;
			//ShadowLightTarget = (Lights[0] as PointLightCircle).Center;
		}
		public void PreDraw()
		{
			DrawDepthNormalMap();
			DrawLightMap();
			if (DoShadowMapping) {
				DrawShadowDepthMap();
				BlurShadow(shadowBlurTarg, shadowDepthTarg, 0);
				BlurShadow(shadowDepthTarg, shadowBlurTarg, 1);// shadowDepthTagに戻す
			}/**/
			PrepareMainPass();
		}
		/// <summary>
		/// LightsのBoundingSphereを描画するだけ
		/// </summary>
		public void Draw(GameTime gameTime)
		{
			foreach (PointLight p in Lights) {
				p.DrawBoundingSphere();
			}
		}

		public PrelightingRenderer(GraphicsDevice GraphicsDevice, ContentManager Content)
		{
			viewWidth = GraphicsDevice.Viewport.Width;
			viewHeight = GraphicsDevice.Viewport.Height;
			// Create the three render targets
			depthTarg = new RenderTarget2D(GraphicsDevice, viewWidth,
				viewHeight, false, SurfaceFormat.Single, DepthFormat.Depth24);
			normalTarg = new RenderTarget2D(GraphicsDevice, viewWidth,
				viewHeight, false, SurfaceFormat.Color, DepthFormat.Depth24);
				lightTarg = new RenderTarget2D(GraphicsDevice, viewWidth,
				viewHeight, false, SurfaceFormat.Color, DepthFormat.Depth24);

			// Load effects
			//depthNormalEffect = Content.Load<Effect>("PPDepthNormal");
			depthNormalEffect = Content.Load<Effect>("PPDepthNormalV2");
			//depthNormalEffect.Parameters["FarPlane"].SetValue(shadowFarPlane);

			lightingEffect = Content.Load<Effect>("PPLight");

			// Set effect parameters to light mapping effect
			lightingEffect.Parameters["viewportWidth"].SetValue(viewWidth);
			lightingEffect.Parameters["viewportHeight"].SetValue(viewHeight);

			// Load point light mesh and set light mapping effect to it
			lightMesh = Content.Load<Model>("Models\\PPLightMesh");
			lightMesh.Meshes[0].MeshParts[0].Effect = lightingEffect;
			this.graphicsDevice = GraphicsDevice;


			// shadow関係
			//shadowDepthTarg = new RenderTarget2D(GraphicsDevice, shadowMapSize, shadowMapSize, false, SurfaceFormat.Single, DepthFormat.Depth24);
			shadowDepthTarg = new RenderTarget2D(GraphicsDevice, shadowMapSize, shadowMapSize, false, SurfaceFormat.HalfVector2, DepthFormat.Depth24);
			shadowDepthEffect = Content.Load<Effect>("ShadowDepthEffectV2");
			shadowDepthEffect.Parameters["FarPlane"].SetValue(shadowFarPlane);

			// VSM
			spriteBatch = new SpriteBatch(GraphicsDevice);
			shadowBlurEffect = Content.Load<Effect>("GaussianBlur");
			shadowBlurTarg = new RenderTarget2D(GraphicsDevice, shadowMapSize,
			shadowMapSize, false, SurfaceFormat.Color, DepthFormat.Depth24);

			quadRenderer = new QuadRenderer();
		}
	}
}
