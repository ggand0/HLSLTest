using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace HLSLTest
{
	/// <summary>
	/// Now implementing...
	/// </summary>
	public class LoadParticleSettings
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

		private object GetValue(string type, string value, ContentManager content)
		{
			switch (type) {
				case "int":
					return Int32.Parse(value);
				case "float":
					return float.Parse(value);
				case "Vector2":
					return new Vector2(float.Parse(value), float.Parse(value));
				case "Texture2D":
					return content.Load<Texture2D>(value);
				default:
					return value;
			}
		}

		//public List<object> Load(string fileName)
		public List<ExplosionParticleEmitter> Load(GraphicsDevice graphicsDevice, ContentManager content, Vector3 position, string fileName)
		{
			List<ExplosionParticleEmitter> emitters = new List<ExplosionParticleEmitter>();
			//List<object> emitters = new List<object>();
			XmlReader xmlReader = XmlReader.Create(fileName);

			while (xmlReader.Read()) {// XMLファイルを１ノードずつ読み込む
				xmlReader.MoveToContent();

				if (xmlReader.NodeType == XmlNodeType.Element) {
					if (xmlReader.Name == "Item") {
						xmlReader.MoveToAttribute(0);

						if (xmlReader.Name == "Type") {
							var emitterType = Type.GetType(xmlReader.Value);// stringからとりあえず型だけ取得


							//var emitter = Activator.CreateInstance(emitterType, new object[]{});// stringからインスタンス生成
							List<object> arguments = new List<object>();
							arguments.Add(graphicsDevice);
							arguments.Add(content);
							arguments.Add(position);

							xmlReader.MoveToContent();
							xmlReader.Read();

							// 以下、Itemタグ内の各パラメータを読み込む処理
							// Nameが"arguments"の要素はコンストラクタの引数として読み込み後に与える
							while (!(xmlReader.NodeType == XmlNodeType.EndElement
								&& xmlReader.Name == "Item")) {
								xmlReader.Read();
								//xmlReader.MoveToElement();

								xmlReader.MoveToFirstAttribute();
								if (xmlReader.Name == "use") {
									if (xmlReader.Value == "argument") {// 引数として与えるべき要素なら
										xmlReader.MoveToNextAttribute();
										string type = xmlReader.Value;


										xmlReader.MoveToContent();
										//arguments.Add(xmlReader.Value);
										arguments.Add(GetValue(type, xmlReader.ReadString(), content));
									}
								}
								
							}

							object[] test = new object[] { graphicsDevice, content, position, arguments[3] };
							//ParticleEmitter emitter = (ParticleEmitter)Activator.CreateInstance(emitterType, arguments);// stringからインスタンス生成
							ExplosionParticleEmitter emitter = (ExplosionParticleEmitter)Activator.CreateInstance(emitterType, arguments.ToArray());// stringからインスタンス生成
							//ExplosionParticleEmitter emitter = (ExplosionParticleEmitter)Activator.CreateInstance(typeof(ExplosionParticleEmitter), test);// stringからインスタンス生成
							emitters.Add(emitter);
						}
					}
				}


			}

			return emitters;
		}


	}
}
