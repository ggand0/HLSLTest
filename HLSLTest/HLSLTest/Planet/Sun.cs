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
		private RenderTarget2D albedoLayer, additiveLayer0, additiveLayer1, hdrExtractedLayer, finalLayer;
		private ChainedRenderTarget bluredLayer;
		private Effect sunEffect, sunLayerEffect, extractHDREffect;
		private Effect linearFilter, gaussianFilter, quadEffect;
		private FullScreenQuadRenderer quadRenderer;

		private Vector3 rotationAxis, rotationAxisLayer0, rotationAxisLayer1;
		private float rotationAngle, rotationAngleLayer0, rotationAngleLayer1;

		//public Vector3 Position { get; set; }
		public Vector3 Colour;
		//public float Scale;
		private GraphicsDevice graphicsDevice;
		private SpriteBatch spriteBatch;
		BasicEffect basicEffect;
		BillboardSystem b1, b2;

        #region flare fields
        OcclusionQuery occlusionQuery;
		bool occlusionQueryActive = false;
		float occlusionAlpha;
        Texture2D glowSprite;
        float glowSize = 400;
        GraphicsDevice graphics;
        bool holdoff;

        Flare[] flares =
        {
            new Flare(-0.5f, 0.7f, new Color( 50,  25,  50), "flare1"),
            new Flare( 0.3f, 0.4f, new Color(100, 255, 200), "flare1"),
            new Flare( 1.2f, 1.0f, new Color(100,  50,  50), "flare1"),
            new Flare( 1.5f, 1.5f, new Color( 50, 100,  50), "flare1"),

            new Flare(-0.3f, 0.7f, new Color(200,  50,  50), "flare2"),
            new Flare( 0.6f, 0.9f, new Color( 50, 100,  50), "flare2"),
            new Flare( 0.7f, 0.4f, new Color( 50, 200, 200), "flare2"),

            new Flare(-0.7f, 0.7f, new Color( 50, 100,  25), "flare3"),
            new Flare( 0.0f, 0.6f, new Color( 25,  25,  25), "flare3"),
            new Flare( 2.0f, 1.4f, new Color( 25,  50, 100), "flare3"),
        };
        #endregion
        public float sunFrontDepth;



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

			World = Matrix.CreateScale(Scale) * Matrix.CreateTranslation(Position);
			//sunFrontDepth = Vector3.Transform(Vector3.Transform(Position + (Vector3.Normalize(Position - level.camera.CameraPosition) * 200), world), level.camera.View).Z;
			sunFrontDepth = Vector3.Transform(Position + (Vector3.Normalize(level.camera.CameraPosition - Position) * 200), level.camera.View).Z;
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
			b1 = new BillboardSystem(graphicsDevice, content, null, new Vector2(graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height), new Vector3[] { Position });
			b2 = new BillboardSystem(graphicsDevice, content, null, new Vector2(graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height), new Vector3[] { Position });

			basicEffect = new BasicEffect(graphicsDevice) {
				TextureEnabled = true,
				VertexColorEnabled = true,
			};

			SetupRenderTargets();
		}

		bool hasSaved;
		private void DrawSurface(bool postEffect, Matrix View, Matrix Projection)
		{
			Matrix World = Matrix.CreateScale(Scale) * Matrix.CreateFromAxisAngle(rotationAxis, rotationAngle)
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
				//graphicsDevice.BlendState = BlendState.AlphaBlend;
			} else {
				// 直に描画する場合はここを適用する
				graphicsDevice.DepthStencilState = DepthStencilState.None;
				graphicsDevice.BlendState = BlendState.Additive;
			}

			Matrix World = Matrix.CreateScale(Scale) * Matrix.CreateFromAxisAngle(rotationAxisLayer0, rotationAngleLayer0)
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


			World = Matrix.CreateScale(Scale) * Matrix.CreateFromAxisAngle(rotationAxisLayer1, rotationAngleLayer1)
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
			/*if (!hasSaved) {// debug
				hasSaved = true;
				using (Stream stream = File.OpenWrite("sun_transparent_layer.png")) {
					additiveLayer0.SaveAsPng(stream, hdrExtractedLayer.Width, hdrExtractedLayer.Height);
					stream.Position = 0;
				}
			}*/ // ここまでアルファ維持
			graphicsDevice.SetRenderTarget(hdrExtractedLayer);
			graphicsDevice.Clear(Color.Transparent);
			graphicsDevice.Textures[0] = additiveLayer0;
			extractHDREffect.CurrentTechnique = extractHDREffect.Techniques[0];
			extractHDREffect.Parameters["Threshold"].SetValue(0.08f);// 0.02f
			extractHDREffect.Parameters["Brightness"].SetValue(blurIntensity);
			extractHDREffect.Parameters["GradientTexture"].SetValue(gradientTexture);
			foreach (EffectPass pass in extractHDREffect.CurrentTechnique.Passes) {
				//spriteBatch.Begin(SpriteSortMode.Immediate,	BlendState.NonPremultiplied);
				spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);
				//spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.Additive);
				//spriteBatch.Begin(); // アルファ確認、しかしエフェクトが適用されてない？←Immediateじゃないとダメらしい
				pass.Apply();
				graphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
				spriteBatch.Draw(additiveLayer0,
					new Rectangle(0, 0, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height),
					Color.White);
				spriteBatch.End();
			}
			graphicsDevice.SetRenderTarget(null);
			graphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;


			if (JoyStick.IsOnKeyDown(8)) {
				using (Stream stream = File.OpenWrite("sun_extracted.png")) {
					hdrExtractedLayer.SaveAsPng(stream, hdrExtractedLayer.Width, hdrExtractedLayer.Height);
					stream.Position = 0;
				}
			}
		}
		private void BlurScreen(Matrix world)
		{
			
			// this stuff is done with my rendertarget class
			bluredLayer.GenerateMipMapLevels(ref hdrExtractedLayer);
			bluredLayer.ApplyBlur(ref gaussianFilter, world);
			bluredLayer.AdditiveBlend();

			if (JoyStick.IsOnKeyDown(8)) {
				using (Stream stream = File.OpenWrite("sun_blurred.png")) {
					hdrExtractedLayer.SaveAsPng(stream, hdrExtractedLayer.Width, hdrExtractedLayer.Height);
					stream.Position = 0;
				}
			}
		}

		public void PreDraw(Matrix View, Matrix Projection)
		{
			DrawSurface(true, View, Projection);
			DrawLayers(true, View, Projection);
			ExtractHDR(View, Projection, 0.5f);
			Matrix World = Matrix.CreateScale(Scale) * Matrix.CreateTranslation(Position);
			//* Matrix.CreateFromAxisAngle(rotationAxisLayer0, rotationAngleLayer0) * Matrix.CreateTranslation(Position);
			BlurScreen(World);


			graphicsDevice.SetRenderTarget(finalLayer);
			graphicsDevice.Clear(Color.Transparent);
			float sunDepth = Vector3.Transform(Position, View).Z;
			GraphicsDevice d = graphicsDevice;
			//graphicsDevice.DepthStencilState = DepthStencilState.None;
			//graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
			graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
			graphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
			//graphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
			//graphicsDevice.SamplerStates[1] = SamplerState.PointWrap;
			//graphicsDevice.SamplerStates[2] = SamplerState.PointWrap;
			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);

			//basicEffect.World = Matrix.CreateScale(Scale) * Matrix.CreateTranslation(Position);
			//basicEffect.View = View;
			//basicEffect.Projection = Projection;
			//spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, DepthStencilState.DepthRead, RasterizerState.CullNone, basicEffect);
			//spriteBatch.Begin(0, null, null, null, RasterizerState.CullNone, basicEffect);

			graphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
			spriteBatch.Draw(albedoLayer, Vector2.Zero, Color.White);// Drawの中でSamplerStateがLinearに戻されるらしい
			//spriteBatch.Draw(albedoLayer, Vector2.Zero, null, Color.White, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, sunDepth);

			graphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
			//spriteBatch.Draw(additiveLayer0, Vector2.Zero, Color.White);
			spriteBatch.Draw(hdrExtractedLayer, Vector2.Zero, Color.White);
			//spriteBatch.Draw(hdrExtractedLayer, Vector2.Zero, null, Color.White, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, sunDepth);
			spriteBatch.End();
			graphicsDevice.SetRenderTarget(null);
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
				/*DrawSurface(postEffect, View, Projection);
				DrawLayers(postEffect, View, Projection);
				ExtractHDR(View, Projection, 0.5f);
				Matrix World = Matrix.CreateScale(Scale) * Matrix.CreateTranslation(Position);
				//* Matrix.CreateFromAxisAngle(rotationAxisLayer0, rotationAngleLayer0) * Matrix.CreateTranslation(Position);
				BlurScreen(World);*/
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
				/*//b1.effect.Parameters["AlphaTestValue"].SetValue(0.1f);
				//b2.effect.Parameters["AlphaTestValue"].SetValue(0.1f);
				//b1.effect.Parameters["AlphaTest"].SetValue(false);
				//b2.effect.Parameters["AlphaTest"].SetValue(false);
				b1.Texture = albedoLayer;
				b2.Texture = hdrExtractedLayer;
				b1.Draw(View, Projection, level.camera.Up, level.camera.Right);
				b2.Draw(View, Projection, level.camera.Up, level.camera.Right);*/

				
				/*graphicsDevice.SetRenderTarget(finalLayer);
				graphicsDevice.Clear(Color.Transparent);
				
				GraphicsDevice d = graphicsDevice;
				//graphicsDevice.DepthStencilState = DepthStencilState.None;
				//graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
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
				graphicsDevice.SetRenderTarget(null);*/


				// Axial billboard状態なのでやはり普通にBillboard使うかFull screen quadを用いるべきだろう
				/*//basicEffect.World =  Matrix.CreateTranslation(Position);
				basicEffect.World = Matrix.Identity;
				basicEffect.View = View;
				basicEffect.Projection = Projection;
				//spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
				spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, basicEffect);
				//spriteBatch.Begin(0, null, null, null, RasterizerState.CullNone, basicEffect);
				spriteBatch.Draw(finalLayer, Vector2.Zero, Color.White);
				//spriteBatch.Draw(finalLayer, Vector2.Zero, null, Color.White, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, Math.Abs(sunDepth / level.camera.FarPlaneDistance));
				spriteBatch.End();*/

				//float sunDepth = Vector3.Transform(Position, View).Z;
				Matrix world = Matrix.CreateScale(Scale) * Matrix.CreateTranslation(Position);
				float sunDepth = Vector3.Transform(Vector3.Transform(Position, world), View).Z;
				float sunFrontDepth = Vector3.Transform(Vector3.Transform(Position + (Vector3.Normalize(Position - level.camera.CameraPosition) * 200), world), View).Z;
				//graphicsDevice.BlendState = BlendState.Opaque;
				graphicsDevice.BlendState = BlendState.AlphaBlend;
				graphicsDevice.DepthStencilState = DepthStencilState.Default;
				//graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
				quadEffect.Parameters["colorMap"].SetValue(finalLayer);
				//quadEffect.Parameters["Depth"].SetValue(1 - Math.Abs(sunDepth / level.camera.FarPlaneDistance));// 中心のDepth利用
				//quadEffect.Parameters["Depth"].SetValue(1 - Math.Abs(sunFrontDepth / level.camera.FarPlaneDistance));// 中心のDepth利用
				quadEffect.Parameters["Depth"].SetValue(1 - (-sunFrontDepth / level.camera.FarPlaneDistance));
				//quadEffect.Parameters["Depth"].SetValue((-sunDepth / level.camera.FarPlaneDistance));
				//quadEffect.Parameters["Depth"].SetValue(0.98f);//0.99
				//quadEffect.Parameters["Depth"].SetValue(sunDepth);
				//quadEffect.Parameters["Depth"].SetValue(0f);
				quadEffect.Parameters["FarPlane"].SetValue(level.camera.FarPlaneDistance);
				quadEffect.Parameters["Position"].SetValue(Position);
				quadEffect.Parameters["World"].SetValue(world);
				quadEffect.Parameters["View"].SetValue(View);
				quadEffect.Parameters["wvp"].SetValue(world * View * Projection);
				quadRenderer.RenderFullScreenQuad(quadEffect);
				graphicsDevice.BlendState = BlendState.Opaque;
				graphicsDevice.DepthStencilState = DepthStencilState.Default;/**/
			}

			if (occlusionQueryActive) {
				if (!holdoff)
					occlusionQuery.End();
			}
			// If it is visible, draw the flare effect.
			if (occlusionAlpha > 0) {
				graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
                graphicsDevice.RasterizerState = RasterizerState.CullNone;
				DrawGlow(TransformPosition(View, Projection, Position));//Position
				DrawFlares(TransformPosition(View, Projection, Position));//Position
			}/**/
			graphicsDevice.BlendState = BlendState.Opaque;
			graphicsDevice.DepthStencilState = DepthStencilState.Default;
			graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
			graphicsDevice.SamplerStates[0] = SamplerState.PointWrap;///\ null;//SamplerState.LinearWrap;
			graphicsDevice.SamplerStates[1] = SamplerState.LinearWrap;
			graphicsDevice.SamplerStates[2] = SamplerState.LinearWrap;
		}
		public void Draw(bool postEffect, Matrix View, Matrix Projection, RenderTarget2D mask)
		{
			//float sunDepth = Vector3.Transform(Position, View).Z;
			Matrix world = Matrix.CreateScale(Scale) * Matrix.CreateTranslation(Position);
			float sunDepth = Vector3.Transform(Vector3.Transform(Position, world), View).Z;
			
			//graphicsDevice.BlendState = BlendState.Opaque;
			graphicsDevice.BlendState = BlendState.AlphaBlend;
			//graphicsDevice.DepthStencilState = DepthStencilState.Default;
			graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
			quadEffect.Parameters["colorMap"].SetValue(finalLayer);
			quadEffect.Parameters["Mask"].SetValue(mask);
			//quadEffect.Parameters["Depth"].SetValue(1 - Math.Abs(sunDepth / level.camera.FarPlaneDistance));// 中心のDepth利用
			//quadEffect.Parameters["Depth"].SetValue(1 - Math.Abs(sunFrontDepth / level.camera.FarPlaneDistance));// 中心のDepth利用
			//quadEffect.Parameters["Depth"].SetValue(1 - (-sunFrontDepth / level.camera.FarPlaneDistance));
			//quadEffect.Parameters["Depth"].SetValue((-sunDepth / level.camera.FarPlaneDistance));
			//quadEffect.Parameters["Depth"].SetValue(0.98f);//0.99
			//quadEffect.Parameters["Depth"].SetValue(sunDepth);
			quadEffect.Parameters["Depth"].SetValue(0f);
			quadEffect.Parameters["FarPlane"].SetValue(level.camera.FarPlaneDistance);
			quadEffect.Parameters["Position"].SetValue(Position);
			quadEffect.Parameters["World"].SetValue(world);
			quadEffect.Parameters["View"].SetValue(View);
			quadEffect.Parameters["wvp"].SetValue(world * View * Projection);
			quadRenderer.RenderFullScreenQuad(quadEffect);

            /*if (occlusionAlpha > 0) {
				graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
                graphicsDevice.RasterizerState = RasterizerState.CullNone;
				DrawGlow(TransformPosition(View, Projection, Position));//Position
				DrawFlares(TransformPosition(View, Projection, Position));//Position
			}*/

			graphicsDevice.BlendState = BlendState.Opaque;
			graphicsDevice.DepthStencilState = DepthStencilState.Default;
			graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
			graphicsDevice.SamplerStates[0] = SamplerState.PointWrap;///\ null;//SamplerState.LinearWrap;
		}
		public void DrawAsBackGround(Matrix View, Matrix Projection)
		{
		}


        #region Draw flares
        /// <summary>
        /// Draws a large circular glow sprite, centered on the sun.
        /// </summary>
        void DrawGlow(Vector2 lightPosition)
        {
            Vector4 color = new Vector4(1, 1, 1, occlusionAlpha);
            Vector2 origin = new Vector2(glowSprite.Width, glowSprite.Height) / 2;
            float scale = glowSize * 2 / glowSprite.Width;

            //spriteBatch.Begin(SpriteBlendMode.AlphaBlend);
            //spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);
            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.Additive);
            spriteBatch.Draw(glowSprite, lightPosition, null, new Color(color), 0,
                             origin, scale, SpriteEffects.None, 0);

            spriteBatch.End();
        }

        public Vector2 TransformPosition(Matrix View, Matrix Projection, Vector3 oPosition)
        {
            Vector4 oTransformedPosition = Vector4.Transform(oPosition, View * Projection);
            if (oTransformedPosition.W != 0) {
                oTransformedPosition.X = oTransformedPosition.X / oTransformedPosition.W;
                oTransformedPosition.Y = oTransformedPosition.Y / oTransformedPosition.W;
                oTransformedPosition.Z = oTransformedPosition.Z / oTransformedPosition.W;
            }

            Vector2 oPosition2D = new Vector2(
              oTransformedPosition.X * graphicsDevice.PresentationParameters.BackBufferWidth / 2 + graphicsDevice.PresentationParameters.BackBufferWidth / 2,
              -oTransformedPosition.Y * graphicsDevice.PresentationParameters.BackBufferHeight / 2 + graphicsDevice.PresentationParameters.BackBufferHeight / 2);

            return oPosition2D;
        }
        /// <summary>
        /// Draws the lensflare sprites, computing the position
        /// of each one based on the current angle of the sun.
        /// </summary>
        public void DrawFlares(Vector2 lightPosition)
        {
            Viewport viewport = graphicsDevice.Viewport;

            // Lensflare sprites are positioned at intervals along a line that
            // runs from the 2D light position toward the center of the screen.
            Vector2 screenCenter = new Vector2(viewport.Width, viewport.Height) / 2;

            Vector2 flareVector = screenCenter - lightPosition;

            // Draw the flare sprites using additive blending.
            //spriteBatch.Begin(SpriteBlendMode.Additive);
            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.Additive);

            foreach (Flare flare in flares) {
                // Compute the position of this flare sprite.
                Vector2 flarePosition = lightPosition + flareVector * flare.Position;

                // Set the flare alpha based on the previous occlusion query result.
                Vector4 flareColor = flare.Color.ToVector4();

                flareColor.W *= occlusionAlpha;

                // Center the sprite texture.
                Vector2 flareOrigin = new Vector2(flare.Texture.Width,
                                                  flare.Texture.Height) / 2;

                // Draw the flare.
                spriteBatch.Draw(flare.Texture, flarePosition, null,
                                 new Color(flareColor), 1, flareOrigin,
                                 flare.Scale, SpriteEffects.None, 0);
            }

            spriteBatch.End();
        }
        #endregion

        private void SetupRenderTargets()
		{
			albedoLayer = new RenderTarget2D(graphicsDevice, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height, true, SurfaceFormat.HalfVector4, DepthFormat.Depth24);
			//additiveLayer0 = new RenderTarget2D(graphicsDevice, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height, true, SurfaceFormat.HalfVector4, DepthFormat.Depth24);
			additiveLayer0 = new RenderTarget2D(graphicsDevice, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height, true, SurfaceFormat.Color, DepthFormat.Depth24);
			additiveLayer1 = new RenderTarget2D(graphicsDevice, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height, true, SurfaceFormat.HalfVector4, DepthFormat.Depth24);

			//hdrExtractedLayer = new RenderTarget2D(graphicsDevice, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height, true, SurfaceFormat.HalfVector4, DepthFormat.Depth24);
			//hdrExtractedLayer = new RenderTarget2D(graphicsDevice, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height, true, SurfaceFormat.Vector4, DepthFormat.Depth24);
			hdrExtractedLayer = new RenderTarget2D(graphicsDevice, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height, true, SurfaceFormat.Color, DepthFormat.Depth24);

			finalLayer = new RenderTarget2D(graphicsDevice, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height, true, SurfaceFormat.Color, DepthFormat.Depth24);

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

			quadRenderer = new FullScreenQuadRenderer(graphics);
			quadEffect = content.Load<Effect>("Effects\\QuadEffect");

            Scale = 500;
		}
		#endregion
	}
}
