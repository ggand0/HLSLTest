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
		public Vector2 billboardSize { get; protected set; }
		protected Texture2D texture;
		// GraphicsDevice and Effect
		protected GraphicsDevice graphicsDevice;
		protected Effect effect;
		public bool EnsureOcclusion = true;
		public enum BillboardMode { Cylindrical, Spherical };
		public BillboardMode RenderMode = BillboardMode.Spherical;
		public int UpdateMode { get; private set; }

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

		/// <summary>
		/// 使ってない
		/// </summary>
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
		/// <summary>
		/// BillboardStrip(old ver)用
		/// </summary>
		public void UpdatePositions(Vector3 start, Vector3 end)
		{
			this.Start = start;
			this.End = end;
			Mid = (Start + End) / 2f;

			for (int i = 0; i < particles.Length; i++) {
				particles[i].StartPosition = Mid;
				particles[i].DirectedPosition = End;
			}

			vertexBuffers.SetData<ParticleVertex>(particles);
			indexBuffers.SetData<int>(indices);
		}
		
		
		/// <summary>
		/// LaserBillboardBullet向けに速度に合わせて始点・終点を更新する
		/// </summary>
		/// <param name="gameTime"></param>
		public void MoveLaser(Vector3 direction, float speed)
		{
			switch (UpdateMode) {
				default :
					for (int i = 0; i < particles.Length; i++) {
						particles[i].StartPosition += direction * speed;
						particles[i].DirectedPosition += direction * speed;// これをUpdateしていないせいでは？？？
					}

					Start = particles[0].StartPosition;
					End = particles[0].DirectedPosition;
					Mid = (Start + End) / 2f;
					break;
				case 1:
					// 更新されたStart, Endを設定
					for (int i = 0; i < particles.Length; i++) {
						particles[i].StartPosition = Start;
						particles[i].DirectedPosition = End;
					}
					break;
			}

			vertexBuffers.SetData<ParticleVertex>(particles);
			indexBuffers.SetData<int>(indices);
		}
		public float GetTraveledDistance(int particleIndex)
		{
			return Vector3.Distance(Start, particles[particleIndex].StartPosition);
		}
		/// <summary>
		/// 線分と球の交叉判定を行う
		/// </summary>
		/// <param name="o"></param>
		/// <returns></returns>
		public override bool IsHitWith(Object o)
		{
			BoundingSphere bs = o.transformedBoundingSphere;
			// 移動しているかもしれないので、Start・Endは使わず配列から参照。
			// むしろ毎フレームStartへ値を入れて更新させたほうがいいのかもしれないが。
			Vector3 lineVector = particles[0].DirectedPosition - particles[0].StartPosition;
			Vector3 startToCenterVector = bs.Center - particles[0].StartPosition;

			float dotProduct = Vector3.Dot(lineVector, startToCenterVector);
			if (dotProduct < 0) {
				return startToCenterVector.Length() < bs.Radius;
			} else {
				float squared = lineVector.LengthSquared();
				if (dotProduct > squared) {
					Vector3 endToCenterVector = bs.Center - particles[0].DirectedPosition;
					return endToCenterVector.LengthSquared() < bs.Radius * bs.Radius;
				} else {
					float verticalSquared = startToCenterVector.LengthSquared() - (dotProduct * dotProduct) / lineVector.LengthSquared();
					return verticalSquared < bs.Radius * bs.Radius;
				}
			}
		}


		public Color LaserColor { get; private set; }
		public BlendState LaserBlendState { get; private set; }

		#region Draw methods
		public void Draw(Matrix View, Matrix Projection, Vector3 Up, Vector3 Right, Vector3 CameraPosition)
		{
			// Set the vertex and index buffer to the graphics card
			graphicsDevice.SetVertexBuffer(vertexBuffers);
			graphicsDevice.Indices = indexBuffers;

			SetEffectParameters(View, Projection, Up, Right, CameraPosition);
			effect.Parameters["LaserColor"].SetValue(LaserColor.ToVector4());
			// Enable alpha blending
			//graphicsDevice.BlendState = BlendState.AlphaBlend;
			graphicsDevice.BlendState = LaserBlendState;
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
		#endregion

		public Vector3 Start { get; private set; }
		public Vector3 End { get; private set; }
		public Vector3 Mid { get; private set; }
		public void ChangePosition(Vector3 start, Vector3 end)
		{
			this.Start = start;
			this.End = end;
			Mid = (Start + End) / 2f;
		}

		public LaserBillboard(GraphicsDevice graphicsDevice,
			ContentManager content, Texture2D texture, Vector2 billboardSize, Vector3 start, Vector3 end)//, Vector3[] particlePositions)
			:this(graphicsDevice, content, texture, billboardSize, start, end, Color.White,  BlendState.AlphaBlend, 0)
		{
		}
		public LaserBillboard(GraphicsDevice graphicsDevice, ContentManager content,
			Texture2D texture, Vector2 billboardSize, Vector3 start, Vector3 end, Color laserColor, BlendState laserBlendState, int updateMode)//, Vector3[] particlePositions)
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
			this.LaserBlendState = laserBlendState;
			this.UpdateMode = updateMode;

			float debugLength = (end - start).Length();
			Mid = (start + end) / 2.0f;
			//generateParticles(particlePositions);


			GenerateParticles(new Vector3[] { Mid });
			//GenerateParticles(new Vector3[] { start });
		}
	}
}
