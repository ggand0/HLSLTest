using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace HLSLTest
{
	public class ParticleSystem
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


		void generateParticles()
		{
			// Create particle and index arrays
			particles = new ParticleVertex[nParticles * 4];
			indices = new int[nParticles * 6];
			Vector3 z = Vector3.Zero;
			int x = 0;

			// Initialize particle settings and fill index and vertex arrays
			for (int i = 0; i < nParticles * 4; i += 4) {
				particles[i + 0] = new ParticleVertex(z, new Vector2(0, 0),	z, 0, -1);
				particles[i + 1] = new ParticleVertex(z, new Vector2(0, 1),	z, 0, -1);
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
			// If there are no available particles, give up
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
				0, 0, nParticles * 4, 0, nParticles * 2);

			// Un-set the buffers
			graphicsDevice.SetVertexBuffer(null);
			graphicsDevice.Indices = null;

			// Reset render states
			graphicsDevice.BlendState = BlendState.Opaque;
			graphicsDevice.DepthStencilState = DepthStencilState.Default;
			graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
		}


		// Constructor
		public ParticleSystem(GraphicsDevice graphicsDevice, ContentManager content, Texture2D tex, int nParticles,
			Vector2 particleSize, float lifespan, Vector3 wind, float FadeInTime)
		{
			this.nParticles = nParticles;
			this.particleSize = particleSize;
			this.lifespan = lifespan;
			this.graphicsDevice = graphicsDevice;
			this.wind = wind;
			this.texture = tex;
			this.fadeInTime = FadeInTime;


			// Create vertex and index buffers to accomodate all particles
			vertexBuffers = new VertexBuffer(graphicsDevice, typeof(ParticleVertex),
			nParticles * 4, BufferUsage.WriteOnly);
			indexBuffers = new IndexBuffer(graphicsDevice,
				IndexElementSize.ThirtyTwoBits, nParticles * 6,
				BufferUsage.WriteOnly);
			generateParticles();
			effect = content.Load<Effect>("ParticleEffect");
			start = DateTime.Now;
		}
	}

}
