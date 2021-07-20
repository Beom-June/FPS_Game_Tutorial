using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    //[SerializeField] float speed = 1f;
    //[SerializeField] float destroyAfter = 5f;
    //[SerializeField] float damage = 10f;
    //[SerializeField] string instantiator;

    void Update()
    {
        //transform.Translate(Vector3.forward * speed * Time.deltaTime);
        //Destroy(gameObject, destroyAfter);
    }

    void OnTriggerEnter(Collider other)
    {
        //if (!other.CompareTag(instantiator))
        {
            // 추후 이 부분 수정
            //other.GetComponent<PlayerManager>().ApplyDamage();
            //Destroy(gameObject, 1f);
        }
    }
    /*
    public void SetInstantiator(string tag)
    {
        instantiator = tag;
    }
    */
}
