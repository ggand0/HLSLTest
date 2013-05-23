using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace HLSLTest
{
	public class FullScreenQuadRenderer
	{
		private VertexPositionTexture[] vertices;
        private GraphicsDevice graphicsDevice;
        private short[] indices = null;

        public FullScreenQuadRenderer(GraphicsDevice device)
        {
            this.graphicsDevice = device;         
            vertices = new VertexPositionTexture[] {
                            new VertexPositionTexture(
                                new Vector3(0,0,0),
                                new Vector2(1,1)),
                            new VertexPositionTexture(
                                new Vector3(0,0,0),
                                new Vector2(0,1)),
                            new VertexPositionTexture(
                                new Vector3(0,0,0),
                                new Vector2(0,0)),
                            new VertexPositionTexture(
                                new Vector3(0,0,0),
                                new Vector2(1,0))
                        };
             indices = new short[] { 0, 1, 2, 2, 3, 0 };
        }             

        public void RenderFullScreenQuad(Effect effect)
        {
            effect.CurrentTechnique.Passes[0].Apply();
            RenderQuad(Vector2.One * -1, Vector2.One);
        }
        public void RenderQuad(Vector2 v1, Vector2 v2)
        {          
            vertices[0].Position.X = v2.X;
            vertices[0].Position.Y = v1.Y;

            vertices[1].Position.X = v1.X;
            vertices[1].Position.Y = v1.Y;

            vertices[2].Position.X = v1.X;
            vertices[2].Position.Y = v2.Y;

            vertices[3].Position.X = v2.X;
            vertices[3].Position.Y = v2.Y;

            graphicsDevice.DrawUserIndexedPrimitives
                (PrimitiveType.TriangleList, vertices, 0, 4, indices, 0, 2);
        }

		public void RenderFullScreenQuad(Effect effect, float depth)
		{
			effect.CurrentTechnique.Passes[0].Apply();
			RenderQuad(Vector2.One * -1, Vector2.One, depth);
		}
		public void RenderQuad(Vector2 v1, Vector2 v2, float depth)
		{
			vertices[0].Position.X = v2.X;
			vertices[0].Position.Y = v1.Y;
			vertices[0].Position.Z = depth;

			vertices[1].Position.X = v1.X;
			vertices[1].Position.Y = v1.Y;
			vertices[1].Position.Z = depth;

			vertices[2].Position.X = v1.X;
			vertices[2].Position.Y = v2.Y;
			vertices[2].Position.Z = depth;

			vertices[3].Position.X = v2.X;
			vertices[3].Position.Y = v2.Y;
			vertices[3].Position.Z = depth;

			graphicsDevice.DrawUserIndexedPrimitives
				(PrimitiveType.TriangleList, vertices, 0, 4, indices, 0, 2);
		}
	}
}
