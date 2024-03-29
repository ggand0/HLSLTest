﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace HLSLTest
{
	public class Drawable
	{
		public bool IsActive { get; set; }
		public bool IsAlive { get; set; }
		public Vector3 Position { get; set; }
		

		public virtual void Update(GameTime gameTime)
		{
		}
		public virtual void Draw(GameTime gameTime)
		{
		}
		/*public virtual void Draw(Matrix View, Matrix Projection, Vector3 CameraPosition)
		{
		}
		public virtual void Draw(Matrix View, Matrix Projection, Vector3 Up, Vector3 Right)
		{
		}*/
		public virtual void Draw(Camera camera)
		{
		}


		public virtual bool IsHitWith(Drawable d)
		{
			return false;
		}
		public virtual bool IsHitWith(Object o)
		{
			return false;
		}
		public virtual void Die()
		{
			IsActive = false;
			IsAlive = false;
		}

		public Drawable()
		{
			IsActive = true;
			IsAlive = true;
		}
	}
}
