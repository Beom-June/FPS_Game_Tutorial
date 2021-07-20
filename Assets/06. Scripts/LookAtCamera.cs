using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    /// <summary>
    /// 메인 카메를 갖고 있는 플레이어를 쳐다보게 선언
    /// </summary>
    private Camera cameraToLookAt;

    void Start()
    {
        cameraToLookAt = Camera.main;
    }

    void Update()
    {
        Vector3 v = cameraToLookAt.transform.position - transform.position;
        v.x = v.z = 0;
        transform.LookAt(cameraToLookAt.transform.position - v);
    }
}
