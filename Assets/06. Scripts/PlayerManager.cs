using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
    // 플레이어 설정
    public float hitPoint = 100f;
    private float maxHitPoint;                                  // 체력

    // 레퍼런스
    public Text hpText;                                         // hp text 담는 변수
    public RectTransform hpBar;                                 // hpBar 설정 

    void Start()
    {
        maxHitPoint = hitPoint;
        hpText.text = hitPoint + " / " + maxHitPoint;
        hpBar.localScale = Vector3.one;
    }


    // Debug
    void Update()
    {
        // 디버그를 위해서 k를 눌렀을 때 1 ~ 20의 랜덤 데미지가 들어오도록 설정
        if(Input.GetKeyDown(KeyCode.K))
        {
            ApplyDamage(Random.Range(1, 20));
        }
    }

    public void ApplyDamage(float damage)
    {
        UpdateHP();
        hitPoint -= damage;
        if(hitPoint <= 0)
        {
            hitPoint = 0;

            // Die;
            Debug.Log("플레이어 죽음");
        }
    }

    // 데미지가 들어와서 깎인 체력 만큼 hpText를 변경, hpBar 크기 줄임
    void UpdateHP()
    {
        hpText.text = hitPoint + " / " + maxHitPoint;
        hpBar.localScale = new Vector3(hitPoint / maxHitPoint, 1, 1);
    }
}
