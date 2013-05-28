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
	public class Turret : Object
	{
		private SoundEffect shootSound;
		private List<SoundEffectInstance> currentSounds = new List<SoundEffectInstance>();
		EnergyShieldEffect shieldEffect;

		private List<Object> visibleEnemies;
		private BoundingSphere sensorSphere;

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);
		}

		#region Constructors
		public Turret(Vector3 position, float scale, string fileName)
			: base(position, scale, fileName)
		{

		}
		#endregion
	}
}
