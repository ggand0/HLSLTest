using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace HLSLTest
{
	public class PrelightingRenderer
	{
		// Normal, depth, and light map render targets
		RenderTarget2D depthTarg;
		RenderTarget2D normalTarg;
		RenderTarget2D lightTarg;

		// Depth/normal effect and light mapping effect
		Effect depthNormalEffect;
		Effect lightingEffect;

		// Point light (sphere) mesh
		Model lightMesh;

		// List of models, lights, and the camera
		public List<Object> Models { get; set; }
		public List<PPPointLight> Lights { get; set; }
		public ArcBallCamera Camera { get; set; }
		GraphicsDevice graphicsDevice;
		int viewWidth = 0, viewHeight = 0;

		void drawDepthNormalMap()
		{
			// Set the render targets to 'slots' 1 and 2
			graphicsDevice.SetRenderTargets(normalTarg, depthTarg);

			// Clear the render target to 1 (infinite depth)
			graphicsDevice.Clear(Color.White);

			// Draw each model with the PPDepthNormal effect
			// 法線マップと深度マップをDrawするエフェクトをセットしてそれぞれ書き込む
			foreach (Object o in Models) {
				o.CacheEffects();// すでにあるエフェクトを上書きさせないために退避させておく
				o.SetModelEffect(depthNormalEffect, false);// 空いたスペースで法線マップをDrawする
				o.Draw();
				o.RestoreEffects();// 退避させておいたエフェクトを戻す
			}

			// Un-set the render targets
			graphicsDevice.SetRenderTargets(null);
		}
		void drawLightMap()
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
			graphicsDevice.Clear(Color.Black);

			// Set the render target to the graphics device
			graphicsDevice.SetRenderTarget(lightTarg);

			// Clear the render target to black (no light)
			graphicsDevice.Clear(Color.Black);

			// Set render states to additive (lights will add their influences)
			graphicsDevice.BlendState = BlendState.Additive;
			graphicsDevice.DepthStencilState = DepthStencilState.None;

			foreach (PPPointLight light in Lights) {
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
				}
				// Draw the point-light-sphere
				// Additiveにしてるから個々の光源の色が加算される？
				lightMesh.Meshes[0].Draw();// ここでライトマップを作成する（重要）

				// Revert the cull mode
				graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
			}

			// Revert the blending and depth render states
			graphicsDevice.BlendState = BlendState.Opaque;
			graphicsDevice.DepthStencilState = DepthStencilState.Default;

			// Un-set the render target
			graphicsDevice.SetRenderTarget(null);
		}
		void prepareMainPass()
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
					}
		}
		public void Draw()
		{
			drawDepthNormalMap();
			drawLightMap();
			prepareMainPass();
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
			depthNormalEffect = Content.Load<Effect>("PPDepthNormal");
			lightingEffect = Content.Load<Effect>("PPLight");

			// Set effect parameters to light mapping effect
			lightingEffect.Parameters["viewportWidth"].SetValue(viewWidth);
			lightingEffect.Parameters["viewportHeight"].SetValue(viewHeight);

			// Load point light mesh and set light mapping effect to it
			lightMesh = Content.Load<Model>("Models\\PPLightMesh");
			lightMesh.Meshes[0].MeshParts[0].Effect = lightingEffect;
			this.graphicsDevice = GraphicsDevice;
		}
	}
}
