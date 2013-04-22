using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace HLSLTest
{
	public class BillboardSystem
	{
		// Vertex buffer and index buffer, particle
		// and index arrays
		VertexBuffer verts;
		IndexBuffer ints;
		VertexPositionTexture[] particles;
		int[] indices;
		// Billboard settings
		int nBillboards;
		Vector2 billboardSize;
		Texture2D texture;
		// GraphicsDevice and Effect
		GraphicsDevice graphicsDevice;
		Effect effect;

		void generateParticles(Vector3[] particlePositions)
		{
			// Create vertex and index arrays
			particles = new VertexPositionTexture[nBillboards * 4];
			indices = new int[nBillboards * 6];
			int x = 0;
			// For each billboard...
			for (int i = 0; i < nBillboards * 4; i += 4) {
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
			verts = new VertexBuffer(graphicsDevice,
				typeof(VertexPositionTexture),
				nBillboards * 4, BufferUsage.WriteOnly);
			verts.SetData<VertexPositionTexture>(particles);
			// Create and set the index buffer
			ints = new IndexBuffer(graphicsDevice,
			IndexElementSize.ThirtyTwoBits,
			nBillboards * 6, BufferUsage.WriteOnly);
			ints.SetData<int>(indices);
		}

		void setEffectParameters(Matrix View, Matrix Projection, Vector3 Up,
			Vector3 Right)
		{
			effect.Parameters["ParticleTexture"].SetValue(texture);
			effect.Parameters["View"].SetValue(View);
			effect.Parameters["Projection"].SetValue(Projection);
			effect.Parameters["Size"].SetValue(billboardSize / 2f);
			effect.Parameters["Up"].SetValue(Up);
			effect.Parameters["Side"].SetValue(Right);
			effect.CurrentTechnique.Passes[0].Apply();
		}
		public void Draw(Matrix View, Matrix Projection, Vector3 Up, Vector3 Right)
		{
			// Set the vertex and index buffer to the graphics card
			graphicsDevice.SetVertexBuffer(verts);
			graphicsDevice.Indices = ints;

			setEffectParameters(View, Projection, Up, Right);

			// Draw the billboards
			graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList,
			0, 0, 4 * nBillboards, 0, nBillboards * 2);
			// Un-set the vertex and index buffer
			graphicsDevice.SetVertexBuffer(null);
			graphicsDevice.Indices = null;
		}

		public BillboardSystem(GraphicsDevice graphicsDevice,
			ContentManager content, Texture2D texture, Vector2 billboardSize, Vector3[] particlePositions)
		{
			this.nBillboards = particlePositions.Length;
			this.billboardSize = billboardSize;
			this.graphicsDevice = graphicsDevice;
			this.texture = texture;
			effect = content.Load<Effect>("BillboardEffect");

			generateParticles(particlePositions);
		}
	}
}
