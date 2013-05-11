using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HLSLTest
{
	/// <summary>
	/// BoundingSphereの描画クラス ref先のコードをXNA4.0用に修正して使用
	/// <see cref="http://sharky.bluecog.co.nz/?page_id=113"/>
	/// </summary>
	public class BoundingSphereRenderer
	{
		public static float RADIANS_FOR_90DEGREES = MathHelper.ToRadians(90);//(float)(Math.PI / 2.0);
		public static float RADIANS_FOR_180DEGREES = RADIANS_FOR_90DEGREES * 2;

		private Game1 game;
		public static Level level;

		private BasicEffect basicEffect;
		private const int CIRCLE_NUM_POINTS = 32;
		private IndexBuffer _indexBuffer;
		private VertexPositionNormalTexture[] _vertices;
		protected VertexBuffer buffer;
		protected VertexDeclaration vertexDecl;

		public BoundingSphereRenderer(Game1 game)
		{
			this.game = game;
		}

		public void OnCreateDevice()
		{
			IGraphicsDeviceService graphicsService = (IGraphicsDeviceService)game.Services.GetService(typeof(IGraphicsDeviceService));

			basicEffect = new BasicEffect(graphicsService.GraphicsDevice);

			CreateShape();
		}

		public void CreateShape()
		{
			IGraphicsDeviceService graphicsService = (IGraphicsDeviceService)game.Services.GetService(typeof(IGraphicsDeviceService));

			/*vertexDecl = new VertexDeclaration(
			graphicsService.GraphicsDevice,
			VertexPositionNormalTexture.VertexElements);*/
			vertexDecl = new VertexDeclaration(VertexPositionNormalTexture.VertexDeclaration.GetVertexElements());

			double angle = MathHelper.TwoPi / CIRCLE_NUM_POINTS;

			_vertices = new VertexPositionNormalTexture[CIRCLE_NUM_POINTS + 1];
			_vertices[0] = new VertexPositionNormalTexture(
				Vector3.Zero, Vector3.Forward, Vector2.One);

			for (int i = 1; i <= CIRCLE_NUM_POINTS; i++) {
				float x = (float)Math.Round(Math.Sin(angle * i), 4);
				float y = (float)Math.Round(Math.Cos(angle * i), 4);
				Vector3 point = new Vector3(x, y, 0.0f);
				_vertices[i] = new VertexPositionNormalTexture(
					point,
					Vector3.Forward,
					new Vector2());
			}

			// Initialize the vertex buffer, allocating memory for each vertex
			/*buffer = new VertexBuffer(graphicsService.GraphicsDevice,
				VertexPositionNormalTexture.SizeInBytes * (_vertices.Length),
				BufferUsage.None);*/
			// 3番目の引数をそれっぽく変更してみたが合ってなさそう
			/*buffer = new VertexBuffer(graphicsService.GraphicsDevice, typeof(VertexPositionTexture),
									VertexPositionNormalTexture.VertexDeclaration.VertexStride * (_vertices.Length), BufferUsage.None);*/
			buffer = new VertexBuffer(graphicsService.GraphicsDevice, vertexDecl,
									(_vertices.Length), BufferUsage.None);


			// Set the vertex buffer data to the array of vertices
			buffer.SetData<VertexPositionNormalTexture>(_vertices);

			InitializeLineStrip();
		}

		private void InitializeLineStrip()
		{
			IGraphicsDeviceService graphicsService = (IGraphicsDeviceService)game.Services.GetService(typeof(IGraphicsDeviceService));

			// Initialize an array of indices of type short
			//short[] lineStripIndices = new short[CIRCLE_NUM_POINTS + 1];
			short[] lineStripIndices = new short[CIRCLE_NUM_POINTS + 1];

			// Populate the array with references to indices in the vertex buffer
			for (int i = 0; i < CIRCLE_NUM_POINTS; i++) {
				lineStripIndices[i] = (short)(i + 1);
			}
			lineStripIndices[CIRCLE_NUM_POINTS] = 1;

			// Initialize the index buffer, allocating memory for each index
			/*_indexBuffer = new IndexBuffer(
				graphicsService.GraphicsDevice,
				sizeof(short) * lineStripIndices.Length,
				BufferUsage.None,
				IndexElementSize.SixteenBits
				);*/

			//sizeof(VertexPositionNormalTexture)
			/*_indexBuffer = new IndexBuffer(
				graphicsService.GraphicsDevice, IndexElementSize.SixteenBits,
				lineStripIndices.Length,
				BufferUsage.None);*/
			_indexBuffer = new IndexBuffer(
				graphicsService.GraphicsDevice,
				IndexElementSize.SixteenBits,
				lineStripIndices.Length,
				BufferUsage.None);

			// Set the data in the index buffer to our array
			_indexBuffer.SetData<short>(lineStripIndices);

		}

		public void Draw(BoundingSphere bs, Color color)
		{
			IGraphicsDeviceService graphicsService = (IGraphicsDeviceService)game.Services.GetService(typeof(IGraphicsDeviceService));
			GraphicsDevice device = graphicsService.GraphicsDevice;

			if (bs != null) {
				Matrix scaleMatrix = Matrix.CreateScale(bs.Radius);
				Matrix translateMat = Matrix.CreateTranslation(bs.Center);
				Matrix rotateYMatrix = Matrix.CreateRotationY(RADIANS_FOR_90DEGREES);
				Matrix rotateXMatrix = Matrix.CreateRotationX(RADIANS_FOR_90DEGREES);
				Matrix rotateZMatrix = Matrix.CreateRotationZ(RADIANS_FOR_90DEGREES);
				
				/*device.RenderState.DepthBufferEnable = true;
				device.RenderState.DepthBufferWriteEnable = true;
				device.RenderState.AlphaBlendEnable = true;
				device.RenderState.SourceBlend = Blend.SourceAlpha;
				device.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
				device.RenderState.CullMode = CullMode.CullCounterClockwiseFace;*/
				// XNA4.0ではrenderStateが４つのカテゴライズされたStateに置換された
				DepthStencilState dss = new DepthStencilState();
				dss.DepthBufferEnable = true;
				dss.DepthBufferWriteEnable = true;
				device.DepthStencilState = dss;//.DepthBufferEnable = true;

				BlendState bst = new BlendState();
				bst.AlphaSourceBlend = Blend.SourceAlpha;
				bst.AlphaDestinationBlend = Blend.InverseSourceAlpha;
				device.BlendState = bst;

				RasterizerState rs = new RasterizerState();
				rs.CullMode = CullMode.CullCounterClockwiseFace;
				device.RasterizerState = rs;

				// effect is a compiled effect created and compiled elsewhere
				// in the application
				basicEffect.EnableDefaultLighting();
				//basicEffect.View = _gameInstance.camera.View;
				//basicEffect.Projection = _gameInstance.camera.Projection;
				basicEffect.View = level.camera.View;
				basicEffect.Projection = level.camera.Projection;

				//basicEffect.Begin();
				foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes) {
					//pass.Begin();
					
					//using (VertexDeclaration vertexDecl = new VertexDeclaration(device, VertexPositionNormalTexture.VertexElements)) {
					using (VertexDeclaration vertexDecl = new VertexDeclaration(VertexPositionNormalTexture.VertexDeclaration.GetVertexElements())) {
						//device.VertexDeclaration = vertexDecl;
						//device.Vertices[0].SetSource(buffer, 0, VertexPositionNormalTexture.SizeInBytes);
						device.SetVertexBuffer(buffer);


						device.Indices = _indexBuffer;

						basicEffect.Alpha = ((float)color.A / (float)byte.MaxValue);

						basicEffect.World = scaleMatrix * translateMat;// ここが原因じゃないかあ？　なんだこのWorldはぁ！？ ←そうでもなかった
						
						basicEffect.DiffuseColor = color.ToVector3();
						//basicEffect.CommitChanges();
						pass.Apply();
						device.DrawIndexedPrimitives(
								PrimitiveType.LineStrip,
								0,  // vertex buffer offset to add to each element of the index buffer
								0,  // minimum vertex index
								CIRCLE_NUM_POINTS + 1, // number of vertices. If this gets an exception for you try changing it to 0.  Seems to work just as well.
								0,  // first index element to read
								CIRCLE_NUM_POINTS); // number of primitives to draw

						basicEffect.World = rotateYMatrix * scaleMatrix * translateMat;
						basicEffect.DiffuseColor = color.ToVector3() * 0.5f;
						//basicEffect.CommitChanges();
						pass.Apply();

						device.DrawIndexedPrimitives(
							PrimitiveType.LineStrip,
							0,  // vertex buffer offset to add to each element of the index buffer
							0,  // minimum vertex index
							CIRCLE_NUM_POINTS + 1, // number of vertices. If this gets an exception for you try changing it to 0.  Seems to work just as well.
							0,  // first index element to read
							CIRCLE_NUM_POINTS); // number of primitives to draw

						basicEffect.World = rotateXMatrix * scaleMatrix * translateMat;
						basicEffect.DiffuseColor = color.ToVector3() * 0.5f;
						//basicEffect.DiffuseColor = color.ToVector3();
						pass.Apply();

						device.DrawIndexedPrimitives(
							PrimitiveType.LineStrip,
							0,  // vertex buffer offset to add to each element of the index buffer
							0,  // minimum vertex index
							CIRCLE_NUM_POINTS + 1, // number of vertices. If this gets an exception for you try changing it to 0.  Seems to work just as well.
							0,  // first index element to read
							CIRCLE_NUM_POINTS); // number of primitives to draw
						//pass.End();
					}
				}
				//basicEffect.End();


			}
			device.BlendState = BlendState.Opaque;
			device.DepthStencilState = DepthStencilState.Default;
		}

	}
}