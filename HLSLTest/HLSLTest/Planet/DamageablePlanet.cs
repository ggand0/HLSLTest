using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace HLSLTest
{
	/// <summary>
	/// 守るべき対象である惑星クラス
	/// </summary>
	public class DamageablePlanet : Planet, IDamageable
	{
		protected static readonly int DEF_HIT_POINT = 500;

		private Texture2D lifeBar, lifeBarBack;
		public int HitPoint { get; private set; }
		private static BasicEffect basicEffect;

		protected void DrawLifeBar(SpriteBatch spriteBatch)
		{
			// 以下、具体的数値は全てcamera.FarPlaneDistance = 10000000;時に調整したものであることに注意
			Vector3 transformed = Vector3.Transform(Position, level.camera.View);
			float depthRatio = level.camera.FarPlaneDistance / -transformed.Z;
			float lengthRatio = depthRatio / 10000f;
			float defRatio = lifeBar.Height / (float)lifeBar.Width;

			Viewport viewport = graphicsDevice.Viewport;
			Vector3 v = viewport.Project(Position + Vector3.Up * (Scale), level.camera.Projection, level.camera.View, Matrix.Identity);
			Vector2 drawPos = new Vector2(v.X + lengthRatio * 200, v.Y - lengthRatio * 100);//new Vector2(v.X - lifeBar.Width / 2f, v.Y - 50);
			int endOfLifeBarGreen = (int)(HitPoint / (float)DEF_HIT_POINT) * lifeBar.Width;
			
			

			//graphicsDevice.SetRenderTarget(null);
			spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);
			//graphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;
			graphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
			graphicsDevice.SamplerStates[1] = SamplerState.PointWrap;
			spriteBatch.Draw(lifeBarBack, drawPos, new Rectangle(0, 0, endOfLifeBarGreen, lifeBar.Height), Color.White, 0, new Vector2(lifeBar.Width / 2f, lifeBar.Height / 2f),
				new Vector2(lengthRatio, defRatio * lengthRatio), SpriteEffects.None, 0);
			spriteBatch.Draw(lifeBar, drawPos, new Rectangle(0, 0, endOfLifeBarGreen, lifeBar.Height), Color.White, 0, new Vector2(lifeBar.Width / 2f, lifeBar.Height/2f),
				//new Vector2(1 * (-transformed.Z / level.camera.FarPlaneDistance) * 10000, 0.5f), SpriteEffects.None, 0);
				//new Vector2(1 * (depthRatio) / 10000f, 0.5f), SpriteEffects.None, 0);
				new Vector2(lengthRatio, defRatio * (depthRatio) / 10000f), SpriteEffects.None, 0);//0.8f


			float leng1 = 50;
			float leng2 = 50;
			/*Primitives2D.DrawLine(spriteBatch, new Vector2(drawPos.X - lifeBar.Width / 2f - 5, drawPos.Y)
			 * , new Vector2(drawPos.X - lifeBar.Width / 2f - 5 - leng1, drawPos.Y), Color.Green);
			Primitives2D.DrawLine(spriteBatch, new Vector2(drawPos.X - lifeBar.Width / 2f - 5 - leng1, drawPos.Y)
			 * , new Vector2(drawPos.X - lifeBar.Width / 2f - 5 - leng1 -leng2, drawPos.Y + leng2), Color.Green);*/
			spriteBatch.End();


			Matrix projectionMatrix = Matrix.CreateOrthographicOffCenter(0, graphicsDevice.Viewport.Width,
				graphicsDevice.Viewport.Height, 0, 0, 1f);
			/// these wont change, so we can set them now
			/// and not have to set them in every Draw() call
			basicEffect.World = Matrix.Identity;
			basicEffect.View = Matrix.Identity;
			basicEffect.Projection = projectionMatrix;
			basicEffect.VertexColorEnabled = true;
			VertexPositionColor[] vertices = new VertexPositionColor[4];
			vertices[0] = new VertexPositionColor(new Vector3(drawPos.X+(-lifeBar.Width / 2f - 5) * lengthRatio, drawPos.Y, 0), Color.Green);
			vertices[1] = new VertexPositionColor(new Vector3(drawPos.X+( - lifeBar.Width / 2f - 5 - leng1) * lengthRatio, drawPos.Y, 0), Color.Green);
			vertices[2] = new VertexPositionColor(new Vector3(drawPos.X+( - lifeBar.Width / 2f - 5 - leng1) * lengthRatio, drawPos.Y, 0), Color.Green);
			vertices[3] = new VertexPositionColor(new Vector3(drawPos.X+( - lifeBar.Width / 2f - 5 - leng1 - leng2) * lengthRatio, drawPos.Y + leng2 * lengthRatio, 0), Color.Green);
			foreach (EffectPass p in basicEffect.CurrentTechnique.Passes) {
				p.Apply();
				graphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineList, vertices, 0, 2);
			}
		}
		public override void Draw(Matrix View, Matrix Projection, Vector3 CameraPosition)
		{
			base.Draw(View, Projection, CameraPosition);

			if (!DrawingPrePass) {
				DrawLifeBar(spriteBatch);
				graphicsDevice.BlendState = BlendState.Opaque;
				graphicsDevice.DepthStencilState = DepthStencilState.Default;
			}
		}
		public void Damage()
		{
			HitPoint--;
			if (HitPoint <= 0) {
				IsActive = false;
			}
		}

		// Constructor
		public DamageablePlanet(Vector3 position, GraphicsDevice graphicsDevice, ContentManager content)
			:base(position, graphicsDevice, content)
		{
			HitPoint = DEF_HIT_POINT;
			lifeBar = content.Load<Texture2D>("Textures\\UI\\LifeBar2");
			lifeBarBack = content.Load<Texture2D>("Textures\\UI\\LifeBar2Back");
		}
		public DamageablePlanet(Vector3 position, Vector3 starPosition, GraphicsDevice graphics, ContentManager content)
			: base(position, starPosition, graphics, content) 
		{
			HitPoint = DEF_HIT_POINT;
			lifeBar = content.Load<Texture2D>("Textures\\UI\\LifeBar2");
			lifeBarBack = content.Load<Texture2D>("Textures\\UI\\LifeBar2Back");
			basicEffect = new BasicEffect(graphics);

			
		}
	}
}
