using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace HLSLTest
{
	public class LaserBillboard : Drawable
	{
		// Vertex buffer and index buffer, particle
		// and index arrays
		protected VertexBuffer vertexBuffers;
		protected IndexBuffer indexBuffers;
		//protected ParticleVertex[] particles;
		protected ParticleVertex[] particles;


		protected int[] indices;
		// Billboard settings
		protected int nBillboards;
		protected Vector2 billboardSize;
		protected Texture2D texture;
		// GraphicsDevice and Effect
		protected GraphicsDevice graphicsDevice;
		protected Effect effect;
		public bool EnsureOcclusion = true;
		public enum BillboardMode { Cylindrical, Spherical };
		public BillboardMode Mode = BillboardMode.Spherical;

		void GenerateParticles(Vector3[] particlePositions)
		{
			// Create vertex and index arrays
			particles = new ParticleVertex[nBillboards * 4];
			indices = new int[nBillboards * 6];
			int x = 0;
			Vector3 z = Vector3.Zero;
			// For each billboard...
			for (int i = 0; i < nBillboards * 4; i += 4) {
				Vector3 pos = particlePositions[i / 4];
				// Add 4 vertices at the billboard's position
				particles[i + 0] = new ParticleVertex(pos, new Vector2(0, 0), z, 0, -1, 0, End);
				particles[i + 1] = new ParticleVertex(pos, new Vector2(0, 1), z, 0, -1, 0, End);
				particles[i + 2] = new ParticleVertex(pos, new Vector2(1, 1), z, 0, -1, 0, End);
				particles[i + 3] = new ParticleVertex(pos, new Vector2(1, 0), z, 0, -1, 0, End);

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
				typeof(ParticleVertex),
				nBillboards * 4, BufferUsage.WriteOnly);
			vertexBuffers.SetData<ParticleVertex>(particles);
			// Create and set the index buffer
			indexBuffers = new IndexBuffer(graphicsDevice,
				IndexElementSize.ThirtyTwoBits,
				nBillboards * 6, BufferUsage.WriteOnly);
			indexBuffers.SetData<int>(indices);
		}

		void SetEffectParameters(Matrix View, Matrix Projection, Vector3 Up,
			Vector3 Right, Vector3 CameraPosition)
		{
			effect.Parameters["ParticleTexture"].SetValue(texture);
			effect.Parameters["View"].SetValue(View);
			effect.Parameters["Projection"].SetValue(Projection);

			//effect.Parameters["Size"].SetValue(new Vector2(billboardSize.X / 2f, (End - Start).Length() ));
			effect.Parameters["Size"].SetValue(billboardSize / 2f);

			effect.Parameters["Up"].SetValue(Up);
			effect.Parameters["Side"].SetValue(Right);
			/*effect.Parameters["Up"].SetValue(Mode == BillboardMode.Spherical ? Up : Vector3.Up);
			effect.Parameters["Side"].SetValue(Right);*/

			effect.CurrentTechnique.Passes[0].Apply();

		}

		protected void UpdateParticles()
		{
			//float now = (float)(DateTime.Now - start).TotalSeconds;
			//int startIndex = activeStart;
			//int end = activeParticlesNum;

			// For each particle marked as active...
			for (int i = 0; i < nBillboards * 4; i++) {
				particles[i].StartPosition += Vector3.Normalize(End - Start) * 3;
			}

			// Update the vertex and index buffers
			vertexBuffers.SetData<ParticleVertex>(particles);
			indexBuffers.SetData<int>(indices);
		}

		public void Draw(Matrix View, Matrix Projection, Vector3 Up, Vector3 Right, Vector3 CameraPosition)
		{
			// Set the vertex and index buffer to the graphics card
			graphicsDevice.SetVertexBuffer(vertexBuffers);
			graphicsDevice.Indices = indexBuffers;

			SetEffectParameters(View, Projection, Up, Right, CameraPosition);

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
		public override void Draw(Camera camera)
		{
			//base.Draw(camera);
			this.Draw(camera.View, camera.Projection, camera.Up, camera.Right, camera.Position);
		}

		void DrawOpaquePixels()
		{
			graphicsDevice.DepthStencilState = DepthStencilState.Default;
			//graphicsDevice.DepthStencilState = DepthStencilState.None;
			graphicsDevice.RasterizerState = RasterizerState.CullNone;
			effect.Parameters["AlphaTest"].SetValue(true);
			effect.Parameters["AlphaTestGreater"].SetValue(true);
			DrawBillboards();
		}
		void DrawBillboards()
		{
			effect.CurrentTechnique.Passes[0].Apply();
			graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0,
				4 * nBillboards, 0, nBillboards * 2);
		}
		void DrawTransparentPixels()
		{
			graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
			effect.Parameters["AlphaTest"].SetValue(true);
			effect.Parameters["AlphaTestGreater"].SetValue(false);
			DrawBillboards();
		}

		public Vector3 Start { get; private set; }
		public Vector3 End { get; private set; }
		public Vector3 Mid { get; private set; }
		//public Vector3 CameraPos { get44
		public LaserBillboard(GraphicsDevice graphicsDevice,
			ContentManager content, Texture2D texture, Vector2 billboardSize, Vector3 start, Vector3 end)//, Vector3[] particlePositions)
		{
			//this.nBillboards = particlePositions.Length;
			this.nBillboards = 1;
			this.billboardSize = billboardSize;
			this.graphicsDevice = graphicsDevice;
			this.texture = texture;
			this.Start = start;
			this.End = end;
			//effect = content.Load<Effect>("LaserBillboardEffectV3");
			effect = content.Load<Effect>("Billboard\\LaserBillboardEffectV4");

			Mid = (start + end) / 2.0f;
			//generateParticles(particlePositions);


			//GenerateParticles(new Vector3[] { Mid });
			GenerateParticles(new Vector3[] { Mid });
		}
	}
}
