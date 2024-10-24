using System.Collections.Generic;
using UnityEngine;
using Pool;

namespace Managers
{
    public class SoundManager : PersistentSingleton<SoundManager>
    {
        #region Fields
        [SerializeField] private GlobalSfxDatabase _globalSfxDatabase;
        #endregion
        #region Music
        [SerializeField] private AudioClip bgMusic;
        [Range(0,1)]
        [SerializeField] private float musicVolume;
        private bool _isPlaying;
        private bool _isPaused;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.M))
            {
                PlaySoundLoop(bgMusic, Stop, musicVolume);
                _isPlaying = true;
            }           
            
            if ((Input.GetKeyDown(KeyCode.P)))
            {
                if (_isPaused)
                    Resume();
                else
                    Pause();                      
            }       

            if ((Input.GetKeyDown(KeyCode.Backspace)))            
                Stop();          
        }

        private void PlayBgMusic(AudioClip clip)
        {
            PlaySoundLoop(bgMusic, Stop, 0.4f);
            _isPlaying = true;
        }

        private void Pause() 
        {
            foreach (var timer in _activeLoopSfx)            
                timer.Pause();
            _isPaused = true;
        }

        private void Resume()
        {
            foreach (var timer in _activeLoopSfx)
                timer.Resume();
            _isPaused = false;
        }
        private void Stop() 
        {
            //RemoveLoopSfx(bgMusic);
            _isPlaying = false; 
        }
        #endregion
        #region Commands
        public static void PlaySound(AudioClip clip, float volume = 1, float pitch = 1)
        {            
            var sfxObj = PoolManager.GetObject<SFX>();       

            sfxObj.Set(clip, volume, pitch);  

            sfxObj.Play();            
        }
        public static void PlaySound(string clipName, float volume = 1, float pitch = 1)
        {
            var clip = Instance._globalSfxDatabase.GetAudioClip(clipName);

            var sfxObj = PoolManager.GetObject<SFX>();

            sfxObj.Set(clip, volume, pitch);

            sfxObj.Play();
        }
        #endregion
        #region Loops
        private HashSet<SFX> _activeLoopSfx = new HashSet<SFX>();

        /// <summary>   
        /// El método recibe la action de stop
        /// </summary>
        /// <param name="clip"></param>
        /// <param name="stopAction"></param>
        /// <param name="volume"></param>
        /// <param name="pitch"></param>
        public static void PlaySoundLoop(AudioClip clip, System.Action stopAction, float volume = 1, float pitch = 1)
        { 
            var sfxObj = PoolManager.GetObject<SFX>();          

            sfxObj.Set(clip, volume, pitch);

            sfxObj.PlayLoop(stopAction);
            AddLoopSfx(sfxObj);
        }

        public static void PlaySoundLoop(string clipName, System.Action stopAction, float volume = 1, float pitch = 1)
        {
            var clip = Instance._globalSfxDatabase.GetAudioClip(clipName);
            var sfxObj = PoolManager.GetObject<SFX>();

            sfxObj.Set(clip, volume, pitch);

            sfxObj.PlayLoop(stopAction);
            AddLoopSfx(sfxObj);
        }

        private static void AddLoopSfx(SFX sfx) => Instance._activeLoopSfx.Add(sfx);
        
        public static void RemoveLoopSfx(SFX sfx)
        {
            if(Instance._activeLoopSfx.Contains(sfx))
                Instance._activeLoopSfx.Remove(sfx);
        }
        #endregion
    }
}
