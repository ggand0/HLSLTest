using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace HLSLTest
{
	public interface IBullet
	{
		bool IsActiveNow();
		void Draw(Matrix View, Matrix Projection, Matrix CameraPosition);
		void Update(GameTime gameTime);
	}
}
