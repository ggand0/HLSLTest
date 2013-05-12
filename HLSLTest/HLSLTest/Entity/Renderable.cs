using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace HLSLTest
{
	public class Drawable
	{
		public bool IsActive { get; set; }

		public virtual void Update(GameTime gameTime)
		{
		}
		public virtual void Draw(GameTime gameTime)
		{
		}
		public virtual void Draw(Matrix View, Matrix Projection, Vector3 CameraPosition)
		{
		}
		public virtual void Draw(Matrix View, Matrix Projection, Vector3 Up, Vector3 Right)
		{
		}

		public Drawable()
		{
			IsActive = true;
		}
	}
}
