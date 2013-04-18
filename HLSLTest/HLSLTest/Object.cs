using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace HLSLTest
{
	public class Object
	{
		public static Game1 game;
		public static ContentManager content;


		protected Vector3 _direction, _up, _down, _right;
		protected BoundingSphereRenderer _boundingSphereRenderer;
		public BoundingSphere transformedBoundingSphere { get; protected set; }
		protected const float MinimumAltitude = 350.0f;
		protected const float VelocityScale = 5;
		protected readonly float BoundingSphereScale = 0.95f;// = 95% scale
		public Matrix RotationMatrix = Matrix.CreateRotationX(MathHelper.PiOver2);


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
				// 0~2piにする
				while (newVal >= MathHelper.TwoPi) {
					newVal -= MathHelper.TwoPi;
				}
				while (newVal < 0) {
					newVal += MathHelper.TwoPi;
				}
				if (_rotation != value) {
					_rotation = value;
					RotationMatrix =
						Matrix.CreateRotationY(MathHelper.PiOver2) *	//x モデルの向き？
						Matrix.CreateRotationY(_rotation);				//z 回転方向？
				}
			}
		}
		public bool IsActive { get; set; }
		public float Scale;
		/// <summary>
		/// エフェクトのパラメータを設定・保持する
		/// </summary>
		public Material Material { get; set; }

		protected virtual void Load()
		{
		}
		protected void Load(string fileName)
		{
			Model = content.Load<Model>(fileName);

			generateTags();
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
		protected virtual void UpdateWorldMatrix()
		{
			//world = Matrix.CreateTranslation(Position) * scale;// * Matrix.CreateRotationZ(MathHelper.PiOver2);

			// ワールド行列を再構築する
			_world = Matrix.Identity;
			_world.Forward = Direction;		// 前方ベクトル：向いている方向
			_world.Up = Up;					// 上方ベクトル：頭頂部方向
			_world.Right = Right;			// 右ベクトル：右側面方向
			_world.Translation = Position;	// 平行移動ベクトル：位置
			_world *= Matrix.CreateScale(Scale);
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
					effect.Projection = game.camera.Projection;//game.projectionMatrix;
					effect.View = game.camera.View;//game.viewMatrix;
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
					effect.View = game.camera.View;
					effect.Projection = game.camera.Projection;
				}
				mesh.Draw();
			}
		}
		/// <summary>
		/// モデルがビルトインなBounding Sphereを描画する
		/// </summary>
		protected void DrawBoundingSphere()
		{
			BoundingSphere defBS = new BoundingSphere(Vector3.Transform(Model.Meshes[0].BoundingSphere.Center, _world), Model.Meshes[0].BoundingSphere.Radius);
			defBS.Radius = defBS.Radius * _world.Forward.Length();

			transformedBoundingSphere = defBS;
			_boundingSphereRenderer.Draw(defBS, Color.Red);
		}

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
			//ApplyPhysicalEffect();
			/*Position += Velocity;
			Velocity *= 0.90f;		// braking
			Position = new Vector3(Position.X, Math.Max(Position.Y, MinimumAltitude), Position.Z);

			UpdateWorldMatrix();*/

			
			Direction = Vector3.UnitX;
			Up = Vector3.UnitY;
			Up.Normalize();
			Right = Vector3.Cross(Direction, Up);
			Up = Vector3.Cross(Right, Direction);
			UpdateWorldMatrix();

			// Bounding Sphereを更新：_worldの更新後に行うこと。
			transformedBoundingSphere = new BoundingSphere(
				Vector3.Transform(Model.Meshes[0].BoundingSphere.Center, _world)
				, Model.Meshes[0].BoundingSphere.Radius * _world.Forward.Length());
		}

		private void generateTags()
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
		// Store references to all of the model's current effects
		public void CacheEffects()
		{
			foreach (ModelMesh mesh in Model.Meshes)
				foreach (ModelMeshPart part in mesh.MeshParts)
					((MeshTag)part.Tag).CachedEffect = part.Effect;
		}
		// Restore the effects referenced by the model's cache
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
						setEffectParameter(toSet, "BasicTexture", tag.Texture);
						setEffectParameter(toSet, "TextureEnabled", true);
					} else
						setEffectParameter(toSet, "TextureEnabled", false);
					// Set our remaining parameters to the effect
					setEffectParameter(toSet, "DiffuseColor", tag.Color);
					setEffectParameter(toSet, "SpecularPower", tag.SpecularPower);
					part.Effect = toSet;
				}
		}
		// Sets the specified effect parameter to the given effect, if it
		// has that parameter
		void setEffectParameter(Effect effect, string paramName, object val)
		{
			if (effect.Parameters[paramName] == null)
				return;
			if (val is Vector3)
				effect.Parameters[paramName].SetValue((Vector3)val);
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
					effect.View = game.camera.View;
					effect.Projection = game.camera.Projection;
				}
				mesh.Draw();
			}
		}
		/// <summary>
		/// カスタムエフェクトを使用したDraw
		/// </summary>
		public void Draw()
		{
			Matrix[] modelTransforms = new Matrix[Model.Bones.Count];
			Model.CopyAbsoluteBoneTransformsTo(modelTransforms);

			foreach (ModelMesh mesh in Model.Meshes) {
				Matrix localWorld = modelTransforms[mesh.ParentBone.Index] * _world;
				foreach (ModelMeshPart meshPart in mesh.MeshParts) {
					Effect effect = meshPart.Effect;
					if (effect is BasicEffect) {
						((BasicEffect)effect).World = localWorld;
						((BasicEffect)effect).View = game.camera.View;
						((BasicEffect)effect).Projection = game.camera.Projection;
						((BasicEffect)effect).EnableDefaultLighting();
					} else {
						setEffectParameter(effect, "World", localWorld);
						setEffectParameter(effect, "View", game.camera.View);
						setEffectParameter(effect, "Projection", game.camera.Projection);
						setEffectParameter(effect, "CameraPosition", game.camera.CameraPosition);
						setEffectParameter(effect, "TextureEnabled", true);// どうやらデフォルトでtrueらしい

						//setEffectParameter(effect, "ProjectorEnabled", true);// どうやらデフォルトでtrueらしい
					}
					if (Material is ProjectedTextureMaterial) {
						int d = 0;
					}
					Material.SetEffectParameters(effect);// light mapだけの時は消すべき？
				}
				mesh.Draw();
			}

			//DrawBoundingSphere();
		}

		// Constructor
		public Object()
		{
			Material = new HLSLTest.Material();
			IsActive = true;
			Load();
			_boundingSphereRenderer = new BoundingSphereRenderer(game);
			_boundingSphereRenderer.OnCreateDevice();
		}
		public Object(Vector3 position)
			: this()
		{
			this.Position = position;
		}

		public Object(string fileName)
		{
			IsActive = true;
			Load(fileName);
			_boundingSphereRenderer = new BoundingSphereRenderer(game);
			_boundingSphereRenderer.OnCreateDevice();
		}
		public Object(Vector3 position, string fileName)
			: this(fileName)
		{
			this.Position = position;
		}
	}
}
