using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HLSLTest
{
	public abstract class Camera
	{
		public Matrix View { get; set; }
		public Matrix Projection { get; set; }
		protected GraphicsDevice GraphicsDevice { get; set; }
		protected float FarPlaneDistance = 1000000.0f;

		public Camera(GraphicsDevice graphicsDevice)
		{
			//FarPlaneDistance = 2000;
			this.GraphicsDevice = graphicsDevice;
			generatePerspectiveProjectionMatrix(MathHelper.PiOver4);
		}
		private void generatePerspectiveProjectionMatrix(float FieldOfView)
		{
			PresentationParameters pp = GraphicsDevice.PresentationParameters;
			float aspectRatio = (float)pp.BackBufferWidth /
			(float)pp.BackBufferHeight;

			this.Projection = Matrix.CreatePerspectiveFieldOfView(
				MathHelper.ToRadians(45), aspectRatio, 0.1f, FarPlaneDistance);
		}
		public virtual void Update()
		{
		}
	}

	public class TargetCamera : Camera
	{
		public Vector3 Position { get; set; }
		public Vector3 Target { get; set; }
		public TargetCamera(Vector3 Position, Vector3 Target,
			GraphicsDevice graphicsDevice)
			: base(graphicsDevice)
		{
			this.Position = Position;
			this.Target = Target;
			
		}
		public override void Update()
		{
			Vector3 forward = Target - Position;
			Vector3 side = Vector3.Cross(forward, Vector3.Up);
			Vector3 up = Vector3.Cross(forward, side);

			//up.Normalize();
			this.View = Matrix.CreateLookAt(Position, Target, up);
		}
	}
}
