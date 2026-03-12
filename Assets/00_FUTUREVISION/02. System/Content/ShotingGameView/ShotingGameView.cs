/*
 * 작성자: Kim Bummoo
 * 작성일: 2025.05.13
 */

using FUTUREVISION.WebAR;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FUTUREVISION.Content
{
    public class GameView : MonoBehaviour
    {
        public EventTrigger EventTrigger;
        public GameObject CrossHair;
        public Animator Animator;
        public GameObject ShotPrefab;
        public Button ReplaceButton;

        bool IsEnded = false;

        //public virtual void Initialize()
        //{
        //    // Add touch down event
        //    EventTrigger.Entry touchDownEntry = new EventTrigger.Entry
        //    {
        //        eventID = EventTriggerType.PointerDown
        //    };
        //    touchDownEntry.callback.AddListener((data) => { OnTouchDown(); });
        //    EventTrigger.triggers.Add(touchDownEntry);

        //    // Add touch up event
        //    EventTrigger.Entry touchUpEntry = new EventTrigger.Entry
        //    {
        //        eventID = EventTriggerType.PointerUp
        //    };
        //    touchUpEntry.callback.AddListener((data) => { OnTouchUp(); });
        //    EventTrigger.triggers.Add(touchUpEntry);
        //}

        //private void OnTouchDown()
        //{
        //    Debug.Log("Touch Down Detected");
        //    Animator.SetBool("CrossHair", true);
        //}

        //private void OnTouchUp()
        //{
        //    Debug.Log("Touch Up Detected");
        //    Animator.SetBool("CrossHair", false);

        //    // 화면 가운데서 레이케스트
        //    Ray ray = Camera.main.ScreenPointToRay(new Vector2(Screen.width / 2, Screen.height / 2));
        //    RaycastHit hit;
        //    if (Physics.Raycast(ray, out hit))
        //    {
        //        // 레이케스트가 충돌한 오브젝트의 이름을 출력
        //        Debug.Log("Hit Object: " + hit.collider.gameObject.name);

        //        // ARObjectItem을 가져옵니다.
        //        Dino arObjectItem = hit.collider.gameObject.GetComponentInParent<Dino>();
        //        StartCoroutine(ShotBullet(arObjectItem));
        //    }
        //    else
        //    {
        //        StartCoroutine(ShotBullet(null));
        //    }
        //    GlobalManager.Instance.SoundModel.PlayShotSound();
        //}

        //private IEnumerator ShotBullet(Dino dino)
        //{
        //    // 카메라 위치 조금 아래에 총알을 발사할 위치로 설정
        //    Vector3 bulletStartPosition = Camera.main.transform.position + Camera.main.transform.up * -0.1f;
        //    // 총알 프리팹을 인스턴스화
        //    GameObject bullet = Instantiate(ShotPrefab, bulletStartPosition, Quaternion.identity);
        //    // 총알이 발사될 방향을 카메라의 정면으로 설정
        //    Vector3 bulletDirection = Camera.main.transform.forward;

        //    float spandTime = 0.0f;
        //    // 일정시간 동안 총알이 날아가도록 설정
        //    while (spandTime < 0.5f)
        //    {
        //        spandTime += Time.deltaTime;
        //        bullet.transform.position += bulletDirection * Time.deltaTime * 10.0f; // 속도 조절
        //        yield return null;
        //    }

        //    // 총알이 날아간 후 제거
        //    Destroy(bullet);

        //    // 총알이 ARObjectItem에 닿았을 때
        //    if (dino != null)
        //    {
        //        if (dino.IsAnswer)
        //        {
        //            dino.Particle_Sucess.Play();
                    
        //            if (IsEnded == false)
        //            {
        //                //WebARManager.Instance.EndFindARObject();
        //                IsEnded = true;
        //            }
        //        }
        //        else
        //        {
        //            dino.Object.SetActive(false);
        //            dino.Particle_Fail.Play();
        //            // Play 실패 효과 Sound
        //            GlobalManager.Instance.SoundModel.PlayWrongSound();
        //        }
        //    }
        //}

    }
}
