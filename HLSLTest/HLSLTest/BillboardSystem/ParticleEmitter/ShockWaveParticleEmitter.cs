using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace HLSLTest
{
	public class ShockWaveParticleEmitter : ExplosionParticleEmitter
	{
		public Vector2 currentSize { get; private set; }

		protected override void MoveParticle()
		{
			AddParticle(Position, Vector3.Zero, 0, 0, Position);
		}
		protected override void UpdateParticles()
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
					//currentSize = ParticleSize;
				}
			}

			// Update the vertex and index buffers
			vertexBuffers.SetData<ParticleVertex>(particles);
			indexBuffers.SetData<int>(indices);
		}
		protected override void AddParticle(Vector3 Position, Vector3 Direction, float rotation, float Speed, Vector3 directedPosition)
		{
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
				particles[index + i].DirectedPosition = directedPosition;
			}

			currentSize = ParticleSize;
		}


		public override void Update()
		{
			if (Reset) {
				MoveParticle();
				Reset = false;
			}

			UpdateParticles();
		}
		public override void Draw(Matrix View, Matrix Projection, Vector3 CameraPosition, Vector3 Up, Vector3 Right)
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

			currentSize += new Vector2(Speed);
			if (currentSize.X > 2000) currentSize = ParticleSize;
			
			//effect.Parameters["Size"].SetValue(ParticleSize / 2f);
			effect.Parameters["Size"].SetValue(currentSize / 2.0f);

			// groundに合わせる
			effect.Parameters["Up"].SetValue(Vector3.UnitX);
			effect.Parameters["Side"].SetValue(Vector3.UnitZ);
			//effect.Parameters["Up"].SetValue(Up);
			//effect.Parameters["Side"].SetValue(Right);
			effect.Parameters["FadeInTime"].SetValue(FadeInTime);


			if (Mode == BillboardMode.Line) {
				effect.Parameters["LineBillboard"].SetValue(true);
			} else {
				effect.Parameters["LineBillboard"].SetValue(false);
			}

			// Enable blending render states
			//graphicsDevice.BlendState = BlendState.AlphaBlend;
			graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
			graphicsDevice.BlendState = BlendState.Additive;

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

		public ShockWaveParticleEmitter(GraphicsDevice graphicsDevice, ContentManager content, Vector3 position, Texture2D texture, int particleNum,
			Vector2 particleSize, float lifespan, float fadeInTime, int movementType, float speed, bool initialize)
			: base(graphicsDevice, content, position, texture, particleNum, particleSize, lifespan, fadeInTime, movementType, speed, BillboardMode.Spherical, initialize)
		{
			currentSize = particleSize;
		}

	}
}
