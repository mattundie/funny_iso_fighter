using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class PlayerRigging : MonoBehaviour
{
    private MultiAimConstraint constraint;
    private Transform target;

    // Start is called before the first frame update
    void Start()
    {
        constraint = GetComponent<MultiAimConstraint>();
        target = constraint.data.sourceObjects.GetTransform(0);
    }

    // Update is called once per frame
    void Update()
    {
        if (target)
        {
            float dist = (target.transform.position - transform.position).magnitude;
            if(dist < 2  && dist > 0)
            {
                constraint.weight = dist / 2;
            }
        }
    }
}
