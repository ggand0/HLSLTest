﻿/*
 * file: ChainedRenderTarget.cs
 * desc: receives pointer to XNA rendertarget, creates mipmap chain
 * 
 * this class is pretty much a mess, because I manually have to scale
 * the rendertarget five times. therefore, I have to create five
 * rendertargets, five temporary targets...I'm sure this could
 * be done in a more efficient way, but you might just get the idea.
 * 
 * (c) 2008 Nicolas Menzel
 */

using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace HLSLTest
{
	class ChainedRenderTarget
	{
		private static int MAX_DOWNSAMPLE = 6;             // do *not* change
		private static float SCALE_FACTOR = 2.0f;//1.0f;            // blur-radius
		private static String BLUR_QUALITY = "Quality3x3";    // default: 3x3

		private RenderTarget2D m_RenderTarget;
		private RenderTarget2D[] m_DownSampleTargets;
		private RenderTarget2D[] m_UpSampleTargets;
		private RenderTarget2D[] m_TempTargets;
		private GraphicsDevice m_Device;
		private int m_Width;
		private int m_Height;
		private Effect m_Effect;
		private SpriteBatch m_SpriteBatch;

		// -----------------------------------------------------------------
		// self-explanatory constructor
		public ChainedRenderTarget(
			SpriteBatch Sprite,
			Effect DownSampleEffect,
			GraphicsDevice Device,
			int Width,
			int Height)
		{
			m_SpriteBatch = Sprite;
			m_Effect = DownSampleEffect;
			m_Device = Device;
			m_Width = Width;
			m_Height = Height;

			m_DownSampleTargets = new RenderTarget2D[MAX_DOWNSAMPLE];
			m_TempTargets = new RenderTarget2D[MAX_DOWNSAMPLE];
			m_UpSampleTargets = new RenderTarget2D[MAX_DOWNSAMPLE - 1];

			PrepareMipMapLevels();
		}
		// -----------------------------------------------------------------
		public void PrepareMipMapLevels()
		{
			// create temporary targets:
			// five targets just do store smaller levels
			// five targets to create final result
			// could be done in an array!
			/*m_DownSampleTargets[0] = new RenderTarget2D(
				m_Device,
				m_Width / 2,
				m_Height / 2,
				1,
				SurfaceFormat.HalfVector4);*/

			// pentium : changed all rendertarget's format from hv4 to color
			m_DownSampleTargets[0] = new RenderTarget2D(
				m_Device,
				m_Width / 2,
				m_Height / 2,
				true,
				SurfaceFormat.Color, DepthFormat.Depth24);

			m_DownSampleTargets[1] = new RenderTarget2D(
				m_Device,
				m_Width / 4,
				m_Height / 4,
				true,
				SurfaceFormat.Color, DepthFormat.Depth24);

			m_DownSampleTargets[2] = new RenderTarget2D(
				m_Device,
				m_Width / 8,
				m_Height / 8,
				true,
				SurfaceFormat.Color, DepthFormat.Depth24);

			m_DownSampleTargets[3] = new RenderTarget2D(
				m_Device,
				m_Width / 16,
				m_Height / 16,
				true,
				SurfaceFormat.Color, DepthFormat.Depth24);

			m_DownSampleTargets[4] = new RenderTarget2D(
				m_Device,
				m_Width / 32,
				m_Height / 32,
				true,
				SurfaceFormat.Color, DepthFormat.Depth24);

			m_DownSampleTargets[5] = new RenderTarget2D(
				m_Device,
				m_Width / 64,
				m_Height / 64,
				true,
				SurfaceFormat.Color, DepthFormat.Depth24);

			// Temporary Targets
			m_TempTargets[0] = new RenderTarget2D(
				m_Device,
				m_Width / 2,
				m_Height / 2,
				true,
				SurfaceFormat.Color, DepthFormat.Depth24);

			m_TempTargets[1] = new RenderTarget2D(
				m_Device,
				m_Width / 4,
				m_Height / 4,
				true,
				SurfaceFormat.Color, DepthFormat.Depth24);

			m_TempTargets[2] = new RenderTarget2D(
				m_Device,
				m_Width / 8,
				m_Height / 8,
				true,
				SurfaceFormat.Color, DepthFormat.Depth24);

			m_TempTargets[3] = new RenderTarget2D(
				m_Device,
				m_Width / 16,
				m_Height / 16,
				true,
				SurfaceFormat.Color, DepthFormat.Depth24);

			m_TempTargets[4] = new RenderTarget2D(
				m_Device,
				m_Width / 32,
				m_Height / 32,
			true,
				SurfaceFormat.Color, DepthFormat.Depth24);

			m_TempTargets[5] = new RenderTarget2D(
				m_Device,
				m_Width / 64,
				m_Height / 64,
				true,
				SurfaceFormat.Color, DepthFormat.Depth24);

			// Upsample Targets
			m_UpSampleTargets[0] = new RenderTarget2D(
				m_Device,
				m_Width / 2,
				m_Height / 2,
				true,
				SurfaceFormat.Color, DepthFormat.Depth24);

			m_UpSampleTargets[1] = new RenderTarget2D(
				m_Device,
				m_Width / 4,
				m_Height / 4,
				true,
				SurfaceFormat.Color, DepthFormat.Depth24);

			m_UpSampleTargets[2] = new RenderTarget2D(
				m_Device,
				m_Width / 8,
				m_Height / 8,
				true,
				SurfaceFormat.Color, DepthFormat.Depth24);

			m_UpSampleTargets[3] = new RenderTarget2D(
				m_Device,
				m_Width / 16,
				m_Height / 16,
				true,
				SurfaceFormat.Color, DepthFormat.Depth24);

			m_UpSampleTargets[4] = new RenderTarget2D(
				m_Device,
				m_Width / 32,
				m_Height / 32,
				true,
				SurfaceFormat.Color, DepthFormat.Depth24);
		}
		// -----------------------------------------------------------------
		public RenderTarget2D RenderTarget
		{
			get
			{
				return m_RenderTarget;
			}
			set
			{
				m_RenderTarget = value;
			}
		}
		// -----------------------------------------------------------------
		public RenderTarget2D GetTarget(int MipMapLevel)
		{
			if (MipMapLevel == 0) {
				return m_RenderTarget;
			} else {
				return m_DownSampleTargets[MipMapLevel - 1];
			}
		}
		// -----------------------------------------------------------------
		public Texture2D GetTexture(int MipMapLevel)
		{
			if (MipMapLevel == 0) {
				return m_RenderTarget;
			} else {
				return m_DownSampleTargets[MipMapLevel - 1];
			}
		}
		// -----------------------------------------------------------------
		public void GenerateMipMapLevels(ref RenderTarget2D rt)
		{
			// here we create five smaller versions of the original target
			// the gaussian filter is used here (3x3 tap)
			Vector2 TextureSize = new Vector2(m_Width, m_Height);
			Rectangle Rect = new Rectangle(
				0,
				0,
				(int)m_Width,
				(int)m_Height);

			//m_Device.SetRenderTarget( 0, null );
			Texture2D CurrentTex = m_RenderTarget;
			for (int i = 0; i < MAX_DOWNSAMPLE; ++i) {
				TextureSize /= 2f;
				Rect.Width /= 2;
				Rect.Height /= 2;

				m_Device.SetRenderTarget(m_DownSampleTargets[i]);
				//m_Device.Clear(Color.Black);
				m_Device.Clear(Color.Transparent);
				m_Device.Textures[0] = CurrentTex;
				m_Effect.CurrentTechnique = m_Effect.Techniques[0];
				m_Effect.Parameters["gTextureSize"].SetValue(TextureSize);
				//m_Effect.Begin();
				
				EffectPass pass = m_Effect.CurrentTechnique.Passes[0];
				/*m_SpriteBatch.Begin(
					SpriteBlendMode.None,
					SpriteSortMode.Immediate,
					SaveStateMode.SaveState);*/

				m_SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);
				//m_SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
//m_Device.SamplerStates[0] = SamplerState.LinearClamp;
m_Device.SamplerStates[0] = SamplerState.PointClamp;
m_Device.SamplerStates[1] = SamplerState.PointClamp;
m_Device.SamplerStates[2] = SamplerState.PointClamp;
				//pass.Begin();
				m_Effect.CurrentTechnique.Passes[0].Apply();

				m_SpriteBatch.Draw(CurrentTex, Rect, Color.White);

				//pass.End();
				m_SpriteBatch.End();
				//m_Effect.End();

				m_Device.SetRenderTarget(null);

				// debug
				/*if (JoyStick.IsOnKeyDown(8)) {
					using (Stream stream = File.OpenWrite("sundebug\\sun_currenttex" + i + ".png")) {
						//CurrentTex.SaveAsPng(stream, CurrentTex.Width, CurrentTex.Height);
						m_DownSampleTargets[i].SaveAsJpeg(stream, m_DownSampleTargets[i].Width, m_DownSampleTargets[i].Height);

						stream.Position = 0;
					}
				}*/

				CurrentTex = m_DownSampleTargets[i];
				m_Device.SamplerStates[0] = SamplerState.LinearWrap;
			} // for 
			m_Device.SetRenderTarget(null);//m_DownSampleTargets

			/*if (JoyStick.IsOnKeyDown(8)) {

				using (Stream stream = File.OpenWrite("sundebug\\sun_downsample5_before.png")) {
					m_DownSampleTargets[5].SaveAsPng(stream, m_DownSampleTargets[5].Width, m_DownSampleTargets[5].Height);
					stream.Position = 0;
				}
				using (Stream stream = File.OpenWrite("sundebug\\sun_downsample0_before.png")) {
					m_DownSampleTargets[0].SaveAsPng(stream, m_DownSampleTargets[0].Width, m_DownSampleTargets[0].Height);
					stream.Position = 0;
				}
			}*/
		}
		// -----------------------------------------------------------------
		public void AdditiveBlend()
		{
			// this is one ugly ass method: 
			// all the blurred targets are combined to one target, again

			m_Device.Textures[ 0 ] = m_DownSampleTargets[ 5 ];
			m_Device.Textures[ 1 ] = m_DownSampleTargets[ 4 ];
			if (JoyStick.IsOnKeyDown(8)) {
				using (Stream stream = File.OpenWrite("sundebug\\sun_downsample5.png")) {
					m_DownSampleTargets[5].SaveAsPng(stream, m_DownSampleTargets[5].Width, m_DownSampleTargets[5].Height);
					stream.Position = 0;
				}
				using (Stream stream = File.OpenWrite("sundebug\\sun_downsample0.png")) {
					m_DownSampleTargets[0].SaveAsPng(stream, m_DownSampleTargets[0].Width, m_DownSampleTargets[0].Height);
					stream.Position = 0;
				}
			}


			// 1/64x1/64->1/32x1/32
			m_Device.SetRenderTarget(m_UpSampleTargets[4]);
			// pentium : changed all paras from Black to Transparent
			m_Device.Clear(Color.Transparent);
			//m_Device.Clear(Color.Black);

			/*m_SpriteBatch.Begin(
				SpriteBlendMode.Additive,
				SpriteSortMode.Immediate,
				SaveStateMode.SaveState);*/
			
			m_SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);
			m_Device.SamplerStates[0] = SamplerState.PointClamp;
			m_SpriteBatch.Draw(
				m_DownSampleTargets[5],
				new Rectangle(0, 0, m_Width / 32, m_Height / 32),
				Color.White);
			m_SpriteBatch.Draw(
				m_DownSampleTargets[4],
				new Rectangle(0, 0, m_Width / 32, m_Height / 32),
				Color.White);
			m_SpriteBatch.End();
			m_Device.SetRenderTarget(null);

			// 1/32x1/32->1/16x1/16
			m_Device.SetRenderTarget(m_UpSampleTargets[3]);
			m_Device.Clear(Color.Transparent);
			m_SpriteBatch.Begin(
				SpriteSortMode.Immediate, BlendState.Additive);
			m_SpriteBatch.Draw(
				m_UpSampleTargets[4],
				new Rectangle(0, 0, m_Width / 16, m_Height / 16),
				Color.White);
			m_SpriteBatch.Draw(
				m_DownSampleTargets[3],
				new Rectangle(0, 0, m_Width / 16, m_Height / 16),
				Color.White);
			m_SpriteBatch.End();

			// 1/16x1/16->1/8x1/8
			m_Device.SetRenderTarget(m_UpSampleTargets[2]);
			m_Device.Clear(Color.Transparent);
			m_SpriteBatch.Begin(
				SpriteSortMode.Immediate, BlendState.Additive);
			m_SpriteBatch.Draw(
				m_UpSampleTargets[3],
				new Rectangle(0, 0, m_Width / 8, m_Height / 8),
				Color.White);
			m_SpriteBatch.Draw(
				m_DownSampleTargets[2],
				new Rectangle(0, 0, m_Width / 8, m_Height / 8),
				Color.White);
			m_SpriteBatch.End();

			// 1/8x1/8->1/4x1/4
			m_Device.SetRenderTarget(m_UpSampleTargets[1]);
			m_Device.Clear(Color.Transparent);
			m_SpriteBatch.Begin(
				SpriteSortMode.Immediate, BlendState.Additive);
			m_SpriteBatch.Draw(
				m_UpSampleTargets[2],
				new Rectangle(0, 0, m_Width / 4, m_Height / 4),
				Color.White);
			m_SpriteBatch.Draw(
				m_DownSampleTargets[1],
				new Rectangle(0, 0, m_Width / 4, m_Height / 4),
				Color.White);
			m_SpriteBatch.End();

			// 1/4x1/4->1/2x1/2
			m_Device.SetRenderTarget(m_UpSampleTargets[0]);
			m_Device.Clear(Color.Transparent);
			m_SpriteBatch.Begin(
				SpriteSortMode.Immediate, BlendState.Additive);
			m_SpriteBatch.Draw(
				m_UpSampleTargets[1],
				new Rectangle(0, 0, m_Width / 2, m_Height / 2),
				Color.White);
			m_SpriteBatch.Draw(
				m_DownSampleTargets[0],
				new Rectangle(0, 0, m_Width / 2, m_Height / 2),
				Color.White);
			m_SpriteBatch.End();

			// 1/2x1/2->1x1
			m_Device.SetRenderTarget(m_RenderTarget);
			m_Device.Clear(Color.Transparent);
			m_SpriteBatch.Begin(
				SpriteSortMode.Immediate, BlendState.Additive);
			m_SpriteBatch.Draw(
				m_UpSampleTargets[0],
				new Rectangle(0, 0, m_Width, m_Height),
				Color.White);
			m_SpriteBatch.End();

			m_Device.SetRenderTarget(null);

			if (JoyStick.IsOnKeyDown(8)) {
				using (Stream stream = File.OpenWrite("sundebug\\sun_upsample4.png")) {
					m_UpSampleTargets[4].SaveAsPng(stream, m_UpSampleTargets[4].Width, m_UpSampleTargets[4].Height);
					stream.Position = 0;
				}
			}
		}
		// -----------------------------------------------------------------
		public void ApplyBlur(ref Effect effect, Matrix world)
		{
			// apply blur to all scales of the original target
			// again, this is done manually and might be done in
			// an array
			// ps: the temporary targets are used, here

			// custom spritebatch vertexshader
			Vector2 Size;

			/*
			 * copy Rendertargets to temporary buffers
			 */
			m_Device.SetRenderTarget(m_TempTargets[5]);
			// changed black to transparent
			m_Device.Clear(Color.Transparent);
			m_SpriteBatch.Begin();
			m_Device.SamplerStates[0] = SamplerState.PointClamp;
			m_SpriteBatch.Draw(m_DownSampleTargets[5],
				Vector2.Zero,
				Color.White);
			m_SpriteBatch.End();

			m_Device.SetRenderTarget(m_TempTargets[4]);
			m_Device.Clear(Color.Transparent);
			m_SpriteBatch.Begin();
			m_Device.SamplerStates[0] = SamplerState.PointClamp;
			m_SpriteBatch.Draw(m_DownSampleTargets[4],
				Vector2.Zero,
				Color.White);
			m_SpriteBatch.End();

			m_Device.SetRenderTarget(m_TempTargets[3]);
			m_Device.Clear(Color.Transparent);
			m_SpriteBatch.Begin();
			m_SpriteBatch.Draw(m_DownSampleTargets[3],
				Vector2.Zero,
				Color.White);
			m_SpriteBatch.End();

			m_Device.SetRenderTarget(m_TempTargets[2]);
			m_Device.Clear(Color.Transparent);
			m_SpriteBatch.Begin();
			m_SpriteBatch.Draw(m_DownSampleTargets[2],
				Vector2.Zero,
				Color.White);
			m_SpriteBatch.End();

			m_Device.SetRenderTarget(m_TempTargets[1]);
			m_Device.Clear(Color.Transparent);
			m_SpriteBatch.Begin();
			m_SpriteBatch.Draw(m_DownSampleTargets[1],
				Vector2.Zero,
				Color.White);
			m_SpriteBatch.End();

			m_Device.SetRenderTarget(m_TempTargets[0]);
			m_Device.Clear(Color.Transparent);
			m_SpriteBatch.Begin();
			m_SpriteBatch.Draw(m_DownSampleTargets[0],
				Vector2.Zero,
				Color.White);
			m_SpriteBatch.End();



			/*
			 * render temporary buffers to downscaled targets
			 */
			Size.X = m_Width / 64;
			Size.Y = m_Height / 64;

			m_Device.SetRenderTarget(m_DownSampleTargets[5]);
			//m_Device.Clear(Color.Black);
			m_Device.Clear(Color.Transparent);
			m_Device.Textures[0] = m_TempTargets[5];
			effect.CurrentTechnique = effect.Techniques[BLUR_QUALITY];
			effect.Parameters["ViewportSize"].SetValue(Size);
			effect.Parameters["TextureSize"].SetValue(Size);
			effect.Parameters["MatrixTransform"].SetValue(Matrix.Identity);
			//effect.Parameters["MatrixTransform"].SetValue(world);
			effect.Parameters["gScreenWidth"].SetValue(Size.X);
			effect.Parameters["gScreenHeight"].SetValue(Size.Y);
			effect.Parameters["gScaleFactor"].SetValue(SCALE_FACTOR);
			//effect.Begin();
			EffectPass pass = effect.CurrentTechnique.Passes[0];
			m_SpriteBatch.Begin(
				SpriteSortMode.Immediate, BlendState.Additive);
			//pass.Begin();
			pass.Apply();
			m_Device.SamplerStates[0] = SamplerState.PointClamp;
			m_SpriteBatch.Draw(m_TempTargets[5],
				Vector2.Zero,
				Color.White);
			//pass.End();
			m_SpriteBatch.End();
			//effect.End();

			Size.X = m_Width / 32;
			Size.Y = m_Height / 32;
			m_Device.SetRenderTarget(m_DownSampleTargets[4]);
			m_Device.Clear(Color.Transparent);
			m_Device.Textures[0] = m_TempTargets[4];
			effect.CurrentTechnique = effect.Techniques[BLUR_QUALITY];
			effect.Parameters["ViewportSize"].SetValue(Size);
			effect.Parameters["TextureSize"].SetValue(Size);
			effect.Parameters["MatrixTransform"].SetValue(Matrix.Identity);
			effect.Parameters["gScreenWidth"].SetValue(Size.X);
			effect.Parameters["gScreenHeight"].SetValue(Size.Y);
			effect.Parameters["gScaleFactor"].SetValue(SCALE_FACTOR);
			//effect.Begin();
			pass = effect.CurrentTechnique.Passes[0];
			m_SpriteBatch.Begin(
				SpriteSortMode.Immediate, BlendState.Additive);
			//pass.Begin();
			pass.Apply();
			m_Device.SamplerStates[0] = SamplerState.PointClamp;
			m_SpriteBatch.Draw(m_TempTargets[4],
				Vector2.Zero,
				Color.White);
			//pass.End();
			m_SpriteBatch.End();
			//effect.End();

			Size.X = m_Width / 16;
			Size.Y = m_Height / 16;
			m_Device.SetRenderTarget(m_DownSampleTargets[3]);
			m_Device.Clear(Color.Transparent);
			m_Device.Textures[0] = m_TempTargets[3];
			effect.CurrentTechnique = effect.Techniques[BLUR_QUALITY];
			effect.Parameters["ViewportSize"].SetValue(Size);
			effect.Parameters["TextureSize"].SetValue(Size);
			effect.Parameters["MatrixTransform"].SetValue(Matrix.Identity);
			effect.Parameters["gScreenWidth"].SetValue(Size.X);
			effect.Parameters["gScreenHeight"].SetValue(Size.Y);
			effect.Parameters["gScaleFactor"].SetValue(SCALE_FACTOR);
			//effect.Begin();
			pass = effect.CurrentTechnique.Passes[0];
			m_SpriteBatch.Begin(
				SpriteSortMode.Immediate, BlendState.Additive);
			//pass.Begin();
			pass.Apply();
			m_Device.SamplerStates[0] = SamplerState.PointClamp;
			m_SpriteBatch.Draw(m_TempTargets[3],
				Vector2.Zero,
				Color.White);
			//pass.End();
			m_SpriteBatch.End();
			//effect.End();

			Size.X = m_Width / 8;
			Size.Y = m_Height / 8;
			m_Device.SetRenderTarget(m_DownSampleTargets[2]);
			m_Device.Clear(Color.Transparent);
			m_Device.Textures[0] = m_TempTargets[2];
			effect.CurrentTechnique = effect.Techniques[BLUR_QUALITY];
			effect.Parameters["ViewportSize"].SetValue(Size);
			effect.Parameters["TextureSize"].SetValue(Size);
			effect.Parameters["MatrixTransform"].SetValue(Matrix.Identity);
			effect.Parameters["gScreenWidth"].SetValue(Size.X);
			effect.Parameters["gScreenHeight"].SetValue(Size.Y);
			effect.Parameters["gScaleFactor"].SetValue(SCALE_FACTOR);
			//effect.Begin();
			pass = effect.CurrentTechnique.Passes[0];
			m_SpriteBatch.Begin(
				SpriteSortMode.Immediate, BlendState.Additive);
			//pass.Begin();
			pass.Apply();
			m_Device.SamplerStates[0] = SamplerState.PointClamp;
			m_SpriteBatch.Draw(m_TempTargets[2],
				Vector2.Zero,
				Color.White);
			//pass.End();
			m_SpriteBatch.End();
			//effect.End();

			Size.X = m_Width / 4;
			Size.Y = m_Height / 4;
			m_Device.SetRenderTarget(m_DownSampleTargets[1]);
			m_Device.Clear(Color.Transparent);
			m_Device.Textures[0] = m_TempTargets[1];
			effect.CurrentTechnique = effect.Techniques[BLUR_QUALITY];
			effect.Parameters["ViewportSize"].SetValue(Size);
			effect.Parameters["TextureSize"].SetValue(Size);
			effect.Parameters["MatrixTransform"].SetValue(Matrix.Identity);
			effect.Parameters["gScreenWidth"].SetValue(Size.X);
			effect.Parameters["gScreenHeight"].SetValue(Size.Y);
			effect.Parameters["gScaleFactor"].SetValue(SCALE_FACTOR);
			//effect.Begin();
			pass = effect.CurrentTechnique.Passes[0];
			m_SpriteBatch.Begin(
				SpriteSortMode.Immediate, BlendState.Additive);
			//pass.Begin();
			pass.Apply();
			m_Device.SamplerStates[0] = SamplerState.PointClamp;
			m_SpriteBatch.Draw(m_TempTargets[1],
				Vector2.Zero,
				Color.White);
			//pass.End();
			m_SpriteBatch.End();
			//effect.End();



			m_Device.SetRenderTarget(null);
			Size.X = m_Width / 2;
			Size.Y = m_Height / 2;
			m_Device.SetRenderTarget(m_DownSampleTargets[0]);
			m_Device.Clear(Color.Transparent);
			m_Device.SamplerStates[0] = SamplerState.LinearClamp;
			m_Device.Textures[0] = m_TempTargets[0];
			effect.CurrentTechnique = effect.Techniques[BLUR_QUALITY];
			effect.Parameters["ViewportSize"].SetValue(Size);
			effect.Parameters["TextureSize"].SetValue(Size);
			effect.Parameters["MatrixTransform"].SetValue(Matrix.Identity);
			effect.Parameters["gScreenWidth"].SetValue(Size.X);
			effect.Parameters["gScreenHeight"].SetValue(Size.Y);
			effect.Parameters["gScaleFactor"].SetValue(SCALE_FACTOR);// blurのスケール

			effect.Parameters["down0"].SetValue(m_TempTargets[0]);
			pass = effect.CurrentTechnique.Passes[0];
			m_SpriteBatch.Begin(
				SpriteSortMode.Immediate, BlendState.Additive);
			pass.Apply();
			m_Device.SamplerStates[0] = SamplerState.PointClamp;
			m_SpriteBatch.Draw(m_TempTargets[0],
				Vector2.Zero,
				Color.White);
			m_SpriteBatch.End();
			m_Device.SetRenderTarget(null);



			/*Size.X = m_Width / 2;
			Size.Y = m_Height / 2;
			//m_Device.SetRenderTarget(m_DownSampleTargets[0]);
			m_Device.Clear(Color.Black);
			m_Device.Textures[0] = m_TempTargets[0];
			m_Device.SamplerStates[0] = SamplerState.LinearClamp;

			effect.CurrentTechnique = effect.Techniques[BLUR_QUALITY];
			effect.Parameters["ViewportSize"].SetValue(Size);
			effect.Parameters["TextureSize"].SetValue(Size);
			effect.Parameters["MatrixTransform"].SetValue(Matrix.Identity);
			//effect.Parameters["MatrixTransform"].SetValue(world);
			effect.Parameters["gScreenWidth"].SetValue(Size.X);
			effect.Parameters["gScreenHeight"].SetValue(Size.Y);
			effect.Parameters["gScaleFactor"].SetValue(SCALE_FACTOR);// blurのスケール
			effect.Parameters["down0"].SetValue(m_TempTargets[0]);
			//effect.Begin();
			pass = effect.CurrentTechnique.Passes[0];
			m_SpriteBatch.Begin(
				SpriteSortMode.Immediate, BlendState.NonPremultiplied);
			//pass.Begin();
			pass.Apply();
			m_Device.SamplerStates[0] = SamplerState.PointClamp;
			m_SpriteBatch.Draw(m_TempTargets[0],
				Vector2.Zero,
				Color.White);
			//pass.End();
			m_SpriteBatch.End();
			//effect.End();
			m_Device.SetRenderTarget(null);*/

			/*if (JoyStick.IsOnKeyDown(8)) {
				using (Stream stream = File.OpenWrite("sundebug\\sun_downsample0_tmp.png")) {
					m_TempTargets[0].SaveAsPng(stream, m_TempTargets[0].Width, m_TempTargets[0].Height);
					stream.Position = 0;
				}
				using (Stream stream = File.OpenWrite("sundebug\\sun_downsample0.png")) {
					m_DownSampleTargets[0].SaveAsPng(stream, m_DownSampleTargets[0].Width, m_DownSampleTargets[0].Height);
					stream.Position = 0;
				}
			}*/
		}
		// -----------------------------------------------------------------
		public void SaveTextures(String Directory, String Prefix)
		{
			/*
			for( int i = 0; i < MAX_DOWNSAMPLE + 1; ++i )
			{
				Texture2D tex;
				if( i == 0 )
				{
					tex = m_RenderTarget.GetTexture( );
				}
				else
				{
					tex = m_DownSampleTargets[ i - 1 ].GetTexture( );
				}
				tex.Save( "C:/test" + i + ".dds", ImageFileFormat.Dds );
			}
			*/
			for (int i = 0; i < MAX_DOWNSAMPLE - 1; ++i) {
				Texture2D tex;
				tex = m_DownSampleTargets[i];
				//tex.Save("C:/test" + i + ".dds", ImageFileFormat.Dds);
			} // for
		}
		// -----------------------------------------------------------------
	}
}