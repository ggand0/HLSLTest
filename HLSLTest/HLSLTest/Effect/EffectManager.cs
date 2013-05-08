using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace HLSLTest
{
	/// <summary>
	/// 各Effectの管理クラス
	/// </summary>
	public class EffectManager
	{
		public static Game1 game;
		private static Level level;

		public List<SpecialEffect> Effects { get; private set; }
		

		public EffectManager()
		{
			Effects = new List<SpecialEffect>();
		}

		public void Add(SpecialEffect effect)
		{
			Effects.Add(effect);
		}
		public void Update(GameTime gameTime)
		{
			foreach (SpecialEffect e in Effects) {
				e.Update(gameTime);
			}

			if (Effects.Count > 0) {
				for (int j = 0; j < Effects.Count; j++) {
					if (Effects[j].Removable) {
						Effects.RemoveAt(j);
					}
				}
			}
		}
		public void Draw(GameTime gameTime, Camera camera)
		{
			foreach (SpecialEffect e in Effects) {
				e.Draw(gameTime, camera);
			}
		}

		/*private void NeedEffect(Object targetObj, int i)
		{
			if (effects.Count > 0 && effects.Any((x) => x.targetObject == (targetObj))) { }
					else {
				if (level.activeObjects[i] is Enemy && (targetObj as Enemy).deathEffected) { }
				effects.Add(new Effect(level, targetObj, i));
			}
		}
		public void Update()
		{
			for (int i = 0; i < level.activeObjects.Count; i++) {
				if (level.activeObjects[i] is JumpingEnemy && level.activeObjects[i].HP <= 0) { }
				if (level.activeObjects[i] is JumpingEnemy && level.activeObjects[i].HP <= 0 && level.activeObjects[i].deathEffected) { }

				if (level.activeObjects[i].isEffected && !level.activeObjects[i].hasDeathEffected) {
					if (level.activeObjects[i].deathEffected) { }
					// NeedEffect(stage.activeObjects[i], i);
					if (effects.Count > 0 && effects.Any((x) => x.targetObject.Equals(level.activeObjects[i]))) { }
					else {
						if (level.activeObjects[i] is Enemy && (level.activeObjects[i] as Enemy).deathEffected) { }
						if (level.activeObjects[i] is Fuujin) { }
						effects.Add(new Effect(level, level.activeObjects[i], i));
					}
				}
			}
			if (effects.Count > 0) {
				for (int j = 0; j < effects.Count; j++) {
					effects[j].DrawObjectEffects(game.spriteBatch);
					if (effects[j].hasEffected) {
						effects[j].targetObject.isEffected = false;
						effects[j].targetObject.damageEffected = false;

						effects.RemoveAt(j);
					}
				}
			}

		}*/
	}
}
