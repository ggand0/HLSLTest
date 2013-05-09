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
		public static ArcBallCamera camera;

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
		public Color ParticleColor { get; set; }
		public BlendState blendState { get; set; }

		/// <summary>
		/// Position of this emitter.
		/// </summary>
		public Vector3 Position { get; set; }
		public int EmitType { get; set; }
		public float Rotation { get; set; }


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

		#region Various Particle Movement
		protected float NextDouble(Random r, double min, double max)
		{
			return (float)(min + r.NextDouble() * (max - min));
		}
		protected virtual void RandomDirectionExplosion()
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

				AddParticle(Position + new Vector3(0, -2, 0), direction, s);
			}
		}
		protected void EmitRandomDirection()
		{
			var speed = 4.0f;

			if (frameCount <= maxEmitFrameCount) {
				for (int i = 0; i < emitNumPerFrame; i++) {
					Vector3 dir = new Vector3(NextDouble(rand, -1, 1), NextDouble(rand, -1, 1), NextDouble(rand, -1, 1));
					Vector3 velocity = dir * speed;
					AddParticle(Position, velocity, speed);
				}
			}
		}
		protected void EmitOptionalDirection(Vector3 direction)
		{
			var speed = 4.0f;
			var width = 5;

			if (frameCount <= maxEmitFrameCount) {
				for (int i = 0; i < emitNumPerFrame; i++) {
					var normal = Vector3.Cross(direction, Vector3.Cross(camera.Up, camera.Right));
					var dir = direction + normal * NextDouble(rand, -width, width);
					dir.Normalize();
					Vector3 velocity = dir * speed;

					// calc theta
					Viewport view = graphicsDevice.Viewport;
					Vector3 projectedDir = view.Project(Position + dir*10, camera.Projection, camera.View, Matrix.Identity);
					Vector3 projectedPosition = view.Project(Position, camera.Projection, camera.View, Matrix.Identity);
					//Vector3 originPosition = projectedPosition + new Vector3(ParticleSize.X, 0, 0);// 多分これの取り方が良くないと思うんだよな...
					Vector3 originPosition = Vector3.Cross(dir, Vector3.Cross(camera.Up, camera.Right));
					originPosition = view.Project(originPosition, camera.Projection, camera.View, Matrix.Identity);
					//originPosition.Normalize();

					Vector2 v1 = new Vector2((originPosition - projectedPosition).X, (originPosition - projectedPosition).Y);
					Vector2 v2 = new Vector2((projectedDir - projectedPosition).X, (projectedDir - projectedPosition).Y);
					//var theta = (float)Math.Atan2(v1.X, -v1.Y) - (float)Math.Atan2(v2.X, -v2.Y);
					//var theta = (float)Math.Atan2(v1.X - v2.X, -(v1.Y - v2.Y));
					var ang1 = (float)Math.Atan2(v1.X, v1.Y);
					var ang2 = (float)Math.Atan2(v2.X, v2.Y);
					var theta = ang1 - ang2;

					var ang =  MathHelper.ToDegrees(theta);
					/*Matrix w = Matrix.Identity; //w.Up = Vector3.Cross(Up, Right);
					w.Up = Up; w.Right = -Right; w.Forward = Vector3.Normalize(Vector3.Cross(Up, Right));*/


					//AddParticle(Position, velocity, 90, speed);
					//AddParticle(Position, velocity, MathHelper.ToRadians(90), speed); // ok
					AddParticle(Position, velocity, theta, speed);
				}
			}
		}
		#endregion


		/// <summary>
		/// Increases the 'start' parameter by 'count' positions, wrapping
		/// around the particle array if necessary
		/// </summary>
		private int OffsetIndex(int start, int count)
		{
			for (int i = 0; i < count; i++) {
				start++;
				if (start == particles.Length) {
					start = 0;
				}
			}
			return start;
		}
		protected void GenerateParticles()
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
			//RandomDirectionExplosion();
			//emitNumPerFrame = 1; ParticleColor = Color.CadetBlue; EmitRandomDirection();
			emitNumPerFrame = 1; ParticleColor = Color.LightGreen; EmitOptionalDirection(new Vector3(1, 0, 0));
			
		}
		protected void UpdateParticles()
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
		public void AddParticle(Vector3 Position, Vector3 Direction, float Speed)
		{
			this.AddParticle(Position, Direction, 0, Speed);
		}
		protected void AddParticle(Vector3 Position, Vector3 Direction, float rotation, float Speed)
		{
			// If there are no available particles, give up
			if (activeParticlesNum + 4 == ParticleNum * 4) {
				return;
			}

			// Determine the index at which this particle should be created
			int index = OffsetIndex(activeStart, activeParticlesNum);
			activeParticlesNum += 4;

			// Determine the start time
			float startTime = (float)(DateTime.Now - start).TotalSeconds;

			// Set the particle settings to each of the particle's vertices
			for (int i = 0; i < 4; i++) {
				particles[index + i].StartPosition = Position;
				particles[index + i].Direction = Direction;
				particles[index + i].Rotation = rotation;
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
			MoveParticle();
			UpdateParticles();
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
			effect.Parameters["Time"].SetValue((float)(DateTime.Now - start).TotalSeconds);
			effect.Parameters["Lifespan"].SetValue(Lifespan);
			effect.Parameters["Size"].SetValue(ParticleSize / 2f);
			effect.Parameters["Up"].SetValue(Up);
			effect.Parameters["Side"].SetValue(Right);
			effect.Parameters["FadeInTime"].SetValue(FadeInTime);

			effect.Parameters["ParticleColor"].SetValue(ParticleColor.ToVector4());
			effect.Parameters["AttachColor"].SetValue(true);
			//effect.Parameters["Rotation"].SetValue(Rotation);

			// Enable blending render states
			graphicsDevice.BlendState = blendState;//BlendState.AlphaBlend;
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


		public void Initialize()
		{
			// Create vertex and index buffers to accomodate all particles
			vertexBuffers = new VertexBuffer(graphicsDevice, typeof(ParticleVertex),
				ParticleNum * 4, BufferUsage.WriteOnly);
			indexBuffers = new IndexBuffer(graphicsDevice,
				IndexElementSize.ThirtyTwoBits, ParticleNum * 6,
				BufferUsage.WriteOnly);
			GenerateParticles();
			
			start = DateTime.Now;

			this.emitNumPerFrame = 10;
			this.maxEmitFrameCount = ParticleNum / emitNumPerFrame;
			blendState = BlendState.AlphaBlend;
		}


		#region Constructors
		public ParticleEmitter(GraphicsDevice graphicsDevice, ContentManager content, Texture2D texture, Vector3 position , int particleNum,
			Vector2 particleSize, float lifespan, float FadeInTime)
			:this(graphicsDevice, content, texture, position, particleNum, particleSize, lifespan, FadeInTime, true)
		{
		}
		public ParticleEmitter(GraphicsDevice graphicsDevice, ContentManager content, Texture2D texture, Vector3 position, int particleNum,
			Vector2 particleSize, float lifespan, float fadeInTime, bool initialize)
		{
			this.ParticleNum = particleNum;
			this.ParticleSize = particleSize;
			this.Lifespan = lifespan;
			this.graphicsDevice = graphicsDevice;
			this.Texture = texture;
			this.Position = position;
			this.FadeInTime = fadeInTime;

			effect = content.Load<Effect>("Billboard\\ParticleEffect");

			// 基本的にはすぐに初期化。
			// Cloneして使うときなどはfalseを与えておく。
			if (initialize) {
				Initialize();
			}
		}
		#endregion
	}

}
