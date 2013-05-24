using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HLSLTest
{
	/// <summary>
	/// ダメージを与えられるオブジェクトのインターフェース
	/// </summary>
	public interface IDamageable
	{
		//bool[] DamageType { get; set; }
		//bool IsDamaged { get; set; }

		void Damage();
	}
}
