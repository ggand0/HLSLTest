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
		private float[][,] cubeHeights;

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
			CubeMapFace[] orients = new CubeMapFace[] {
				CubeMapFace.PositiveY,
				CubeMapFace.NegativeY,
				CubeMapFace.PositiveZ,
				CubeMapFace.NegativeZ,
				CubeMapFace.NegativeX,
				CubeMapFace.PositiveX
			};
			cubeHeights = new float[6][,];

			for (int orientation = 0; orientation < 6; orientation++) {
				/*// Extract pixel data
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
				}*/

				Color[] heightMapData = new Color[width * length];
				heightMapCube.GetData<Color>(orients[orientation], heightMapData);

				// Create heights[,] array
				cubeHeights[orientation] = new float[width, length];

				// For each pixel
				for (int y = 0; y < length; y++) {
					for (int x = 0; x < width; x++) {
						// Get color value (0 - 255)
						float amt = heightMapData[y * width + x].R;
						// Scale to (0 - 1)
						amt /= 255.0f;
						// Multiply by max height to get final height
						cubeHeights[orientation][x, y] = amt * height + BaseHeight;// 指定された高さからのheightにしたver
						//heights[x, y] = amt * height;
					}
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

			/*Vector3[] offsetToFaces = new Vector3[] {
				new Vector3(0, length/2.0f, 0),// Top
				new Vector3(0, -length/2.0f, 0),// Bottom
				//new Vector3(0, 0, -length/2),// Back
				//new Vector3(0, 0, length/2),// Front
				//new Vector3(-width/2, 0, 0),// Left
				//new Vector3(width/2, 0, 0),// Right
				new Vector3(0, 0, length/2),// Back
				new Vector3(0, 0, -length/2),// Front
				new Vector3(-width/2, 0, 0),// Left
				new Vector3(width/2, 0, 0),// Right
			};*/
			//float offset = 2f;// for gapping seams!
			float offset = 2f;
			Vector3[] offsetToFaces = new Vector3[] {
				new Vector3(0, length/2.0f-offset, 0),// Top
				new Vector3(0, -length/2.0f+offset, 0),// Bottom
				//new Vector3(0, 0, -length/2),// Back
				//new Vector3(0, 0, length/2),// Front
				//new Vector3(-width/2, 0, 0),// Left
				//new Vector3(width/2, 0, 0),// Right
				new Vector3(0, 0, length/2-offset),// Back
				new Vector3(0, 0, -length/2+offset),// Front
				new Vector3(-width/2+offset, 0, 0),// Left
				new Vector3(width/2-offset, 0, 0),// Right
			};
			/*Vector3[] offsetToFaces = new Vector3[] {
				new Vector3(0, 1, 0),// Top
				new Vector3(0, -1, 0),// Bottom
				//new Vector3(0, 0, -length/2),// Back
				//new Vector3(0, 0, length/2),// Front
				//new Vector3(-width/2, 0, 0),// Left
				//new Vector3(width/2, 0, 0),// Right
				new Vector3(0, 0, 1),// Back
				new Vector3(0, 0, -1),// Front
				new Vector3(-1, 0, 0),// Left
				new Vector3(1, 0, 0),// Right
			};*/

			//cellSize /= (float)heightMapCube.Size;
			// For each pixel in the image
			for (int orientation = 0; orientation < 6; orientation++) {
				for (int z = 0; z < length; z++)
					for (int x = 0; x < width; x++) {
						//Vector3 v = new Vector3(x * cellSize, heights[x, z], z * cellSize) + offsetToCenter;
						Vector3 v = new Vector3(x * cellSize, cubeHeights[orientation][x, z], z * cellSize) + offsetToCenter;
						//Vector3 v = new Vector3(x * cellSize, 0, z * cellSize) + offsetToCenter;

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
						// 全体を移動させるのは球状に変換した後で。
						Vector3 position = v + offsetToFaces[orientation] * cellSize;
						//Vector3 position = v;


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

		public static Vector3 CubeVertexToSphere(Vector3 v)
		{
			/*float squareX = v.X * v.X;
			float squareY = v.Y * v.Y;
			float squareZ = v.Z * v.Z;
			float tmpX = 1 - squareY / 2.0f - squareZ / 2.0f + squareY * squareZ / 3.0f;
			float tmpY = 1 - squareZ / 2.0f - squareX / 2.0f + squareZ * squareX / 3.0f;
			float tmpZ = 1 - squareX / 2.0f - squareY / 2.0f + squareX * squareY / 3.0f;

			return new Vector3(
				v.X * (float)Math.Sqrt(tmpX),
				v.Y * (float)Math.Sqrt(tmpY),
				v.Z * (float)Math.Sqrt(tmpZ)
			);*/
			float sx = v.X * (float)Math.Sqrt(1.0f - v.Y * v.Y * 0.5f - v.Z * v.Z * 0.5f + v.Y * v.Y * v.Z * v.Z / 3.0f);
			float sy = v.Y * (float)Math.Sqrt(1.0f - v.Z * v.Z * 0.5f - v.X * v.X * 0.5f + v.Z * v.Z * v.X * v.X / 3.0f);
			float sz = v.Z * (float)Math.Sqrt(1.0f - v.X * v.X * 0.5f - v.Y * v.Y * 0.5f + v.X * v.X * v.Y * v.Y / 3.0f);
			return new Vector3(sx, sy, sz);
		}

		private void TransformVertices()
		{
			/*Vector3[] faceVec = new Vector3[] {
				new Vector3(0, 1, 0),// Top
				new Vector3(0, 1, 0),// Bottom
				new Vector3(0, 0, 1),// Back
				new Vector3(0, 0, 1),// Front
				new Vector3(1, 0, 0),// Left
				new Vector3(1, 0, 0),// Right
			};*/
			Vector3[] faceVec = new Vector3[] {
				new Vector3(1, 0, 1),// Top
				new Vector3(1, 0, 1),// Bottom
				new Vector3(1, 1, 0),// Back
				new Vector3(1, 1, 0),// Front
				new Vector3(0, 1, 1),// Left
				new Vector3(0, 1, 1),// Right
			};

			float maxX = 0, maxY = 0, maxZ = 0;
			// transform vertices on a cube to a sphrical map
			// 全ての頂点の全ての成分が-1to1である必要があるのでそこを要確認

			// というかx,y,zが-1to1のcubeの表面の頂点なら常にどれかの成分は１でなければならないはず。
			// 単にnormalizeするだけでは意味がないのではないか。
			for (int orientation = 0; orientation < 6; orientation++) {
				for (int i = 0; i < vertices[orientation].Length; i++) {

					float length = vertices[orientation][i].Position.Length();// l = 109.7087, {X:-64 Y:62 Z:-64}
					vertices[orientation][i].Position.Normalize();// normalizeだけだと0to1になるだけ?
					switch (orientation) {
						case 0:
							//vertices[orientation][i].Position.Y = 1;
							vertices[orientation][i].Position *= 1 / vertices[orientation][i].Position.Y;
							break;
						case 1:
							//vertices[orientation][i].Position.Y = -1;
							vertices[orientation][i].Position *= -1 / vertices[orientation][i].Position.Y;
							break;
						case 2:
							//vertices[orientation][i].Position.Z = 1;
							vertices[orientation][i].Position *= 1 / vertices[orientation][i].Position.Z;
							break;
						case 3:
							//vertices[orientation][i].Position.Z = -1;
							vertices[orientation][i].Position *= -1 / vertices[orientation][i].Position.Z;
							break;
						case 4:
							vertices[orientation][i].Position *= -1 / vertices[orientation][i].Position.X;
							break;
						case 5:
							vertices[orientation][i].Position *= 1 / vertices[orientation][i].Position.X;
							break;
					}

					//vertices[orientation][i].Position *= length;// lengthを掛ければ元の値に戻ることを確認

					//vertices[orientation][i].Position = vertices[orientation][i].Position * 2 - Vector3.One;// その方向に1引く？
					//vertices[orientation][i].Position =	vertices[orientation][i].Position * faceVec[orientation] * 2 - faceVec[orientation];
					// Topだったら「その面方向に」-1to1である必要があるはず
					//Vector3 debug = new Vector3(1, 0, 2) * new Vector3(2, 0, 2);
					// どのvertexも0.5付近の値らしい?
					if (vertices[orientation][i].Position.X > 1.0f) {
						string debug = "maybe ok";//1,0,0の点はある。
					}
					if (vertices[orientation][i].Position.X > maxX) maxX = vertices[orientation][i].Position.X;// どれも1.0
					if (vertices[orientation][i].Position.Y > maxY) maxY = vertices[orientation][i].Position.Y;
					if (vertices[orientation][i].Position.Z > maxZ) maxZ = vertices[orientation][i].Position.Z;

					vertices[orientation][i].Position = CubeVertexToSphere(vertices[orientation][i].Position);

					// -1to1から0to1に戻す必要あり？
					//vertices[orientation][i].Position += Vector3.One;
					//vertices[orientation][i].Position = vertices[orientation][i].Position / 2.0f;

					//vertices[orientation][i].Position += faceVec[orientation];
					//vertices[orientation][i].Position = vertices[orientation][i].Position / (faceVec[orientation] * 2);

					//vertices[orientation][i].Position *= new Vector3(heightMapCube.Size, 0, heightMapCube.Size);
					//vertices[orientation][i].Position *= 200;// これはマジックナンバーすぎるでしょう...2*length?
					vertices[orientation][i].Position *= Radius;
					// {X:-0.5827481 Y:0.5664003 Z:-0.5827481}→ {X:-64.19194 Y:62.39118 Z:-64.19194} / {X:-64.10229 Y:62.30404 Z:-64.10229} 
					vertices[orientation][i].Position += Center;
				}
			}
			int d = 0;
		}
		private void AddVertices()
		{
			/*for (int i = 0; i < vertices.GetLength(0); i++) {
				for (int j = 0; j < vertices.GetLength(1); j++) {
					finalVertices[i * j + j] = vertices[i, j];
				}
				//if (i==2) break;
			}*/


			//TransformVertices();
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
			Radius = 200;
			Center = new Vector3(0, 200, -200);

			this.baseTexture = BaseTexture;
			this.textureTiling = TextureTiling;
			this.lightDirection = LightDirection;

			this.heightMap = HeightMap;
			//this.width = HeightMap.Width;
			//this.length = HeightMap.Height;
			//heightMapCube = Content.Load<TextureCube>("Textures\\Terrain\\sphericalHeightmap0");
			heightMapCube = Content.Load<TextureCube>("Textures\\Terrain\\CubeWrap");
			this.width = heightMapCube.Size;
			this.length = heightMapCube.Size;
			Radius = heightMapCube.Size;
			//int[] test = new int[heightMapCube.Size]
			//heightMapCube.GetData<int>(CubeMapFace.NegativeX,);

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
			//GetHeights();
			GetCubeHeights();
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
