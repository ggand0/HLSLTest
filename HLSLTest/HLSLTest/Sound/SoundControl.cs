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
	public static class SoundControl
	{
		public static readonly float defVolume = .25f;
		public static Game1 game { get; private set; }
		public static ContentManager content { get; private set; }
		public static float volumeAll = defVolume;
		public static SoundEffect menuMusic, music;
		public static SoundEffectInstance menuMusicInstance, musicInstance,musicTmp;

		static SoundControl()
		{
		}
		public static void Initialize(Game1 game, ContentManager Content)
		{
			SoundControl.game = game;
			SoundControl.content = Content;
			menuMusic = content.Load<SoundEffect>("Audio\\BGM\\menu_new");
			menuMusicInstance = menuMusic.CreateInstance();

			menuMusicInstance.Volume = defVolume;
		}
		
		public static void IniMusic(string fileName, bool isLooped)
		{
			music = content.Load<SoundEffect>(fileName);
			musicInstance = music.CreateInstance();
			musicInstance.Volume = defVolume;
		}

        public static void CacheMusic(SoundEffectInstance musicInstance) { musicTmp = SoundControl.musicInstance; }
        public static void RestoreMusic() { SoundControl.musicInstance = musicTmp; }

        public static void Play(/*bool isLooped*/)
        {
            musicInstance.Volume = SoundControl.volumeAll;
            //if (musicInstance.State == SoundState.Stopped) musicInstance.IsLooped = isLooped;
            if (musicInstance.State == SoundState.Paused) musicInstance.Resume();
            else if (musicInstance.State == SoundState.Stopped) musicInstance.Play();
        }
        public static void Play(bool isLooped, float volume) { musicInstance.Volume = volume; musicInstance.Play(); }
		public static void Pause() { musicInstance.Pause(); }
		public static void Stop() { musicInstance.Stop(); }
		public static void Resume() { musicInstance.Resume(); }
	}
}
