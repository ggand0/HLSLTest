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
		Random rand = new Random();


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

		public int EffectType { get; private set; }
		int emitNumPerFrame = 20;
		int maxEmitFrameCount;
		int frameCount;
		public void MakeDiscoid(Vector3 position)
		{
			var speed = 4.0f;
			var innerRadius = 0.5f;
			var outerRadius = 1.5f;
			
			//thickness

			if (frameCount <= maxEmitFrameCount) {
				for (int i = 0; i < emitNumPerFrame; i++) {
					Vector3 ruv = RandomUnitVectorInPlane(Matrix.CreateTranslation(position), Vector3.Up);// 方向を決める
					Vector3 newPos = position + ((ruv * innerRadius) + (ruv * (float)NextDouble(rand, innerRadius, outerRadius)));
					Vector3 pos = newPos;
					Vector3 velocity = ruv * speed;
					AddParticle(pos + new Vector3(0, 2, 0), velocity, speed);
				}
			}
		}
		Vector3 RandomUnitVectorInPlane(Matrix xform, Vector3 axis)
		{
			xform *= Matrix.CreateFromAxisAngle(axis, (float)NextDouble(rand, 0.0, 360.0));
			Vector3 ruv = xform.Right;
			ruv.Normalize();
			return ruv;
		}
		private double NextDouble(Random r, double min, double max)
		{
			return min + r.NextDouble() * (max - min);
		}
		public void MakeFlame()
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
		public void MakeLaser(Vector3 position)
		{
			var speed = 0.1f;

			if (frameCount <= maxEmitFrameCount) {
				for (int i = 0; i < emitNumPerFrame; i++) {
					//Vector3 ruv = RandomUnitVectorInPlane(Matrix.CreateTranslation(position), Vector3.Up);// 方向を決める
					//Vector3 newPos = position + ((ruv * innerRadius) + (ruv * (float)NextDouble(rand, innerRadius, outerRadius)));

					Vector3 dir = (rand.NextDouble() <= 0.5) ? new Vector3(0, 1, 0) : new Vector3(0, -1, 0);
					Vector3 pos = position;
					Vector3 velocity = dir * speed;
					AddParticle(pos + new Vector3(0, 2, 0), velocity, speed);
				}
			}
		}
		// Returns a random Vector3 between min and max
		Vector3 randVec3(Vector3 min, Vector3 max)
		{
			return new Vector3(
			min.X + (float)rand.NextDouble() * (max.X - min.X),
			min.Y + (float)rand.NextDouble() * (max.Y - min.Y),
			min.Z + (float)rand.NextDouble() * (max.Z - min.Z));
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
			switch (EffectType) {
				default:
					MakeFlame();
					break;
				case 1:
					MakeDiscoid(Vector3.Zero);
					break;
				case 2:
					MakeLaser(Vector3.Zero);
					break;
			}

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
			Vector2 particleSize, float lifespan, Vector3 wind, float FadeInTime, int particleType)
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

			this.EffectType = particleType;
			this.emitNumPerFrame = EffectType == 1 ? 100 : 1;//50;
			this.maxEmitFrameCount = nParticles / emitNumPerFrame;
		}
	}

}
