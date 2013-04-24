using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace HLSLTest
{
	public class LaserBillboard
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
		public bool EnsureOcclusion = true;
		public enum BillboardMode { Cylindrical, Spherical };
		public BillboardMode Mode = BillboardMode.Spherical;

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

		static float Rad2Deg()
		{
			return 360 / (float)(Math.PI * 2);
		}
		float AxisAngleOnAxisPlane( Vector3 origin, Vector3 fromDirection, Vector3 toDirection, Vector3 axis )
		{
			fromDirection.Normalize();
			axis.Normalize();
			Vector3 toDirectionProjected = toDirection - axis * Vector3.Dot(axis,toDirection);
			toDirectionProjected.Normalize();
			//return Mathf.Acos(Mathf.Clamp(Vector3.Dot(fromDirection,toDirectionProjected),-1f,1f))) * 
			//	(Vector3.Dot(Vector3.Cross(axis,fromDirection), toDirectionProjected) < 0f ? -Mathf.Rad2Deg : Mathf.Rad2Deg);
			return (float)Math.Acos((double)MathHelper.Clamp(Vector3.Dot(fromDirection, toDirectionProjected), -1f, 1f)) *
				(Vector3.Dot(Vector3.Cross(axis,fromDirection), toDirectionProjected) < 0f ? -Rad2Deg() : Rad2Deg());
		}
		Vector3 AxisProjectedVectorAxisPlane(Vector3 fromDirection, Vector3 toDirection, Vector3 axis)
		{
			fromDirection.Normalize();
			axis.Normalize();
			Vector3 toDirectionProjected = toDirection - axis * Vector3.Dot(axis, toDirection);
			toDirectionProjected.Normalize();

			return toDirectionProjected;
		}

		void setEffectParameters(Matrix View, Matrix Projection, Vector3 Up,
			Vector3 Right, Vector3 CameraPosition)
		{
			effect.Parameters["ParticleTexture"].SetValue(texture);
			effect.Parameters["View"].SetValue(View);
			effect.Parameters["Projection"].SetValue(Projection);
			
			//effect.Parameters["Size"].SetValue(new Vector2(billboardSize.X / 2f, (End - Start).Length() ));
			effect.Parameters["Size"].SetValue(billboardSize / 2f);

			//effect.Parameters["Up"].SetValue(Up);
			effect.Parameters["Up"].SetValue(Mode == BillboardMode.Spherical ? Up : Vector3.Up);
			effect.Parameters["Side"].SetValue(Right);

			effect.Parameters["StartPos"].SetValue(new Vector4(Start, 1));
			effect.Parameters["EndPos"].SetValue(new Vector4(End, 1));
			//effect.Parameters["theta"].SetValue(AxisAngleOnAxisPlane(Vector3.Zero, Start, End, Vector3.Cross(Up, Right)));
			//effect.Parameters["theta"].SetValue(AxisAngleOnAxisPlane(End, (Start - End), End + Right * (-5) - End, Vector3.Cross(Up, Right)));
			//effect.Parameters["theta"].SetValue(AxisAngleOnAxisPlane(End, (Start - End), (End + Up * 5) - End, Vector3.Cross(Up, Right)));
			Vector3 debug =  Vector3.Cross(Up, Right);
			Vector3 d = AxisProjectedVectorAxisPlane(Up, (Start - End), debug);
			Vector3 pv = AxisProjectedVectorAxisPlane(Up, (End - Start), debug);

			Viewport view = graphicsDevice.Viewport;
			Vector3 v3 = view.Project(End - Start,
								Projection,
								View,
								Matrix.Identity);
			//effect.Parameters["ProjectedVector"].SetValue(new Vector4(AxisProjectedVectorAxisPlane(Up, (Start - End), debug), 1));
			effect.Parameters["ProjectedVector"].SetValue(v3);// ktkr!!!!!!!!!!!!!!!!!!!!!!

			effect.Parameters["World"].SetValue(World);
			effect.Parameters["theta"].SetValue(45f);
			effect.Parameters["CameraDir"].SetValue(Vector3.Normalize(CameraPosition - Start));

			effect.CurrentTechnique.Passes[0].Apply();

		}

		Matrix World;
		public void Update(Vector3 Up,
			Vector3 Right, Vector3 CameraPosition)
		{
			Vector3 dir = End - Start;
			//Vector3 dir = Start - End;
			dir.Normalize();
			Vector3 toCamera = CameraPosition - Start;
			//Vector3 right = Vector3.Cross(dir, Vector3.Cross(Up, Right));
			Vector3 right = Vector3.Cross(dir, toCamera);
			right.Normalize();
			Vector3 up = Vector3.Cross(right, dir);
			

			World = Matrix.Identity;
			World.Forward = dir;
			World.Right = right;
			World.Up = up;
			//World.Translation = Start;
			World.Translation = Vector3.Zero;


			World = Matrix.CreateFromAxisAngle(toCamera, 10);
			World.Translation = Vector3.Zero;
		}
		public void Draw(Matrix View, Matrix Projection, Vector3 Up, Vector3 Right, Vector3 CameraPosition)
		{
			// Set the vertex and index buffer to the graphics card
			graphicsDevice.SetVertexBuffer(verts);
			graphicsDevice.Indices = ints;

			setEffectParameters(View, Projection, Up, Right, CameraPosition);

			// Enable alpha blending
			graphicsDevice.BlendState = BlendState.AlphaBlend;
			// Draw the billboards
			//graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 4 * nBillboards, 0, nBillboards * 2);
			if (EnsureOcclusion) {
				drawOpaquePixels();
				drawTransparentPixels();
			} else {
				graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
				effect.Parameters["AlphaTest"].SetValue(false);
				drawBillboards();
			}

			// Reset render states
			graphicsDevice.BlendState = BlendState.Opaque;
			graphicsDevice.DepthStencilState = DepthStencilState.Default;

			// Un-set the vertex and index buffer
			graphicsDevice.SetVertexBuffer(null);
			graphicsDevice.Indices = null;
		}
		void drawOpaquePixels()
		{
			graphicsDevice.DepthStencilState = DepthStencilState.Default;
			effect.Parameters["AlphaTest"].SetValue(true);
			effect.Parameters["AlphaTestGreater"].SetValue(true);
			drawBillboards();
		}
		void drawBillboards()
		{
			effect.CurrentTechnique.Passes[0].Apply();
			graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0,
			4 * nBillboards, 0, nBillboards * 2);
		}
		void drawTransparentPixels()
		{
			graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
			effect.Parameters["AlphaTest"].SetValue(true);
			effect.Parameters["AlphaTestGreater"].SetValue(false);
			drawBillboards();
		}

		public Vector3 Start { get; set; }
		public Vector3 End { get; set; }
		//public Vector3 CameraPos { get44
		public LaserBillboard(GraphicsDevice graphicsDevice,
			ContentManager content, Texture2D texture, Vector2 billboardSize, Vector3 start, Vector3 end, Vector3[] particlePositions)
		{
			this.nBillboards = particlePositions.Length;
			this.billboardSize = billboardSize;
			this.graphicsDevice = graphicsDevice;
			this.texture = texture;
			this.Start = start;
			this.End = end;
			effect = content.Load<Effect>("LaserBillboardEffectV2");

			generateParticles(particlePositions);
		}
	}
}
