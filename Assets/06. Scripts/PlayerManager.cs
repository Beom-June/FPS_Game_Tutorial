using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
    // 플레이어 설정
    [Header("Player Specification")]
    public float hitPoint = 100f;
    public float stamina = 500f;
    private float maxHitPoint;                                  // 체력
    private float maxStamina;

    // 레퍼런스
    [Header("References")]
    public Text hpText;                                         // hp text 담는 변수
    public Text stText;                                         // st text 담는 변수
    public RectTransform hpBar;                                 // hpBar 설정 
    public RectTransform stBar;                                 // stBar 설정 

    private CharacterController characterController;

    void Start()
    {
        characterController = GetComponent<CharacterController>();

        // 체력 설정
        maxHitPoint = hitPoint;
        hpText.text = hitPoint + " / " + maxHitPoint;
        hpBar.localScale = Vector3.one;

        // 스테미나 설정
        maxStamina = stamina;
        stText.text = ((int)(stamina / maxStamina * 100f)).ToString() + "%";
        stBar.localScale = Vector3.one;
    }


    void Update()
    {
        if (characterController.velocity.sqrMagnitude > 99 && Input.GetKey(KeyCode.LeftShift) && stamina > 0)
        {
            stamina--;
            UpdateST();
        }
        else if (stamina < maxStamina)
        {
            // 스테미나 회복량 설정
            stamina += 1.5f;
            UpdateST();
        }

        //// 디버그를 위해서 k를 눌렀을 때 1 ~ 20의 랜덤 데미지가 들어오도록 설정
        //if(Input.GetKeyDown(KeyCode.K))
        //{
        //    ApplyDamage(Random.Range(1, 20));
        //}
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

     void UpdateST()
    {
        stText.text = ((int)(stamina / maxStamina * 100f)).ToString() + "%";
        stBar.localScale = new Vector3(stamina / maxStamina, 1, 1);
    }
}
