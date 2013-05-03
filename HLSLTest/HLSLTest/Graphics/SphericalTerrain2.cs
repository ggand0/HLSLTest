using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace HLSLTest
{
	public class SphericalTerrain2
	{

		private VertexPositionNormalTexture[][] vertices; // Vertex array
		private VertexPositionNormalTexture[] finalVertices;

		private VertexBuffer[] vertexBuffer; // Vertex buffer
		private int[][] indices; // Index array
		private int[] finalIndices;
		private IndexBuffer[] indexBuffer; // Index buffer


		private float[,] heights; // Array of vertex heights
		private float height; // Maximum height of terrain
		private float cellSize; // Distance between vertices on x and z axes
		private int width, length; // Number of vertices on x and z axes
		private int nVertices, nIndices; // Number of vertices and indices
		private Effect effect; // Effect used for rendering
		private GraphicsDevice graphicsDevice; // Graphics device to draw with
		private Texture2D heightMap; // Heightmap texture
		private TextureCube heightMapCube;
		public Texture2D RTexture, BTexture, GTexture, WeightMap;

		private Texture2D baseTexture;
		private float textureTiling;
		private Vector3 lightDirection;

		public Texture2D DetailTexture;
		public float DetailDistance = 2500;//2500;
		public float DetailTextureTiling = 100;

		public float Radius { get; private set; }
		public Vector3 Center { get; private set; }

		private void GetHeights()
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
		private void GetCubeHeights()
		{
			heightMapCube.GetData<int>(CubeMapFace.NegativeX, );

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

		/// <summary>
		/// 球状に配置したい
		/// </summary>
		private void CreateVertices()
		{
			//vertices = new VertexPositionNormalTexture[6, nVertices];
			vertices = new VertexPositionNormalTexture[6][];
			for (int i = 0; i < 6; i++) {
				vertices[i] = new VertexPositionNormalTexture[nVertices];
			}

			// Calculate the position offset that will center the terrain at (0, 0, 0)
			Vector3 offsetToCenter = -new Vector3(((float)width / 2.0f) * cellSize,
				0, ((float)length / 2.0f) * cellSize);
			Vector3[] offsetToFaces = new Vector3[] {
				new Vector3(0, length/2.0f, 0),// Top
				new Vector3(0, -length/2.0f, 0),// Bottom
				/*new Vector3(0, 0, -length/2),// Back
				new Vector3(0, 0, length/2),// Front
				new Vector3(-width/2, 0, 0),// Left
				new Vector3(width/2, 0, 0),// Right*/
				new Vector3(0, 0, length/2),// Back
				new Vector3(0, 0, -length/2),// Front
				new Vector3(-width/2, 0, 0),// Left
				new Vector3(width/2, 0, 0),// Right
			};


			// For each pixel in the image
			for (int orientation = 0; orientation < 6; orientation++) {
				for (int z = 0; z < length; z++)
					for (int x = 0; x < width; x++) {
						Vector3 v = new Vector3(x * cellSize, heights[x, z], z * cellSize) + offsetToCenter;

						// rotate according to side orientation
						switch (orientation) {
							case 0: break;// nothing
							case 1: v = new Vector3(v.X, -v.Y, -v.Z); break;// Bottom
							case 2: v = new Vector3(v.X, -v.Z, v.Y); break;// Back
							case 3: v = new Vector3(v.X, v.Z, -v.Y); break;// Front
							case 4: v = new Vector3(-v.Y, v.X, v.Z); break;// Left
							case 5: v = new Vector3(v.Y, -v.X, v.Z); break;// Right/**/
							/*case 0: break;// nothing = Top
							case 1: v = new Vector3(v.X, v.Z, -v.Y); break;// Front
							case 2: v = new Vector3(v.X, -v.Y, -v.Z); break;// Bottom
							case 3: v = new Vector3(v.X, -v.Z, v.Y); break;// Back
							case 4: v = new Vector3(-v.Y, v.X, v.Z); break;// Left
							case 5: v = new Vector3(v.Y, -v.X, v.Z); break;// Right*/
						}
						Vector3 position = Center + v + offsetToFaces[orientation];


						// UV coordinates range from (0, 0) at grid location (0, 0) to 
						// (1, 1) at grid location (width, length)
						Vector2 uv = new Vector2((float)x / width, (float)z / length);

						// Create the vertex
						//vertices[orientation, z * width + x] = new VertexPositionNormalTexture(position, Vector3.Zero, uv);
						vertices[orientation][z * width + x] = new VertexPositionNormalTexture(position, Vector3.Zero, uv);
					}
			}
		}

		private void CreateIndices()
		{
			//indices = new int[6, nIndices];
			indices = new int[6][];
			for (int j = 0; j < 6; j++) {
				indices[j] = new int[nIndices];
			}
			int i = 0;

			// For each cell
			for (int orientation = 0; orientation < 6; orientation++) {
				i = 0;
				for (int x = 0; x < width - 1; x++) {
					for (int z = 0; z < length - 1; z++) {
						// Find the indices of the corners
						int upperLeft = z * width + x;
						int upperRight = upperLeft + 1;
						int lowerLeft = upperLeft + width;
						int lowerRight = lowerLeft + 1;

						// Specify upper triangle
						indices[orientation][i++] = upperLeft;
						indices[orientation][i++] = upperRight;
						indices[orientation][i++] = lowerLeft;

						// Specify lower triangle
						indices[orientation][i++] = lowerLeft;
						indices[orientation][i++] = upperRight;
						indices[orientation][i++] = lowerRight;
						/*// Specify upper triangle
						indices[orientation, i++] = upperLeft;
						indices[orientation, i++] = upperRight;
						indices[orientation, i++] = lowerLeft;

						// Specify lower triangle
						indices[orientation, i++] = lowerLeft;
						indices[orientation, i++] = upperRight;
						indices[orientation, i++] = lowerRight;*/
					}
				}
			}
		}

		private void GenerateNormals()
		{
			// For each triangle
			for (int orientation = 0; orientation < 6; orientation++) {
				for (int i = 0; i < nIndices; i += 3) {
					/*// Find the position of each corner of the triangle
					Vector3 v1 = vertices[orientation, indices[orientation, i]].Position;
					Vector3 v2 = vertices[orientation, indices[orientation, i + 1]].Position;
					Vector3 v3 = vertices[orientation, indices[orientation, i + 2]].Position;
					// Cross the vectors between the corners to get the normal
					Vector3 normal = Vector3.Cross(v1 - v2, v1 - v3);
					normal.Normalize();
					// Add the influence of the normal to each vertex in the
					// triangle
					vertices[orientation, indices[orientation, i]].Normal += normal;
					vertices[orientation, indices[orientation, i + 1]].Normal += normal;
					vertices[orientation, indices[orientation, i + 2]].Normal += normal;*/
					// Find the position of each corner of the triangle
					Vector3 v1 = vertices[orientation][indices[orientation][i]].Position;
					Vector3 v2 = vertices[orientation][indices[orientation][i + 1]].Position;
					Vector3 v3 = vertices[orientation][indices[orientation][i + 2]].Position;
					// Cross the vectors between the corners to get the normal
					Vector3 normal = Vector3.Cross(v1 - v2, v1 - v3);
					normal.Normalize();
					// Add the influence of the normal to each vertex in the
					// triangle
					vertices[orientation][indices[orientation][i]].Normal += normal;
					vertices[orientation][indices[orientation][i + 1]].Normal += normal;
					vertices[orientation][indices[orientation][i + 2]].Normal += normal;
				}

				// Average the influences of the triangles touching each
				// vertex
				for (int i = 0; i < nVertices; i++) {
					vertices[orientation][i].Normal.Normalize();
				}
			}
		}

		private void AddVertices()
		{
			/*for (int i = 0; i < vertices.GetLength(0); i++) {
				for (int j = 0; j < vertices.GetLength(1); j++) {
					finalVertices[i * j + j] = vertices[i, j];
				}
				//if (i==2) break;
			}*/
			for (int i = 0; i < vertices.GetLength(0); i++) {
				//vertexBuffer[i].SetData<VertexPositionNormalTexture>(vertices[i,]);

				//VertexPositionNormalTexture[] tmp = new VertexPositionNormalTexture[vertices.GetLength(1)];
				//Buffer.BlockCopy(vertices, 

				vertexBuffer[i].SetData<VertexPositionNormalTexture>(vertices[i]);
			}
			
		}
		private void AddIndices()
		{
			
			/*for (int i = 0; i < indices.GetLength(0); i++) {
				for (int j = 0; j < indices.GetLength(1); j++) {
					//finalIndices[i * j + j] = indices[i, j];
					
				}
				//if (i == 2) break;
			}*/
			for (int i = 0; i < indices.GetLength(0); i++) {
				indexBuffer[i].SetData<int>(indices[i]);
			}
		}
		/*public void Draw(bool wireFrame, Matrix View, Matrix Projection)
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
		}*/
		public void Draw(bool wireFrame, Matrix View, Matrix Projection)
		{
			if (wireFrame) {
				RasterizerState rs = new RasterizerState();
				rs.FillMode = FillMode.WireFrame;
				graphicsDevice.RasterizerState = rs;
			}
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



			for (int orientation = 0; orientation < 6; orientation++) {
				graphicsDevice.SetVertexBuffer(vertexBuffer[orientation]);
				graphicsDevice.Indices = indexBuffer[orientation];

				effect.Techniques[0].Passes[0].Apply();

				graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0,
					nVertices, 0, nIndices / 3);
			}
			// Un-set the buffers
			graphicsDevice.SetVertexBuffer(null);
			graphicsDevice.Indices = null;
			graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
		}

		public float BaseHeight { get; private set; }
		public SphericalTerrain2(Texture2D HeightMap, float CellSize, float Height,
			GraphicsDevice GraphicsDevice, ContentManager Content)
			: this(HeightMap, CellSize, Height, 0, null, 0, Vector3.Zero, GraphicsDevice, Content)
		{
		}
		public SphericalTerrain2(Texture2D HeightMap, float CellSize, float Height,
			float baseHeight, Texture2D BaseTexture, float TextureTiling, Vector3 LightDirection,
			GraphicsDevice GraphicsDevice, ContentManager Content)
		{
			Radius = 50;
			Center = new Vector3(0, 200, -200);

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
			heightMapCube = Content.Load<TextureCube>("Textures\\Terrain\\heightmapCube0");

			// 1 vertex per pixel
			nVertices = width * length;
			// (Width-1) * (Length-1) cells, 2 triangles per cell, 3 indices per
			// triangle
			nIndices = (width - 1) * (length - 1) * 6;



			/*vertexBuffer = new VertexBuffer(GraphicsDevice,
				typeof(VertexPositionNormalTexture), nVertices * 6,
				BufferUsage.WriteOnly);
			indexBuffer = new IndexBuffer(GraphicsDevice,
				IndexElementSize.ThirtyTwoBits,
				nIndices * 6, BufferUsage.WriteOnly);

			// setting the vertices and indices
			GetHeights();
			CreateVertices();
			CreateIndices();
			GenerateNormals();

			finalVertices = new VertexPositionNormalTexture[nVertices * 6];//1572864
			AddVertices();
			vertexBuffer.SetData<VertexPositionNormalTexture>(finalVertices);
			finalIndices = new int[nIndices * 6];
			AddIndices();
			indexBuffer.SetData<int>(finalIndices);*/

			vertexBuffer = new VertexBuffer[6];
			indexBuffer = new IndexBuffer[6];
			for (int i = 0; i < 6; i++) {
				vertexBuffer[i] = new VertexBuffer(GraphicsDevice,
					typeof(VertexPositionNormalTexture), nVertices,
					BufferUsage.WriteOnly);
				indexBuffer[i] = new IndexBuffer(GraphicsDevice,
					IndexElementSize.ThirtyTwoBits,
					nIndices, BufferUsage.WriteOnly);
			}

			// setting the vertices and indices
			GetHeights();
			CreateVertices();
			CreateIndices();
			GenerateNormals();

			//finalVertices = new VertexPositionNormalTexture[nVertices * 6];//1572864
			AddVertices();
			//vertexBuffer.SetData<VertexPositionNormalTexture>(finalVertices);
			//finalIndices = new int[nIndices * 6];
			AddIndices();
			//indexBuffer.SetData<int>(finalIndices);
		}

	}
}
