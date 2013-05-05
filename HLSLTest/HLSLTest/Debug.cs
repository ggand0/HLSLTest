using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
//using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.GamerServices;

namespace HLSLTest
{
	/// <summary>
	/// デバッグ情報を表示するクラス
	/// </summary>
	public class Debug
	{
		public static Game1 game;
		public static Level level;
		public static SpriteBatch spriteBatch;
		private static readonly int AXIS_DRAW_RANGE = 100000;
		private static readonly int fontSize = 10;

		List<String> debugInfos = new List<string>();
		SpriteFont font;
		BasicEffect basicEffect = new BasicEffect(game.GraphicsDevice);
		//Matrix world;
		VertexBuffer vertexBuffer = new VertexBuffer(game.GraphicsDevice, typeof(VertexPositionColor), 6, BufferUsage.None);
		VertexPositionColor[] vertices = {
			new VertexPositionColor(new Vector3(-AXIS_DRAW_RANGE, 1, 0), Color.Red), new VertexPositionColor(new Vector3(AXIS_DRAW_RANGE, 1, 0), Color.Red),		// X
			new VertexPositionColor(new Vector3(0, -AXIS_DRAW_RANGE, 0), Color.Green), new VertexPositionColor(new Vector3(0, AXIS_DRAW_RANGE, 0), Color.Green),	// Y
			new VertexPositionColor(new Vector3(0, 1, -AXIS_DRAW_RANGE), Color.Blue), new VertexPositionColor(new Vector3(0, 1, AXIS_DRAW_RANGE), Color.Blue)		// Z
		};

		Vector3[] axisStringPos = { new Vector3(300, 0, 0), new Vector3(0, 300, 0), new Vector3(0, 0, 300) };
		String[] axisName = { "X", "Y", "Z" };

		public void Load()
		{
			font = game.Content.Load<SpriteFont>("Fonts\\debugFont");
		}
		public void Draw(GameTime gameTime)
		{
			// 軸描画
			this.basicEffect.View = level.camera.View;
			this.basicEffect.Projection = level.camera.Projection;
			game.GraphicsDevice.SetVertexBuffer(this.vertexBuffer);
			//game.GraphicsDevice.DrawPrimitives(PrimitiveType.LineList, 0, 3);
			foreach (EffectPass pass in this.basicEffect.CurrentTechnique.Passes) {
				// パスの開始
				pass.Apply();
				// ラインを描画する
				game.GraphicsDevice.DrawPrimitives(PrimitiveType.LineList, 0, 3);	
			}

			// 軸名描画
			// Viewportには３次元空間座標からスクリーン座標に変換するメソッドが用意されているので、GraphicsDevice から取得しておく
			Viewport viewport = game.GraphicsDevice.Viewport;

			spriteBatch.Begin();
			// 各軸名のスクリーン座標算出＆描画
			/*for (int i = 0; i < axisStringPos.Length; i++) {
				Vector3 v = viewport.Project(axisStringPos[i], level.camera.Projection, level.camera.View, Matrix.Identity);
				spriteBatch.DrawString(font, axisName[i], new Vector2(v.X, v.Y), Color.White);
			}
			for (int i = 0; i < axisStringPos.Length; i++) {
				Vector3 v = viewport.Project(axisStringPos[i], level.camera.Projection, level.camera.View, Matrix.Identity);
				spriteBatch.DrawString(font, axisName[i], new Vector2(v.X, v.Y) * 10, Color.White);
			}*/
			
			// デバッグ情報描画
			DrawString(spriteBatch);
			spriteBatch.End();
		}
		/// <summary>
		/// デバッグ情報を描画する
		/// </summary>
		private void DrawString(SpriteBatch spritebatch)
		{
			/*debugInfos.Clear();
			debugInfos.Add("Player Pos : " + level.player.Position.ToString());
			debugInfos.Add("Camera Pos : " + level.camera.Position.ToString());
			debugInfos.Add("Camera Dir : " + level.camera.ChaseDirection.ToString());
			debugInfos.Add(JoyStick.Vector.ToString());
			debugInfos.Add(JoyStick.vectorOther.ToString());
			debugInfos.Add(level.player.b.ToString());
			debugInfos.Add(level.player.CurrentMovingState.ToString());
			int count = 0;
			foreach (String s in debugInfos) {
				spritebatch.DrawString(font, s, new Vector2(0, count * fontSize), Color.Wheat);
				count++;
			}*/
		}

		public Debug()
		{
			basicEffect.VertexColorEnabled = true;
			vertexBuffer.SetData(vertices);
		}
	}
}
