using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Behavior_Enemy : MonoBehaviour
{
    [SerializeField] private GameObject ref_chase_target = null;
    [SerializeField] private float speed = 0;

    private void FixedUpdate()
    {
        if (ref_chase_target != null)
        {
            float move_dist = speed * Time.fixedDeltaTime;
            transform.position = Vector3.MoveTowards(transform.position, ref_chase_target.transform.position, move_dist);
        }
    }
}
