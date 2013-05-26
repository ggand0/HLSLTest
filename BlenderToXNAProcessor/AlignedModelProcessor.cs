#region File Description
//-----------------------------------------------------------------------------
// SkinnedModelProcessor.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//
// このソースコードはクリエータークラブオンラインのSkinned Modelの
// ShaderInstancePart.csのコメントを翻訳したもの
//
// コード変更点はMatrixの変わりにQuatTransformを使用し、
//  MaxBonesが59から118になった
//
// http://creators.xna.com/en-US/sample/skinnedmodel
//-----------------------------------------------------------------------------
#endregion

#region Using ステートメント
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
#endregion

namespace AlignedModelPipeline
{
	/// <summary>
	/// 既存のModelProcessorを拡張して、スキンアニメーションにサポートした
	/// カスタムプロセッサ
	/// </summary>
	[ContentProcessor]
	public class AlignedModelProcessor : ModelProcessor
	{
		// このサンプルでは頂点テクスチャを使うので最大ボーン数は256個になる
		const int MaxBones = 256;
		public string MergeAnimations { get; set; }

		/// <summary>
		/// コンテント・パイプライン内の中間データであるNodeContentから
		/// アニメーションデータを含んだModelContentへ変換するProcessメソッド。
		/// </summary>
		public override ModelContent Process(NodeContent input,
											 ContentProcessorContext context)
		{
			// 回転させるだけの処理
			//RotateAll(input, 90, 0, 180);
			RotateAll(input, 90, 0, 180);

			// ベースクラスのProcessメソッドを呼び出してモデルデータを変換する
			ModelContent model = base.Process(input, context);

			return model;
		}

		public static void RotateAll(NodeContent node,
							 float degX,
							 float degY,
							 float degZ)
		{
			Matrix rotate = Matrix.Identity *
			  Matrix.CreateRotationX(MathHelper.ToRadians(degX)) *
			  Matrix.CreateRotationY(MathHelper.ToRadians(degY)) *
			  Matrix.CreateRotationZ(MathHelper.ToRadians(degZ));
			MeshHelper.TransformScene(node, rotate);
		}
	}
}
