using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace HLSLTest
{
	public class ExplosionParticleSystem
	{
		VertexBuffer vertexBuffers;
		IndexBuffer indexBuffers;
		GraphicsDevice graphicsDevice;
		Effect effect;

		// Particle settings
		int nParticles;
		Vector2 particleSize;
		float lifespan = 1;
		Vector3 wind;
		Texture2D texture;
		float fadeInTime;

		// Particles and indices
		ParticleVertex[] particles;
		int[] indices;

		// Queue variables
		int activeStart = 0, nActive = 0;

		// Time particle system was created
		DateTime start;

		public int EffectType { get; private set; }


		void generateParticles()
		{
			// Create particle and index arrays
			particles = new ParticleVertex[nParticles * 4];
			indices = new int[nParticles * 6];
			Vector3 z = Vector3.Zero;
			int x = 0;

			// Initialize particle settings and fill index and vertex arrays
			for (int i = 0; i < nParticles * 4; i += 4) {
				particles[i + 0] = new ParticleVertex(z, new Vector2(0, 0), z, 0, -1);
				particles[i + 1] = new ParticleVertex(z, new Vector2(0, 1), z, 0, -1);
				particles[i + 2] = new ParticleVertex(z, new Vector2(1, 1), z, 0, -1);
				particles[i + 3] = new ParticleVertex(z, new Vector2(1, 0), z, 0, -1);
				indices[x++] = i + 0;
				indices[x++] = i + 3;
				indices[x++] = i + 2;
				indices[x++] = i + 2;
				indices[x++] = i + 1;
				indices[x++] = i + 0;
			}
		}
		public void AddParticle(Vector3 Position, Vector3 Direction, float Speed)
		{
			/**/// If there are no available particles, give up
			if (nActive + 4 == nParticles * 4) {
				return;
			}

			// Determine the index at which this particle should be created
			int index = offsetIndex(activeStart, nActive);
			nActive += 4;

			// Determine the start time
			float startTime = (float)(DateTime.Now - start).TotalSeconds;

			// Set the particle settings to each of the particle's vertices
			for (int i = 0; i < 4; i++) {
				particles[index + i].StartPosition = Position;
				particles[index + i].Direction = Direction;
				particles[index + i].Speed = Speed;
				particles[index + i].StartTime = startTime;
			}
		}
		public bool canAddParticle()
		{
			return nActive + 4 != nParticles * 4;
		}

		// Increases the 'start' parameter by 'count' positions, wrapping
		// around the particle array if necessary
		int offsetIndex(int start, int count)
		{
			for (int i = 0; i < count; i++) {
				start++;
				if (start == particles.Length) {
					start = 0;
				}
			}
			return start;
		}

		public void Update()
		{
			//MakeExplosion(Vector3.Zero, nParticles);
			if (nActive > 0) {
				UpdateParticles();
			} else {
				string debug = "ok";// 全てのパーティクルのアクション停止確認
			}
		}
		private void UpdateParticles()
		{
			float now = (float)(DateTime.Now - start).TotalSeconds;
			int startIndex = activeStart;
			int end = nActive;

			// For each particle marked as active...
			for (int i = 0; i < end; i++) {
				// If this particle has gotten older than 'lifespan'...
				if (particles[activeStart].StartTime < now - lifespan) {
					// Advance the active particle start position past
					// the particle's index and reduce the number of
					// active particles by 1
					activeStart++;
					nActive--;
					if (activeStart == particles.Length) {
						activeStart = 0;
					}
				}
			}

			// Update the vertex and index buffers
			vertexBuffers.SetData<ParticleVertex>(particles);
			indexBuffers.SetData<int>(indices);
		}

		public void Draw(Matrix View, Matrix Projection, Vector3 Up, Vector3 Right)
		{
			// Set the vertex and index buffer to the graphics card
			graphicsDevice.SetVertexBuffer(vertexBuffers);
			graphicsDevice.Indices = indexBuffers;

			// Set the effect parameters
			effect.Parameters["ParticleTexture"].SetValue(texture);
			effect.Parameters["View"].SetValue(View);
			effect.Parameters["Projection"].SetValue(Projection);
			effect.Parameters["Time"].SetValue((float)(DateTime.Now - start).
			TotalSeconds);
			effect.Parameters["Lifespan"].SetValue(lifespan);
			effect.Parameters["Wind"].SetValue(wind);
			effect.Parameters["Size"].SetValue(particleSize / 2f);
			effect.Parameters["Up"].SetValue(Up);
			effect.Parameters["Side"].SetValue(Right);
			effect.Parameters["FadeInTime"].SetValue(fadeInTime);

			// Enable blending render states
			//graphicsDevice.BlendState = BlendState.AlphaBlend;
			graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
			graphicsDevice.BlendState = BlendState.Additive;
			//graphicsDevice.BlendState = BlendState.NonPremultiplied;

			// Apply the effect
			effect.CurrentTechnique.Passes[0].Apply();

			// Draw the billboards
			graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList,
				0, 0, nParticles * 4, 0, nParticles * 2);

			// Un-set the buffers
			graphicsDevice.SetVertexBuffer(null);
			graphicsDevice.Indices = null;

			// Reset render states
			graphicsDevice.BlendState = BlendState.Opaque;
			graphicsDevice.DepthStencilState = DepthStencilState.Default;
			graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
		}

		Random rand = new Random();
		public void MakeExplosion(Vector3 position, int particleCount)
		{
			for (int i = 0; i < particleCount; i++) {
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
				AddParticle(position + new Vector3(0, -2, 0), direction, s);
			}
		}
		public void MakeLaser(Vector3 position)
		{
			var speed = 0.1f;

			//if (frameCount <= maxEmitFrameCount) {
				for (int i = 0; i < nParticles; i+=2) {
					//Vector3 ruv = RandomUnitVectorInPlane(Matrix.CreateTranslation(position), Vector3.Up);// 方向を決める
					//Vector3 newPos = position + ((ruv * innerRadius) + (ruv * (float)NextDouble(rand, innerRadius, outerRadius)));

					//Vector3 dir = (rand.NextDouble() <= 0.5) ? new Vector3(0, 1, 0) : new Vector3(0, -1, 0);
					Vector3 dir = new Vector3(0, 1, 0);
					Vector3 pos = position + dir * i;
					Vector3 velocity = dir * speed;
					AddParticle(pos + new Vector3(0, 2, 0), velocity, speed);

					dir = new Vector3(0, -1, 0);
					pos = position + dir * i;
					velocity = dir * speed;
					AddParticle(pos + new Vector3(0, 2, 0), velocity, speed);
				}
			
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
		public ExplosionParticleSystem(GraphicsDevice graphicsDevice, ContentManager content, Texture2D tex, int nParticles,
			Vector2 particleSize, float lifespan, Vector3 wind, float FadeInTime, int particleType)
		{
			this.nParticles = nParticles;
			this.particleSize = particleSize;
			this.lifespan = lifespan;
			this.graphicsDevice = graphicsDevice;
			this.wind = wind;
			this.texture = tex;
			this.fadeInTime = FadeInTime;
			this.EffectType = particleType;


			// Create vertex and index buffers to accomodate all particles
			vertexBuffers = new VertexBuffer(graphicsDevice, typeof(ParticleVertex),
			nParticles * 4, BufferUsage.WriteOnly);
			indexBuffers = new IndexBuffer(graphicsDevice,
				IndexElementSize.ThirtyTwoBits, nParticles * 6,
				BufferUsage.WriteOnly);
			generateParticles();
			effect = content.Load<Effect>("ParticleEffect");
			start = DateTime.Now;


			switch (EffectType) {
				default:
					MakeExplosion(Vector3.Zero, nParticles);
					break;
				case 1:
					MakeRingEffect(Vector3.Zero, nParticles);
					break;
				case 2:
					MakeLaser(Vector3.Zero);
					break;
			}
		}
	}

}
