using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace HLSLTest
{
	public class FlameParticleEmitter : ParticleEmitter
	{
		public Vector3 Wind { get; private set; }


		/// <summary>
		/// Returns a random Vector3 between min and max
		/// </summary>
		private Vector3 randVec3(Vector3 min, Vector3 max)
		{
			return new Vector3(
			min.X + (float)rand.NextDouble() * (max.X - min.X),
			min.Y + (float)rand.NextDouble() * (max.Y - min.Y),
			min.Z + (float)rand.NextDouble() * (max.Z - min.Z));
		}
		protected override void MoveParticle()
		{
			// Particleの挙動を決定する
			// Generate a direction within 15 degrees of (0, 1, 0)
			//Vector3 offset = new Vector3(MathHelper.ToRadians(10.0f));
			Vector3 offset = new Vector3(MathHelper.ToRadians(20.0f));
			Vector3 randAngle = Vector3.Up + randVec3(-offset, offset);
			// Generate a position between (-400, 0, -400) and (400, 0, 400)
			//Vector3 randPosition = randVec3(new Vector3(-400), new Vector3(400));
			Vector3 randPosition = randVec3(new Vector3(-100), new Vector3(100));
			// Generate a speed between 600 and 900
			//float randSpeed = (float)r.NextDouble() * 300 + 600;
			float randSpeed = (float)rand.NextDouble() * 30 + 60;

			AddParticle(randPosition, randAngle, randSpeed);// １つ分しかaddしてない
			/*while (ps.canAddParticle()) {// 1フレームに１つのペースじゃないと固まってspawnしてしまう
				ps.AddParticle(randPosition, randAngle, randSpeed);
			}*/
		}

		public override void Draw(Matrix View, Matrix Projection, Vector3 Up, Vector3 Right)
		{
			// Set the vertex and index buffer to the graphics card
			graphicsDevice.SetVertexBuffer(vertexBuffers);
			graphicsDevice.Indices = indexBuffers;

			// Set the effect parameters
			effect.Parameters["ParticleTexture"].SetValue(Texture);
			effect.Parameters["View"].SetValue(View);
			effect.Parameters["Projection"].SetValue(Projection);
			effect.Parameters["Time"].SetValue((float)(DateTime.Now - start).
			TotalSeconds);
			effect.Parameters["Lifespan"].SetValue(Lifespan);
			effect.Parameters["Wind"].SetValue(Wind);
			effect.Parameters["Size"].SetValue(ParticleSize / 2f);
			effect.Parameters["Up"].SetValue(Up);
			effect.Parameters["Side"].SetValue(Right);
			effect.Parameters["FadeInTime"].SetValue(FadeInTime);

			// Enable blending render states
			graphicsDevice.BlendState = BlendState.AlphaBlend;
			graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
			//graphicsDevice.BlendState = BlendState.Additive;
			//graphicsDevice.BlendState = BlendState.NonPremultiplied;
			//graphicsDevice.DepthStencilState = DepthStencilState.None;
			//graphicsDevice.RasterizerState = RasterizerState.CullNone;

			// Apply the effect
			effect.CurrentTechnique.Passes[0].Apply();

			// Draw the billboards
			graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList,
				0, 0, ParticleNum * 4, 0, ParticleNum * 2);

			// Un-set the buffers
			graphicsDevice.SetVertexBuffer(null);
			graphicsDevice.Indices = null;

			// Reset render states
			graphicsDevice.BlendState = BlendState.Opaque;
			graphicsDevice.DepthStencilState = DepthStencilState.Default;
			graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
		}


		public FlameParticleEmitter(GraphicsDevice graphicsDevice, ContentManager content, Texture2D texture, int particleNum,
			Vector2 particleSize, float lifespan, Vector3 wind, float FadeInTime)
			:base(graphicsDevice, content, texture, particleNum, particleSize, lifespan, FadeInTime)
		{
			this.Wind = wind;
		}
	}
}
