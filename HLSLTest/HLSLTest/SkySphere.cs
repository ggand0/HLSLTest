using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace HLSLTest
{
	public class SkySphere : IRenderable
	{
		public Object model;
		Effect effect;
		GraphicsDevice graphics;
		TextureCube textureCube;

		public SkySphere(ContentManager content, GraphicsDevice graphcisDevice, TextureCube Texture)
		{
			//model = new Object(content.Load<Model>("skysphere_mesh"), Vector3.Zero, Vector3.Zero, new Vector3(100000),GraphicsDevice);
			/*model = new Object(new Vector3(1000), "Models\\SkySphereMesh");
			model.Scale = 0.5f;*/
			model = new Object(new Vector3(0, 0, 0), "Models\\SphereHighPoly");// "Models\\SkySphereMesh");//Model Model, Vector3 Position, Vector3 Rotation,Vector3 Scale,
			//model.Scale = 1000;
			model.Scale = 1000;

			textureCube = Texture;
			effect = content.Load<Effect>("SkySphereEffect");
			effect.Parameters["CubeMap"].SetValue(Texture);
			//effect = content.Load<Effect>("SkySphere");
			//effect.Parameters["SkyboxTexture"].SetValue(Texture);

			model.SetModelEffect(effect, false);
			this.graphics = graphcisDevice;
		}
		public void Update(GameTime gameTime)
		{
			//model.Update(gameTime);
		}
		public void Draw(Matrix View, Matrix Projection, Vector3 CameraPosition)
		{
			// Disable the depth buffer
			/*graphics.DepthStencilState = DepthStencilState.None;
			graphics.RasterizerState = RasterizerState.CullNone;

			// Move the model with the sphere
			model.Position = CameraPosition;
			//model.Update(new GameTime());
			model.World = Matrix.CreateScale(model.Scale) * Matrix.CreateTranslation(model.Position);//p19
			model.Draw(View, Projection, CameraPosition);
			graphics.DepthStencilState = DepthStencilState.Default;
			graphics.RasterizerState = RasterizerState.CullCounterClockwise;*/

			graphics.DepthStencilState = DepthStencilState.None;
			graphics.RasterizerState = RasterizerState.CullNone;
			model.Position = CameraPosition;
			model.World = Matrix.CreateScale(model.Scale) * Matrix.CreateTranslation(model.Position);//p19
			model.Draw(View, Projection, CameraPosition); // debug
			graphics.DepthStencilState = DepthStencilState.Default;
			graphics.RasterizerState = RasterizerState.CullCounterClockwise;
		}
		public void ResetGraphicDevice()
		{
			graphics.BlendState = BlendState.Opaque;
			graphics.DepthStencilState = DepthStencilState.Default;
			graphics.SamplerStates[0] = SamplerState.LinearWrap;
		}
		public void SetClipPlane(Vector4? Plane)
		{
			effect.Parameters["ClipPlaneEnabled"].SetValue(Plane.HasValue);
			if (Plane.HasValue) {
				effect.Parameters["ClipPlane"].SetValue(Plane.Value);
			}
		}
	}
}
