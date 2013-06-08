using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace HLSLTest
{
	public enum MouseButton
	{
		Left,
		Right,
		Mid,
	}

	public static class MouseInput
	{
		/// <summary>
		/// 現在のフレームのキーボードの状態。
		/// </summary>
		private static MouseState cur;
		/// <summary>
		/// １フレーム前のキーボードの状態。
		/// </summary>
		private static MouseState prev;
		private static int buttonNum = Enum.GetValues(typeof(MouseButton)).Length;
		public static Vector2 Position { get; set; }

		static bool[] Button = new bool[buttonNum];										// ボタンが押されているか
		public static bool BUTTON(MouseButton button) { return Button[(int)button]; }

		static bool[] onButtonDown = new bool[buttonNum];									// ボタンが押された瞬間か
		public static bool IsOnButtonDown(MouseButton button) { return onButtonDown[(int)button]; }

		static bool[] onButtonUp = new bool[buttonNum];									// ボタンが離された瞬間か
		public static bool IsOnButtonUp(MouseButton button) { return onButtonUp[(int)button]; }

		static double[] buttonTime = new double[buttonNum];								// ボタンを押している時間
		public static double ButtonTime(MouseButton button) { return buttonTime[(int)button]; }


		// とりあえず左クリック判定だけ
		static bool ButtonL;
		public static bool BUTTONL() { return ButtonL; }
		static bool onButtonDownL;									// ボタンが押された瞬間か
		public static bool IsOnButtonDownL() { return onButtonDownL; }
		static bool onButtonUpL;									// ボタンが離された瞬間か
		public static bool IsOnButtonUpL() { return onButtonUpL; }

		static bool ButtonR;
		public static bool BUTTONR() { return ButtonR; }
		static bool onButtonDownR;									// ボタンが押された瞬間か
		public static bool IsOnButtonDownR() { return onButtonDownR; }
		static bool onButtonUpR;									// ボタンが離された瞬間か
		public static bool IsOnButtonUpR() { return onButtonUpR; }


		public static Vector2 GetMousePosition()//bool cache
		{
			return new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
			//return Position;
		}
		public static Vector2 GetCachedPosition()//bool cache
		{
			return Position;
		}
		public static void SetCachedPosition(Vector2 value)
		{
			Position = value;
		}

		private static void GetMouseState()
		{

		}
		public static void SetMousePosition(Vector2 position)
		{
			//Position = position;
			Mouse.SetPosition((int)Position.X, (int)Position.Y);
		}

		/// <summary>
		/// 入力状態の更新するメソッド。今のところキーボードのみの対応。
		/// </summary>
		public static void Update()
		{
			cur = Mouse.GetState();												// 現在の状態を取得する。

			if (prev.LeftButton == ButtonState.Released && cur.LeftButton == ButtonState.Pressed) {
				onButtonDownL = true;
				onButtonUpL = false;
			} else {
				onButtonDownL = false;
			}

			if (prev.LeftButton == ButtonState.Pressed && cur.LeftButton == ButtonState.Released) {
				onButtonDownL = false;
				onButtonUpL = true;
			} else {
				onButtonUpL = false;
			}


			if (prev.RightButton == ButtonState.Released && cur.RightButton == ButtonState.Pressed) {
				onButtonDownR = true;
				onButtonUpR = false;
			} else {
				onButtonDownR = false;
			}

			if (prev.RightButton == ButtonState.Pressed && cur.RightButton == ButtonState.Released) {
				onButtonDownR = false;
				onButtonUpR = true;
			} else {
				onButtonUpR = false;
			}
			if (prev.RightButton == ButtonState.Pressed && cur.RightButton == ButtonState.Pressed) {
				ButtonR = true;
				//ButtonTime[i]++;													// 押されている時間を１フレーム分更新。
			} else {
				ButtonR = false;
			}
			/*for (int i = 0; i < buttonNum; i++) {
				if (prev.LeftButton((MouseButton)i) && cur.IsButtonDown((MouseButton)i)) {
					onButtonDown[i] = true;
					onButtonUp[i] = false;
					continue;														// 押された瞬間を判定したので、同時に存在し得ない”離された瞬間”を判定する必要はない。
				} else {
					if (onButtonDown[i]) {
						int d = 0;
					}
					onButtonDown[i] = false;
				}

				if (prev.IsButtonDown((MouseButton)i) && cur.IsButtonUp((MouseButton)i)) {
					onButtonUp[i] = true;												// 上でもう判定してるのでonButtonDown[i] = falseは必要ない。
					ButtonTime[i] = 0;
				} else {
					onButtonUp[i] = false;
				}

				if (cur.IsButtonDown((MouseButton)i)) {
					Button[i] = true;
					ButtonTime[i]++;													// 押されている時間を１フレーム分更新。
				} else {
					Button[i] = false;
				}
			}*/

			prev = cur;																// 次のフレームで使えるように今のフレームの情報をprevに持たせる。
		}
	}
}
