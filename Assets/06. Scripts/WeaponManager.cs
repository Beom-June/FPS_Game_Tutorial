using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    [Header("WeaponManager Setting")]
    public float switchDelay = 1f;                                  // 무기 변경 후 다시 변경하기 까지의 딜레이
    public GameObject[] weapon;                                     // 무기 오브젝트 배열

    private int index = 0;                                          // 현재 무기 인덱스
    private bool isSwitching = false;                               // 딜레이 확인용

     void Start()
    {
        InitializeWeapon();
    }
     void Update()
    {
        // 마우스 휠이 내려가고 딜레이가 아니면 인덱스 하나 올림
        if (Input.GetAxis("Mouse ScrollWheel") > 0 && !isSwitching)
        {
            index++;
            if (index >= weapon.Length)
                index = 0;
            StartCoroutine(SwitchDelay(index));
        }

        if (Input.GetAxis("Mouse ScrollWheel") < 0 && !isSwitching)
        {
            index--;
            if (index < 0)
                index = weapon.Length - 1;
            StartCoroutine(SwitchDelay(index));
        }

        // 키보드 위쪽 1 ~ 9 까지의 키를 입력 받아 각각에 해당하는 인덱스를 지정해줌
        for (int i = 49; i < 58; i++)
        {
            if (Input.GetKeyDown((KeyCode)i) && !isSwitching && weapon.Length > i - 49 && index != i - 49)
            {
                // -49를 해준 것은 0부터 시작하기 위함
                index = i - 49;
                StartCoroutine(SwitchDelay(index));
            }
        }
    }

    // 게임이 시작될 때 초기화하는 부분. 0번 인덱스의 무기만 Active. 나머지 False
     void InitializeWeapon()
    {
        for (int i = 0; i < weapon.Length; i++)
        {
            weapon[i].SetActive(false);
        }
        weapon[0].SetActive(true);
        index = 0;
    }

     IEnumerator SwitchDelay(int newIndex)
    {
        isSwitching = true;
        SwitchWeapons(newIndex);
        yield return new WaitForSeconds(switchDelay);
        isSwitching = false;
    }

    // 입력받은 인덱스의 오브젝트를 활성화하고 나머지는 비활성화
     void SwitchWeapons(int newIndex)
    {
        for (int i = 0; i < weapon.Length; i++)
        {
            weapon[i].SetActive(false);
        }
        weapon[newIndex].SetActive(true);
    }
}
