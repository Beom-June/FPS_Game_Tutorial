using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSway : MonoBehaviour
{
    public float swayAmount = 0.02f;                                    // 마우스 움직임에 따라 민감하게 흔들리는 값
    public float smoothAmount = 6f;                                     // Lerp함수에서 시간값에 넣어줌으로써 이동이 얼마나 부드럽게 되는지 결정
    public float maxAmount = 0.06f;                                     // Clamp 함수에서 사용

    private Vector3 originalPosition;


    void Start()
    {
        originalPosition = transform.localPosition;
    }

    void Update()
    {
        float positionX = -Input.GetAxis("Mouse X") * swayAmount;
        float positionY = -Input.GetAxis("Mouse Y") * swayAmount;
        float rotationX = -Input.GetAxis("Mouse Y") * swayAmount;
        float rotationY = -Input.GetAxis("Mouse X") * swayAmount * 2f;

        // Clamp 함수 : 값을 제한. 최대 얼마 만큼 흔들릴지 결정하는 변수
        // 마우스 반대로 흔들려야 하므로 음수 값으로 설정
        Mathf.Clamp(positionX, -maxAmount, maxAmount);
        Mathf.Clamp(positionY, -maxAmount, maxAmount);

        Mathf.Clamp(rotationX, -maxAmount, maxAmount);
        Mathf.Clamp(rotationY, -maxAmount, maxAmount);


        // 무기가 흔들렸을 때의 위치
        Vector3 swayPosition = new Vector3(positionX, positionY, 0);
        Quaternion swayRotation = new Quaternion(rotationY, rotationY, 0, 1);

        transform.localPosition = Vector3.Lerp(transform.localPosition, originalPosition + swayPosition, Time.deltaTime * smoothAmount);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, swayRotation, Time.deltaTime * smoothAmount);
    }
}
