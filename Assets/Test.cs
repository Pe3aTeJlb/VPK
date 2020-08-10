using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


public class Test : MonoBehaviour
{
    public GameObject cam;

    public void Start()
    {
        
    }

    public void Update()
    {
        // Determine which direction to rotate towards
        Vector3 targetDirection = cam.transform.position - transform.position;
        targetDirection.y = 0;

        // The step size is equal to speed times frame time.
        float singleStep = 1 * Time.deltaTime;

        // Rotate the forward vector towards the target direction by one step
        Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection, singleStep, 0.0f);

        // Draw a ray pointing at our target in
      //  Debug.DrawRay(transform.position, newDirection, Color.red);

        // Calculate a rotation a step closer to the target and applies rotation to this object
        transform.rotation = Quaternion.LookRotation(newDirection);
    }

}
