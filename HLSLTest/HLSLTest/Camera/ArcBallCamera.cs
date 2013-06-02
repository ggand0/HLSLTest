using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace HLSLTest
{
	/// <summary>
	/// ターゲットを中心に球状に移動可能なカメラ。カメラ関係を継承使用して整理したいが面倒である
	/// </summary>
	public class ArcBallCamera : Camera
	{
		#region Fields
		// statics
		
		private static readonly float VERTICAL_ANGLE_MIN = 0.01f;
		private static readonly float VERTICAL_ANGLE_MAX = MathHelper.Pi - 0.01f;
		private static readonly float ZOOM_MIN = 100;
		private static readonly float ZOOM_MAX = 10000.0f;
		//private static readonly float ZOOM_RATE = 10; // スムーズにズームさせたい時に使用する予定

		private short zoomMode = 0;
		private float[] zoomState = new float[] { 100, 300, 500, 1000, 4000, 10000 };

		// Matrix
		//public Matrix View { get; set; }
		//public Matrix Projection { get; set; }
		// Current camera properties
		//public Vector3 Position { get; set; }
		public Vector3 Velocity { get; set; }
		// Chased object properties
		//public Vector3 Target { get; set; }//ChasePosition
		public Vector3 ChaseDirection { get; set; }
		//public Vector3 Up { get; private set; }
		//public Vector3 Right { get; private set; }
		//public Vector3 Direction { get; private set; }

		// Desired camera pos
		public Vector3 DesiredPositionOffset { get; set; }
		public Vector3 DesiredPosition { get; set; }
		/// <summary>
		/// 少し上からプレイヤーを見下ろす視点にしたい時など、視点の調整に使用。
		/// </summary>
		//public Vector3 LookAtOffset { get; set; }
		//public Vector3 LookAt { get; set; }
		public Vector3 CameraPosition { get; set; }
		public Matrix rotation = Matrix.Identity;

		private float _zoom = 1000.0f;
		public float Zoom
		{
			get { return _zoom; }
			set { _zoom = MathHelper.Clamp(value, ZOOM_MIN, ZOOM_MAX); }
		}
		private float _verticalAngle = MathHelper.PiOver2;
		public float VerticalAngle
		{
			get	{ return _verticalAngle;	}
			// Keep vertical angle within tolerances
			set	{ _verticalAngle = MathHelper.Clamp(value, VERTICAL_ANGLE_MIN, VERTICAL_ANGLE_MAX); }
		}
		private float _horizontalAngle = MathHelper.PiOver2;
		public float HorizontalAngle
		{
			get	{ return _horizontalAngle;}
			// Keep horizontalAngle between -pi and pi.
			set	{ _horizontalAngle = value % MathHelper.Pi; }
		}

		// Camera physics
		/// <summary>
		/// スプリングの復元力に対するカメラの位置による影響を制御する物理特性係数。
		/// スプリングが硬いほど、追跡対象オブジェクトに近づきます。
		/// </summary>
		public float Stiffness
		{
			get { return stiffness; }
			set { stiffness = value; }
		}
		private float stiffness = 1800.0f;
		/// <summary>
		/// スプリングの内部摩擦を近似する物理特性係数。
		/// 減衰が十分であれば、スプリングが無限に振動しなくなります。
		/// </summary>
		public float Damping
		{
			get { return damping; }
			set { damping = value; }
		}
		private float damping = 600.0f;
		/// <summary>
		/// カメラ本体の重量。オブジェクトが重いほど、軽いオブジェクトと同じ
		/// 速度で動くように、減衰の少ない硬いスプリングが必要になります。
		/// </summary>
		public float Mass
		{
			get { return mass; }
			set { mass = value; }
		}
		private float mass = 50.0f;

		

		// Perspective properties
		/// <summary>
		/// 遠近法縦横比。既定値をアプリケーションで上書きする必要があります。
		/// </summary>
		public float AspectRatio
		{
			get { return aspectRatio; }
			set { aspectRatio = value; }
		}
		private float aspectRatio = 4.0f / 3.0f;

		/// <summary>
		/// 遠近法視界。
		/// </summary>
		public float FieldOfView
		{
			get { return fieldOfView; }
			set { fieldOfView = value; }
		}
		private float fieldOfView = MathHelper.ToRadians(45.0f);


		private BoundingFrustum _frustum = new BoundingFrustum(Matrix.Identity);
		public BoundingFrustum Frustum
		{
			get { return _frustum; }
		}
		Matrix _eyeTransform = Matrix.Identity;
		public Matrix EyeTransform
		{
			get { return _eyeTransform; }
		}
		#endregion

		#region Methods
		/// <summary>
		/// Project bounding sphere on our screen: topLeft in post-projection coordinates [-1..1] 
		/// and size in range [0..2]
		/// http://www.gamasutra.com/view/feature/2942/the_mechanics_of_robust_stencil_.php?page=6
		/// This method was taken from
		/// //////////////////////////////////////////////////////////////////////////////////////////
		//	Point Light/CalculateScissorRectangle.cpp
		//	Calculate the scissor rectangle for a point light
		//	Downloaded from: www.paulsprojects.net
		//	Created:	14th December 2002
		//
		//	Copyright (c) 2006, Paul Baker
		//	Distributed under the New BSD Licence. (See accompanying file License.txt or copy at
		//	http://www.paulsprojects.net/NewBSDLicense.txt)
		//////////////////////////////////////////////////////////////////////////////////////////	
		/// </summary>
		/// <param name="boundingSphere"></param>
		/// <param name="topLeft"></param>
		/// <param name="size"></param>
		public void ProjectBoundingSphereOnScreen(BoundingSphere boundingSphere, out Vector2 topLeft, out Vector2 size)
		{
			
			float _fovYDegrees = 45;

			//l is the bounding sphere's position in eye space
			Vector3 l = Vector3.Transform(boundingSphere.Center, _eyeTransform);

			//Store the coordinates of the scissor rectangle
			//Start by setting them to the outside of the screen
			float scissorLeft = -1.0f;
			float scissorRight = 1.0f;

			float scissorBottom = -1.0f;
			float scissorTop = 1.0f;

			//r is the radius of the bounding sphere
			float r = boundingSphere.Radius;


			//halfNearPlaneHeight is half the height of the near plane, i.e. from the centre to the top
			float halfNearPlaneHeight = NearPlaneDistance * (float)Math.Tan(MathHelper.ToRadians(_fovYDegrees * 0.5f));

			float halfNearPlaneWidth = halfNearPlaneHeight * aspectRatio;

			//All calculations in eye space


			//We wish to find 2 planes parallel to the Y axis which are tangent to the bounding sphere
			//of the light and pass through the origin (camera position)

			//plane normal. Of the form (x, 0, z)
			Vector3 normal;

			//Calculate the discriminant of the quadratic we wish to solve to find nx(divided by 4)
			float d = (l.Z * l.Z) * ((l.X * l.X) + (l.Z * l.Z) - r * r);

			//If d>0, solve the quadratic to get the normal to the plane
			if (d > 0.0f) {
				float rootD = (float)Math.Sqrt(d);

				//Loop through the 2 solutions
				for (int i = 0; i < 2; ++i) {
					//Calculate the normal
					if (i == 0)
						normal.X = r * l.X + rootD;
					else
						normal.X = r * l.X - rootD;

					normal.X /= (l.X * l.X + l.Z * l.Z);

					normal.Z = r - normal.X * l.X;
					normal.Z /= l.Z;

					//We need to divide by normal.X. If ==0, no good
					if (normal.X == 0.0f)
						continue;


					//p is the point of tangency
					Vector3 p;

					p.Z = (l.X * l.X) + (l.Z * l.Z) - r * r;
					p.Z /= l.Z - ((normal.Z / normal.X) * l.X);

					//If the point of tangency is behind the camera, no good
					if (p.Z >= 0.0f)
						continue;

					p.X = -p.Z * normal.Z / normal.X;

					//Calculate where the plane meets the near plane
					//divide by the width to give a value in [-1, 1] for values on the screen
					float screenX = normal.Z * NearPlaneDistance / (normal.X * halfNearPlaneWidth);

					//If this is a left bounding value (p.X<l.X) and is further right than the
					//current value, update
					if (p.X < l.X && screenX > scissorLeft)
						scissorLeft = screenX;

					//Similarly, update the right value
					if (p.X > l.X && screenX < scissorRight)
						scissorRight = screenX;
				}
			}


			//Repeat for planes parallel to the x axis
			//normal is now of the form(0, y, z)
			normal.X = 0.0f;

			//Calculate the discriminant of the quadratic we wish to solve to find ny(divided by 4)
			d = (l.Z * l.Z) * ((l.Y * l.Y) + (l.Z * l.Z) - r * r);

			//If d>0, solve the quadratic to get the normal to the plane
			if (d > 0.0f) {
				float rootD = (float)Math.Sqrt(d);

				//Loop through the 2 solutions
				for (int i = 0; i < 2; ++i) {
					//Calculate the normal
					if (i == 0)
						normal.Y = r * l.Y + rootD;
					else
						normal.Y = r * l.Y - rootD;

					normal.Y /= (l.Y * l.Y + l.Z * l.Z);

					normal.Z = r - normal.Y * l.Y;
					normal.Z /= l.Z;

					//We need to divide by normal.Y. If ==0, no good
					if (normal.Y == 0.0f)
						continue;


					//p is the point of tangency
					Vector3 p;

					p.Z = (l.Y * l.Y) + (l.Z * l.Z) - r * r;
					p.Z /= l.Z - ((normal.Z / normal.Y) * l.Y);

					//If the point of tangency is behind the camera, no good
					if (p.Z >= 0.0f)
						continue;

					p.Y = -p.Z * normal.Z / normal.Y;

					//Calculate where the plane meets the near plane
					//divide by the height to give a value in [-1, 1] for values on the screen
					float screenY = normal.Z * NearPlaneDistance / (normal.Y * halfNearPlaneHeight);

					//If this is a bottom bounding value (p.Y<l.Y) and is further up than the
					//current value, update
					if (p.Y < l.Y && screenY > scissorBottom)
						scissorBottom = screenY;

					//Similarly, update the top value
					if (p.Y > l.Y && screenY < scissorTop)
						scissorTop = screenY;
				}
			}

			//compute the width & height of the rectangle
			size.X = scissorRight - scissorLeft;
			size.Y = scissorTop - scissorBottom;

			topLeft.X = scissorLeft;
			topLeft.Y = -scissorBottom - size.Y;

		}


		/// <summary>
		/// ワールド空間におけるオブジェクト空間の値を再構築します。
		/// ワールド空間の値をパブリックに返す前、またはワールド空間の値に
		/// プライベートでアクセスする前に呼び出します。
		/// </summary>
		private void UpdateWorldPositions()
		{
			// old ver(13/4/25)
			/*Direction = LookAt - CameraPosition;
			Up = Vector3.Up;
			Direction = Vector3.Normalize(Direction);
			Right = Vector3.Normalize(Right);
			Right = Vector3.Cross(this.Direction, Vector3.Up);
			Right = Vector3.Normalize(Right);*/

			//rotation = Matrix.CreateFromYawPitchRoll(HorizontalAngle, MathHelper.ToRadians(270) + VerticalAngle, 0);
			/**/rotation = Matrix.CreateFromYawPitchRoll(HorizontalAngle, MathHelper.ToRadians(-90) + VerticalAngle, 0);
			Direction = LookAt - CameraPosition;
			Direction = Vector3.Normalize(Direction);
			Up = Vector3.Transform(Vector3.Up, rotation);
			//Up = Vector3.Up;
			Up = Vector3.Normalize(Up);
			Right = Vector3.Cross(Direction, Up);
			Right = Vector3.Normalize(Right);


			// オブジェクト(のローカル)空間からワールド空間にトランスフォームする行列を構築する
			/*Matrix transform = Matrix.Identity;
			ChaseDirection = Vector3.Normalize(ChaseDirection);
			transform.Forward = ChaseDirection;
			transform.Up = Up;*/
			Matrix transform = Matrix.Identity;
			ChaseDirection = Vector3.Normalize(ChaseDirection);
			transform.Forward = ChaseDirection;
			transform.Up = Up;
			//transform.Right = Vector3.Cross(Up, ChaseDirection);


			// ワールド空間における目的のカメラ プロパティを計算する
			/*DesiredPosition = ChasePosition +
				Vector3.TransformNormal(DesiredPositionOffset, transform);*/
			//DesiredPosition += new Vector3(JoyStick.Vector;
			CameraPosition = new Vector3(0.0f, _zoom, 0.0f);
			// Rotate vertically
			CameraPosition = Vector3.Transform(CameraPosition, Matrix.CreateRotationX(_verticalAngle));
			// Rotate horizontally
			CameraPosition = Vector3.Transform(CameraPosition, Matrix.CreateRotationY(_horizontalAngle));
			//Position = ChasePosition + CameraPosition;
			DesiredPosition = Target + CameraPosition;

			LookAt = Target +
				Vector3.TransformNormal(LookAtOffset, transform);
		}
		private void UpdateViewProjection()
		{
			//View = Matrix.CreateLookAt(this.Position, this.LookAt, Vector3.Up);
			View = Matrix.CreateLookAt(this.Position, this.LookAt, Up);
			Projection = Matrix.CreatePerspectiveFieldOfView(FieldOfView,
				AspectRatio, NearPlaneDistance, FarPlaneDistance);
		}
		private void HandleInput()
		{
			if (JoyStick.vectorOther.Length() > 0.2) {
				_verticalAngle += MathHelper.ToRadians(JoyStick.vectorOther.Y);
				_horizontalAngle += MathHelper.ToRadians(JoyStick.vectorOther.X);
			}

			/*if (JoyStick.IsOnKeyDown(7)) {
				zoomMode++;
				if (zoomMode == zoomState.Length) zoomMode = 0;
			}
			_zoom = zoomState[zoomMode];*/
			if (JoyStick.KEY(7)) {
				_zoom += 20;
			}
			if (JoyStick.KEY(5)) {
				_zoom -= 20;
			}


			/*//float speed = 10;
			float stickSensitivity = 0.2f;
			//  スティックが倒されていればDirectionを再計算する
			if (JoyStick.Vector.Length() > stickSensitivity) {
				double analogAngle = Math.Atan2(JoyStick.Vector.Y, JoyStick.Vector.X);
				float speed = JoyStick.Vector.Length() * 30;
				analogAngle += MathHelper.ToRadians(-90);
                
				Vector3 tmpVelocity = Vector3.Zero;
				Dirention = tmpCameraPos - camera.Position;
				Dirention = new Vector3(tmpDirention.X, 0, tmpDirention.Z);
				RotationMatrix = Matrix.CreateRotationY((float)analogAngle);
				// 面白い動き : //RotationMatrix = Matrix.CreateRotationY(MathHelper.ToRadians(JoyStick.Vector.Y)) * Matrix.CreateRotationX(MathHelper.ToRadians(-JoyStick.Vector.X));
				tmpDirention = Vector3.TransformNormal(tmpDirention, RotationMatrix);
				tmpDirention = Vector3.Normalize(tmpDirention);// プロパティなので代入しないと反映されないことに注意
				tmpVelocity = new Vector3(tmpDirention.X * speed, tmpVelocity.Y, tmpDirention.Z * speed);

				tmpCameraPos += tmpVelocity;
			}*/
			/*if (JoyStick.stickDirection == HLSLTest.Direction.LEFT) {
				Target += new Vector3(-speed, 0, 0);
			} else if (JoyStick.stickDirection == HLSLTest.Direction.RIGHT) {
				Target += new Vector3(speed, 0, 0);
			}
			if (JoyStick.stickDirection == HLSLTest.Direction.UP) {
				Target += new Vector3(0, 0, speed);
			} else if (JoyStick.stickDirection == HLSLTest.Direction.DOWN) {
				Target += new Vector3(0, 0, -speed);
			}*/

		}
		/// <summary>
		/// カメラを目的の位置に配置し、移動を停止します。これは、
		/// 追跡対象オブジェクトの初回作成時、またはオブジェクトの移動後に便利です。
		/// 追跡対象オブジェクトの位置を大幅に変更した後に、これを呼び出さないと、
		/// カメラが高速で飛んでしまいます。
		/// </summary>
		private void Reset()
		{
			UpdateWorldPositions();

			// 移動を停止する
			Velocity = Vector3.Zero;

			// 目的の位置に設定する
			//Position = ChasePosition + AdjustOffset/**/ + CameraPosition;// AdjustOffsetを加えてPlayerの少し上を追わせるようにする
			Position = Target + CameraPosition;
			//Position = DesiredPosition + AdjustOffset;
			
			UpdateViewProjection();
		}
		


		/// <summary>
		/// カメラによって追尾されるように値を更新する
		/// </summary>
		public void UpdateChaseTarget(Object target)
		{
			/*ChasePosition = target.Position;
			ChaseDirection = target.Direction;
			ChaseDirection = target.Position - Position;
			Up = target.Up;*/

			// ActionGameと違って今は対象の方向と一致していない
			Target = target.Position;
			ChaseDirection = target.Position - Position;
			//Up = target.RotationMatrix.Up;
		}
		public void UpdateChaseTarget(Vector3 target)
		{
			// ActionGameと違って今は対象の方向と一致していない
			Target = target;
			ChaseDirection = target - Position;
			//Up = target.RotationMatrix.Up;
		}


		/// <summary>
		/// カメラの現在位置から、追跡されるオブジェクトの背後の目的のオフセットに向かって
		/// カメラをアニメーション表示します。カメラのアニメーションは、
		/// カメラに取り付けられ、かつ目的位置に固定された単純な物理スプリングによって制御されます。
		/// </summary>
		public override void Update(GameTime gameTime)
		{
			float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

			if (gameTime == null) {
				throw new ArgumentNullException("gameTime is null.");
			}

			UpdateChaseTarget(Vector3.Zero);
			HandleInput();
			UpdateWorldPositions();

			/*// スプリングの力を計算する
			Vector3 stretch = Position - DesiredPosition;
			Vector3 force = -stiffness * stretch - damping * Velocity;
			// 加速度を適用する
			Vector3 acceleration = force / mass;// ma = fより
			Velocity += acceleration * elapsed;
			// 速度を適用する
			Position += Velocity * elapsed;*/

			Position = DesiredPosition;
			//Position += Velocity;
			/*if (Position.Y < 0) {
				Position = new Vector3(Position.X, 0, Position.Z);
			}*/
			UpdateViewProjection();
		}
		#endregion

		/// <summary>
		/// Targetの設定を含む初期化を行う。
		/// </summary>
		public void Initialize(Object target)
		{
			// カメラのオフセットを設定します
			DesiredPositionOffset = new Vector3(0.0f, 2000.0f, 3500.0f);
			//LookAtOffset = new Vector3(0.0f, 150.0f, 0.0f);// 少し上を見るように調整されてあるようだ
			LookAtOffset = new Vector3(0.0f, 25.0f, 0.0f);

			// カメラの視点を設定します
			NearPlaneDistance = 10.0f;
			FarPlaneDistance = 100000.0f;
			// カメラのアスペクト比を設定します
			// これは、グラフィック デバイスを初期化する base.Initalize() の
			// 呼び出しの後で行う必要があります。
			AspectRatio = (float)game.GraphicsDevice.Viewport.Width /
				game.GraphicsDevice.Viewport.Height;

			// カメラで初期リセットを実行し、静止位置で開始するようにます。これを行わないと、カメラは原点で開始し
			// 追尾対象オブジェクトを追ってワールド中を移動します。ここで実行する理由は、Reset でアスペクト比が必要になるためです。
			UpdateChaseTarget(target);
			Reset();
		}
		public void Initialize(Vector3 target)
		{
			// カメラのオフセットを設定します
			DesiredPositionOffset = new Vector3(0.0f, 2000.0f, 3500.0f);
			//LookAtOffset = new Vector3(0.0f, 150.0f, 0.0f);// 少し上を見るように調整されてあるようだ
			LookAtOffset = new Vector3(0.0f, 25.0f, 0.0f);

			// カメラの視点を設定します
			NearPlaneDistance = 10.0f;
			FarPlaneDistance = 100000.0f;
			// カメラのアスペクト比を設定します
			// これは、グラフィック デバイスを初期化する base.Initalize() の
			// 呼び出しの後で行う必要があります。
			AspectRatio = (float)game.GraphicsDevice.Viewport.Width /
				game.GraphicsDevice.Viewport.Height;

			// カメラで初期リセットを実行し、静止位置で開始するようにます。これを行わないと、カメラは原点で開始し
			// 追尾対象オブジェクトを追ってワールド中を移動します。ここで実行する理由は、Reset でアスペクト比が必要になるためです。
			UpdateChaseTarget(target);
			Reset();
		}
		public ArcBallCamera(Vector3 Target)
		{
			this.Target = Target;
			Initialize(Target);
		}
		public ArcBallCamera(Vector3 Position, Vector3 Target, Vector3 up)
		{
			this.Position = Position;
			this.Target = Target;
			this.Up = up;

			LookAt = Target + LookAtOffset;
			View = Matrix.CreateLookAt(this.Position, this.LookAt, this.Up);
			//View = Matrix.CreateLookAt(this.Position, this.LookAt, this.t);
			Projection = Matrix.CreatePerspectiveFieldOfView(FieldOfView,
				AspectRatio, NearPlaneDistance, FarPlaneDistance);

			Initialize(Target);
		}
		public ArcBallCamera()
			:this(Vector3.Zero)
		{
		}

	}
}
