using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace HLSLTest
{
	public struct ParticleAttribute
	{
		public Texture2D Texture;
		public int ParticleNum;
		public Vector2 Scale;
		public float Lifespan;
		public float FadeInTime;
		public int MovementType;
		public float Speed;

		public ParticleAttribute(Texture2D texture, int particleNum, Vector2 scale
			, float Lifespan, float fadeInTime, int movementType, float speed)
		{
			this.Texture = texture;
			this.ParticleNum = particleNum;
			this.Scale = scale;
			this.Lifespan = Lifespan;
			this.FadeInTime = fadeInTime;
			this.MovementType = movementType;
			this.Speed = speed;
		}
	}
	/*public struct ParticleSettings
	{
		public ParticleAttribute[] attributes;

		public ParticleSettings(
	}*/

	/*public class ParticleSettings
	{
		public List<ExplosionParticleEmitter> emitters = new List<ExplosionParticleEmitter>();

		private void LoadEmitters(ContentManager content, GraphicsDevice graphics,
			Vector3 position, string filePath)
		{
			LoadParticleSettings load = new LoadParticleSettings();
			emitters = (List<ExplosionParticleEmitter>)load.Load(graphics, content, position, filePath);
		}
		public ParticleSettings(string filePath)
		{
			//LoaderEmitters(filePath);
		}
	}*/
}
