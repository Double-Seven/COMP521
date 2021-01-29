using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Author: ZiQi Li
public class BulletMagRotator : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //rotate the bullet magazine by changing the rotation values
        //Time.deltaTime is a float representing the difference in seconds since the last frame update occurred,
        //which can makes the rotation more smoothly
        this.transform.Rotate(new Vector3(0, 30, 0) * Time.deltaTime);
    }
}
