using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace HLSLTest
{
	/// <summary>
	/// （予定）
	/// </summary>
	public class KeyConfig
	{
		public KeyConfig()
		{
		}
		/*private bool waitingInput;

		public KeyConfig(Scene previousScene)
			: base(previousScene)
		{
			buttonNum = 3;
			button = new Button[buttonNum];
			for (int i = 0; i < buttonNum; i++) {
				button[i].color = Color.Blue;
			}

			Load();
		}
		public override void Load()
		{
			base.Load();

			//for (int i = 0; i < buttonNum; i++)	// 最後がデバッグ用なので-1
				//button[i].texture = content.Load<Texture2D>("General\\Menu\\MenuButton" + i);
			button[0].name = "Jump";
			button[1].name = "TAS";
			button[2].name = "Back";
		}
		/// <summary>
		/// 決定/戻るボタンの設定はそのまま、
		/// 各項目を設定する状態に入った後の入力をゲーム中のそれぞれの操作に割り当てる
		/// </summary>
		protected override void ButtonUpdate()
		{
			base.ButtonUpdate();

			if (!waitingInput) {
				if (button[0].isSelected && JoyStick.IsOnKeyDown(1)) {
					// crossのconfig処理
					waitingInput = true;
					if (!game.isMuted) choose.Play(SoundControl.volumeAll, 0f, 0f);
				}
				if (button[1].isSelected && JoyStick.IsOnKeyDown(1)) {
					// leftButtonのconfig処理
					if (!game.isMuted) choose.Play(SoundControl.volumeAll, 0f, 0f);
				}
				if (button[2].isSelected && JoyStick.IsOnKeyDown(1)) {
					isEndScene = true;
					if (!game.isMuted) choose.Play(SoundControl.volumeAll, 0f, 0f);
				}
			} else {
				if (JoyStick.curButtons.Count > 0) {
					if (button[0].isSelected) {
						JoyStick.keyMap[2] = JoyStick.curButtons[0];
						waitingInput = false;
					} else if (button[1].isSelected) {
						JoyStick.keyMap[5] = JoyStick.curButtons[0];
						waitingInput = false;
					}
				}
			}
		}
		 
		 public override void Draw(SpriteBatch spriteBatch)
		{
			spriteBatch.Draw(backGround, Vector2.Zero, Color.White);

			for (int i = 0; i < buttonNum; i++) {
				if (button[i].isSelected) //spriteBatch.Draw(button[i].texture, Vector2.Zero, Color.White);
					spriteBatch.DrawString(game.menuFont, button[i].name, new Vector2(100, i * 20), Color.White);
			}
			if (waitingInput) spriteBatch.DrawString(game.menuFont, "waiting Input", new Vector2(200, 20), Color.White);
		}
		 */

		public static void LoadXML(string objectName, string fileName)
		{
			XmlReader xmlReader = XmlReader.Create(fileName);

			while (xmlReader.Read()) {// XMLファイルを１ノードずつ読み込む
				xmlReader.MoveToContent();

				if (xmlReader.NodeType == XmlNodeType.Element) {
					if (xmlReader.Name == "obj") {
						xmlReader.MoveToAttribute(0);
						if (xmlReader.Name == "Name" && xmlReader.Value == objectName) {
							// 以下、各パラメータを読み込む処理
							while (!(xmlReader.NodeType == XmlNodeType.EndElement && xmlReader.Name == "obj")) {
								xmlReader.Read();

								//Type type = this.GetType();
								xmlReader.MoveToFirstAttribute();
								if (xmlReader.Name == "type") {
									if (xmlReader.Value == "key") {
										xmlReader.MoveToContent();
										
										switch (xmlReader.Name) {
											case "sword_lite" :
												JoyStick.keyMap[0] = Int32.Parse(xmlReader.ReadString());
												break;
											case "sword_strong":
												JoyStick.keyMap[1] = Int32.Parse(xmlReader.ReadString());
												break;
											case "jump":
												JoyStick.keyMap[2] = Int32.Parse(xmlReader.ReadString());
												break;
											case "nothing0":
												JoyStick.keyMap[3] = Int32.Parse(xmlReader.ReadString());
												break;
											case "dash":
												JoyStick.keyMap[4] = Int32.Parse(xmlReader.ReadString());
												break;
											case "TAS":
												JoyStick.keyMap[5] = Int32.Parse(xmlReader.ReadString());
												break;
											case "nothing1":
												JoyStick.keyMap[6] = Int32.Parse(xmlReader.ReadString());
												break;
											case "nothing2":
												JoyStick.keyMap[7] = Int32.Parse(xmlReader.ReadString());
												break;
											case "PAUSE":
												JoyStick.keyMap[8] = Int32.Parse(xmlReader.ReadString());
												break;
										}
									}
									
								}
							}
						}
					}
				}

			}
		}

		
	}
}