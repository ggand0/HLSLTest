﻿using System;
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
		public static Camera camera;

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
		public BillboardMode Mode { get; private set; }

		/// <summary>
		/// Position of this emitter.
		/// </summary>
		public Vector3 Position { get; set; }
		public int EmitType { get; set; }
		Matrix world;
		private bool hasInitializedCameraPos = false;


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

				//AddParticle(Position + new Vector3(0, -2, 0), direction, s);
                AddParticle(Position, direction, s);
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

					// calc theta (old)
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


					// calc theta (new)
					/*world = Matrix.Identity;
					Vector3 toCam = Vector3.Normalize(camera.Position - Position);
					world = Matrix.CreateTranslation(Position);
					world.Forward = dir;
					world.Right = Vector3.Cross(dir, toCam);
					world.Up = Vector3.Cross(world.Right, world.Up);
					world.Up.Normalize();*/

					/*Matrix w = Matrix.Identity; //w.Up = Vector3.Cross(Up, Right);
					w.Up = Up; w.Right = -Right; w.Forward = Vector3.Normalize(Vector3.Cross(Up, Right));*/


					//AddParticle(Position, velocity, 90, speed);
					//AddParticle(Position, velocity, MathHelper.ToRadians(90), speed); // ok

					//AddParticle(Position, velocity, theta, speed);
					AddParticle(Position, velocity, 0, speed, Position + dir * 10);
				}
			}
		}
		#endregion


		/// <summary>
		/// Increases the 'start' parameter by 'count' positions, wrapping
		/// around the particle array if necessary
		/// </summary>
		protected int OffsetIndex(int start, int count)
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
			if (Mode == BillboardMode.Cross) {
				int crossBillboardNum = 2;
				Vector3 z = Vector3.Zero;
				particles = new ParticleVertex[ParticleNum * crossBillboardNum * 4];
				indices = new int[ParticleNum * crossBillboardNum * 6];

				int x = 0;
				// For each billboard...
				for (int i = 0; i < ParticleNum * 4 * crossBillboardNum; i += 4 * crossBillboardNum) {
					//Vector3 pos = particlePositions[i / (4 * crossBillboardNum)];
					Vector3 pos = Vector3.Zero;
					Vector3 offsetX = new Vector3(ParticleSize.X / 2.0f,
						ParticleSize.Y / 2.0f, 0);
					Vector3 offsetZ = new Vector3(0, offsetX.Y, offsetX.X);

					// Add 4 vertices per rectangle
					particles[i + 0] = new ParticleVertex(pos +
						new Vector3(-1, 1, 0) * offsetX, new Vector2(0, 0), z, 0, -1);
					particles[i + 1] = new ParticleVertex(pos +
						new Vector3(-1, -1, 0) * offsetX, new Vector2(0, 1), z, 0, -1);
					particles[i + 2] = new ParticleVertex(pos +
						new Vector3(1, -1, 0) * offsetX, new Vector2(1, 1), z, 0, -1);
					particles[i + 3] = new ParticleVertex(pos +
						new Vector3(1, 1, 0) * offsetX, new Vector2(1, 0), z, 0, -1);
					particles[i + 4] = new ParticleVertex(pos +
						new Vector3(0, 1, -1) * offsetZ, new Vector2(0, 0), z, 0, -1);
					particles[i + 5] = new ParticleVertex(pos +
						new Vector3(0, -1, -1) * offsetZ, new Vector2(0, 1), z, 0, -1);
					particles[i + 6] = new ParticleVertex(pos +
						new Vector3(0, -1, 1) * offsetZ, new Vector2(1, 1), z, 0, -1);
					particles[i + 7] = new ParticleVertex(pos +
						new Vector3(0, 1, 1) * offsetZ, new Vector2(1, 0), z, 0, -1);


					// Add 6 indices per rectangle to form four triangles
					indices[x++] = i + 0;
					indices[x++] = i + 3;
					indices[x++] = i + 2;
					indices[x++] = i + 2;
					indices[x++] = i + 1;
					indices[x++] = i + 0;
					indices[x++] = i + 0 + 4;
					indices[x++] = i + 3 + 4;
					indices[x++] = i + 2 + 4;
					indices[x++] = i + 2 + 4;
					indices[x++] = i + 1 + 4;
					indices[x++] = i + 0 + 4;
				}
			} else {
				particles = new ParticleVertex[ParticleNum * 4];
				indices = new int[ParticleNum * 6];
				Vector3 z = Vector3.Zero;
				int x = 0;

				// Initialize particle settings and fill index and vertex arrays
				for (int i = 0; i < ParticleNum * 4; i += 4) {
					particles[i + 0] = new ParticleVertex(z, new Vector2(0, 0), z, 0, -1, 0, z);
					particles[i + 1] = new ParticleVertex(z, new Vector2(0, 1), z, 0, -1, 0, z);
					particles[i + 2] = new ParticleVertex(z, new Vector2(1, 1), z, 0, -1, 0, z);
					particles[i + 3] = new ParticleVertex(z, new Vector2(1, 0), z, 0, -1, 0, z);
					indices[x++] = i + 0;
					indices[x++] = i + 3;
					indices[x++] = i + 2;
					indices[x++] = i + 2;
					indices[x++] = i + 1;
					indices[x++] = i + 0;
				}
			}
		}
		protected virtual void MoveParticle()
		{
			//RandomDirectionExplosion();
			//emitNumPerFrame = 1; ParticleColor = Color.CadetBlue; EmitRandomDirection();
			emitNumPerFrame = 1; ParticleColor = Color.LightGreen; EmitOptionalDirection(new Vector3(1, 0, 0));
			
		}
		protected virtual void UpdateParticles()
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
			this.AddParticle(Position, Direction, 0, Speed, Position);
		}
		protected virtual void AddParticle(Vector3 position, Vector3 Direction, float rotation, float Speed, Vector3 directedPosition)
		{
			if (Mode == BillboardMode.Cross) {
				int billboardCrossNum = 2;
				// If there are no available particles, give up
				if (activeParticlesNum + 2 * billboardCrossNum == ParticleNum * 2 * billboardCrossNum) {
					return;
				}

				// Determine the index at which this particle should be created
				int index = OffsetIndex(activeStart, activeParticlesNum);
				activeParticlesNum += 2 * billboardCrossNum;

				// Determine the start time
				float startTime = (float)(DateTime.Now - start).TotalSeconds;

				// Set the particle settings to each of the particle's vertices
				for (int i = 0; i < 2 * billboardCrossNum; i++) {
					particles[index + i].StartPosition = position;
					particles[index + i].Direction = Direction;
					particles[index + i].Rotation = rotation;
					particles[index + i].Speed = Speed;
					particles[index + i].StartTime = startTime;
					particles[index + i].DirectedPosition = directedPosition;
				}
			} else {
				// If there are no available particles, give up
				/*if (activeParticlesNum + 4 == ParticleNum * 4) {
					return;
				}*/
				if (activeParticlesNum + 4 == (ParticleNum+1) * 4) {
					return;
				}

				// Determine the index at which this particle should be created
				int index = OffsetIndex(activeStart, activeParticlesNum);
				activeParticlesNum += 4;

				// Determine the start time
				float startTime = (float)(DateTime.Now - start).TotalSeconds;

				// Set the particle settings to each of the particle's vertices
				for (int i = 0; i < 4; i++) {
					particles[index + i].StartPosition = position;
					particles[index + i].Direction = Direction;
					particles[index + i].Rotation = rotation;
					particles[index + i].Speed = Speed;
					particles[index + i].StartTime = startTime;
					particles[index + i].DirectedPosition = directedPosition;
				}

                // debug
                if (position == Vector3.Zero) {
                    string s = "";
                }
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
		public virtual void Draw(Matrix View, Matrix Projection, Vector3 CameraPosition, Vector3 Up, Vector3 Right)
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
			if (this.Mode == BillboardMode.Spherical) {
				effect.Parameters["Up"].SetValue(Up);
				effect.Parameters["Side"].SetValue(Right);
			} else if (Mode == BillboardMode.Cylindrical) {
				effect.Parameters["Up"].SetValue(Vector3.Up);
				effect.Parameters["Side"].SetValue(Right);
				effect.Parameters["CameraPosition"].SetValue(CameraPosition);//new Vector4(CameraPosition, 1));

				//effect.Parameters["FaceCamera"].SetValue(false);
				//effect.Parameters["World"].SetValue(world);
			} else if (Mode == BillboardMode.Cross) {
				effect.Parameters["FaceCamera"].SetValue(false);
			} else if (Mode == BillboardMode.Line) {

				/*effect.Parameters["Up"].SetValue(Up);
				effect.Parameters["Side"].SetValue(Right);
				//if (!hasInitializedCameraPos) {
					effect.Parameters["CameraPosition"].SetValue(CameraPosition);
					hasInitializedCameraPos = true;
				
				effect.Parameters["LineBillboard"].SetValue(true);*/

				effect.Parameters["LineBillboard"].SetValue(true);
				effect.Parameters["Up"].SetValue(Up);
				effect.Parameters["Side"].SetValue(Right);
				Viewport view = graphicsDevice.Viewport;
				Matrix w = Matrix.Identity; //w.Up = Vector3.Cross(Up, Right);
				w.Up = Up; w.Right = -Right; w.Forward = Vector3.Normalize(Vector3.Cross(Up, Right));
				/*Vector3 mid = (Start + End) / 2f;
				w.Translation = mid;// 中点を出す
				Vector3 projectedStart = View.Project(Start, Projection, View, w);
				Vector3 projectedEnd = View.Project(End, Projection, View, w);
				Vector3 v3 = Vector3.Normalize(projectedEnd - projectedStart);
				Vector3 Start = Position;
				Vector3 projectedStart = view.Project(Start, Projection, View, w);
				Vector3 projectedEnd = view.Project(End, Projection, View, w);
				Vector3 v3 = Vector3.Normalize(projectedEnd - projectedStart);

				//effect.Parameters["ProjectedVector"].SetValue(new Vector4(AxisProjectedVectorAxisPlane(Up, (Start - End), debug), 1));
				effect.Parameters["ProjectedVector"].SetValue(v3);// ktkr!!!!!!!!!!!!!!!!!!!!!!
				effect.Parameters["CenterNormal"].SetValue(Vector3.Cross(Up, Right));
				Vector3 deb = Vector3.Normalize(Vector3.Cross(v3, Vector3.Cross(Up, Right)));//pvUp*/
			}
			
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

			// Draw BoundingBox for debug
			 int size = 10;
			 BoundingBoxRenderer.Render(new BoundingBox(new Vector3(-size / 2.0f, -size / 2.0f, -size / 2.0f) + Position, new Vector3(size / 2.0f, size / 2.0f, size / 2.0f) + Position)
				 , graphicsDevice, camera.View, camera.Projection, Color.White);
		}


		public void Initialize()
		{
			if (Mode == BillboardMode.Cross) {
				int crossBillboardNum = 2;
				// Create and set the vertex buffer
				vertexBuffers = new VertexBuffer(graphicsDevice,
					typeof(ParticleVertex),
					ParticleNum * 4 * crossBillboardNum, BufferUsage.WriteOnly);
				//vertexBuffers.SetData<ParticleVertex>(particles);
				// Create and set the index buffer
				indexBuffers = new IndexBuffer(graphicsDevice,
				IndexElementSize.ThirtyTwoBits,
				ParticleNum * 6 * crossBillboardNum, BufferUsage.WriteOnly);
				//indexBuffers.SetData<int>(indices);
			} else {
				// Create vertex and index buffers to accomodate all particles
				vertexBuffers = new VertexBuffer(graphicsDevice, typeof(ParticleVertex),
					ParticleNum * 4, BufferUsage.WriteOnly);
				indexBuffers = new IndexBuffer(graphicsDevice,
					IndexElementSize.ThirtyTwoBits, ParticleNum * 6,
					BufferUsage.WriteOnly);
				
			}
			GenerateParticles();
			start = DateTime.Now;

			this.emitNumPerFrame = 10;
			this.maxEmitFrameCount = ParticleNum / emitNumPerFrame;
			blendState = BlendState.AlphaBlend;
		}


		#region Constructors
		public ParticleEmitter(GraphicsDevice graphicsDevice, ContentManager content, Texture2D texture, Vector3 position , int particleNum,
			Vector2 particleSize, float lifespan, float fadeInTime)
			:this(graphicsDevice, content, texture, position, particleNum, particleSize, lifespan, fadeInTime, true)
		{
		}
		public ParticleEmitter(GraphicsDevice graphicsDevice, ContentManager content, Texture2D texture, Vector3 position, int particleNum,
			Vector2 particleSize, float lifespan, float fadeInTime, bool initialize)
			: this(graphicsDevice, content, texture, position, particleNum, particleSize, lifespan, fadeInTime, BillboardMode.Spherical, initialize)
		{
		}
		/// <summary>
		/// BillboardModeをパラメータに含めたコンストラクタ。
		/// 試験用としてコンストラクタを分けたが後で上とまとめても良い。
		/// </summary>
		public ParticleEmitter(GraphicsDevice graphicsDevice, ContentManager content, Texture2D texture, Vector3 position, int particleNum,
			Vector2 particleSize, float lifespan, float fadeInTime, BillboardMode mode, bool initialize)
		{
			this.ParticleNum = particleNum;
			this.ParticleSize = particleSize;
			this.Lifespan = lifespan;
			this.graphicsDevice = graphicsDevice;
			this.Texture = texture;
			this.Position = position;
			this.FadeInTime = fadeInTime;
			this.Mode = mode;

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
