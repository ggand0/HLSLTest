using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace HLSLTest
{
	public class Terrain
	{
		private VertexPositionNormalTexture[] vertices; // Vertex array
		private VertexBuffer vertexBuffer; // Vertex buffer
		private int[] indices; // Index array
		private IndexBuffer indexBuffer; // Index buffer
		private float[,] heights; // Array of vertex heights
		private float height; // Maximum height of terrain
		private float cellSize; // Distance between vertices on x and z axes
		private int width, length; // Number of vertices on x and z axes
		private int nVertices, nIndices; // Number of vertices and indices
		private Effect effect; // Effect used for rendering
		private GraphicsDevice graphicsDevice; // Graphics device to draw with
		private Texture2D heightMap; // Heightmap texture
		public Texture2D RTexture, BTexture, GTexture, WeightMap;

		private Texture2D baseTexture;
		private float textureTiling;
		private Vector3 lightDirection;

		public Texture2D DetailTexture;
		public float DetailDistance = 2500;//2500;
		public float DetailTextureTiling = 100;

		private void getHeights()
		{
			// Extract pixel data
			Color[] heightMapData = new Color[width * length];
			heightMap.GetData<Color>(heightMapData);

			// Create heights[,] array
			heights = new float[width, length];

			// For each pixel
			for (int y = 0; y < length; y++) {
				for (int x = 0; x < width; x++) {
					// Get color value (0 - 255)
					float amt = heightMapData[y * width + x].R;

					// Scale to (0 - 1)
					amt /= 255.0f;

					// Multiply by max height to get final height
					heights[x, y] = amt * height + BaseHeight;// 指定された高さからのheightにしたver
					//heights[x, y] = amt * height;
				}
			}
		}

		private void CreateVertices()
		{
			vertices = new VertexPositionNormalTexture[nVertices];

			// Calculate the position offset that will center the terrain at (0, 0, 0)
			Vector3 offsetToCenter = -new Vector3(((float)width / 2.0f) * cellSize,
				0, ((float)length / 2.0f) * cellSize);

			// For each pixel in the image
			for (int z = 0; z < length; z++)
				for (int x = 0; x < width; x++) {
					// Find position based on grid coordinates and height in heightmap
					Vector3 position = new Vector3(x * cellSize,
						heights[x, z], z * cellSize) + offsetToCenter;

					// UV coordinates range from (0, 0) at grid location (0, 0) to 
					// (1, 1) at grid location (width, length)
					Vector2 uv = new Vector2((float)x / width, (float)z / length);

					// Create the vertex
					vertices[z * width + x] = new VertexPositionNormalTexture(
						position, Vector3.Zero, uv);
				}
		}

		private void CreateIndices()
		{
			indices = new int[nIndices];

			int i = 0;

			// For each cell
			for (int x = 0; x < width - 1; x++)
				for (int z = 0; z < length - 1; z++) {
					// Find the indices of the corners
					int upperLeft = z * width + x;
					int upperRight = upperLeft + 1;
					int lowerLeft = upperLeft + width;
					int lowerRight = lowerLeft + 1;

					// Specify upper triangle
					indices[i++] = upperLeft;
					indices[i++] = upperRight;
					indices[i++] = lowerLeft;

					// Specify lower triangle
					indices[i++] = lowerLeft;
					indices[i++] = upperRight;
					indices[i++] = lowerRight;
				}
		}

		private void GenerateNormals()
		{
			// For each triangle
			for (int i = 0; i < nIndices; i += 3) {
				// Find the position of each corner of the triangle
				Vector3 v1 = vertices[indices[i]].Position;
				Vector3 v2 = vertices[indices[i + 1]].Position;
				Vector3 v3 = vertices[indices[i + 2]].Position;

				// Cross the vectors between the corners to get the normal
				Vector3 normal = Vector3.Cross(v1 - v2, v1 - v3);
				normal.Normalize();

				// Add the influence of the normal to each vertex in the
				// triangle
				vertices[indices[i]].Normal += normal;
				vertices[indices[i + 1]].Normal += normal;
				vertices[indices[i + 2]].Normal += normal;
			}

			// Average the influences of the triangles touching each
			// vertex
			for (int i = 0; i < nVertices; i++)
				vertices[i].Normal.Normalize();
		}

		public void Draw(bool wireFrame, Matrix View, Matrix Projection)
		{
            if (wireFrame) {
                RasterizerState rs = new RasterizerState();
                rs.FillMode = FillMode.WireFrame;
                graphicsDevice.RasterizerState = rs;
            }

			graphicsDevice.SetVertexBuffer(vertexBuffer);
			graphicsDevice.Indices = indexBuffer;

			effect.Parameters["View"].SetValue(View);
			effect.Parameters["Projection"].SetValue(Projection);
			effect.Parameters["BaseTexture"].SetValue(baseTexture);
			effect.Parameters["TextureTiling"].SetValue(textureTiling);
			effect.Parameters["LightDirection"].SetValue(lightDirection);

			effect.Parameters["RTexture"].SetValue(RTexture);
			effect.Parameters["GTexture"].SetValue(GTexture);
			effect.Parameters["BTexture"].SetValue(BTexture);
			effect.Parameters["WeightMap"].SetValue(WeightMap);

			effect.Parameters["DetailTexture"].SetValue(DetailTexture);
			effect.Parameters["DetailDistance"].SetValue(DetailDistance);
			effect.Parameters["DetailTextureTiling"].SetValue(DetailTextureTiling);

			effect.Techniques[0].Passes[0].Apply();

			graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0,
				nVertices, 0, nIndices / 3);

			// Un-set the buffers
			graphicsDevice.SetVertexBuffer(null);
			graphicsDevice.Indices = null;
            graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
		}

		public float BaseHeight { get; private set; }
		public Terrain(Texture2D HeightMap, float CellSize, float Height,
			GraphicsDevice GraphicsDevice, ContentManager Content)
			:this(HeightMap, CellSize, Height, 0, null, 0, Vector3.Zero, GraphicsDevice, Content)
		{
			Texture2D t = new Texture2D(GraphicsDevice, 2, 2);
			t.SetData<Color>(null);
		}
		public Terrain(Texture2D HeightMap, float CellSize, float Height,
			float baseHeight, Texture2D BaseTexture, float TextureTiling, Vector3 LightDirection,
			GraphicsDevice GraphicsDevice, ContentManager Content)
		{
			this.baseTexture = BaseTexture;
			this.textureTiling = TextureTiling;
			this.lightDirection = LightDirection;

			this.heightMap = HeightMap;
			this.width = HeightMap.Width;
			this.length = HeightMap.Height;
			this.cellSize = CellSize;
			this.BaseHeight = baseHeight;
			this.height = Height;
			this.graphicsDevice = GraphicsDevice;
			effect = Content.Load<Effect>("Terrain\\TerrainEffect");
			effect.Parameters["BaseTexture"].SetValue(baseTexture);
			effect.Parameters["TextureTiling"].SetValue(textureTiling);
			effect.Parameters["LightDirection"].SetValue(lightDirection);

			// 1 vertex per pixel
			nVertices = width * length;
			// (Width-1) * (Length-1) cells, 2 triangles per cell, 3 indices per
			// triangle
			nIndices = (width - 1) * (length - 1) * 6;

			vertexBuffer = new VertexBuffer(GraphicsDevice,
				typeof(VertexPositionNormalTexture), nVertices,
				BufferUsage.WriteOnly);
			indexBuffer = new IndexBuffer(GraphicsDevice,
				IndexElementSize.ThirtyTwoBits,
				nIndices, BufferUsage.WriteOnly);

			// setting the vertices and indices
			getHeights();
			CreateVertices();
			CreateIndices();
			GenerateNormals();

			vertexBuffer.SetData<VertexPositionNormalTexture>(vertices);
			indexBuffer.SetData<int>(indices);
		}
	}
}
