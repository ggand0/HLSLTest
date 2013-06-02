using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace HLSLTest
{
	public class EnemyManager
	{
		//public static Level4 level;
		private Level4 level;
		private static readonly int ASTEROID_MAX_SPAWN_NUM = 15;
		private static readonly int FIGHTER_MAX_SPAWN_NUM = 8;
		private Random random = new Random();

		public static float NextDouble(Random r, double min, double max)
		{
			return (float)(min + r.NextDouble() * (max - min));
		}
		private void AddAsteroids(int asteroidNum, float radius)
		{
			for (int i = 0; i < asteroidNum; i++) {
				//random = new Random();
				level.Asteroids.Add(new Asteroid(new Vector3(NextDouble(random, -radius, radius), 0, NextDouble(random, -radius, radius))
					, level.sun.Position, 0.05f, "Models\\Asteroid"));
				//Asteroids[i].Scale = 0.02f;//0.1f;
				//Asteroids[i].SetModelEffect(lightingEffect, true);					// set effect to each modelmeshpart
			}
		}
		private void SpawnAsteroids()
		{
			if (level.Enemies.Count < ASTEROID_MAX_SPAWN_NUM) {// = 15 -5
				float radius = 3000;
				Asteroid a = new Asteroid(new Vector3(NextDouble(random, -radius, radius)
					, 0, NextDouble(random, -radius, radius)), level.sun.Position, 0.05f, "Models\\Asteroid");
				//Asteroids[i].Scale = 0.02f;//0.1f;
				//a.SetModelEffect(shadowEffect, true);					// set effect to each modelmeshpart
				a.IsActive = true;
				a.RenderBoudingSphere = false;
				level.Enemies.Add(a);
				level.Models.Add(a);
			}
		}
		private void SpawnFighters()
		{
			if (level.Enemies.Count < ASTEROID_MAX_SPAWN_NUM) {// = 15 -5
				float radius = 3000;
				Fighter f = new Fighter(new Vector3(NextDouble(random, -radius, radius)	, NextDouble(random, -radius, radius), NextDouble(random, -radius, radius))
					, level.Planets[0].Position, 20f, "Models\\fighter0");
				f.IsActive = true;
				f.RenderBoudingSphere = false;
				level.Enemies.Add(f);
				level.Models.Add(f);
			}
		}
		public void Update(GameTime gameTime)
		{
			SpawnAsteroids();
			SpawnFighters();
		}

		public EnemyManager(Level4 level)
		{
			this.level = level;
		}
	}
}
