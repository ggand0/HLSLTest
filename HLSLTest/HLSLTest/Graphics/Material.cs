using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HLSLTest
{
	public class Material
	{
		public virtual void SetEffectParameters(Effect effect)
		{
		}
	}

	public class LightingMaterial : Material
	{
		public Vector3 AmbientColor { get; set; }
		public Vector3 LightDirection { get; set; }
		public Vector3 LightColor { get; set; }
		public Vector3 SpecularColor { get; set; }
		public LightingMaterial()
		{
			AmbientColor = new Vector3(.1f, .1f, .1f);
			LightDirection = new Vector3(1, 1, 1);
			LightColor = new Vector3(.9f, .9f, .9f);
			SpecularColor = new Vector3(1, 1, 1);
		}
		public override void SetEffectParameters(Effect effect)
		{
			if (effect.Parameters["AmbientColor"] != null)
				effect.Parameters["AmbientColor"].SetValue(AmbientColor);
			if (effect.Parameters["LightDirection"] != null)
				effect.Parameters["LightDirection"].SetValue(LightDirection);
			if (effect.Parameters["LightColor"] != null)
				effect.Parameters["LightColor"].SetValue(LightColor);
			if (effect.Parameters["SpecularColor"] != null)
				effect.Parameters["SpecularColor"].SetValue(SpecularColor);
		}
	}

	/// <summary>
	/// K_{att} = 1 - (d/a)^f
	/// </summary>
	public class PointLightMaterial : Material
	{
		public Vector3 AmbientLightColor { get; set; }
		public Vector3 LightPosition { get; set; }
		public Vector3 LightColor { get; set; }
		/// <summary>
		/// 輝度の減少を表す
		/// </summary>
		public float LightAttenuation { get; set; }
		public float LightFalloff { get; set; }


		public PointLightMaterial()
		{
			AmbientLightColor = new Vector3(.15f, .15f, .15f);
			LightPosition = new Vector3(0, 0, 0);
			LightColor = new Vector3(.85f, .85f, .85f);
			LightAttenuation = 5000;
			LightFalloff = 2;
		}
		public override void SetEffectParameters(Effect effect)
		{
			if (effect.Parameters["AmbientLightColor"] != null)
				effect.Parameters["AmbientLightColor"].SetValue(AmbientLightColor);
			if (effect.Parameters["LightPosition"] != null)
				effect.Parameters["LightPosition"].SetValue(LightPosition);
			if (effect.Parameters["LightColor"] != null)
				effect.Parameters["LightColor"].SetValue(LightColor);
			if (effect.Parameters["LightAttenuation"] != null)
				effect.Parameters["LightAttenuation"].SetValue(LightAttenuation);
			if (effect.Parameters["LightFalloff"] != null)
				effect.Parameters["LightFalloff"].SetValue(LightFalloff);
		}
	}

	/// <summary>
	/// K_{att} = (dot(p - lp, ld) / cos(a))^f
	/// </summary>
	public class SpotLightMaterial : Material
	{
		public Vector3 AmbientLightColor { get; set; }
		public Vector3 LightPosition { get; set; }
		public Vector3 LightColor { get; set; }
		public Vector3 LightDirection { get; set; }
		public float ConeAngle { get; set; }
		public float LightFalloff { get; set; }
		public SpotLightMaterial()
		{
			AmbientLightColor = new Vector3(.15f, .15f, .15f);
			LightPosition = new Vector3(0, 3000, 0);
			LightColor = new Vector3(.85f, .85f, .85f);
			ConeAngle = 30;
			LightDirection = new Vector3(0, -1, 0);
			LightFalloff = 20;
		}
		public override void SetEffectParameters(Effect effect)
		{
			if (effect.Parameters["AmbientLightColor"] != null)
				effect.Parameters["AmbientLightColor"].SetValue(
				AmbientLightColor);
			if (effect.Parameters["LightPosition"] != null)
				effect.Parameters["LightPosition"].SetValue(LightPosition);
			if (effect.Parameters["LightColor"] != null)
				effect.Parameters["LightColor"].SetValue(LightColor);
			if (effect.Parameters["LightDirection"] != null)
				effect.Parameters["LightDirection"].SetValue(LightDirection);
			if (effect.Parameters["ConeAngle"] != null)
				effect.Parameters["ConeAngle"].SetValue(
				MathHelper.ToRadians(ConeAngle / 2));
			if (effect.Parameters["LightFalloff"] != null)
				effect.Parameters["LightFalloff"].SetValue(LightFalloff);
		}
	}

	public class MultiLightingMaterial : Material
	{
		public Vector3 AmbientColor { get; set; }
		public Vector3[] LightDirection { get; set; }
		public Vector3[] LightColor { get; set; }
		public Vector3 SpecularColor { get; set; }
		public MultiLightingMaterial()
		{
			AmbientColor = new Vector3(.1f, .1f, .1f);
			LightDirection = new Vector3[3];
			LightColor = new Vector3[] { Vector3.One, Vector3.One, Vector3.One };
			SpecularColor = new Vector3(1, 1, 1);
		}
		public override void SetEffectParameters(Effect effect)
		{
			if (effect.Parameters["AmbientColor"] != null)
				effect.Parameters["AmbientColor"].SetValue(AmbientColor);
			if (effect.Parameters["LightDirection"] != null)
				effect.Parameters["LightDirection"].SetValue(LightDirection);
			if (effect.Parameters["LightColor"] != null)
				effect.Parameters["LightColor"].SetValue(LightColor);
			if (effect.Parameters["SpecularColor"] != null)
				effect.Parameters["SpecularColor"].SetValue(SpecularColor);
		}
	}
	public class ProjectedTextureMaterial : Material
	{
		public Vector3 ProjectorPosition { get; set; }
		public Vector3 ProjectorTarget { get; set; }
		public Texture2D ProjectedTexture { get; set; }
		public bool ProjectorEnabled { get; set; }
		public float Scale { get; set; }
		float halfWidth, halfHeight;

		public ProjectedTextureMaterial(Texture2D Texture,
			GraphicsDevice graphicsDevice)
		{
			ProjectorPosition = new Vector3(1500, 1500, 1500);
			ProjectorTarget = new Vector3(0, 150, 0);
			ProjectorEnabled = true;
			ProjectedTexture = Texture;

			// We determine how large the texture will be based on the
			// texture dimensions and a scaling value
			halfWidth = Texture.Width / 2.0f;
			halfHeight = Texture.Height / 2.0f;
			Scale = 1;
		}
		public override void SetEffectParameters(Effect effect)
		{
			if (effect.Parameters["ProjectorEnabled"] != null)
				effect.Parameters["ProjectorEnabled"].SetValue(ProjectorEnabled);
			if (!ProjectorEnabled)
				return;

			// Calculate an orthographic projection matrix for the
			// projector "camera"
			// projectionの対象位置に仮想カメラがあると想定して計算する。
			// ただし、普通のprojectionと違い、距離が変わってもスケールが変わらないことに注意する。（orthographicな行列）
			Matrix projection = Matrix.CreateOrthographicOffCenter(
			-halfWidth * Scale, halfWidth * Scale,
			-halfHeight * Scale, halfHeight * Scale,
			-100000, 100000);

			// Calculate view matrix as usual
			Matrix view = Matrix.CreateLookAt(ProjectorPosition,// viewがNANになってたのでVector3.UnitXとかにする？
			ProjectorTarget, Vector3.UnitX);//Vector3.UnitX);//Vector3.Up);

			if (effect.Parameters["ProjectorViewProjection"] != null)
				effect.Parameters["ProjectorViewProjection"].SetValue(view * projection);
			if (effect.Parameters["ProjectedTexture"] != null)
				effect.Parameters["ProjectedTexture"].SetValue(ProjectedTexture);
		}

	}

}
