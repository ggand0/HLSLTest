using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace HLSLTest
{
	public class DebugOverlay
	{
		private const String mFontPath = "Fonts\\menuFont";

		public bool IsEnabled { get; set; }

		private GraphicsDevice mGraphicsDevice;

		private SpriteFont mFont;
		private SpriteBatch mBatch;
		private List<ScreenTextPrimitive> mScreenText = new List<ScreenTextPrimitive>();

		private VertexDeclaration mVertexDeclaration;
		private BasicEffect mBasicEffect;

		private List<Primitive> mPointPrimitives = new List<Primitive>();
		private List<Primitive> mLinePrimitives = new List<Primitive>();
		private List<Primitive> mTrianglePrimitives = new List<Primitive>();

		private class ScreenTextPrimitive
		{
			public ScreenTextPrimitive(string text, Vector2 pos, Color color)
			{
				mText = text;
				mPos = pos;
				mColor = color;
			}

			public string mText;
			public Vector2 mPos;
			public Color mColor;
		}

		private class Primitive
		{
			public Primitive(Color color)
			{
				mColor = color;
			}

			public void AddVertex(Vector3 pos)
			{
				mVertices.Add(new VertexPositionColor(
					pos,
					mColor));
			}

			public Color mColor;
			public List<VertexPositionColor> mVertices = new List<VertexPositionColor>();
		}

		public static DebugOverlay Singleton { get { return mSingleton; } }
		private static DebugOverlay mSingleton = null;

		public DebugOverlay(GraphicsDevice graphicsDevice, ContentManager contentManager)
		{
			System.Diagnostics.Debug.Assert(mSingleton == null);
			mSingleton = this;

			System.Diagnostics.Debug.Assert(graphicsDevice != null);
			mGraphicsDevice = graphicsDevice;

			IsEnabled = true;

			mBatch = new SpriteBatch(mGraphicsDevice);
			mFont = contentManager.Load<SpriteFont>(mFontPath);

			/*mVertexDeclaration = new VertexDeclaration(
				mGraphicsDevice,
				VertexPositionColor.VertexElements
			);*/
			mVertexDeclaration = VertexPositionColor.VertexDeclaration;

			mBasicEffect = new BasicEffect(mGraphicsDevice);

			//mGraphicsDevice.RenderState.PointSize = 2;
		}

		[Conditional("DEBUG")]
		public void Draw(Matrix projectionMatrix, Matrix viewMatrix)
		{
			if (!IsEnabled) {
				return;
			}

			mBasicEffect.Projection = projectionMatrix;
			mBasicEffect.World = Matrix.Identity;
			mBasicEffect.View = viewMatrix;

			//bool savedDepthBuffer = mGraphicsDevice.RenderState.DepthBufferEnable;
			//mGraphicsDevice.RenderState.DepthBufferEnable = true;

			#region Triangle primitives.
			// Change device states.
			//FillMode savedFillMode = mGraphicsDevice.RenderState.FillMode;
			//mGraphicsDevice.RenderState.FillMode = FillMode.WireFrame;
			//mGraphicsDevice.VertexDeclaration = mVertexDeclaration;

			foreach (Primitive p in mTrianglePrimitives) {
				// Index buffer.
				short[] triListIndices = new short[p.mVertices.Count];

				// Populate the array with references to indices in the vertex buffer.
				for (int i = 0; i < triListIndices.Length; i++) {
					triListIndices[i] = (short)i;
				}

				// Vertex buffer setup.
				VertexBuffer vertexBuffer = null;/*new VertexBuffer(
					mGraphicsDevice,
					VertexPositionColor.SizeInBytes * p.mVertices.Count,
					BufferUsage.None);*/
				vertexBuffer = new VertexBuffer(mGraphicsDevice, mVertexDeclaration, p.mVertices.Count, BufferUsage.None);

				vertexBuffer.SetData<VertexPositionColor>(p.mVertices.ToArray());

				// Effect setup.
				mBasicEffect.DiffuseColor = p.mColor.ToVector3();

				//mBasicEffect.Begin();

				foreach (EffectPass pass in mBasicEffect.CurrentTechnique.Passes) {
					pass.Apply();

					mGraphicsDevice.DrawUserIndexedPrimitives<VertexPositionColor>(
						PrimitiveType.TriangleList,
						p.mVertices.ToArray(),	// Vertex data.
						0,						// Vertex buffer offset for each element in index buffer.
						p.mVertices.Count,		// # of vertices.
						triListIndices,			// Index buffer.
						0,						// First index element to read.
						p.mVertices.Count / 3	// # of primitives to draw.
					);

					//pass.End();
				}
				//mBasicEffect.End();
			}

			// Reset changed device states.
			//mGraphicsDevice.RenderState.FillMode = savedFillMode;

			mTrianglePrimitives.Clear();
			#endregion

			#region Line primitives.
			foreach (Primitive p in mLinePrimitives) {
				// Index buffer.
				short[] lineListIndices = new short[p.mVertices.Count];

				// Populate the array with references to indices in the vertex buffer.
				for (int i = 0; i < lineListIndices.Length - 1; i += 2) {
					lineListIndices[i] = (short)(i);
					lineListIndices[i + 1] = (short)(i + 1);
				}

				// Vertex buffer setup.
				VertexBuffer vertexBuffer = null;/*new VertexBuffer(
					mGraphicsDevice,
					VertexPositionColor.SizeInBytes * p.mVertices.Count,
					BufferUsage.None);*/
				vertexBuffer = new VertexBuffer(mGraphicsDevice, mVertexDeclaration, p.mVertices.Count, BufferUsage.None);

				vertexBuffer.SetData<VertexPositionColor>(p.mVertices.ToArray());

				// Effect setup.
				mBasicEffect.DiffuseColor = p.mColor.ToVector3();

				//mGraphicsDevice.VertexDeclaration = mVertexDeclaration;/**/

				//mBasicEffect.Begin();

				foreach (EffectPass pass in mBasicEffect.CurrentTechnique.Passes) {
					pass.Apply();

					mGraphicsDevice.DrawUserIndexedPrimitives<VertexPositionColor>(
						PrimitiveType.LineStrip,
						p.mVertices.ToArray(),	// Vertex data.
						0,						// Vertex buffer offset for each element in index buffer.
						p.mVertices.Count,		// # of vertices.
						lineListIndices,		// Index buffer.
						0,						// First index element to read.
						p.mVertices.Count - 1	// # of primitives to draw.
					);

					//pass.End();
				}
				//mBasicEffect.End();
			}

			mLinePrimitives.Clear();
			#endregion

			#region Point primitives.
			foreach (Primitive p in mPointPrimitives) {
				/*if (p.mVertices.Count > 0) {
					// Vertex buffer setup.
					VertexBuffer pointVertexBuffer = new VertexBuffer(
						mGraphicsDevice,
						VertexPositionColor.SizeInBytes * p.mVertices.Count,
						BufferUsage.None);

					pointVertexBuffer.SetData<VertexPositionColor>(p.mVertices.ToArray());

					// Effect setup.
					mBasicEffect.DiffuseColor = p.mColor.ToVector3();

					mGraphicsDevice.VertexDeclaration = mVertexDeclaration;

					//mBasicEffect.Begin();

					foreach (EffectPass pass in mBasicEffect.CurrentTechnique.Passes) {
						pass.Apply();

						mGraphicsDevice.DrawUserPrimitives<VertexPositionColor>(
							PrimitiveType.PointList,
							p.mVertices.ToArray(),
							0,
							p.mVertices.Count
						);

						//pass.End();
					}
					//mBasicEffect.End();
				}*/
			}

			mPointPrimitives.Clear();
			#endregion

			#region Screen text.
			// Change device states.
			//savedFillMode = mGraphicsDevice.RenderState.FillMode;
			//mGraphicsDevice.RenderState.FillMode = FillMode.Solid;

			//mBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.SaveState);
			mBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

			foreach (ScreenTextPrimitive st in mScreenText) {
				mBatch.DrawString(mFont, st.mText, st.mPos, st.mColor);
			}

			mBatch.End();

			// Reset changed device states.
			//mGraphicsDevice.RenderState.FillMode = savedFillMode;

			mScreenText.Clear();
			#endregion

			//mGraphicsDevice.RenderState.DepthBufferEnable = savedDepthBuffer;
		}

		[Conditional("DEBUG")]
		public static void ScreenText(string text, Vector2 pos, Color c)
		{
			if (mSingleton == null ||
				!mSingleton.IsEnabled) {
				return;
			}

			mSingleton.mScreenText.Add(new ScreenTextPrimitive(text, pos, c));
		}

		[Conditional("DEBUG")]
		public static void Line(Vector3 start, Vector3 end, Color color)
		{
			if (mSingleton == null ||
				!mSingleton.IsEnabled) {
				return;
			}

			Primitive p = new Primitive(color);
			p.AddVertex(start);
			p.AddVertex(end);

			mSingleton.mLinePrimitives.Add(p);
		}

		[Conditional("DEBUG")]
		public static void Arrow(Vector3 start, Vector3 end, float arrowSize, Color color)
		{
			if (mSingleton == null ||
				!mSingleton.IsEnabled) {
				return;
			}

			Primitive p = new Primitive(color);
			p.AddVertex(start);
			p.AddVertex(end);

			Vector3 dir = Vector3.Normalize(end - start);
			Vector3 right;

			float dot = Vector3.Dot(dir, Vector3.UnitY);
			if (dot > .99f || dot < -.99f)
				right = Vector3.Cross(dir, Vector3.UnitX);
			else
				right = Vector3.Cross(dir, Vector3.UnitY);

			Vector3 top = Vector3.Cross(right, dir);

			dir *= arrowSize;
			right *= arrowSize;
			top *= arrowSize;

			// added by pentium @13/5/25
			float slantSize = 50;
			dir *= slantSize;
			right *= slantSize;
			top *= slantSize;

			// Right slant.
			p.AddVertex(end);
			p.AddVertex(end + right - dir);

			// Left slant.
			p.AddVertex(end);
			p.AddVertex(end - right - dir);

			// Top slant.
			p.AddVertex(end);
			p.AddVertex(end + top - dir);

			// Bottom slant.
			p.AddVertex(end);
			p.AddVertex(end - top - dir);

			mSingleton.mLinePrimitives.Add(p);
		}

		[Conditional("DEBUG")]
		public static void BoundingBox(BoundingBox boundingBox, Color color)
		{
			if (mSingleton == null ||
				!mSingleton.IsEnabled) {
				return;
			}

			Vector3 halfExtents = (boundingBox.Max - boundingBox.Min) * 0.5f;
			Vector3 center = (boundingBox.Max + boundingBox.Min) * 0.5f;

			Vector3 edgecoord = Vector3.One, pa, pb;

			Primitive primitive = new Primitive(color);

			for (int i = 0; i < 4; i++) {
				for (int j = 0; j < 3; j++) {
					pa = new Vector3(edgecoord.X * halfExtents.X, edgecoord.Y * halfExtents.Y,
						edgecoord.Z * halfExtents.Z);
					pa += center;

					int othercoord = j % 3;
					SetElement(ref edgecoord, othercoord, GetElement(edgecoord, othercoord) * -1f);
					pb = new Vector3(edgecoord.X * halfExtents.X, edgecoord.Y * halfExtents.Y,
						edgecoord.Z * halfExtents.Z);
					pb += center;

					primitive.AddVertex(pa);
					primitive.AddVertex(pb);
				}

				edgecoord = new Vector3(-1f, -1f, -1f);

				if (i < 3) SetElement(ref edgecoord, i, GetElement(edgecoord, i) * -1f);
			}

			mSingleton.mLinePrimitives.Add(primitive);
		}

		[Conditional("DEBUG")]
		public static void Point(Vector3 pos, Color color)
		{
			if (mSingleton == null ||
				!mSingleton.IsEnabled) {
				return;
			}

			foreach (Primitive p in mSingleton.mPointPrimitives) {
				// Re-use existing primitive w/ same color for batch rendering.
				if (p.mColor == color) {
					p.AddVertex(pos);
					return;
				}
			}

			Primitive primitive = new Primitive(color);
			primitive.AddVertex(pos);
			mSingleton.mPointPrimitives.Add(primitive);
		}

		[Conditional("DEBUG")]
		public static void Sphere(Vector3 pos, float radius, Color color)
		{
			if (mSingleton == null ||
				!mSingleton.IsEnabled) {
				return;
			}

			Primitive p = new Primitive(color);

			// Decrease these angles to increase the complexity of the sphere.
			int dtheta = 35, dphi = 35;
			int theta, phi;
			const double DegToRads = System.Math.PI / 180;

			for (theta = -90; theta <= 90 - dtheta; theta += dtheta) {
				for (phi = 0; phi <= 360 - dphi; phi += dphi) {
					p.AddVertex((pos + radius * new Vector3((float)(Math.Cos(theta * DegToRads) * Math.Cos(phi * DegToRads)),
												(float)(Math.Cos(theta * DegToRads) * Math.Sin(phi * DegToRads)),
												(float)(Math.Sin(theta * DegToRads)))));

					p.AddVertex((pos + radius * new Vector3((float)(Math.Cos((theta + dtheta) * DegToRads) * Math.Cos(phi * DegToRads)),
												(float)(Math.Cos((theta + dtheta) * DegToRads) * Math.Sin(phi * DegToRads)),
												(float)(Math.Sin((theta + dtheta) * DegToRads)))));

					p.AddVertex((pos + radius * new Vector3((float)(Math.Cos((theta + dtheta) * DegToRads) * Math.Cos((phi + dphi) * DegToRads)),
												(float)(Math.Cos((theta + dtheta) * DegToRads) * Math.Sin((phi + dphi) * DegToRads)),
												(float)(Math.Sin((theta + dtheta) * DegToRads)))));

					if (theta > -90 && theta < 90) {
						p.AddVertex((pos + radius * new Vector3((float)(Math.Cos(theta * DegToRads) * Math.Cos((phi + dphi) * DegToRads)),
													(float)(Math.Cos(theta * DegToRads) * Math.Sin((phi + dphi) * DegToRads)),
													(float)(Math.Sin(theta * DegToRads)))));
					}
				}
			}

			mSingleton.mLinePrimitives.Add(p);
		}

		#region Private utility methods.
		private static float GetElement(Vector3 v, int index)
		{
			if (index == 0) return v.X;
			else if (index == 1) return v.Y;
			else if (index == 2) return v.Z;
			else throw new ArgumentOutOfRangeException("index");
		}

		private static void SetElement(ref Vector3 v, int index, float value)
		{
			if (index == 0) v.X = value;
			else if (index == 1) v.Y = value;
			else if (index == 2) v.Z = value;
			else throw new ArgumentOutOfRangeException("index");
		}
		#endregion
	}
}