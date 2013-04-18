using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HLSLTest
{
	public class MeshTag
	{
		public Vector3 Color;
		public Texture2D Texture;
		public float SpecularPower;
		public Effect CachedEffect;

		public MeshTag(Vector3 Color, Texture2D texture, float specularPower)
		{
			this.Color = Color;
			this.Texture = texture;
			this.SpecularPower = specularPower;
		}

	}
}
