using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HLSLTest
{
	public class EntityBullet : Object, IRenderable
	{
		protected static float MAX_DISTANCE = 10000;
		public float Speed { get; private set; }
		protected Vector3 startPosition;
		protected float distanceTravelled;
		//public bool IsActive { get; set; }

		public override void Update(GameTime gameTime)
		{
			this.Velocity = Direction * Speed;

			Position += Velocity;
			UpdateWorldMatrix();

			this.IsActive = IsActiveNow();
		}

		public bool IsActiveNow()
		{
			distanceTravelled = Vector3.Distance(startPosition, Position);
			if (distanceTravelled > MAX_DISTANCE) {
				return false;
			} else {
				return true;
			}
		}
		protected override void UpdateWorldMatrix()
		{
			_world = Matrix.Identity;
			_world *= Matrix.CreateScale(Scale);
			_world *= Matrix.CreateTranslation(Position);
			Vector3 workVector = _world.Translation;
			workVector.Normalize();

			/*_world.Forward *= Direction;
			_world.Up *= Vector3.Normalize(Vector3.Cross(_world.Forward, workVector));
			_world.Right *= Vector3.Normalize(Vector3.Cross(_world.Forward, _world.Up));*/
		}




		public override void Draw(Camera camera)
		{
			Matrix[] modelTransforms = new Matrix[Model.Bones.Count];
			Model.CopyAbsoluteBoneTransformsTo(modelTransforms);


			foreach (ModelMesh mesh in Model.Meshes) {
				Matrix localWorld = modelTransforms[mesh.ParentBone.Index] * _world;
				foreach (ModelMeshPart meshPart in mesh.MeshParts) {
					Effect effect = meshPart.Effect;
					if (effect is BasicEffect) {
						((BasicEffect)effect).World = localWorld;
						((BasicEffect)effect).View = camera.View;
						((BasicEffect)effect).Projection = camera.Projection;
						((BasicEffect)effect).EnableDefaultLighting();
					} else {
						SetEffectParameter(effect, "World", localWorld);
						SetEffectParameter(effect, "View", camera.View);
						SetEffectParameter(effect, "Projection", camera.Projection);
						SetEffectParameter(effect, "CameraPosition", camera.Position);
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

		
		protected override void Load()
		{
			base.Load();
			Model = content.Load<Model>("Models\\pea_proj");
		}

		// Constructors
		public EntityBullet(Object user, float speed)
			: this(user, speed, user.Direction, user.Position, 1, "Models\\pea_proj")
		{
		}
		public EntityBullet(Object user, float speed, Vector3 direction, Vector3 position, float scale, string filePath)
		{
			this.Position = position;
			startPosition = position;

			this.Speed = speed;
			this.Direction = direction;
			Direction.Normalize();
			this.Right = user.Right;
			this.Up = user.Up;
			this.Velocity = Direction * speed;

			Load(filePath);
		}
	}
}