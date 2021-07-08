using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Weapon : MonoBehaviour
{
    [Header("Weapon Specification")]
    public string weaponName;                                    // 무기 이름
    public int bulletsPerMag;                                   // 한 탄창의 탄환 수
    public int bulletsTotal;                                    // 최대 총알 갯수
    public int currentBullets;                                  // 현재 총알
    public float range;                                         // 사거리
    public float fireRate;                                      // 딜레이
    public Vector3 aimPostion;                                  // 조준 위치
    public Vector3 originalPosition;                            // 원래 위치


    // 파라미터 선언
    private float fireTimer;                                    // 파라미터. 총 쏘는 시간
    private bool isReloading;                                   // 재장전 시간
    private bool isAiming;                                      // 조준하고 있는지 아닌지를 판별

    // 레퍼런스 선언
    private Animator anim;                                      // 애니메이터 설정
    public ParticleSystem muzzleFlash;                          // 격발위치 플래시 설정
    public Text bulletsText;                                    // 총알 텍스트 담는 변수

    // 프리팹 담는 변수
    public GameObject hitSparkPrefab;                           // 히트 파티클 넣는 프리팹 변수
    public GameObject hitHolePrefab;                           // 히트홀 넣는 프리팹 변수


    public Transform shootPoint;                                // 격발 위치

    // 사운드 담는 변수
    public AudioSource audioSource;                             // 사운드 설정
    public AudioClip shootSound;                                // 격발 소리 설정
    public AudioClip reloadSound;                                // 재장전 소리

    // 반동 주는 변수
    public Transform camRecoil;                                 // 카메라와 무기에 반동
    public Vector3 recoilKickback;                              // 원위치
    public float recoilAmount;
    

    void Start()
    {
        currentBullets = bulletsPerMag;
        anim = GetComponent<Animator>();
        bulletsText.text = currentBullets + " / " + bulletsTotal;
        originalPosition = transform.localPosition;
    }

    void Update()
    {
        // 현재 애니메이터가 0번 레이어의 어떤 스테이트에 있는지 정보를 가져오는 것.
        AnimatorStateInfo info = anim.GetCurrentAnimatorStateInfo(0);

        isReloading = info.IsName("Reload");

        // 좌측 마우스 누르면 격발
        if(Input.GetButton("Fire1"))
        {
            if(currentBullets > 0)
            {
                 Fire();
            }
            else            
            {
                // 총알이 0 미만이면 재장전
                DoReload();
            }
        }

        // R 키 누르면 재장전
        if(Input.GetKeyDown(KeyCode.R))
        {
            // 정조준 중이면 재장전 불가능
            if(!isAiming)
            {
                DoReload();
            }
        }

        if(fireTimer < fireRate)
        {
            fireTimer += Time.deltaTime;
        }

        // 정조준 선언
        AimDownSights();
        // 카메라 원위치 선언
        RecoilBack();
    }

    void Fire()
    {
        if(fireTimer < fireRate)
        {
            return;
        }
        Debug.Log("격발");

        // 레이 캐스트
        RaycastHit raycastHit;
        if(Physics.Raycast (shootPoint.position, shootPoint.transform.forward, out raycastHit, range))
        {
            //Debug.Log("맞음");
            // hitSpark 프리팹 생성. Quaternion.FromToRotation : 축을 방향으로 회전
            GameObject hitSpark = Instantiate(hitHolePrefab, raycastHit.point, Quaternion.FromToRotation(Vector3.up, raycastHit.normal));
            // 0.5초 후에 hitSpark 삭제
            Destroy(hitSpark, 0.5f);

            // hitHole 프리팹 생성.
            GameObject hitHole = Instantiate(hitHolePrefab, raycastHit.point, Quaternion.FromToRotation(Vector3.up, raycastHit.normal));
            // 3초 후에 hitHole 삭제
            Destroy(hitHole, 3f);
        }
        // 격발 되어서 총알 감소
        currentBullets--;
        // 격발 시간
        fireTimer = 0.0f;
        // State가 전이하는 도중에는 적용 X. 격발 애니메이션. 0.01초만에 Fade 지정
        anim.CrossFadeInFixedTime("Fire", 0.01f);
        // 격발시 사운드 플레이
        audioSource.PlayOneShot(shootSound);
        // 격발 플래시 설정
        muzzleFlash.Play();
        // 격발시 텍스트 수정
        bulletsText.text = currentBullets + " / " + bulletsTotal;

        // 반동 호출
        Recoil();
    }

    // 재장전
    void DoReload()
    {
            if (!isReloading && currentBullets < bulletsPerMag && bulletsTotal > 0)
            {
                anim.CrossFadeInFixedTime("Reload", 0.01f);
                audioSource.PlayOneShot(reloadSound);
            }
    }

    public void Reload()
    {
        int bulletsToReload = bulletsPerMag - currentBullets;
            // 만약 탄약 리로드가 전체 탄약 보다 많으면
            if (bulletsToReload > bulletsTotal)
            {
                bulletsToReload = bulletsTotal;
            }
            // 현재 탄약에서 재장전한 탄약 더하기
            currentBullets += bulletsToReload;
            // 전체 탄약 갯수에서 재장전한 탄약 빼기
            bulletsTotal -= bulletsToReload;
            bulletsText.text = currentBullets + " / " + bulletsTotal;
    }

    // 정조준
    void AimDownSights()
    {
        // 오른쪽 클릭을 입력으로 받으면서 && 장전중이 아니면
        if(Input.GetButton("Fire2") && !isReloading)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, aimPostion, Time.deltaTime * 8f);
            Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, 40f, Time.deltaTime * 8f);
            isAiming = true;
        }
        else
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, aimPostion, Time.deltaTime * 5f);
            Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, 60f, Time.deltaTime * 8f);
            isAiming = false;
        }
    }

    // 반동주는 함수
     void Recoil()
    {
        //
        Vector3 recoilVector = new Vector3(Random.Range(-recoilKickback.x, recoilKickback.x), recoilKickback.y, recoilKickback.z);
        //
        Vector3 recoilCamVector = new Vector3(-recoilVector.y * 400f, recoilVector.x * 200f, 0);

        // Lerp는 무기 자체가 밀리고 흔들리는 효과를 넣기 위해 사용.
        // position recoil
        transform.localPosition = Vector3.Lerp(transform.localPosition, transform.localPosition + recoilVector, recoilAmount / 2f);
        // cam recoil
        camRecoil.localRotation = Quaternion.Slerp(camRecoil.localRotation, Quaternion.Euler(camRecoil.localEulerAngles + recoilCamVector), recoilAmount);
    }

    // 카메라를 원위치 시키는 함수
    void RecoilBack()
    {
        camRecoil.localRotation = Quaternion.Slerp(camRecoil.localRotation, Quaternion.identity, Time.deltaTime * 2f);
    }
}
