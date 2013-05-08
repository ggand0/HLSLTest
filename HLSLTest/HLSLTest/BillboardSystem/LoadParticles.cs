using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace HLSLTest
{
	/// <summary>
	/// Now implementing...
	/// </summary>
	public static class LoadParticleSettings
	{
		#region sample method
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
											case "sword_lite":
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
		#endregion
		public static void Load(ParticleEmitter particle, string fileName)
		{
			XmlReader xmlReader = XmlReader.Create(fileName);

			while (xmlReader.Read()) {// XMLファイルを１ノードずつ読み込む
				xmlReader.MoveToContent();

				if (xmlReader.NodeType == XmlNodeType.Element) {
					if (xmlReader.Name == "obj") {
						xmlReader.MoveToAttribute(0);
						if (xmlReader.Name == "Name" && xmlReader.Value == "ParticleSettings") {
							// 以下、各パラメータを読み込む処理
							while (!(xmlReader.NodeType == XmlNodeType.EndElement
								&& xmlReader.Name == "obj")) {
								xmlReader.Read();

								//Type type = this.GetType();
								xmlReader.MoveToFirstAttribute();
								if (xmlReader.Name == "type") {
									if (xmlReader.Value == "key") {
										xmlReader.MoveToContent();

										//switch (xmlReader.Name) {
										
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
