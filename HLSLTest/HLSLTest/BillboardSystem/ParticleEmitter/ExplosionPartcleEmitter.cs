using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using LuaInterface;

namespace HLSLTest
{
	public class ExplosionParticleEmitter : ParticleEmitter
	{
		public Vector3 Velocity { get; set; }

		private Vector3 RandomUnitVectorInPlane(Matrix xform, Vector3 axis)
		{
			xform *= Matrix.CreateFromAxisAngle(axis, (float)NextDouble(rand, 0.0, 360.0));
			Vector3 ruv = xform.Right;
			ruv.Normalize();
			return ruv;
		}
		public float speed = 4.0f;
		public float innerRadius = 0.5f;
		public float outerRadius = 1.5f;

		public bool Reset { get; set; }
		public int Type { get; private set; }

		public bool EnableScripting { get; private set; }
		private Lua lua;
		private string scriptPath;

		protected override void RandomDirectionExplosion()
		{
			for (int i = 0; i < ParticleNum; i++) {
				float duration = (float)(rand.Next(0, 20)) / 10f + 2;
				float x = Level2.NextDouble(rand, -1, 1);
				float y = Level2.NextDouble(rand, -1, 1);
				float z = Level2.NextDouble(rand, -1, 1);

				//float s = (float)rand.NextDouble() + 1.0f;
				float s = (float)rand.NextDouble() + 1.0f;
				Vector3 direction = Vector3.Normalize(
					new Vector3(x, y, z)) *
					(((float)rand.NextDouble() * 3f) + 6f);

				AddParticle(Position + new Vector3(0, -2, 0), direction, s);
			}
		}
		protected void DiscoidExplosion()
		{
			/*var speed = 4.0f;
			var innerRadius = 0.5f;
			var outerRadius = 1.5f;*/

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

				if (i == 0) Velocity = velocity;

				//AddParticle(position + new Vector3(0, -2, 0), direction, duration, s);
				AddParticle(pos + new Vector3(0, 2, 0), velocity, speed);
			}
		}
		protected override void MoveParticle()
		{
			if (EnableScripting) {
				if (lua != null) {
					var x = lua["Random"];
					//lua["Random"] = rand;
					lua.DoFile(scriptPath); // Execute file, full of Lua stuff
				} else {
					throw new Exception("Lua instance is null ! Initialize it in constructors.");
				}
			} else {
				switch (Type) {
					case 0:
						RandomDirectionExplosion();
						break;
					case 1:
						DiscoidExplosion();
						break;
				}
			}
		}


		public override void Update()
		{
			//MakeExplosion(Vector3.Zero, nParticles);

			if (Reset) {
				MoveParticle();
				Reset = false;
			}
			if (activeParticlesNum > 0) {
				UpdateParticles();
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
			//:base:(graphicsDevice, content, texture, position, particleNum, particleSize, lifespan, FadeInTime)
			:this(graphicsDevice, content, texture, position, particleNum, particleSize, lifespan, FadeInTime, 0)
		{
		}
		public ExplosionParticleEmitter(GraphicsDevice graphicsDevice, ContentManager content, Texture2D texture, Vector3 position, int particleNum,
			Vector2 particleSize, float lifespan, float FadeInTime, int type)//Action moveFunction)
			: base(graphicsDevice, content, texture, position, particleNum, particleSize, lifespan, FadeInTime)
		{
			//emitNumPerFrame = 100;//50;
			this.Type = type;
			speed = 4.0f * 5;
			MoveParticle();
		}

		// lua scripting test
		public Vector3 CreateVector(float x, float y, float z)
		{
			return new Vector3(x, y, z);
		}
		public Vector3 MultiplyVector(Vector3 v, float value)
		{
			return v * value;
		}
		public Vector3 NormalizeVector(Vector3 v)
		{
			return Vector3.Normalize(v);
		}
		public Vector3 AddVector(Vector3 v1, Vector3 v2)
		{
			return v1 + v2;
		}
		public ExplosionParticleEmitter(GraphicsDevice graphicsDevice, ContentManager content, Texture2D texture, Vector3 position, int particleNum,
			Vector2 particleSize, float lifespan, float FadeInTime, string scriptPath)
			: base(graphicsDevice, content, texture, position, particleNum, particleSize, lifespan, FadeInTime)
		{
			EnableScripting = true;
			this.scriptPath = scriptPath;

			// Create new Lua instance
			lua = new Lua();
			// Register Function params 1= name you use IN lua, to call this method
			// 2 = object target, the object, whos function you are registering
			// 3 = Method you want to call 
			lua.RegisterFunction("Addparticle", this, GetType().GetMethod("AddParticle"));
			//lua.DoString("LuaName()"); // Executes Lua code. This example calls MyFunction()

			object[] test = lua.DoString("luanet.load_assembly('Microsoft.Xna.Framework')");
			lua["ExplosionParticleEmitter"] = this;
			lua["Random"] = rand;
			
			//lua["Vector3"] = new Vector3();
			Vector3 a = Vector3.Zero;
			//lua.RegisterFunction("Vector3Multipy", null, GetType().GetMethod("Vector3.Multiply"));
		}
	}

}
