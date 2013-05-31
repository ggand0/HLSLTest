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
		public List<Turret> Turrets { get; private set; }
		private Vector3[] turretPositions = new Vector3[] { new Vector3(100, 0, 0), new Vector3(200, 0, 0), new Vector3(300, 0, 0) };

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			foreach (Turret t in Turrets) {
				t.Update(gameTime);
			}
		}

		public override void Draw(Matrix View, Matrix Projection, Vector3 CameraPosition)
		{
			base.Draw(View, Projection, CameraPosition);

			if (!DrawingPrePass)
				foreach (Turret t in Turrets) {
					t.Draw(View, Projection, CameraPosition);
				}
		}

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

			Turrets = new List<Turret>();
			foreach (Vector3 v in turretPositions) {
				Turrets.Add(new Turret(position + v, 1));
			}
		}
		#endregion
	}
}
