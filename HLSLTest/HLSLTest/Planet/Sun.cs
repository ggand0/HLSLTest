using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;

namespace HLSLTest
{
	public class Sun : Object
	{
		//public static Level level;

		private Model sphere;
		private TextureCube sunTexture, sunLayerTexture0, sunLayerTexture1;
		private Texture2D gradientTexture;
		private RenderTarget2D albedoLayer, additiveLayer0, additiveLayer1, hdrExtractedLayer;
		private Effect sunEffect, sunLayerEffect, extractHDREffect;

		//public Vector3 Position { get; set; }
		public Vector3 Colour;
		//public float Scale;
		private GraphicsDevice graphicsDevice;
		private SpriteBatch spriteBatch;

		OcclusionQuery occlusionQuery;
		bool occlusionQueryActive = false;
		float occlusionAlpha;

		
		
		

		public void LoadContent(GraphicsDevice graphics, ContentManager content)
		{
			sphere = content.Load<Model>("Models\\sphere");
			sunTexture = content.Load<TextureCube>("Textures\\SkyBox\\SunTexture");
			sunLayerTexture0 = content.Load<TextureCube>("Textures\\SkyBox\\SunLayer0");
			sunLayerTexture1 = content.Load<TextureCube>("Textures\\SkyBox\\SunLayer1");
			gradientTexture = content.Load<Texture2D>("Textures\\Planet\\FireGradient");
			occlusionQuery = new OcclusionQuery(graphics);
			sunEffect = content.Load<Effect>("Planets\\SunEffect");
			sunLayerEffect = content.Load<Effect>("Planets\\SunLayerEffect");
			extractHDREffect = content.Load<Effect>("Planets\\ExtractHDREffect");
			/*glowSprite = content.Load<Texture2D>("Textures\\glow");
			spriteBatch = new SpriteBatch(graphics);
			
			foreach (Flare flare in flares) {
				flare.Texture = content.Load<Texture2D>("Textures\\" + flare.TextureName);
			}*/

			SetupRenderTargets();
		}

