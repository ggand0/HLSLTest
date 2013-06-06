using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace HLSLTest
{
	// Controller対応にするなら。
	/*public enum Button
	{
		A ,
		B ,
		key3 ,
		X ,
		Y ,
		key6 ,
		MENU ,
		LOCK ,
		NUM_KEY 
	}*/
	
	/// <summary>
	/// キーボードの入力処理を使いやすく拡張したクラス。
	/// デフォルトでは無い”キーが押された瞬間”などを判定する。
	/// </summary>
	public static class KeyInput
	{
		// メンバ
		//private static Keys[] KeyboardButton;
		/// <summary>
		/// 現在のフレームのキーボードの状態。
		/// </summary>
		private static KeyboardState cur;
		/// <summary>
		/// １フレーム前のキーボードの状態。
		/// </summary>
		private static KeyboardState prev;
		private static int keyNum = (int)Keys.OemClear;//Enum.GetValues(typeof(Keys)).Length;

		static bool[] Key = new bool[keyNum];										// キーが押されているか
		public static bool KEY(Keys key) { return Key[(int)key]; }

		static bool[] onKeyDown = new bool[keyNum];									// キーが押された瞬間か
		public static bool IsOnKeyDown(Keys key) { return onKeyDown[(int)key]; }

		static bool[] onKeyUp = new bool[keyNum];									// キーが離された瞬間か
		public static bool IsOnKeyUp(Keys key) { return onKeyUp[(int)key]; }

		static double[] keyTime = new double[keyNum];								// キーを押している時間
		public static double KeyTime(Keys key) { return keyTime[(int)key]; }

		/// <summary>
		/// 入力状態の更新するメソッド。今のところキーボードのみの対応。
		/// </summary>
		public static void Update()
		{
			cur = Keyboard.GetState();												// 現在のキーボードの状態を取得する。

			for (int i = 0; i < keyNum; i++) {
				if (prev.IsKeyUp((Keys)i) && cur.IsKeyDown((Keys)i)) {
					onKeyDown[i] = true;
					onKeyUp[i] = false;
					continue;														// 押された瞬間を判定したので、同時に存在し得ない”離された瞬間”を判定する必要はない。
				} else {
                    if (onKeyDown[i]) {
                        int d = 0;
                    }
					onKeyDown[i] = false;
				}

				if (prev.IsKeyDown((Keys)i) && cur.IsKeyUp((Keys)i)) {
					onKeyUp[i] = true;												// 上でもう判定してるのでonKeyDown[i] = falseは必要ない。
					keyTime[i] = 0;
				} else {
					onKeyUp[i] = false;
				}

				if (cur.IsKeyDown((Keys)i)) {
					Key[i] = true;
					keyTime[i]++;													// 押されている時間を１フレーム分更新。
				} else {
					Key[i] = false;
				}
			}

			prev = cur;																// 次のフレームで使えるように今のフレームの情報をprevに持たせる。
		}
	}
}
