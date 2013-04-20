using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HLSLTest
{
	public class CubeMapReflectMaterial : Material
	{
		public TextureCube CubeMap { get; set; }
		public CubeMapReflectMaterial(TextureCube CubeMap)
		{
			this.CubeMap = CubeMap;
		}

		public override void SetEffectParameters(Effect effect)
		{
			if (effect.Parameters["CubeMap"] != null) {
				effect.Parameters["CubeMap"].SetValue(CubeMap);
			}
		}
	}
}
