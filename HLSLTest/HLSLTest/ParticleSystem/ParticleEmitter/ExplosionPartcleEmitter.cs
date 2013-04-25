using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace HLSLTest
{
	public class ExplosionParticleEmitter : ParticleEmitter
	{
		protected override void MoveParticle()
		{
			for (int i = 0; i < ParticleNum; i++) {
				float duration = (float)(rand.Next(0, 20)) / 10f + 2;
				float x = ((float)rand.NextDouble() - 0.5f) * 1.5f;
				float y = ((float)rand.Next(1, 100)) / 10f;
				float z = ((float)rand.NextDouble() - 0.5f) * 1.5f;
				//float s = (float)rand.NextDouble() + 1.0f;
				float s = (float)rand.NextDouble() + 10.0f;
				Vector3 direction = Vector3.Normalize(
					new Vector3(x, y, z)) *
					(((float)rand.NextDouble() * 3f) + 6f);

				//AddParticle(position + new Vector3(0, -2, 0), direction, duration, s);
				AddParticle(Position + new Vector3(0, -2, 0), direction, s);
			}
		}


		public override void Update()
		{
			//MakeExplosion(Vector3.Zero, nParticles);
			if (activeParticlesNum > 0) {
				UpdateParticles();
			} else {
				string debug = "ok";// 全てのパーティクルのアクション停止確認
			}
		}
		private void UpdateParticles()
		{
			float now = (float)(DateTime.Now - start).TotalSeconds;
			int startIndex = activeStart;
			int end = activeParticlesNum;

			// For each particle marked as active...
			for (int i = 0; i < end; i++) {
				// If this particle has gotten older than 'lifespan'...
				if (particles[activeStart].StartTime < now - Lifespan) {
					// Advance the active particle start position past
					// the particle's index and reduce the number of
					// active particles by 1
					activeStart++;
					activeParticlesNum--;
					if (activeStart == particles.Length) {
						activeStart = 0;
					}
				}
			}

			// Update the vertex and index buffers
			vertexBuffers.SetData<ParticleVertex>(particles);
			indexBuffers.SetData<int>(indices);
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
			effect.Parameters["Time"].SetValue((float)(DateTime.Now - start).TotalSeconds);
			effect.Parameters["Lifespan"].SetValue(Lifespan);
			effect.Parameters["Size"].SetValue(ParticleSize / 2f);
			effect.Parameters["Up"].SetValue(Up);
			effect.Parameters["Side"].SetValue(Right);
			effect.Parameters["FadeInTime"].SetValue(FadeInTime);

			// Enable blending render states
			//graphicsDevice.BlendState = BlendState.AlphaBlend;
			graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
			graphicsDevice.BlendState = BlendState.Additive;
			//graphicsDevice.BlendState = BlendState.NonPremultiplied;

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

		

		private double NextDouble(Random r, double min, double max)
		{
			return min + r.NextDouble() * (max - min);
		}
		public void MakeRingEffect(Vector3 position, int particleCount)
		{
			var speed  = 4.0f;
			var innerRadius  = 0.5f;
			var outerRadius = 1.5f;

			for (int i = 0; i < particleCount; i++) {
				// Generate a random unit vector in the plane defined by our transform's red axis centered around the 
				// transform's green axis.  This vector serves as the basis for the initial position and velocity of the particle.
				//Vector3 ruv = RandomUnitVectorInPlane(effectObject.transform, effectObject.transform.up);
				Vector3 ruv = RandomUnitVectorInPlane(Matrix.CreateTranslation(position), Vector3.Up);// 方向を決める

				// Calc the initial position of the particle accounting for the specified ring radii.  Note the use of Range
				// to get a random distance distribution within the ring
				Vector3 newPos = position +
					((ruv * innerRadius) + (ruv * (float)NextDouble(rand, innerRadius, outerRadius)));
				Vector3 pos = newPos;

				// The velocity vector is simply the unit vector modified by the speed.  The velocity vector is used by the 
				// Particle Animator component to move the particles.
				Vector3 velocity = ruv * speed;

				//AddParticle(position + new Vector3(0, -2, 0), direction, duration, s);
				AddParticle(pos + new Vector3(0, 2, 0), velocity, speed);
			}
		}
		Vector3 RandomUnitVectorInPlane(Matrix xform, Vector3 axis)
		{
			// Rotate the specified transform's axis thru a random angle.
			//xform.Rotate(axis, NextDouble(rand, 0.0, 360.0), Space.World);
			
			//xform = Matrix.CreateWorld(xform.Translation, axis, Vector3.Up);
			xform *= Matrix.CreateFromAxisAngle(axis, (float)NextDouble(rand, 0.0, 360.0));

			// Get a copy of the rotated axis and normalize it.
			Vector3 ruv = xform.Right;//new Vector3(xform.right.x, xform.right.y, xform.right.z);   
			ruv.Normalize();

			return ruv;
		}


		// Constructor
		public ExplosionParticleEmitter(GraphicsDevice graphicsDevice, ContentManager content, Texture2D texture, int particleNum,
			Vector2 particleSize, float lifespan, float FadeInTime)
			:base(graphicsDevice, content, texture, particleNum, particleSize, lifespan, FadeInTime)
		{
			MoveParticle();
			/*switch (EffectType) {
				default:
					MakeExplosion(Vector3.Zero, particleNum);
					break;
				case 1:
					MakeRingEffect(Vector3.Zero, particleNum);
					break;
				case 2:
					MakeLaser(Vector3.Zero);
					break;
			}*/
		}
	}

}
