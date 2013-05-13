using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;

namespace HLSLTest
{
	/// <summary>
	/// The lensflare effect is made up from several individual flare graphics,
	/// which move across the screen depending on the position of the sun. This
	/// helper class keeps track of the position, size, and color for each flare.
	/// </summary>
	class Flare
	{
		public Flare(float position, float scale, Color color, string textureName)
		{
			Position = position;
			Scale = scale;
			Color = color;
			TextureName = textureName;
		}

		public float Position;
		public float Scale;
		public Color Color;
		public string TextureName;
		public Texture2D Texture;
	}


	public enum StarType
	{
		M,
		K,
		G,
		F,
		A,
		B,
		O,
		Undefined,
	};

	public class Star
	{
		public static Level level;

		private Model sphere;

		public Vector3 Position { get; set; }
		public Vector3 Colour;
		public float Scale;
		public StarType type;

		OcclusionQuery occlusionQuery;
		bool occlusionQueryActive = false;
		float occlusionAlpha;

		Effect starEffect;
		Texture2D glowSprite;
		SpriteBatch spriteBatch;
		float glowSize = 400;
		GraphicsDevice graphics;
		bool holdoff;

		Flare[] flares =
        {
            new Flare(-0.5f, 0.7f, new Color( 50,  25,  50), "flare1"),
            new Flare( 0.3f, 0.4f, new Color(100, 255, 200), "flare1"),
            new Flare( 1.2f, 1.0f, new Color(100,  50,  50), "flare1"),
            new Flare( 1.5f, 1.5f, new Color( 50, 100,  50), "flare1"),

            new Flare(-0.3f, 0.7f, new Color(200,  50,  50), "flare2"),
            new Flare( 0.6f, 0.9f, new Color( 50, 100,  50), "flare2"),
            new Flare( 0.7f, 0.4f, new Color( 50, 200, 200), "flare2"),

            new Flare(-0.7f, 0.7f, new Color( 50, 100,  25), "flare3"),
            new Flare( 0.0f, 0.6f, new Color( 25,  25,  25), "flare3"),
            new Flare( 2.0f, 1.4f, new Color( 25,  50, 100), "flare3"),
        };

		public Star(GraphicsDevice graphics, ContentManager content, StarType starType)
			:this(Vector3.Zero, graphics, content, starType)
		{
		}
		public Star(Vector3 position, GraphicsDevice graphics, ContentManager content, StarType starType)
		{
			this.Position = position;
			SetType(starType);
			LoadContent(graphics, content);
		}

		public void LoadContent(GraphicsDevice graphics, ContentManager content)
		{
			sphere = content.Load<Model>("Models\\sphere");

			occlusionQuery = new OcclusionQuery(graphics);
			starEffect = content.Load<Effect>("Planets\\Star");
			glowSprite = content.Load<Texture2D>("Textures\\glow");
			spriteBatch = new SpriteBatch(graphics);
			this.graphics = graphics;


			foreach (Flare flare in flares) {
				flare.Texture = content.Load<Texture2D>("Textures\\" + flare.TextureName);
			}
		}

		public void SetType(StarType _type)
		{
			type = _type;
			switch (type) {
				case StarType.A:
					Scale = 4;
					Colour = new Vector3(0.5f, 1, 0.5f);
					break;

				case StarType.B:
					Scale = 6;
					Colour = new Vector3(0.8f, 0.8f, 1.0f);
					break;

				case StarType.F:
					Scale = 1;
					Colour = new Vector3(1, 1, 1);
					break;

				case StarType.G:
					Scale = 0.8f;
					Colour = new Vector3(1, 1, 0.3f);
					break;

				case StarType.K:
					Scale = 0.7f;
					Colour = new Vector3(1, 0.8f, 0.3f);
					break;

				case StarType.M:
					Scale = 0.5f;
					Colour = new Vector3(1, 0.3f, 0.3f);
					break;

				case StarType.O:
					Scale = 10;
					Colour = new Vector3(0.3f, 0.3f, 1);
					break;

			}

			glowSize = 30 * Scale;
		}

