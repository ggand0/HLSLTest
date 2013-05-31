using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace HLSLTest
{
	public enum IFF
	{
		Friend,
		Foe
	}

	/// <summary>
	/// やはりこの基底クラスを作らざるをえない
	/// </summary>
	public class Bullet : Drawable
	{
		protected static float MAX_DISTANCE = 10000;
		public static ContentManager content;

		/// <summary>
		/// 敵弾か味方弾かの情報
		/// </summary>
		public IFF Identification { get; private set; }
		public float Speed { get; private set; }
		public Vector3 Direction { get; protected set; }
		public Vector3 StartPosition { get; private set; }

		
		protected float distanceTravelled;
		private int count;

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);
		}
		public virtual bool IsActiveNow()
		{
			return false;
		}

		public Bullet(IFF identification, Vector3 position, Vector3 direction, float speed)
		{
			this.Identification = identification;
			this.StartPosition = position;
			this.Direction = direction;
			this.Speed = speed;
		}
	}
}
