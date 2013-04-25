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
		private Vector3 RandomUnitVectorInPlane(Matrix xform, Vector3 axis)
		{
			xform *= Matrix.CreateFromAxisAngle(axis, (float)NextDouble(rand, 0.0, 360.0));
			Vector3 ruv = xform.Right;
			ruv.Normalize();
			return ruv;
		}
		protected override void MoveParticle()
		{
			var speed = 4.0f;
			var innerRadius = 0.5f;
			var outerRadius = 1.5f;

			for (int i = 0; i < ParticleNum; i++) {
				// Generate a random unit vector in the plane defined by our transform's red axis centered around the 
				// transform's green axis.  This vector serves as the basis for the initial position and velocity of the particle.
				//Vector3 ruv = RandomUnitVectorInPlane(effectObject.transform, effectObject.transform.up);
				Vector3 ruv = RandomUnitVectorInPlane(Matrix.CreateTranslation(Position), Vector3.Up);// 方向を決める

				// Calc the initial position of the particle accounting for the specified ring radii.  Note the use of Range
				// to get a random distance distribution within the ring
				Vector3 newPos = Position +
					((ruv * innerRadius) + (ruv * (float)NextDouble(rand, innerRadius, outerRadius)));
				Vector3 pos = newPos;

				// The velocity vector is simply the unit vector modified by the speed.  The velocity vector is used by the 
				// Particle Animator component to move the particles.
				Vector3 velocity = ruv * speed;

				//AddParticle(position + new Vector3(0, -2, 0), direction, duration, s);
				AddParticle(pos + new Vector3(0, 2, 0), velocity, speed);
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


		// Constructor
		public ExplosionParticleEmitter(GraphicsDevice graphicsDevice, ContentManager content, Texture2D texture, Vector3 position, int particleNum,
			Vector2 particleSize, float lifespan, float FadeInTime)
			:base(graphicsDevice, content, texture, position, particleNum, particleSize, lifespan, FadeInTime)
		{
			//emitNumPerFrame = 100;//50;
			MoveParticle();
		}
	}

}
