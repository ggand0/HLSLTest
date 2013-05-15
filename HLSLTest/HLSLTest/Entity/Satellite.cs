using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Audio;

namespace HLSLTest
{
	public class Satellite : Object
	{
		public bool Rotate { get; private set; }
		public bool Revolution { get; private set; }
		/// <summary>
		/// Means planet position
		/// </summary>
		public Vector3 Center { get; private set; }
		public float Radius { get; private set; }
		public float Roll { get; private set; }
		public float Pitch { get; private set; }
		private float rotationSpeed, revolutionSpeed, revolutionAngle;

		public override void Update(GameTime gameTime)
		{
			if (Rotate) {
				Roll += rotationSpeed;
			}
			if (Revolution) {
				revolutionSpeed = 1f;
				revolutionAngle += MathHelper.ToRadians(revolutionSpeed);
				Vector3 velocity = new Vector3((float)Math.Cos(revolutionAngle), 0, (float)Math.Sin(revolutionAngle));
				//Vector3 tmp = StarPosition + velocity * 3000;

				float radius = (Position - Center).Length();
				Vector3 tmp = Center + velocity * radius;

				Position = tmp;
			}

			base.Update(gameTime);
		}
		protected override void UpdateWorldMatrix()
		{
			//base.UpdateWorldMatrix();
			_world = Matrix.CreateScale(Radius) * Matrix.CreateRotationY(Roll) * Matrix.CreateRotationX(Pitch)
				* Matrix.CreateTranslation(Position);
		}

		#region Constructors
		public Satellite(Vector3 position, float scale, string fileName)
			: base(position)
		{
			Rotate = false;
			Revolution = false;
		}
		public Satellite(Vector3 position, Vector3 center, float scale, string fileName)
			:base(position, scale, fileName)
		{
			Revolution = true;
			this.Center = center;
		}
		#endregion
	}
}
