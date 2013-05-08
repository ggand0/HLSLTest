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
		public Vector3 Up { get; protected set; }
		public Vector3 Right { get; protected set; }
		public Vector3 Direction { get; protected set; }

		protected GraphicsDevice GraphicsDevice { get; set; }
		/// <summary>
		/// 近くのクリップ面との距離。
		/// </summary>
		public float NearPlaneDistance
		{
			get { return nearPlaneDistance; }
			set { nearPlaneDistance = value; }
		}
		private float nearPlaneDistance = 1.0f;

		/// <summary>
		/// 遠くのクリップ面との距離。
		/// </summary>
		public float FarPlaneDistance
		{
			get { return farPlaneDistance; }
			set { farPlaneDistance = value; }
		}
		private float farPlaneDistance = 100000.0f;

		public Camera()
		{
		}
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

		public Vector3 Up { get; private set; }
		public Vector3 Right { get; private set; }

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

			this.Up = up;
			this.Right = Vector3.Cross(forward, up);

			//up.Normalize();
			this.View = Matrix.CreateLookAt(Position, Target, up);
		}
	}

	public class FreeCamera : Camera
	{
		public float Yaw { get; set; }
		public float Pitch { get; set; }
		public Vector3 Position { get; set; }
		public Vector3 Target { get; private set; }
		private Vector3 translation;

		public void Rotate(float YawChange, float PitchChange)
		{
			this.Yaw += YawChange;
			this.Pitch += PitchChange;
		}
		public void Move(Vector3 Translation)
		{
			this.translation += Translation;
		}

		public override void Update()
		{
			// Calculate the rotation matrix
			Matrix rotation = Matrix.CreateFromYawPitchRoll(Yaw, Pitch, 0);
			// Offset the position and reset the translation
			translation = Vector3.Transform(translation, rotation);
			Position += translation;
			translation = Vector3.Zero;
			// Calculate the new target
			Vector3 forward = Vector3.Transform(Vector3.Forward, rotation);
			Target = Position + forward;
			// Calculate the up vector
			Vector3 up = Vector3.Transform(Vector3.Up, rotation);
			// Calculate the view matrix
			View = Matrix.CreateLookAt(Position, Target, up);
		}

		public FreeCamera(Vector3 Position, float Yaw, float Pitch,
		GraphicsDevice graphicsDevice)
			: base(graphicsDevice)
		{
			this.Position = Position;
			this.Yaw = Yaw;
			this.Pitch = Pitch;
			translation = Vector3.Zero;
		}
	}
}
