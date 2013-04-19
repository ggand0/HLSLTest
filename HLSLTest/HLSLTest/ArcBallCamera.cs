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
	public class ArcBallCamera
	{
		#region Fields
		// statics
		private static readonly float VERTICAL_ANGLE_MIN = 0.01f;
		private static readonly float VERTICAL_ANGLE_MAX = MathHelper.Pi - 0.01f;
		private static readonly float ZOOM_MIN = 100;//100;
		private static readonly float ZOOM_MAX = 00.0f;
		//private static readonly float ZOOM_RATE = 10; // スムーズにズームさせたい時に使用する予定

		private short zoomMode = 0;
		private float[] zoomState = new float[] { 100, 300, 500, 1000, 2000 };

		// Chased object properties
		public Vector3 ChasePosition { get; set; }
		public Vector3 ChaseDirection { get; set; }
		public Vector3 Up { get; set; }

		// Desired camera pos
		public Vector3 DesiredPositionOffset { get; set; }
		public Vector3 DesiredPosition { get; set; }
		/// <summary>
		/// 少し上からプレイヤーを見下ろす視点にしたい時など、視点の調整に使用。
		/// </summary>
		public Vector3 LookAtOffset { get; set; }
		public Vector3 LookAt { get; set; }
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

		// Current camera properties
		public Vector3 Position { get; set; }
		public Vector3 Velocity { get; set; }

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

		// Matrix
		public Matrix View { get; set; }
		public Matrix Projection { get; set; }
		#endregion

		#region Methods
		/// <summary>
		/// ワールド空間におけるオブジェクト空間の値を再構築します。
		/// ワールド空間の値をパブリックに返す前、またはワールド空間の値に
		/// プライベートでアクセスする前に呼び出します。
		/// </summary>
		private void UpdateWorldPositions()
		{
			// オブジェクト(のローカル)空間からワールド空間にトランスフォームする行列を構築する
			Matrix transform = Matrix.Identity;
			transform.Forward = ChaseDirection;
			transform.Up = Up;
			transform.Right = Vector3.Cross(Up, ChaseDirection);

			// ワールド空間における目的のカメラ プロパティを計算する
			/*DesiredPosition = ChasePosition +
				Vector3.TransformNormal(DesiredPositionOffset, transform);*/
			//DesiredPosition += new Vector3(JoyStick.Vector;

			if (JoyStick.vectorOther.Length() > 0.2) {
				_verticalAngle += MathHelper.ToRadians(JoyStick.vectorOther.Y);
				_horizontalAngle += MathHelper.ToRadians(JoyStick.vectorOther.X);
			}
			CameraPosition = new Vector3(0.0f, _zoom, 0.0f);
			// Rotate vertically
			CameraPosition = Vector3.Transform(CameraPosition, Matrix.CreateRotationX(_verticalAngle));
			// Rotate horizontally
			CameraPosition = Vector3.Transform(CameraPosition, Matrix.CreateRotationY(_horizontalAngle));
			//Position = ChasePosition + CameraPosition;
			DesiredPosition = ChasePosition + CameraPosition;

			LookAt = ChasePosition +
				Vector3.TransformNormal(LookAtOffset, transform);
		}
		private void UpdateMatrices()
		{
			View = Matrix.CreateLookAt(this.Position, this.LookAt, this.Up);
			//View = Matrix.CreateLookAt(this.Position, this.LookAt, this.t);
			Projection = Matrix.CreatePerspectiveFieldOfView(FieldOfView,
				AspectRatio, NearPlaneDistance, FarPlaneDistance);
		}
		private void HandleInput()
		{
			if (JoyStick.IsOnKeyDown(7)) {
				zoomMode++;
				if (zoomMode == zoomState.Length) zoomMode = 0;
			}

			_zoom = zoomState[zoomMode];
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
			Position = ChasePosition + CameraPosition;
			//Position = DesiredPosition + AdjustOffset;
			
			UpdateMatrices();
		}
		public void Initialize(Game1 game, Object target)
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
		/// <summary>
		/// カメラによって追尾されるように値を更新します
		/// </summary>
		public void UpdateChaseTarget(Object target)
		{
			/*camera.ChasePosition = ship.Position;
			camera.ChaseDirection = ship.Direction;
			camera.Up = ship.Up;*/

			/*ChasePosition = target.Position;
			ChaseDirection = target.Direction;
			ChaseDirection = target.Position - Position;
			Up = target.Up;*/

			// ActionGameと違って今は対象の方向と一致していない
			ChasePosition = target.Position;
			ChaseDirection = target.Position - Position;
			Up = target.Up;
		}
		/// <summary>
		/// カメラの現在位置から、追跡されるオブジェクトの背後の目的のオフセットに向かって
		/// カメラをアニメーション表示します。カメラのアニメーションは、
		/// カメラに取り付けられ、かつ目的位置に固定された単純な物理スプリングによって制御されます。
		/// </summary>
		public void Update(GameTime gameTime)
		{
			float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

			if (gameTime == null) {
				throw new ArgumentNullException("gameTime");
			}
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
			if (Position.Y < 0) Position = new Vector3(Position.X, 0, Position.Z);

			UpdateMatrices();
		}
		#endregion
	}
}
