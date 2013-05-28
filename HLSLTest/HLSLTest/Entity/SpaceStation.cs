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
	public class SpaceStation : Satellite
	{
		#region Constructors
		public SpaceStation(Vector3 position, float scale, string fileName)
			: base(position, scale, fileName)
		{
			Rotate = false;
			Revolution = false;
		}
		public SpaceStation(bool revolution, Vector3 position, Vector3 center, float scale, string fileName)
			:base(revolution, position, center, scale, fileName)
		{
			this.Revolution = revolution;
			this.Center = center;
		}
		#endregion
	}
}
