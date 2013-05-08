using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace HLSLTest
{
	public abstract class SpecialEffect
	{
		public static int MAX_LIFE_SPAN = 10000;
		protected int count;

		public Vector3 Position { get; protected set; }
		public bool Removable { get; protected set; }

		public virtual void Update(GameTime gameTime)
		{
			count++;

			if (count > MAX_LIFE_SPAN) {
				Removable = true;
			}
		}
		/*public void Draw(GameTime gameTime, Matrix View, Matrix Projection, Vector3 CameraPosition, Vector3 CameraDirection, Vector3 Up, Vector3 Right)
		{
		}*/
		public virtual void Draw(GameTime gameTime, Camera camera)
		{
		}
	}
}
