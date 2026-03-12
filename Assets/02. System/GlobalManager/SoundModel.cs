
/*
 * 작성자: Kim, Bummoo
 * 작성일: 2024.12.04
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FUTUREVISION
{
    public class SoundModel : BaseModel
    {
        // 2024.12.05 김범무
        // 사용할 사운드를 추가하고 아래와 같이 호출해서 이용
        // 
        //[Header("SoundManager")]
        //public AudioClip SampleAudio;
        // 
        // SoundManager.Instance.PlaySound(SampleAudio);


        public AudioClip ButtonAudioClip;

        [Space(10)]
        public AudioClip SucessAudioClip;
        public AudioClip FailAudioClip;
        public AudioClip WrongAudioClip;
        public AudioClip PopupAudioClip;
        public AudioClip ShotSoundAudioClip;

        [Space(10)]
        [SerializeField] private AudioSource BGMPlayer;

        public void PlaySound(AudioClip clip, float volume = 1.0f)
        {
            if (clip == null)
            {
                Debug.LogWarning("PlaySound: clip is null");
                return;
            }

            // 하위 오브젝트로 오디오 소스 생성 후 플레이
            var audioSource = new GameObject("AudioSource").AddComponent<AudioSource>();
            audioSource.clip = clip;
            audioSource.volume = volume;
            audioSource.Play();

            audioSource.transform.SetParent(transform);

            Debug.Log("PlaySound: " + clip.name);

            StartCoroutine(DestroySoundCoroutine(audioSource));
        }

        IEnumerator DestroySoundCoroutine(AudioSource audioSource)
        {
            yield return new WaitForSeconds(audioSource.clip.length);

            while (audioSource.isPlaying)
            {
                yield return null;
            }

            Destroy(audioSource.gameObject);
        }

        public void PlayBGM(AudioClip clip)
        {
            if (clip == null)
            {
                // stop BGM
                BGMPlayer.Stop();
                BGMPlayer.clip = null;
                return;
            }

            BGMPlayer.clip = clip;
            BGMPlayer.loop = true;
            BGMPlayer.Play();
        }


        public void PlayButtonClickSound()
        {
            PlaySound(ButtonAudioClip);
        }

        public void PlayPopupSound()
        {
            PlaySound(PopupAudioClip);
        }

        public void PlayMissionSound(bool isSuccess)
        {
            if (isSuccess)
            {
                PlaySound(SucessAudioClip);
            }
            else
            {
                PlaySound(FailAudioClip);
            }
        }

        public void PlayWrongSound()
        {
            PlaySound(WrongAudioClip);
        }

        public void PlayShotSound()
        {
            PlaySound(ShotSoundAudioClip);
        }
    }
}