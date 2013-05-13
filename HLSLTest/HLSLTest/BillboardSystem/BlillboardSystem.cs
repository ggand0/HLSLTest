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
	public enum BillboardMode
	{
		Cylindrical,
		Spherical,
		Cross,
		Line,
	};
	public class BillboardSystem : Drawable
	{
		// Vertex buffer and index buffer, particle and index arrays
		protected VertexBuffer vertexBuffers;
		protected IndexBuffer indexBuffers;
		protected VertexPositionTexture[] particles;
		protected int[] indices;

		// GraphicsDevice and Effect
		protected GraphicsDevice graphicsDevice;
		protected Effect effect;

		// Billboard settings
		public int BillboardNum { get; private set; }
		public Vector2 BillboardSize { get; private set; }
		public Texture2D Texture { get; private set; }
		/// <summary>
		/// ソフトパーティクルを使うかどうか
		/// </summary>
		public int DepthMode { get; set; }
		private RenderTarget2D depthTarget;
		private Effect depthEffect;
		private List<Object> Models;

		
		public bool EnsureOcclusion { get; set; }
		
		public BillboardMode Mode { get; set; }


		protected void DrawOpaquePixels()
		{
			graphicsDevice.DepthStencilState = DepthStencilState.Default;
			//
			if (DepthMode == 1) {
				//BlendState bs = BlendState.AlphaBlend;
				//graphicsDevice.BlendState = BlendState.;
			}
			effect.Parameters["AlphaTest"].SetValue(true);
			effect.Parameters["AlphaTestGreater"].SetValue(true);
			DrawBillboards();
		}
		protected void DrawBillboards()
		{
			effect.CurrentTechnique.Passes[0].Apply();
			graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0,
			4 * BillboardNum, 0, BillboardNum * 2);
		}
		protected void DrawTransparentPixels()
		{
			graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
			effect.Parameters["AlphaTest"].SetValue(true);
			effect.Parameters["AlphaTestGreater"].SetValue(false);
			DrawBillboards();
		}
		protected virtual void generateParticles(Vector3[] particlePositions)
		{
			// Create vertex and index arrays
			particles = new VertexPositionTexture[BillboardNum * 4];
			indices = new int[BillboardNum * 6];
			int x = 0;

			// For each billboard...
			for (int i = 0; i < BillboardNum * 4; i += 4) {
				Vector3 pos = particlePositions[i / 4];

				// Add 4 vertices at the billboard's position
				particles[i + 0] = new VertexPositionTexture(pos,
					new Vector2(0, 0));
				particles[i + 1] = new VertexPositionTexture(pos,
					new Vector2(0, 1));
				particles[i + 2] = new VertexPositionTexture(pos,
					new Vector2(1, 1));
				particles[i + 3] = new VertexPositionTexture(pos,
					new Vector2(1, 0));

				// Add 6 indices to form two triangles
				indices[x++] = i + 0;
				indices[x++] = i + 3;
				indices[x++] = i + 2;
				indices[x++] = i + 2;
				indices[x++] = i + 1;
				indices[x++] = i + 0;
			}

			// Create and set the vertex buffer
			vertexBuffers = new VertexBuffer(graphicsDevice,
				typeof(VertexPositionTexture),
				BillboardNum * 4, BufferUsage.WriteOnly);
			vertexBuffers.SetData<VertexPositionTexture>(particles);
			// Create and set the index buffer
			indexBuffers = new IndexBuffer(graphicsDevice,
			IndexElementSize.ThirtyTwoBits,
			BillboardNum * 6, BufferUsage.WriteOnly);
			indexBuffers.SetData<int>(indices);
		}

		protected virtual void setEffectParameters(Matrix View, Matrix Projection, Vector3 Up, Vector3 Right)
		{
			effect.Parameters["ParticleTexture"].SetValue(Texture);
			effect.Parameters["View"].SetValue(View);
			effect.Parameters["Projection"].SetValue(Projection);
			effect.Parameters["Size"].SetValue(BillboardSize / 2f);
			//effect.Parameters["Up"].SetValue(Up);
			effect.Parameters["Up"].SetValue(Mode == BillboardMode.Spherical ? Up : Vector3.Up);
			effect.Parameters["Side"].SetValue(Right);

			if (DepthMode == 1) {
				effect.Parameters["DepthMap"].SetValue(depthTarget);
			}

			effect.CurrentTechnique.Passes[0].Apply();
		}

		bool hasSaved;
		public void DrawDepth(Matrix View, Matrix Projection, Vector3 CameraPosition)
		{
			graphicsDevice.SetRenderTargets(depthTarget);
			// Clear the render target to 1 (infinite depth)
			graphicsDevice.Clear(Color.White);

			foreach (Object o in Models) {
				o.CacheEffects();// すでにあるエフェクトを上書きさせないために退避させておく
				o.SetModelEffect(depthEffect, false);// 空いたスペースで法線マップをDrawする
				//o.Draw(_gameInstance.camera.View, _gameInstance.camera.Projection, _gameInstance.camera.CameraPosition);
				o.Draw(View, Projection, CameraPosition);
				o.RestoreEffects();// 退避させておいたエフェクトを戻す
			}

			// Un-set the render targets
			graphicsDevice.SetRenderTargets(null);

			/*if (JoyStick.IsOnKeyDown(0)) {
				using (Stream stream = File.OpenWrite("DepthMapForSoftParticles.png")) {
					depthTarget.SaveAsPng(stream, depthTarget.Width, depthTarget.Height);
					stream.Position = 0;
					hasSaved = true;
				}
			}*/
		}
		public override void Update(GameTime gameTime)
		{
		}
		public void Draw(Matrix View, Matrix Projection, Vector3 Up, Vector3 Right)
		{
			// Set the vertex and index buffer to the graphics card
			graphicsDevice.SetVertexBuffer(vertexBuffers);
			graphicsDevice.Indices = indexBuffers;

			setEffectParameters(View, Projection, Up, Right);

			// Enable alpha blending
			graphicsDevice.BlendState = BlendState.AlphaBlend;
			// Draw the billboards
			//graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 4 * nBillboards, 0, nBillboards * 2);
			if (EnsureOcclusion) {
				DrawOpaquePixels();
				DrawTransparentPixels();
			} else {
				graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
				effect.Parameters["AlphaTest"].SetValue(false);
				DrawBillboards();
			}

			// Reset render states
			graphicsDevice.BlendState = BlendState.Opaque;
			graphicsDevice.DepthStencilState = DepthStencilState.Default;

			// Un-set the vertex and index buffer
			graphicsDevice.SetVertexBuffer(null);
			graphicsDevice.Indices = null;
		}


		public BillboardSystem(GraphicsDevice graphicsDevice,
			ContentManager content, Texture2D texture, Vector2 billboardSize, Vector3[] particlePositions)
			:this(graphicsDevice, content, texture, 0, null, billboardSize, particlePositions)
		{
		}
		public BillboardSystem(GraphicsDevice graphicsDevice,
			ContentManager content, Texture2D texture, int depthMode, List<Object> Models, Vector2 billboardSize, Vector3[] particlePositions)
		{
			this.BillboardNum = particlePositions.Length;
			this.BillboardSize = billboardSize;
			this.graphicsDevice = graphicsDevice;
			this.Texture = texture;
			this.DepthMode = depthMode;
			this.Models = Models;

			// SoftParticleTestはbillboardeffectを元に実験用に作成したエフェクト
			if (DepthMode == 0) {
				effect = content.Load<Effect>("Billboard\\BillboardEffect");
			} else if (DepthMode == 1) {
				effect = content.Load<Effect>("Billboard\\SoftParticleTest");
			}
			depthEffect = content.Load<Effect>("Billboard\\CreateDepthMapFromCamera");

			generateParticles(particlePositions);
			EnsureOcclusion = true;
			Mode = BillboardMode.Spherical;

			int viewWidth = graphicsDevice.Viewport.Width;
			int viewHeight = graphicsDevice.Viewport.Height;
			/*depthTarget = new RenderTarget2D(graphicsDevice, viewWidth,
				viewHeight, false, SurfaceFormat.Single, DepthFormat.Depth24);*/
			depthTarget = new RenderTarget2D(graphicsDevice, viewWidth,
				viewHeight, false, SurfaceFormat.Vector4, DepthFormat.Depth24);/**/
		}
	}
}
