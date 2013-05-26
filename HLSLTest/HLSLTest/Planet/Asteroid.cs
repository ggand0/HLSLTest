using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace HLSLTest
{
	public class Asteroid : Object
	{
		public Vector3 Destination { get; private set; }
		public float Speed { get; private set; }
		Effect lightingEffect;

		protected override void UpdateWorldMatrix()
		{
			//base.UpdateWorldMatrix();
			_world = Matrix.CreateScale(Scale) * //Matrix.CreateRotationY(Roll) * Matrix.CreateRotationX(Pitch) *
				Matrix.CreateTranslation(Position);
		}
		public override void Update(GameTime gameTime)
		{
			Velocity = (Vector3.Normalize(Destination - Position)) * Speed;
			Position += Velocity;

			UpdateWorldMatrix();
		}
		int blinkCount;
		float e;
		public override void Draw(Matrix View, Matrix Projection, Vector3 CameraPosition)
		{
			blinkCount++;
			if (blinkCount % 5 == 0) e += 60f;//30f;//.02f;
			float dColor = (float)Math.Sin(e * 8) / 2.0f + 0.5f;
			SetEffectParameter(lightingEffect, "AccentColor", Color.Red * dColor);

			base.Draw(View, Projection, CameraPosition);
		}

		public Asteroid(Vector3 position, float scale, string fileName)
			:base(position, scale, fileName)
		{
		}
		public Asteroid(Vector3 position, Vector3 destination, float scale, string fileName)
			: base(position, scale, fileName)
		{
			this.Destination = destination;
			Speed = 1;

			lightingEffect = content.Load<Effect>("Lights\\AsteroidLightingEffect");	// load Prelighting Effect
			SetEffectParameter(lightingEffect, "AccentColor", Color.Red);
			// Accent Colorを後で変えたいので、変更をそのまま反映させるためfalseにする
			SetModelEffect(lightingEffect, true);					// set effect to each modelmeshpart
		}
	}
}
