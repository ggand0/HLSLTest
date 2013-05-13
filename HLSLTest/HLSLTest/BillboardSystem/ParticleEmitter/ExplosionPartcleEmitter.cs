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
	public class ExplosionParticleEmitter : ParticleEmitter, ICloneable
	{
		public Vector3 Velocity { get; set; }


		private Vector3 RandomUnitVectorInPlane(Matrix xform, Vector3 axis)
		{
			xform *= Matrix.CreateFromAxisAngle(axis, (float)NextDouble(rand, 0.0, 360.0));
			Vector3 ruv = xform.Right;
			ruv.Normalize();
			return ruv;
		}
		public float Speed { get; set; }
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
				float x = ((float)rand.NextDouble() - 0.5f) * 1.5f;//Level2.NextDouble(rand, -1, 1);
				float y = ((float)rand.NextDouble() - 0.5f) * 1.5f;
				float z = ((float)rand.NextDouble() - 0.5f) * 1.5f;

				//float s = (float)rand.NextDouble() + 1.0f;
				float s = (float)rand.NextDouble() + Speed;
				Vector3 direction = Vector3.Normalize(
					new Vector3(x, y, z)) *
					(((float)rand.NextDouble() * 3f) + 6f);

				if (Position == Vector3.Zero) {
					string d = "";
				}
				//AddParticle(Position + new Vector3(0, -2, 0), direction, 0, s, Position + direction * 10);
				AddParticle(Position, direction, 0, s, Position + direction * 10);
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
				Vector3 velocity = ruv * Speed;

				if (i == 0) Velocity = velocity;

				//AddParticle(position + new Vector3(0, -2, 0), direction, duration, s);
				AddParticle(pos + new Vector3(0, 2, 0), velocity, Speed);
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


        protected override void UpdateParticles()
        {
            float now = (float)(DateTime.Now - start).TotalSeconds;
            int startIndex = activeStart;
            int end = activeParticlesNum;

            // For each particle marked as active...
            /*for (int i = 0; i < end; i++) {
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
            }*/

            // Update the vertex and index buffers
            vertexBuffers.SetData<ParticleVertex>(particles);
            indexBuffers.SetData<int>(indices);
        }
		private void CheckArray()
		{
			for (int i = 0; i < particles.Length; i++) {
				if (particles[i].StartPosition == Vector3.Zero) {
					string d = "need debug!";// i== 36 !
				}
			}
		}
		public override void Update()
		{
			//MakeExplosion(Vector3.Zero, nParticles);

			if (Reset) {
				//MoveParticle();
				Run();
				Reset = false;
			}

            // EPはactiveなパーティクルかどうかを認識する必要がないのでは？
			//if (activeParticlesNum > 0) {
				//UpdateParticles();
			CheckArray();
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
			effect.Parameters["Size"].SetValue(ParticleSize / 2f);
			effect.Parameters["Up"].SetValue(Up);
			effect.Parameters["Side"].SetValue(Right);
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
			//graphicsDevice.BlendState = BlendState.NonPremultiplied;

			// Apply the effect
			effect.CurrentTechnique.Passes[0].Apply();

			// Draw the billboards
			graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, ParticleNum * 4, 0, ParticleNum * 2);
            //graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, ParticleNum * 4, particles.Length - 4, ParticleNum * 2);

			// Un-set the buffers
			graphicsDevice.SetVertexBuffer(null);
			graphicsDevice.Indices = null;

			// Reset render states
			graphicsDevice.BlendState = BlendState.Opaque;
			graphicsDevice.DepthStencilState = DepthStencilState.Default;
			graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
		}
		public void Run()
		{
			MoveParticle();
            vertexBuffers.SetData<ParticleVertex>(particles);
            indexBuffers.SetData<int>(indices);
		}
		public object Clone()
		{
			ExplosionParticleEmitter cloned = (ExplosionParticleEmitter)MemberwiseClone();

			string d1 = cloned.activeParticlesNum.ToString();
			string d2 = cloned.start.ToString();

			// Newしないと最初の1つしか描画されない
			cloned.vertexBuffers = new VertexBuffer(graphicsDevice, typeof(ParticleVertex),
				ParticleNum * 4, BufferUsage.WriteOnly);
			cloned.indexBuffers = new IndexBuffer(graphicsDevice,
				IndexElementSize.ThirtyTwoBits, ParticleNum * 6,
				BufferUsage.WriteOnly);

			// 使いまわしても良いかもしれない
			if (this.effect != null) {
				cloned.effect = this.effect.Clone();
			}

			return cloned;
		}

		#region Constructors
		public ExplosionParticleEmitter(GraphicsDevice graphicsDevice, ContentManager content, Vector3 position, Texture2D texture, int particleNum,
			Vector2 particleSize, float lifespan, float FadeInTime)
			//:base:(graphicsDevice, content, texture, position, particleNum, particleSize, lifespan, FadeInTime)
			: this(graphicsDevice, content, position, texture, particleNum, particleSize, lifespan, FadeInTime, 0, 4.0f)
		{
		}
		/*public ExplosionParticleEmitter(GraphicsDevice graphicsDevice, ContentManager content, Vector3 position, Texture2D texture, int particleNum,
			Vector2 particleSize, float lifespan, float FadeInTime, int type)
			: this(graphicsDevice, content, position, texture, particleNum, particleSize, lifespan, FadeInTime, 0, 5.0f)
		{
		}*/
		public ExplosionParticleEmitter(GraphicsDevice graphicsDevice, ContentManager content, Vector3 position, Texture2D texture, int particleNum,
			Vector2 particleSize, float lifespan, float fadeInTime, int movementType, float speed)
			: this(graphicsDevice, content, position, texture, particleNum, particleSize, lifespan, fadeInTime, movementType, speed, BillboardMode.Spherical,true)
		{
		}
		/// <summary>
		/// 個々のパラメータを細かに設定させるタイプのコンストラクタにしているが、
		/// 構造体にまとめてもいいかもしれない。
		/// </summary>
		/// <param name="position">エミッタの中心位置。パーティクルの出現位置</param>
		/// <param name="texture"></param>
		/// <param name="particleNum"></param>
		/// <param name="particleSize"></param>
		/// <param name="lifespan">パーティクルの寿命[second]</param>
		/// <param name="fadeInTime">フェードインする時間[second]。0ならば即座に描画される</param>
		/// <param name="movementType">移動のタイプ</param>
		/// <param name="speed">速さ</param>
		/// <param name="initialize">すぐに初期化するかどうか</param>
		public ExplosionParticleEmitter(GraphicsDevice graphicsDevice, ContentManager content, Vector3 position, Texture2D texture, int particleNum,
			Vector2 particleSize, float lifespan, float fadeInTime, int movementType, float speed, bool initialize)
			: this(graphicsDevice, content, position, texture, particleNum, particleSize, lifespan, fadeInTime, movementType, speed, BillboardMode.Spherical, initialize)
		{
		}
		/// <summary>
		/// 個々のパラメータを細かに設定させるタイプのコンストラクタにしているが、
		/// 構造体にまとめてもいいかもしれない。
		/// </summary>
		/// <param name="position">エミッタの中心位置。パーティクルの出現位置</param>
		/// <param name="texture"></param>
		/// <param name="particleNum"></param>
		/// <param name="particleSize"></param>
		/// <param name="lifespan">パーティクルの寿命[second]</param>
		/// <param name="fadeInTime">フェードインする時間[second]。0ならば即座に描画される</param>
		/// <param name="movementType">移動のタイプ</param>
		/// <param name="speed">速さ</param>
		/// <param name="initialize">すぐに初期化するかどうか</param>
		public ExplosionParticleEmitter(GraphicsDevice graphicsDevice, ContentManager content, Vector3 position, Texture2D texture, int particleNum,
			Vector2 particleSize, float lifespan, float fadeInTime, int movementType, float speed, BillboardMode mode, bool initialize)
			: base(graphicsDevice, content, texture, position, particleNum, particleSize, lifespan, fadeInTime, mode, initialize)
		{
			//emitNumPerFrame = 100;//50;
			this.Type = movementType;
			this.Speed = speed;
			//speed = 4.0f * 5;
			//MoveParticle();
		}
		#endregion

		#region Lua scripting test
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
			Vector2 particleSize, float lifespan, float fadeInTime, string scriptPath)
			: base(graphicsDevice, content, texture, position, particleNum, particleSize, lifespan, fadeInTime)
		{
			EnableScripting = true;
			this.scriptPath = scriptPath;

			/*// Create new Lua instance
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
			//lua.RegisterFunction("Vector3Multipy", null, GetType().GetMethod("Vector3.Multiply"));*/

			//MoveParticle();
		}
		#endregion


		
	}

}
