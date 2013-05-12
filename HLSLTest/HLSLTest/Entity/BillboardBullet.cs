using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace HLSLTest
{
	public class BillboardBullet : BillboardSystem
	{
		protected static float MAX_DISTANCE = 10000;
		/// <summary>
		/// インターフェース実装で関数として機能があるからプロパティは消してもいいかも
		/// </summary>
		public bool IsActive { get; set; }
		public float Speed { get; private set; }
		public Vector3 Direction { get; private set; }
		private float distanceTravelled;
		public Vector3 StartPosition { get; private set; }

		public override void Update(GameTime gameTime)
		{
			for (int i = 0; i < particles.Length; i++) {
				particles[i].Position += Direction * Speed;
			}

			vertexBuffers.SetData<VertexPositionTexture>(particles);
			indexBuffers.SetData<int>(indices);

			IsActive = IsActiveNow();
		}
		public bool IsActiveNow()
		{
			distanceTravelled = Vector3.Distance(StartPosition, particles[0].Position);
			if (distanceTravelled > MAX_DISTANCE) {
				return false;
			} else {
				return true;
			}
		}

		// Constructor
		public BillboardBullet(GraphicsDevice graphicsDevice,
			ContentManager content, Vector3 position, Vector3 direction, float speed, Texture2D texture, Vector2 billboardSize)
			:base(graphicsDevice, content, texture, 0, null, billboardSize, new Vector3[] { position })
		{
			this.Direction = direction;
			this.Speed = speed;
			IsActive = true;
			Direction.Normalize();
			StartPosition = position;
		}
	}
}
