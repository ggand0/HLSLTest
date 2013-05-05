using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Audio;

namespace HLSLTest
{
	public struct Button
	{
		public Texture2D texture { get; set; }
		public Color color { get; set; }
		public String name { get; set; }
		public bool isSelected { get; set; }
	}

    /// <summary>
    /// シーンの基本クラス。
    /// </summary>
    public abstract class Scene 
    {
		public static Game1 game { get; set; }
		public static SpriteBatch spriteBatch { get; private set; }
		public static ContentManager content { get; set; }
		public static void Initialize(Game1 game, SpriteBatch spriteBatch, ContentManager Content)
		{
			Scene.game = game;
			Scene.spriteBatch = spriteBatch;
			Scene.content = Content;
			//choose = content.Load<SoundEffect>("Audio\\SE\\choose");
			//cancel = content.Load<SoundEffect>("Audio\\SE\\cancel"); //choose.Play(SoundControl.volumeAll, 0f, 0f);
		}

		protected static SoundEffect choose, cancel;
		protected Texture2D backGround, mask;
		protected SoundEffect music;
		protected int counter;

		public SoundEffectInstance musicInstance { get; protected set; }

		/// <summary>
		/// ゲームに新たなシーンを追加し、即座にシーンを変更する。
		/// シーンが終了状態であれば、自動的に開始状態にする。
		/// </summary>
		/// <param name="scene">追加するシーン</param>
		public static void PushScene(Scene scene)
		{
			game.PushScene(scene);
			if (scene.isEndScene) {
				scene.isEndScene = false;
			}
		}
		/// <summary>
		/// 2つ以上前のシーンに戻らせる時に呼ぶ。
		/// </summary>
		/// <param name="backNum">遡りたいシーン数</param>
		public static void BackScene(int backNum)
		{
			// StackはPeekのindexが0であることに注意！
			for (int i = 0; i < backNum; i++) {
				game.scenes.ElementAtOrDefault(i).isEndScene = true;
			}
		}

#if DEBUG_MODE
		protected void Debug()
		{
			if (KeyInput.IsOnKeyDown(Keys.U)) {
				if (!game.isMuted) game.isMuted = true;
				else game.isMuted = false;
			}
		}
#endif

		/// <summary>
		/// シーンを終了させるときtrueにする。
		/// </summary>
		public bool isEndScene { get; /*protected */set; }
		/// <summary>
		/// このシーンが終わったら呼び出される、一つ上のシーンクラス。
		/// 一番上のシーンの場合はnullにしておく。
		/// ownerScene.isEndSceneを操作することでさらに上のシーンに戻ったりできる。
		/// </summary>
		protected Scene higherScene { get; private set; }


		public Scene()
		{
		}
		/// <summary>
		/// 開始状態のシーンを生成する。
		/// </summary>
		public Scene(Scene privousScene)
		{
			isEndScene = false;
			higherScene = privousScene;
		}
		public abstract void Load();
		public abstract void Update(GameTime gameTime);
		public abstract void Draw(GameTime gameTime);
    }
}

