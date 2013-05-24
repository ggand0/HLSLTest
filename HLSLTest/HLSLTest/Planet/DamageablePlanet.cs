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
	/// <summary>
	/// 守るべき対象である惑星クラス
	/// </summary>
	public class DamageablePlanet : Planet, IDamageable
	{
		protected static readonly int DEF_HIT_POINT = 50;

		public int HitPoint { get; private set; }

		public void Damage()
		{
			HitPoint--;
			if (HitPoint <= 0) {
				IsActive = false;
			}
		}

		public DamageablePlanet(Vector3 position, GraphicsDevice graphicsDevice, ContentManager content)
			:base(position, graphicsDevice, content)
		{
			HitPoint = DEF_HIT_POINT;
		}
	}
}