		private void DrawSurface(Matrix View, Matrix Projection)
		{
			Matrix World = Matrix.CreateScale(Scale * 200) * Matrix.CreateTranslation(Position);//Position

			Matrix wvp = World * View * Projection;
			/*sunEffect.Parameters["wvp"].SetValue(wvp);
			//sunEffect.Parameters["colour"].SetValue(Colour);
			for (int pass = 0; pass < sunEffect.CurrentTechnique.Passes.Count; pass++) {
				for (int msh = 0; msh < sphere.Meshes.Count; msh++) {
					ModelMesh mesh = sphere.Meshes[msh];

					for (int prt = 0; prt < mesh.MeshParts.Count; prt++)
						mesh.MeshParts[prt].Effect = sunEffect;
					mesh.Draw();
				}
			}*/

			graphicsDevice.SetRenderTarget(albedoLayer);
			graphicsDevice.Clear(Color.Transparent);
			foreach (ModelMesh mesh in sphere.Meshes) {
				foreach (ModelMeshPart part in mesh.MeshParts) {
					part.Effect = sunEffect;
				}
			}
			/*Matrix World = Matrix.CreateFromAxisAngle(
				vecRotationAxis,
				fRotationAngle);*/

			foreach (ModelMesh mesh in sphere.Meshes) {
				foreach (Effect effect in mesh.Effects) {
					effect.CurrentTechnique = effect.Techniques[0];
					sunEffect.Parameters["wvp"].SetValue(wvp);
					effect.Parameters["BaseTexture"].SetValue(sunTexture);
					effect.Parameters["WorldIT"].SetValue(Matrix.Invert(World));
				}
				mesh.Draw();
			}
			graphicsDevice.SetRenderTarget(null);
		}
		private void DrawLayers(Matrix View, Matrix Projection)
		{
			Matrix World = Matrix.CreateScale(Scale * 200) * Matrix.CreateTranslation(Position);//Position

			Matrix wvp = World * View * Projection;
			/*Device.RenderState.DepthBufferEnable = false;
			Device.RenderState.AlphaBlendEnable = true;
			Device.RenderState.SourceBlend = Blend.One;
			Device.RenderState.DestinationBlend = Blend.One;*/

			//graphicsDevice.DepthStencilState = DepthStencilState.None;
			//graphicsDevice.BlendState = BlendState.Additive;	// 直に描画する場合はここを適用する

			graphicsDevice.SetRenderTarget(additiveLayer0);
			graphicsDevice.Clear(Color.Transparent);
			foreach (ModelMesh mesh in sphere.Meshes) {
				foreach (ModelMeshPart part in mesh.MeshParts) {
					part.Effect = sunLayerEffect;
				}
			}

			foreach (ModelMesh mesh in sphere.Meshes) {
				foreach (Effect effect in mesh.Effects) {
					effect.CurrentTechnique = effect.Techniques[0];
					effect.Parameters["BaseTexture"].SetValue(sunLayerTexture0);
					effect.Parameters["wvp"].SetValue(wvp);
					effect.Parameters["WorldIT"].SetValue(Matrix.Invert(World));
				}
				mesh.Draw();
			}

			foreach (ModelMesh mesh in sphere.Meshes) {
				foreach (Effect effect in mesh.Effects) {
					effect.CurrentTechnique = effect.Techniques[0];
					effect.Parameters["BaseTexture"].SetValue(sunLayerTexture1);
					effect.Parameters["wvp"].SetValue(wvp);
					effect.Parameters["WorldIT"].SetValue(Matrix.Invert(World));
				}
				mesh.Draw();
			}
			graphicsDevice.SetRenderTarget(null);

			//Device.RenderState.DepthBufferEnable = true;
			//Device.RenderState.AlphaBlendEnable = false;
			graphicsDevice.DepthStencilState = DepthStencilState.Default;
			graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
			graphicsDevice.BlendState = BlendState.Opaque;
		}
		private void ExtractHDR(Matrix View, Matrix Projection, float blurIntensity)
		{
			graphicsDevice.BlendState = BlendState.Opaque;

			graphicsDevice.SetRenderTarget(hdrExtractedLayer);
			graphicsDevice.Clear(Color.Transparent);
			graphicsDevice.Textures[0] = additiveLayer0;
			extractHDREffect.CurrentTechnique = extractHDREffect.Techniques[0];
			extractHDREffect.Parameters["Threshold"].SetValue(1.0f);
			extractHDREffect.Parameters["Brightness"].SetValue(blurIntensity);
			extractHDREffect.Parameters["GradientTexture"].SetValue(gradientTexture);
			//extractHDREffect.Begin();
			foreach (EffectPass pass in extractHDREffect.CurrentTechnique.Passes) {
				//spriteBatch.Begin(SpriteSortMode.Immediate,	BlendState.NonPremultiplied);
				spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
				pass.Apply();
				graphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
				spriteBatch.Draw(additiveLayer0,
					new Rectangle(0, 0, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height),
					Color.White);
				//pass.End();
				spriteBatch.End();
			}
			//extractHDREffect.End();
			graphicsDevice.SetRenderTarget(null);

			graphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
		}
		private void RenderMesh(Matrix View, Matrix Projection)
		{
			Matrix World = Matrix.CreateScale(Scale * 200) * Matrix.CreateTranslation(Position);//Position
			Matrix wvp = World * View * Projection;


			foreach (ModelMesh mesh in sphere.Meshes) {
				foreach (ModelMeshPart part in mesh.MeshParts) {
					part.Effect = sunEffect;
				}
			}
			foreach (ModelMesh mesh in sphere.Meshes) {
				foreach (Effect effect in mesh.Effects) {
					effect.CurrentTechnique = effect.Techniques[0];
					sunEffect.Parameters["wvp"].SetValue(wvp);
					effect.Parameters["BaseTexture"].SetValue(sunTexture);
					effect.Parameters["WorldIT"].SetValue(Matrix.Invert(World));
				}
				mesh.Draw();
			}
		}
		public void Draw(Matrix View, Matrix Projection)
		{
			// Give up if the current graphics card does not support occlusion queries.
			// HiDefなので対応されていること前提
			//if (occlusionQuery.IsSupported) {
			/*if (occlusionQueryActive) {
				// If the previous query has not yet completed, wait until it does.
				if (occlusionQuery.IsComplete) {
					const float queryArea = 16 * 16;
					if (occlusionQuery.PixelCount > 0) {
						occlusionAlpha = Math.Min(occlusionQuery.PixelCount / queryArea, 1);
					} else {
						occlusionAlpha = 0;
					}
					occlusionQuery.Begin();
					holdoff = false;
				} else {
					holdoff = true;
				}

			} else {
				occlusionQuery.Begin();
				occlusionQueryActive = true;
				holdoff = false;
			}*/

			//graphics.RenderState.DepthBufferEnable = true;
			
			//graphics.DepthStencilState = DepthStencilState.DepthRead;
			//graphicsDevice.BlendState = BlendState.AlphaBlend;
			//graphics.BlendState = BlendState.Additive;

			//graphics.RasterizerState = RasterizerState.CullNone;
			graphicsDevice.DepthStencilState = DepthStencilState.Default;
			graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
			
			DrawSurface(View, Projection);
			DrawLayers(View, Projection);
			ExtractHDR(View, Projection, 0.5f);

			graphicsDevice.DepthStencilState = DepthStencilState.Default;
			graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
			graphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
			//graphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
			//graphicsDevice.SamplerStates[1] = SamplerState.PointWrap;
			//graphicsDevice.SamplerStates[2] = SamplerState.PointWrap;
			spriteBatch.Begin(
				SpriteSortMode.Immediate,
				BlendState.Additive);
			graphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
			spriteBatch.Draw(albedoLayer, Vector2.Zero, Color.White);// Drawの中でSamplerStateがLinearに戻されるらしい
			graphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
			//spriteBatch.Draw(additiveLayer0, Vector2.Zero, Color.White);
			spriteBatch.Draw(hdrExtractedLayer, Vector2.Zero, Color.White);
			spriteBatch.End();
			graphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;/**/

			/*if (occlusionQueryActive) {
				if (!holdoff)
					occlusionQuery.End();
			}
			// If it is visible, draw the flare effect.
			if (occlusionAlpha > 0) {
				graphics.DepthStencilState = DepthStencilState.DepthRead;
				graphics.RasterizerState = RasterizerState.CullNone;
				DrawGlow(TransformPosition(View, Projection, Position));//Position
				DrawFlares(TransformPosition(View, Projection, Position));//Position
			}*/
			graphicsDevice.BlendState = BlendState.Opaque;
			graphicsDevice.DepthStencilState = DepthStencilState.Default;
			graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
			graphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
			graphicsDevice.SamplerStates[1] = SamplerState.LinearWrap;
			graphicsDevice.SamplerStates[2] = SamplerState.LinearWrap;
		}

		private void SetupRenderTargets()
		{
			albedoLayer = new RenderTarget2D(graphicsDevice, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height, true, SurfaceFormat.HalfVector4, DepthFormat.Depth24);
			additiveLayer0 = new RenderTarget2D(graphicsDevice, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height, true, SurfaceFormat.HalfVector4, DepthFormat.Depth24);
			additiveLayer1 = new RenderTarget2D(graphicsDevice, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height, true, SurfaceFormat.HalfVector4, DepthFormat.Depth24);
			hdrExtractedLayer = new RenderTarget2D(graphicsDevice, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height, true, SurfaceFormat.HalfVector4, DepthFormat.Depth24);
		}
		#region Constructors
		public Sun(GraphicsDevice graphics, ContentManager content)
			:this(Vector3.Zero, graphics, content, null)
		{
		}
		public Sun(Vector3 position, GraphicsDevice graphics, ContentManager content, SpriteBatch spriteBatch)
			:base(position)
		{
			//this.Position = position;
			this.graphicsDevice = graphics;
			this.spriteBatch = spriteBatch;
			//SetType(SunType);
			LoadContent(graphics, content);
		}
		#endregion
	}
}
