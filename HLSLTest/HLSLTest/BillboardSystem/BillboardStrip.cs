using System;
//using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace HLSLTest
{
	/// <summary>
	/// シームレスに繋がったビルボードを管理するクラス。
	/// 軌跡エフェクト向けにLaserBillboardを複数繋げる試み
	/// 13/5/26 Quad stripを用いる方針に変更
	/// </summary>
	public class BillboardStrip : Drawable
	{
		public static readonly int MAX_SIZE = 120;
		public List<Vector3> Positions { get; set; }
		public List<LaserBillboard> Billboards { get; private set; }

		protected VertexBuffer vertexBuffers;
		protected IndexBuffer indexBuffers;
		//protected ParticleVertex[] particles;
		protected List<BillboardStripVertex> particles;
		//protected int[] indices;
		protected List<int> indices;

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
		public Vector3 Start { get; private set; }
		public Vector3 End { get; private set; }
		public Vector3 Mid { get; private set; }
		public Color LaserColor { get; private set; }
		public bool AdjustedWidth { get; private set; }


		#region private methods
		/*private void GenerateParticles(Vector3[] particlePositions)
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
		}*/
		private void SetEffectParameters(Matrix View, Matrix Projection, Vector3 Up,
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
		private void DrawOpaquePixels()
		{
			graphicsDevice.DepthStencilState = DepthStencilState.Default;
			//graphicsDevice.DepthStencilState = DepthStencilState.None;
			graphicsDevice.RasterizerState = RasterizerState.CullNone;
			effect.Parameters["AlphaTest"].SetValue(true);
			effect.Parameters["AlphaTestGreater"].SetValue(true);
			DrawBillboards();
		}
		private void DrawBillboards()
		{
			effect.CurrentTechnique.Passes[0].Apply();
			graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0,
				4 * nBillboards, 0, nBillboards * 2);
		}
		private void DrawTransparentPixels()
		{
			graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
			effect.Parameters["AlphaTest"].SetValue(true);
			effect.Parameters["AlphaTestGreater"].SetValue(false);
			DrawBillboards();
		}
		#endregion

		public void AddBillboard(GraphicsDevice graphicsDevice, ContentManager content, Texture2D texture, Vector2 billboardSize, Vector3 start, Vector3 end)//List<Vector3> positions)
		{
			//Billboards.Add(new LaserBillboard(graphicsDevice, content, texture, billboardSize, start, end));
			Billboards.Add(new LaserBillboard(graphicsDevice, content, texture, billboardSize, start, end, Color.White, BlendState.Additive, 0));
		}
		/// <summary>
		/// Billboard stripさせるために、動的にstripの端に新たに２頂点加えて（=quadを１つ加える）、
		/// インデックスを更新する（tricky part）
        /// （Positionsは毎フレーム更新されるので、一番最後の要素が新たに追加された点だと思い込んで頂点を更新するようにした）
		/// </summary>
        public void AddVertices()//Vector3 newPosition
		{
			// For each billboard...
			/*for (int i = 0; i < nBillboards * 4; i += 4) {
				Vector3 pos = particlePositions[i / 4];
				// Add 4 vertices at the billboard's position
				particles[i + 0] = new BillboardStripVertex(pos, new Vector2(0, 0), z, 0, -1, 0, End);
				particles[i + 1] = new BillboardStripVertex(pos, new Vector2(0, 1), z, 0, -1, 0, End);
				particles[i + 2] = new BillboardStripVertex(pos, new Vector2(1, 1), z, 0, -1, 0, End);
				particles[i + 3] = new BillboardStripVertex(pos, new Vector2(1, 0), z, 0, -1, 0, End);
				// Add 6 indices to form two triangles
				indices[x++] = i + 0;
				indices[x++] = i + 3;
				indices[x++] = i + 2;
				indices[x++] = i + 2;
				indices[x++] = i + 1;
				indices[x++] = i + 0;
			}*/

			// 最低でも２点無いと頂点シェーダで方向ベクトルを計算できないので、頂点追加はしない
			if (Positions.Count <= 1) {
				return;
			}


			// 要デバッグ
			Vector3 z = Vector3.Zero;
            // 2頂点をStripに足すことで、quad(今回はbilboardだが)が1単位追加される
            // quadを構成する残り2頂点は、既存のstripの一番最後の2要素を、前のquadと共有する
			//if (Positions.Count > 1) nBillboards++;
			if (Positions.Count > 2) nBillboards++;
            if (nBillboards > MAX_SIZE) nBillboards = MAX_SIZE;
            //nBillboards = Positions.Count;

            // 頂点の情報(特にUV座標など)はUpdatePositionで更新するので、インデックス付けが重要
            particles.Add(new BillboardStripVertex(Positions[Positions.Count - 2], new Vector2(0, 0), z, 0, -1, 0, Positions[Positions.Count - 1], 0, 1));
            particles.Add(new BillboardStripVertex(Positions[Positions.Count - 2], new Vector2(0, 1), z, 0, -1, 0, Positions[Positions.Count - 1], 0, 0));

            // ここが怪しい
			/*indices.Add((nBillboards-1) * 2 + 0);
            indices.Add((nBillboards-1) * 2 + 3);
            indices.Add((nBillboards-1) * 2 + 2);
            indices.Add((nBillboards-1) * 2 + 2);
            indices.Add((nBillboards-1) * 2 + 1);
            indices.Add((nBillboards-1) * 2 + 0);*/

			// 一番最初に呼ばれるときは頂点が２つしかなく、quadを作れないので何もしない
			// (無理に呼ぶと原点を結ぶ謎のtriangleが出来る)
			//if (Positions.Count > 1) {
			if (Positions.Count > 2) {// particles.Count >= 4のはず
				indices.Add((nBillboards - 1) * 2 + 0);
				indices.Add((nBillboards - 1) * 2 + 1);
				indices.Add((nBillboards - 1) * 2 + 2);
				indices.Add((nBillboards - 1) * 2 + 2);
				indices.Add((nBillboards - 1) * 2 + 1);
				indices.Add((nBillboards - 1) * 2 + 3);
			}

			// 保存範囲を超えたら、Quad１つ分の情報を古い順に削除する
			if (nBillboards > MAX_SIZE) {
				particles.RemoveRange(0, 2);
				indices.RemoveRange(0, 6);
			}

			
			//if (Positions.Count > 1) {
			if (Positions.Count > 2) {
				vertexBuffers = new VertexBuffer(graphicsDevice,
					typeof(BillboardStripVertex),
					nBillboards * 4, BufferUsage.WriteOnly);
				vertexBuffers.SetData<BillboardStripVertex>(particles.ToArray());

				indexBuffers = new IndexBuffer(graphicsDevice,
					IndexElementSize.ThirtyTwoBits,
					nBillboards * 6, BufferUsage.WriteOnly);
				indexBuffers.SetData<int>(indices.ToArray());
			}
		}
		/// <summary>
		/// Positionsに合わせて現在のstripの位置等をUpdate
		/// </summary>
		private void UpdatePositions()
		{
			int posIndex = 0;
			/*for (int i = 0; i < particles.Count; i += 2) {
				// 各頂点のPositionには、positions[i]とpositions[i+1]の中点を与える？
				Vector3 pos = Vector3.Zero;

                if (i == 0) {
                    // 一番最初の2頂点だけは例外的にstart,endに同じ座標を入れておく
                    particles[i] = new BillboardStripVertex(Positions[posIndex], new Vector2(0, 0), Vector3.Zero, 0, 0, 0,
                        Positions[posIndex], posIndex, 1);
                    particles[i + 1] = new BillboardStripVertex(Positions[posIndex], new Vector2(0, 1), Vector3.Zero, 0, 0, 0,
                        Positions[posIndex], posIndex, 0);
                } else {
                    particles[i] = new BillboardStripVertex(Positions[posIndex], new Vector2(posIndex / (float)Positions.Count, 0), Vector3.Zero, 0, 0, 0,
                        Positions[posIndex + 1], posIndex+1, 1);
					particles[i + 1] = new BillboardStripVertex(Positions[posIndex], new Vector2(posIndex / (float)Positions.Count, 1), Vector3.Zero, 0, 0, 0,
						Positions[posIndex + 1], posIndex + 1, 0);
                    posIndex++;
                }
			}*/
			// 必ず方向が計算できるように設定する
			for (int i = 0; i < particles.Count; i += 2) {
				// 各頂点のPositionには、positions[i]とpositions[i+1]の中点を与える？
				Vector3 pos = Vector3.Zero;

				particles[i] = new BillboardStripVertex(Positions[posIndex], new Vector2(posIndex / (float)Positions.Count, 0), Vector3.Zero, 0, 0, 0,
					Positions[posIndex + 1], posIndex + 1, 1);
				particles[i + 1] = new BillboardStripVertex(Positions[posIndex], new Vector2(posIndex / (float)Positions.Count, 1), Vector3.Zero, 0, 0, 0,
					Positions[posIndex + 1], posIndex + 1, 0);

				posIndex++;
			}

			/*using (System.IO.StreamWriter w = new System.IO.StreamWriter(@"billboardstrip.txt")) {
				//w.Write("書式指定出力もできます → n = {0}, x = {1}", n, x);
				foreach (BillboardStripVertex v in particles) {
					w.WriteLine(v.StartPosition + " " + v.DirectedPosition + " " + v.Id + " " + v.UV);
				}
			}*/

			foreach (BillboardStripVertex bp in particles) {
				if (bp.StartPosition == Vector3.Zero || bp.DirectedPosition == Vector3.Zero) {
					string d = "";
				}
			}/**/

			// bufferを更新
            /*vertexBuffers = new VertexBuffer(graphicsDevice,
                typeof(BillboardStripVertex),
                nBillboards * 4, BufferUsage.WriteOnly);
            indexBuffers = new IndexBuffer(graphicsDevice,
                IndexElementSize.ThirtyTwoBits,
                nBillboards * 6, BufferUsage.WriteOnly);*/
			vertexBuffers.SetData<BillboardStripVertex>(particles.ToArray());
			indexBuffers.SetData<int>(indices.ToArray());
		}
		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			// old
			for (int i = 0; i < Billboards.Count - 1; i++) {
				Billboards[i].UpdatePositions(Positions[i], Positions[i + 1]);
			}

			// new
            //if (nBillboards >= 1 && Positions.Count >= 2) {
			if (nBillboards >= 1 && Positions.Count >= 3) {
                UpdatePositions();
            }
		}
        BasicEffect basicEffect;
        private void DrawDebugLine(Matrix view, Matrix projection)
        {
            Matrix projectionMatrix = Matrix.CreateOrthographicOffCenter(0, graphicsDevice.Viewport.Width,
                graphicsDevice.Viewport.Height, 0, 0, 1f);
            /// these wont change, so we can set them now
            /// and not have to set them in every Draw() call
            basicEffect.World = Matrix.Identity;
			basicEffect.View = view;//Matrix.Identity;
			basicEffect.Projection = projection;//projectionMatrix;
            basicEffect.VertexColorEnabled = true;
            VertexPositionColor[] vertices = new VertexPositionColor[(Positions.Count-1) * 2];//+2
            Color lineColor = Color.White;
            /*vertices[0] = new VertexPositionColor(, lineColor);
            vertices[1] = new VertexPositionColor(, lineColor);
            vertices[2] = new VertexPositionColor(, lineColor);
            vertices[3] = new VertexPositionColor(, lineColor);*/
			vertices[0] = new VertexPositionColor(Positions[0], lineColor);
			
            for (int i = 1, j = 1; i < vertices.Length-1; i+=2, j++) {
                vertices[i] = new VertexPositionColor(Positions[j], lineColor);// 終点
                vertices[i + 1] = new VertexPositionColor(Positions[j], lineColor);// 始点
            }
			vertices[vertices.Length-1] = new VertexPositionColor(Positions[Positions.Count-1], lineColor);
            //int[] indices = new int[] { 0, 1, 1, 2 };

            foreach (EffectPass p in basicEffect.CurrentTechnique.Passes) {
                p.Apply();
                graphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineList, vertices, 0, vertices.Length/2);
            }
        }
		public override void Draw(Camera camera)
		{
			this.Draw(camera.View, camera.Projection, camera.Up, camera.Right, camera.Position);
		}
		public void Draw(Matrix View, Matrix Projection, Vector3 Up, Vector3 Right, Vector3 CameraPosition)
		{
			/*foreach (LaserBillboard lb in Billboards) {
					lb.Draw(View, Projection, Up, Right, CameraPosition);
			}*/


			// 最低１つのbillboard（= 2 triangle）は必要だろう
			if (particles.Count >= 4) {
				// Set the vertex and index buffer to the graphics card
				graphicsDevice.SetVertexBuffer(vertexBuffers);
				graphicsDevice.Indices = indexBuffers;

				SetEffectParameters(View, Projection, Up, Right, CameraPosition);
				effect.Parameters["LaserColor"].SetValue(LaserColor.ToVector4());
				effect.Parameters["AdjustedWidth"].SetValue(AdjustedWidth);
				//graphicsDevice.BlendState = BlendState.AlphaBlend;
				graphicsDevice.BlendState = BlendState.Additive;

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
				//DrawDebugLine(View, Projection);
			}
			
		}

        private void Initialize()
        {
			
            int x = 0;
            Vector3 z = Vector3.Zero;
            nBillboards++;
            // For each billboard...
            for (int i = 0; i < nBillboards * 4; i += 4) {
                Vector3 pos = Start;
                // Add 4 vertices at the billboard's position
                particles.Add(new BillboardStripVertex(pos, new Vector2(0, 0), z, 0, -1, 0, End, 0, 1));
                particles.Add(new BillboardStripVertex(pos, new Vector2(0, 1), z, 0, -1, 0, End, 0, 0));
                particles.Add(new BillboardStripVertex(pos, new Vector2(1, 1), z, 0, -1, 0, End, 1, 1));
                particles.Add(new BillboardStripVertex(pos, new Vector2(1, 0), z, 0, -1, 0, End, 1, 0));

                // Add 6 indices to form two triangles
                indices.Add(i + 0);
                indices.Add(i + 3);
                indices.Add(i + 2);
                indices.Add(i + 2);
                indices.Add(i + 1);
                indices.Add(i + 0);
            }

            // Create and set the vertex buffer
            vertexBuffers = new VertexBuffer(graphicsDevice,
                typeof(BillboardStripVertex),
                nBillboards * 4, BufferUsage.WriteOnly);
            vertexBuffers.SetData<BillboardStripVertex>(particles.ToArray());
            // Create and set the index buffer
            indexBuffers = new IndexBuffer(graphicsDevice,
                IndexElementSize.ThirtyTwoBits,
                nBillboards * 6, BufferUsage.WriteOnly);
            indexBuffers.SetData<int>(indices.ToArray());
        }
		#region Constructors
		public BillboardStrip(GraphicsDevice graphicsDevice,
			ContentManager content, Texture2D texture, Vector2 billboardSize, List<Vector3> positions)
			:this(graphicsDevice, content, texture, billboardSize, positions, false)
		{
		}
		public BillboardStrip(GraphicsDevice graphicsDevice,
			ContentManager content, Texture2D texture, Vector2 billboardSize, List<Vector3> positions, bool adjustedWidth)
		{
			this.billboardSize = billboardSize;
			this.graphicsDevice = graphicsDevice;
			this.texture = texture;
			this.Positions = positions;
			this.AdjustedWidth = adjustedWidth;

			Billboards = new List<LaserBillboard>();
			for (int i = 0; i < positions.Count - 1; i++) {
				Billboards.Add(new LaserBillboard(graphicsDevice, content, texture, billboardSize, positions[i], positions[i + 1]));
			}

			LaserColor = Color.White;
			effect = content.Load<Effect>("Billboard\\BillboardStripEffect");
			particles = new List<BillboardStripVertex>();
			indices = new List<int>();
			// とりあえず１つだけBillboardをbufferにセットしておく?
			//Initialize();
			basicEffect = new BasicEffect(graphicsDevice);
		}

		// こっちは使ってない
		public BillboardStrip(GraphicsDevice graphicsDevice,
			ContentManager content, Texture2D texture, Vector2 billboardSize, Vector3 start, Vector3 end)//, Vector3[] particlePositions)
			:this(graphicsDevice, content, texture, billboardSize, start, end, Color.White)
		{
		}
		public BillboardStrip(GraphicsDevice graphicsDevice,
			ContentManager content, Texture2D texture, Vector2 billboardSize, Vector3 start, Vector3 end, Color laserColor)//, Vector3[] particlePositions)
			:this(graphicsDevice, content, texture, billboardSize, start, end, laserColor, false)
		{
		}
		public BillboardStrip(GraphicsDevice graphicsDevice,
			ContentManager content, Texture2D texture, Vector2 billboardSize, Vector3 start, Vector3 end, Color laserColor, bool adjustedWidth)//, Vector3[] particlePositions)
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
			this.LaserColor = laserColor;
			this.AdjustedWidth = adjustedWidth;

			float debugLength = (end - start).Length();
			Mid = (start + end) / 2.0f;

			//GenerateParticles(particlePositions);
			//GenerateParticles(new Vector3[] { Mid });
		}
		#endregion
	}
}
