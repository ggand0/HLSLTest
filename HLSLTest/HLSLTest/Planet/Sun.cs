#define ROTATE

using System;
using System.IO;
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
		private ChainedRenderTarget bluredLayer;
		private Effect sunEffect, sunLayerEffect, extractHDREffect;
		private Effect linearFilter, gaussianFilter;

		private Vector3 rotationAxis, rotationAxisLayer0, rotationAxisLayer1;
		private float rotationAngle, rotationAngleLayer0, rotationAngleLayer1;

		//public Vector3 Position { get; set; }
		public Vector3 Colour;
		//public float Scale;
		private GraphicsDevice graphicsDevice;
		private SpriteBatch spriteBatch;

		OcclusionQuery occlusionQuery;
		bool occlusionQueryActive = false;
		float occlusionAlpha;


		public override void Update(GameTime gameTime)
		{
			//base.Update(gameTime);

#if ROTATE
            if ( rotationAngle < MathHelper.Pi ) 
                rotationAngle += 0.0005f;
            else 
                rotationAngle = 0f;

			if (rotationAngleLayer0 < MathHelper.Pi)
				rotationAngleLayer0 += 0.0005f;
            else
				rotationAngleLayer0 = 0f;

            if (rotationAngleLayer1 < MathHelper.Pi ) 
                rotationAngleLayer1 += 0.0008f;
            else 
                rotationAngleLayer1 = 0f;
#endif
			string s = Position.ToString();
		}
		

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

			linearFilter = content.Load<Effect>("Planets\\LinearFilter");
			gaussianFilter = content.Load<Effect>("Planets\\GaussianFilter");
			/*glowSprite = content.Load<Texture2D>("Textures\\glow");
			spriteBatch = new SpriteBatch(graphics);
			
			foreach (Flare flare in flares) {
				flare.Texture = content.Load<Texture2D>("Textures\\" + flare.TextureName);
			}*/

			SetupRenderTargets();
		}

		private void DrawSurface(bool postEffect, Matrix View, Matrix Projection)
		{
			Matrix World = Matrix.CreateScale(Scale * 200) * Matrix.CreateFromAxisAngle(rotationAxis, rotationAngle)
				* Matrix.CreateTranslation(Position);
			Matrix wvp = World * View * Projection;

			
			if (postEffect) {
				graphicsDevice.SetRenderTarget(albedoLayer);
				graphicsDevice.Clear(Color.Transparent);
			} else {
				//graphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
			}
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
				graphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;// 一度別のStateを入れてキャッシュをクリアしないとエラー...これは酷い
				graphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
				graphicsDevice.SamplerStates[1] = SamplerState.PointClamp;
				graphicsDevice.SamplerStates[2] = SamplerState.PointClamp;
				mesh.Draw();
				graphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
			}
			if (postEffect) {
				graphicsDevice.SetRenderTarget(null);
			}
		}
		private void DrawLayers(bool postEffect, Matrix View, Matrix Projection)
		{
			
			/*Device.RenderState.DepthBufferEnable = false;
			Device.RenderState.AlphaBlendEnable = true;
			Device.RenderState.SourceBlend = Blend.One;
			Device.RenderState.DestinationBlend = Blend.One;*/
			GraphicsDevice d = graphicsDevice;

			if (postEffect) {
				graphicsDevice.SetRenderTarget(additiveLayer0);
				graphicsDevice.Clear(Color.Transparent);

				graphicsDevice.DepthStencilState = DepthStencilState.None;
				/*BlendState bs = new BlendState();
				bs.AlphaSourceBlend = Blend.One;
				bs.AlphaDestinationBlend = Blend.One;
				graphicsDevice.BlendState = bs;*/
				graphicsDevice.BlendState = BlendState.Additive;
			} else {
				// 直に描画する場合はここを適用する
				graphicsDevice.DepthStencilState = DepthStencilState.None;
				graphicsDevice.BlendState = BlendState.Additive;
			}

			Matrix World = Matrix.CreateScale(Scale * 200) * Matrix.CreateFromAxisAngle(rotationAxisLayer0, rotationAngleLayer0)
				* Matrix.CreateTranslation(Position);
			Matrix wvp = World * View * Projection;
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


			World = Matrix.CreateScale(Scale * 200) * Matrix.CreateFromAxisAngle(rotationAxisLayer1, rotationAngleLayer1)
				* Matrix.CreateTranslation(Position);
			wvp = World * View * Projection;
			foreach (ModelMesh mesh in sphere.Meshes) {
				foreach (Effect effect in mesh.Effects) {
					effect.CurrentTechnique = effect.Techniques[0];
					effect.Parameters["BaseTexture"].SetValue(sunLayerTexture1);
					effect.Parameters["wvp"].SetValue(wvp);
					effect.Parameters["WorldIT"].SetValue(Matrix.Invert(World));
				}
				mesh.Draw();
			}
			if (postEffect) {
				graphicsDevice.SetRenderTarget(null);
			}

			//Device.RenderState.DepthBufferEnable = true;
			//Device.RenderState.AlphaBlendEnable = false;
			graphicsDevice.DepthStencilState = DepthStencilState.Default;
			graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
			graphicsDevice.BlendState = BlendState.Opaque;
		}
		private void ExtractHDR(Matrix View, Matrix Projection, float blurIntensity)
		{
			//graphicsDevice.BlendState = BlendState.Opaque;

			graphicsDevice.SetRenderTarget(hdrExtractedLayer);
			graphicsDevice.Clear(Color.Transparent);
			graphicsDevice.Textures[0] = additiveLayer0;
			extractHDREffect.CurrentTechnique = extractHDREffect.Techniques[0];
			extractHDREffect.Parameters["Threshold"].SetValue(0.02f);
			extractHDREffect.Parameters["Brightness"].SetValue(blurIntensity);
			extractHDREffect.Parameters["GradientTexture"].SetValue(gradientTexture);
			//extractHDREffect.Begin();
			foreach (EffectPass pass in extractHDREffect.CurrentTechnique.Passes) {
				//spriteBatch.Begin(SpriteSortMode.Immediate,	BlendState.NonPremultiplied);
				//spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
				spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);
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
		private void BlurScreen(Matrix world)
		{
			if (JoyStick.IsOnKeyDown(8)) {
				using (Stream stream = File.OpenWrite("sun_extract.png")) {
					hdrExtractedLayer.SaveAsPng(stream, hdrExtractedLayer.Width, hdrExtractedLayer.Height);
					stream.Position = 0;
				}
			}
			// this stuff is done with my rendertarget class
			bluredLayer.GenerateMipMapLevels(ref hdrExtractedLayer);
			bluredLayer.ApplyBlur(ref gaussianFilter, world);
			bluredLayer.AdditiveBlend();
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
		public void Draw(bool postEffect, Matrix View, Matrix Projection)
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

			if (!postEffect) {
				graphicsDevice.SetRenderTarget(null);
				DrawSurface(postEffect, View, Projection);
				DrawLayers(postEffect, View, Projection);
			} else {
				DrawSurface(postEffect, View, Projection);
				DrawLayers(postEffect, View, Projection);
				ExtractHDR(View, Projection, 0.5f);
				Matrix World = Matrix.CreateScale(Scale * 200) * Matrix.CreateTranslation(Position);
					//* Matrix.CreateFromAxisAngle(rotationAxisLayer0, rotationAngleLayer0) * Matrix.CreateTranslation(Position);
				BlurScreen(World);
			}

			graphicsDevice.BlendState = BlendState.Opaque;
			graphicsDevice.DepthStencilState = DepthStencilState.Default;
			graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

			if (JoyStick.IsOnKeyDown(8)) {
				using (Stream stream = File.OpenWrite("sun_layer.png")) {
					additiveLayer0.SaveAsPng(stream, additiveLayer0.Width, additiveLayer0.Height);
					stream.Position = 0;
				}
			}
			
			if (postEffect) {
				float sunDepth = Vector3.Transform(Position, View).Z;

				GraphicsDevice d = graphicsDevice;
				//graphicsDevice.DepthStencilState = DepthStencilState.Default;
				graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
				graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
				graphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
				//graphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
				//graphicsDevice.SamplerStates[1] = SamplerState.PointWrap;
				//graphicsDevice.SamplerStates[2] = SamplerState.PointWrap;
				spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);
				graphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
				spriteBatch.Draw(albedoLayer, Vector2.Zero, Color.White);// Drawの中でSamplerStateがLinearに戻されるらしい
				//spriteBatch.Draw(albedoLayer, Vector2.Zero, null, Color.White, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, sunDepth);

				graphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
				//spriteBatch.Draw(additiveLayer0, Vector2.Zero, Color.White);
				spriteBatch.Draw(hdrExtractedLayer, Vector2.Zero, Color.White);
				//spriteBatch.Draw(hdrExtractedLayer, Vector2.Zero, null, Color.White, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, sunDepth);
				spriteBatch.End();
				graphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
			}/**/

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
			graphicsDevice.SamplerStates[0] = SamplerState.PointWrap;///\ null;//SamplerState.LinearWrap;
			graphicsDevice.SamplerStates[1] = SamplerState.LinearWrap;
			graphicsDevice.SamplerStates[2] = SamplerState.LinearWrap;
		}

		private void SetupRenderTargets()
		{
			albedoLayer = new RenderTarget2D(graphicsDevice, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height, true, SurfaceFormat.HalfVector4, DepthFormat.Depth24);
			//additiveLayer0 = new RenderTarget2D(graphicsDevice, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height, true, SurfaceFormat.HalfVector4, DepthFormat.Depth24);
			additiveLayer0 = new RenderTarget2D(graphicsDevice, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height, true, SurfaceFormat.Color, DepthFormat.Depth24);
			additiveLayer1 = new RenderTarget2D(graphicsDevice, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height, true, SurfaceFormat.HalfVector4, DepthFormat.Depth24);

			//hdrExtractedLayer = new RenderTarget2D(graphicsDevice, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height, true, SurfaceFormat.HalfVector4, DepthFormat.Depth24);
			//hdrExtractedLayer = new RenderTarget2D(graphicsDevice, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height, true, SurfaceFormat.Vector4, DepthFormat.Depth24);
			hdrExtractedLayer = new RenderTarget2D(graphicsDevice, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height, true, SurfaceFormat.Color, DepthFormat.Depth24);

			bluredLayer = new ChainedRenderTarget(spriteBatch, linearFilter ,graphicsDevice, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height);
			bluredLayer.RenderTarget = hdrExtractedLayer;
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

			// arbitrary, but different axes
			rotationAxis = Vector3.Normalize(new Vector3(1f, 2f, 1f));
			rotationAngle = 0f;
			rotationAxisLayer0 = Vector3.Normalize(new Vector3(3f, 1f, 1f));
			rotationAxisLayer1 = Vector3.Normalize(new Vector3(1f, 5f, 2f));  
		}
		#endregion
	}
}
