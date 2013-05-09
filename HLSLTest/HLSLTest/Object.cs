using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace HLSLTest
{
	public class Object : IRenderable
	{
		#region Fields and Properties
		public static Game1 game;
		public static Level level;
		public static ContentManager content;


		protected Vector3 _direction, _up, _down, _right;
		protected BoundingSphereRenderer _boundingSphereRenderer;
		public BoundingSphere transformedBoundingSphere { get; protected set; }
		protected const float MinimumAltitude = 350.0f;
		protected const float VelocityScale = 5;
		/// <summary>
		/// BBのスケール。％で表す。
		/// </summary>
		protected readonly float BoundingSphereScale = 0.95f;
		public Matrix RotationMatrix = Matrix.CreateRotationX(MathHelper.PiOver2);
		public bool RenderBoudingSphere { get; set; }


		public Model Model { get; protected set; }
		/// <summary>
		/// ボーンの変換行列
		/// </summary>
		public Matrix[] Transforms { get; set; }
		/// <summary>
		/// 位置ベクトル
		/// </summary>
		public Vector3 Position { get; set; }
		/// <summary>
		/// 速度ベクトル
		/// </summary>
		public Vector3 Velocity { get; set; }
		/// <summary>
		/// モデルの正面方向の方向ベクトル
		/// </summary>
		public Vector3 Direction
		{
			get { return _direction; }
			set { _direction = value; }
		}
		/// <summary>
		/// 上方ベクトル
		/// </summary>
		public Vector3 Up { get; set; }
		/// <summary>
		/// 右方ベクトル
		/// </summary>
		public Vector3 Right { get; set; }
		/// <summary>
		/// 宇宙船のワールドトランスフォーム行列。
		/// </summary>
		public Matrix World
		{
			get { return _world; }
			set { _world = value; }
		}
		protected Matrix _world;

		private float _rotation;
		public float Rotation
		{
			get { return _rotation; }
			set
			{
				float newVal = value;
				// 0 to 2piにする
				while (newVal >= MathHelper.TwoPi) {
					newVal -= MathHelper.TwoPi;
				}
				while (newVal < 0) {
					newVal += MathHelper.TwoPi;
				}
				if (_rotation != value) {
					_rotation = value;
					RotationMatrix =
						Matrix.CreateRotationY(MathHelper.PiOver2) *	// x モデルの向き？
						Matrix.CreateRotationY(_rotation);				// z 回転方向？
				}
			}
		}
		public bool IsActive { get; set; }
		public float Scale;
		public Vector3 ScaleVector { get; set; }

		/// <summary>
		/// エフェクトのパラメータを設定・保持する
		/// </summary>
		public Material Material { get; set; }
		#endregion

		#region Methods
		protected virtual void Load()
		{
		}
		protected void Load(string fileName)
		{
			Model = content.Load<Model>(fileName);

			GenerateTags();
			Effect lightingEffect = content.Load<Effect>("Lights\\LightingEffect");
			Effect pointLightEffect = content.Load<Effect>("Lights\\PointLightEffect");
			Effect spotLightEffect = content.Load<Effect>("Lights\\SpotLightEffect");
			Effect multiLightingEffect = content.Load<Effect>("Lights\\MultiLightingeffect");
			//SetModelEffect(lightingEffect, true);
			//SetModelEffect(multiLightingEffect, true);

			
			/*LightingMaterial mat = new LightingMaterial();
			mat.AmbientColor = Color.Red.ToVector3() * .15f;
			mat.LightColor = Color.Blue.ToVector3() * .85f;*/
			/*PointLightMaterial mat = new PointLightMaterial();
			mat.LightPosition = new Vector3(200, 150, 0);
			mat.LightAttenuation = 3000;*/
			/*SpotLightMaterial mat = new SpotLightMaterial();
			mat.LightDirection = new Vector3(0, -1, -1);
			//mat.LightPosition = new Vector3(0, 200, 200);
			mat.LightPosition = new Vector3(0, 2000, 2000);
			mat.LightFalloff = 120;*/
			
			/*MultiLightingMaterial mat = new MultiLightingMaterial();
			BasicEffect effect = new BasicEffect(game.GraphicsDevice);
			effect.EnableDefaultLighting();
			mat.LightDirection[0] = -effect.DirectionalLight0.Direction;
			mat.LightDirection[1] = -effect.DirectionalLight1.Direction;
			mat.LightDirection[2] = -effect.DirectionalLight2.Direction;
			mat.LightColor = new Vector3[] {
				new Vector3(0.5f, 0.5f, 0.5f),
				new Vector3(0.5f, 0.5f, 0.5f),
				new Vector3(0.5f, 0.5f, 0.5f) };
			Material = mat;*/
		}
		/// <summary>
		/// ワールド行列を再構築する
		/// </summary>
		protected virtual void UpdateWorldMatrix()
		{
			/*_world = Matrix.Identity;
			_world.Forward = Direction;		// 前方ベクトル：向いている方向
			_world.Up = Up;					// 上方ベクトル：頭頂部方向
			_world.Right = Right;			// 右ベクトル：右側面方向
			_world.Translation = Position;	// 平行移動ベクトル：位置
			_world *= Matrix.CreateScale(Scale);
			_world *= RotationMatrix;*/

			/*Direction = Vector3.UnitX;
			Up = Vector3.UnitY;
			Up.Normalize();
			Right = Vector3.Cross(Direction, Up);
			Up = Vector3.Cross(Right, Direction);
			RotationMatrix.Forward = Direction;
			RotationMatrix.Up = Up;
			RotationMatrix.Right = Right;*/
			_world = Matrix.CreateScale(Scale) * RotationMatrix * Matrix.CreateTranslation(Position);
		}

		/// <summary>
		/// 公式のサンプルにあったもので、このプロジェクトで使用することは基本的にない。
		/// モデルの全てのボーンに対し、カメラのProjectionとViewを適用した後に変換行列を返す。
		/// </summary>
		/// <param name="model">ボーンの変換行列を取得したいモデル</param>
		/// <returns>全ボーンの変換行列</returns>
		protected Matrix[] SetupEffectDefaults(Model model)
		{
			Matrix[] absoluteTransforms = new Matrix[model.Bones.Count];
			model.CopyAbsoluteBoneTransformsTo(absoluteTransforms);

			foreach (ModelMesh mesh in model.Meshes) {
				foreach (BasicEffect effect in mesh.Effects) {
					effect.EnableDefaultLighting();
					effect.Projection = level.camera.Projection;//game.projectionMatrix;
					effect.View = level.camera.View;//game.viewMatrix;
				}
			}

			return absoluteTransforms;
		}
		/// <summary>
		/// 公式のサンプルから。描画部分は直接Drawメソッドに書く予定なので多分使わない。
		/// 原文：単純なモデル描画メソッド。ここで興味深い部分は、ビュー行列と射影行列がカメラ オブジェクトから取得されることです。
		/// </summary>        
		protected void DrawModel(Model model, Matrix world)
		{
			Matrix[] transforms = new Matrix[model.Bones.Count];
			model.CopyAbsoluteBoneTransformsTo(transforms);

			foreach (ModelMesh mesh in model.Meshes) {
				foreach (BasicEffect effect in mesh.Effects) {
					effect.EnableDefaultLighting();
					effect.World = transforms[mesh.ParentBone.Index] * world;

					// 追尾カメラによって提供される行列を使用します
					effect.View = level.camera.View;
					effect.Projection = level.camera.Projection;
				}
				mesh.Draw();
			}
		}
		/// <summary>
		/// モデルがビルトインなBounding Sphereを描画する
		/// </summary>
		public void DrawBoundingSphere()
		{
			BoundingSphere defBS = new BoundingSphere(Vector3.Transform(Model.Meshes[0].BoundingSphere.Center, _world), Model.Meshes[0].BoundingSphere.Radius);
			defBS.Radius = defBS.Radius * _world.Forward.Length();

			transformedBoundingSphere = defBS;
			_boundingSphereRenderer.Draw(defBS, Color.Red);
		}
		/// <summary>
		/// 当たり判定メソッド
		/// </summary>
		public virtual bool IsHitWith(Object o)
		{
			BoundingSphere targetSphere =
					  new BoundingSphere(o.Position,
							   o.Model.Meshes[0].BoundingSphere.Radius *
									 BoundingSphereScale);

			BoundingSphere mySphere = new BoundingSphere(
				Position,
				Model.Meshes[0].BoundingSphere.Radius);

			return targetSphere.Intersects(mySphere);
		}
		public virtual void Update(GameTime gameTime)
		{
			//UpdateWorldMatrix();
			_world = Matrix.CreateScale(Scale) * RotationMatrix * Matrix.CreateTranslation(Position);

			// Bounding Sphereを更新：World行列の更新後に行うこと。
			transformedBoundingSphere = new BoundingSphere(
				Vector3.Transform(Model.Meshes[0].BoundingSphere.Center, _world)
				, Model.Meshes[0].BoundingSphere.Radius * _world.Forward.Length());
		}

		private void GenerateTags()
		{
			foreach (ModelMesh mesh in Model.Meshes)
				foreach (ModelMeshPart part in mesh.MeshParts)
					if (part.Effect is BasicEffect) {
						BasicEffect effect = (BasicEffect)part.Effect;
						MeshTag tag = new MeshTag(effect.DiffuseColor, effect.Texture,
						effect.SpecularPower);
						part.Tag = tag;
					}
		}
		/// <summary>
		/// Store references to all of the model's current effects
		/// </summary>
		public void CacheEffects()
		{
			foreach (ModelMesh mesh in Model.Meshes)
				foreach (ModelMeshPart part in mesh.MeshParts)
					((MeshTag)part.Tag).CachedEffect = part.Effect;
		}
		/// <summary>
		/// Restore the effects referenced by the model's cache
		/// </summary>
		public void RestoreEffects()
		{
			foreach (ModelMesh mesh in Model.Meshes)
				foreach (ModelMeshPart part in mesh.MeshParts)
					if (((MeshTag)part.Tag).CachedEffect != null)
						part.Effect = ((MeshTag)part.Tag).CachedEffect;
		}
		public void SetModelEffect(Effect effect, bool CopyEffect)
		{
			foreach (ModelMesh mesh in Model.Meshes)
				foreach (ModelMeshPart part in mesh.MeshParts) {
					Effect toSet = effect;
					// Copy the effect if necessary
					if (CopyEffect)
						toSet = effect.Clone();
					MeshTag tag = ((MeshTag)part.Tag);
					// If this ModelMeshPart has a texture, set it to the effect
					if (/*tag != null && */tag.Texture != null) {// tag自体がnull
						SetEffectParameter(toSet, "BasicTexture", tag.Texture);
						SetEffectParameter(toSet, "TextureEnabled", true);
					} else
						SetEffectParameter(toSet, "TextureEnabled", false);
					// Set our remaining parameters to the effect
					SetEffectParameter(toSet, "DiffuseColor", tag.Color);
					SetEffectParameter(toSet, "SpecularPower", tag.SpecularPower);
					part.Effect = toSet;
				}
		}
		/// <summary>
		/// Sets the specified effect parameter to the given effect, if it
		/// has that parameter
		/// </summary>
		/// <param name="effect"></param>
		/// <param name="paramName"></param>
		/// <param name="val"></param>
		public static void SetEffectParameter(Effect effect, string paramName, object val)
		{
			if (effect.Parameters[paramName] == null)
				return;
			if (val is Vector3)
				effect.Parameters[paramName].SetValue((Vector3)val);
			else if (val is Vector4)
				effect.Parameters[paramName].SetValue((Vector4)val);
			else if (val is bool)
				effect.Parameters[paramName].SetValue((bool)val);
			else if (val is Matrix)
				effect.Parameters[paramName].SetValue((Matrix)val);
			else if (val is Texture2D)
				effect.Parameters[paramName].SetValue((Texture2D)val);
		}
		public virtual void Draw(GraphicsDevice device)
		{
			// モデルの描画
			Matrix[] transforms = new Matrix[Model.Bones.Count];
			Model.CopyAbsoluteBoneTransformsTo(transforms);

			foreach (ModelMesh mesh in Model.Meshes) {
				foreach (BasicEffect effect in mesh.Effects) {
					effect.EnableDefaultLighting();
					// "ワールド座標を使用してモデルの位置を変更する場合に、この行列を使用します。"
					effect.World = transforms[mesh.ParentBone.Index] * _world;//Matrix.Identity * Matrix.CreateScale(Scale);
					// 追尾カメラによって提供される行列を使用します : ビュー変換と射影変換はcameraに任せる
					effect.View = level.camera.View;
					effect.Projection = level.camera.Projection;
				}
				mesh.Draw();
			}
		}
		/// <summary>
		/// カスタムエフェクトを使用したDraw
		/// </summary>
		public void Draw(Matrix View, Matrix Projection, Vector3 CameraPosition)
		{
			Matrix[] modelTransforms = new Matrix[Model.Bones.Count];
			Model.CopyAbsoluteBoneTransformsTo(modelTransforms);

			
			foreach (ModelMesh mesh in Model.Meshes) {
				Matrix localWorld = modelTransforms[mesh.ParentBone.Index] * _world;
				foreach (ModelMeshPart meshPart in mesh.MeshParts) {
					Effect effect = meshPart.Effect;
					if (effect is BasicEffect) {
						((BasicEffect)effect).World = localWorld;
						((BasicEffect)effect).View = View;
						((BasicEffect)effect).Projection = Projection;
						((BasicEffect)effect).EnableDefaultLighting();
					} else {
						SetEffectParameter(effect, "World", localWorld);
						SetEffectParameter(effect, "View", View);
						SetEffectParameter(effect, "Projection", Projection);
						SetEffectParameter(effect, "CameraPosition", CameraPosition);
						//setEffectParameter(effect, "TextureEnabled", true);// どうやらデフォルトでtrueらしい
						//setEffectParameter(effect, "ProjectorEnabled", true);
					}
					if (Material is CubeMapReflectMaterial) {//ProjectedTextureMaterial) {
						int d = 0;
					}
					if (Material != null) {
						//Material.SetEffectParameters(effect);// light mapだけの時は消すべきかも
					}
				}
				mesh.Draw();
			}

			if (RenderBoudingSphere) DrawBoundingSphere();
		}
		/// <summary>
		/// ReflectionMap作成のためにClipPlaneを決定する
		/// </summary>
		/// <param name="Plane"></param>
		public void SetClipPlane(Vector4? Plane)
		{
			foreach (ModelMesh mesh in Model.Meshes) {
				foreach (ModelMeshPart part in mesh.MeshParts) {
					if (part.Effect.Parameters["ClipPlaneEnabled"] != null) {
						part.Effect.Parameters["ClipPlaneEnabled"].SetValue(Plane.HasValue);
					}
					if (Plane.HasValue) {// 値があれば
						if (part.Effect.Parameters["ClipPlane"] != null)
							part.Effect.Parameters["ClipPlane"].SetValue(Plane.Value);
					}
				}
			}
		}
		#endregion

		#region Constructors
		public Object()
			: this(Vector3.Zero)
		{
		}
		public Object(Vector3 position)
			: this(1, position)
		{
		}
		public Object(float scale, Vector3 position)
		{
			this.Position = position;
			this.Scale = scale;

			Material = new HLSLTest.Material();
			IsActive = true;
			RenderBoudingSphere = true;
			RotationMatrix = Matrix.Identity;
			Load();
			_boundingSphereRenderer = new BoundingSphereRenderer(game);
			_boundingSphereRenderer.OnCreateDevice();
		}


		public Object(string fileName)
			: this (Vector3.Zero, fileName)
		{
		}
		public Object(Vector3 position, string fileName)
			: this(position, 1, fileName)
		{
		}
		public Object(Vector3 position, float scale, string fileName)
		{
			this.Position = position;
			this.Scale = scale;

			IsActive = true;
			RenderBoudingSphere = true;
			Load(fileName);
			RotationMatrix = Matrix.Identity;
			_boundingSphereRenderer = new BoundingSphereRenderer(game);
			_boundingSphereRenderer.OnCreateDevice();
		}
		#endregion
	}
}
