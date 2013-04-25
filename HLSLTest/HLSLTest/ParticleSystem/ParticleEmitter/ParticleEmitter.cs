using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace HLSLTest
{
	public class ParticleEmitter
	{
		protected VertexBuffer vertexBuffers;
		protected IndexBuffer indexBuffers;
		protected GraphicsDevice graphicsDevice;
		protected Effect effect;

		// Particles and indices
		protected ParticleVertex[] particles;
		protected int[] indices;


		// Particle settings
		public int ParticleNum { get; set; }
		public Vector2 ParticleSize { get; set; }
		public float Lifespan { get; set; }
		public float FadeInTime { get; set; }
		public Texture2D Texture { get; private set; }
		public int EffectType { get; private set; }// refactoring後に消す

		/// <summary>
		/// Position of this emitter.
		/// </summary>
		public Vector3 Position { get; set; }


		// Queue variables
		protected int activeStart = 0;
		protected int activeParticlesNum = 0;
		// Time particle system was created
		protected DateTime start;
		protected Random rand = new Random();

		// refactoring中に移動予定
		protected int emitNumPerFrame = 20;
		protected int maxEmitFrameCount;
		protected int frameCount;


		/// <summary>
		/// Increases the 'start' parameter by 'count' positions, wrapping
		/// around the particle array if necessary
		/// </summary>
		private int offsetIndex(int start, int count)
		{
			for (int i = 0; i < count; i++) {
				start++;
				if (start == particles.Length) {
					start = 0;
				}
			}
			return start;
		}
		protected void generateParticles()
		{
			// Create particle and index arrays
			particles = new ParticleVertex[ParticleNum * 4];
			indices = new int[ParticleNum * 6];
			Vector3 z = Vector3.Zero;
			int x = 0;

			// Initialize particle settings and fill index and vertex arrays
			for (int i = 0; i < ParticleNum * 4; i += 4) {
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
		protected virtual void MoveParticle()
		{
		}

		
		public void AddParticle(Vector3 Position, Vector3 Direction, float Speed)
		{
			// If there are no available particles, give up
			if (activeParticlesNum + 4 == ParticleNum * 4) {
				return;
			}

			// Determine the index at which this particle should be created
			int index = offsetIndex(activeStart, activeParticlesNum);
			activeParticlesNum += 4;

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
			return activeParticlesNum + 4 != ParticleNum * 4;
		}
		public virtual void Update()
		{
			/*switch (EffectType) {
				default:
					MakeFlame();
					break;
				case 1:
					MakeDiscoid(Vector3.Zero);
					break;
				case 2:
					MakeLaser(Vector3.Zero);
					break;
			}*/
			MoveParticle();

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
		public virtual void Draw(Matrix View, Matrix Projection, Vector3 Up, Vector3 Right)
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
			effect.Parameters["Size"].SetValue(ParticleSize / 2f);
			effect.Parameters["Up"].SetValue(Up);
			effect.Parameters["Side"].SetValue(Right);
			effect.Parameters["FadeInTime"].SetValue(FadeInTime);

			// Enable blending render states
			graphicsDevice.BlendState = BlendState.AlphaBlend;
			graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;

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


		// Constructor
		public ParticleEmitter(GraphicsDevice graphicsDevice, ContentManager content, Texture2D texture, int particleNum,
			Vector2 particleSize, float lifespan, float FadeInTime)
		{
			this.ParticleNum = particleNum;
			this.ParticleSize = particleSize;
			this.Lifespan = lifespan;
			this.graphicsDevice = graphicsDevice;
			this.Texture = texture;
			this.FadeInTime = FadeInTime;


			// Create vertex and index buffers to accomodate all particles
			vertexBuffers = new VertexBuffer(graphicsDevice, typeof(ParticleVertex),
				particleNum * 4, BufferUsage.WriteOnly);
			indexBuffers = new IndexBuffer(graphicsDevice,
				IndexElementSize.ThirtyTwoBits, particleNum * 6,
				BufferUsage.WriteOnly);
			generateParticles();
			effect = content.Load<Effect>("ParticleEffect");
			start = DateTime.Now;

			//this.EffectType = particleType;
			this.emitNumPerFrame = EffectType == 1 ? 100 : 1;//50;
			this.maxEmitFrameCount = particleNum / emitNumPerFrame;
			Lifespan = 1;
		}
	}

}