		public void Draw(Matrix View, Matrix Projection)
		{
			// Give up if the current graphics card does not support occlusion queries.
			// HiDefなので対応されていること前提
			//if (occlusionQuery.IsSupported) {
				if (occlusionQueryActive) {
					// If the previous query has not yet completed, wait until it does.
					if (occlusionQuery.IsComplete) {
						const float queryArea = 16 * 16;
						if (occlusionQuery.PixelCount > 0) {
							occlusionAlpha = Math.Min(occlusionQuery.PixelCount / queryArea, 1);
						} else {
							occlusionAlpha = 0;
						}
						occlusionQuery.Begin();
						holdoff = false;
					} else {
						holdoff = true;
					}

				} else {
					occlusionQuery.Begin();
					occlusionQueryActive = true;
					holdoff = false;

				}
			
			//graphics.RenderState.DepthBufferEnable = true;
			//graphics.DepthStencilState = DepthStencilState.Default;
			graphics.DepthStencilState = DepthStencilState.DepthRead;
			graphics.BlendState = BlendState.AlphaBlend;
			//graphics.BlendState = BlendState.Additive;
			graphics.RasterizerState = RasterizerState.CullNone;
			Matrix World = Matrix.CreateScale(Scale * 200) * Matrix.CreateTranslation(Position);//-level.LightPosition

			Matrix wvp = World * View * Projection;
			starEffect.Parameters["wvp"].SetValue(wvp);
			starEffect.Parameters["colour"].SetValue(Colour);
			for (int pass = 0; pass < starEffect.CurrentTechnique.Passes.Count; pass++) {
				for (int msh = 0; msh < sphere.Meshes.Count; msh++) {
					ModelMesh mesh = sphere.Meshes[msh];

					for (int prt = 0; prt < mesh.MeshParts.Count; prt++)
						mesh.MeshParts[prt].Effect = starEffect;
					mesh.Draw();
				}
			}
			if (occlusionQueryActive) {
				if (!holdoff)
					occlusionQuery.End();

			}
			// If it is visible, draw the flare effect.
			if (occlusionAlpha > 0) {
				DrawGlow(TransformPosition(View, Projection, Position));//-level.LightPosition
				DrawFlares(TransformPosition(View, Projection, Position));//-level.LightPosition
			}
			graphics.BlendState = BlendState.Opaque;
			graphics.DepthStencilState = DepthStencilState.Default;
			graphics.RasterizerState = RasterizerState.CullCounterClockwise;
		}

		/// <summary>
		/// Draws a large circular glow sprite, centered on the sun.
		/// </summary>
		void DrawGlow(Vector2 lightPosition)
		{
			Vector4 color = new Vector4(1, 1, 1, occlusionAlpha);
			Vector2 origin = new Vector2(glowSprite.Width, glowSprite.Height) / 2;
			float scale = glowSize * 2 / glowSprite.Width;

			//spriteBatch.Begin(SpriteBlendMode.AlphaBlend);
			//spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);
			spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.Additive);
			spriteBatch.Draw(glowSprite, lightPosition, null, new Color(color), 0,
							 origin, scale, SpriteEffects.None, 0);

			spriteBatch.End();
		}

		public Vector2 TransformPosition(Matrix View, Matrix Projection, Vector3 oPosition)
		{
			Vector4 oTransformedPosition = Vector4.Transform(oPosition, View * Projection);
			if (oTransformedPosition.W != 0) {
				oTransformedPosition.X = oTransformedPosition.X / oTransformedPosition.W;
				oTransformedPosition.Y = oTransformedPosition.Y / oTransformedPosition.W;
				oTransformedPosition.Z = oTransformedPosition.Z / oTransformedPosition.W;
			}

			Vector2 oPosition2D = new Vector2(
			  oTransformedPosition.X * graphics.PresentationParameters.BackBufferWidth / 2 + graphics.PresentationParameters.BackBufferWidth / 2,
			  -oTransformedPosition.Y * graphics.PresentationParameters.BackBufferHeight / 2 + graphics.PresentationParameters.BackBufferHeight / 2);

			return oPosition2D;
		}
		/// <summary>
		/// Draws the lensflare sprites, computing the position
		/// of each one based on the current angle of the sun.
		/// </summary>
		public void DrawFlares(Vector2 lightPosition)
		{
			Viewport viewport = graphics.Viewport;

			// Lensflare sprites are positioned at intervals along a line that
			// runs from the 2D light position toward the center of the screen.
			Vector2 screenCenter = new Vector2(viewport.Width, viewport.Height) / 2;

			Vector2 flareVector = screenCenter - lightPosition;

			// Draw the flare sprites using additive blending.
			//spriteBatch.Begin(SpriteBlendMode.Additive);
			spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.Additive);

			foreach (Flare flare in flares) {
				// Compute the position of this flare sprite.
				Vector2 flarePosition = lightPosition + flareVector * flare.Position;

				// Set the flare alpha based on the previous occlusion query result.
				Vector4 flareColor = flare.Color.ToVector4();

				flareColor.W *= occlusionAlpha;

				// Center the sprite texture.
				Vector2 flareOrigin = new Vector2(flare.Texture.Width,
												  flare.Texture.Height) / 2;

				// Draw the flare.
				spriteBatch.Draw(flare.Texture, flarePosition, null,
								 new Color(flareColor), 1, flareOrigin,
								 flare.Scale, SpriteEffects.None, 0);
			}

			spriteBatch.End();
		}

	}
}
