using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Behavior_Player : MonoBehaviour
{
    [SerializeField] private GameObject ref_GC = null;
    [SerializeField] private GameObject ref_NN = null;
    [SerializeField] private string tag_harmful = null;
    [SerializeField] private float start_speed = 0;
    [SerializeField] private int direction_check_rays = 8;
    [SerializeField] private bool player_control = false;

    private Rigidbody2D self_rbody = null;
    private Behavior_Game_Controller ref_GC_script = null;
    private Behavior_NN ref_NN_script = null;
    private float current_speed = 0;
    private float current_direction = 0; // Radians

    private const int NN_OUTPUTS = 2; // direction and speed

    private void Awake()
    {
        self_rbody = this.GetComponent<Rigidbody2D>();
        ref_GC_script = ref_GC.GetComponent<Behavior_Game_Controller>();
        ref_NN_script = ref_NN.GetComponent<Behavior_NN>();
    }

    private void Start()
    {
        ref_NN_script.Init(direction_check_rays, NN_OUTPUTS);
    }

    private void Update()
    {
        if (player_control)
        {
            float move_hori = Input.GetAxis("Horizontal");
            float move_vert = Input.GetAxis("Vertical");

            current_direction = Mathf.Atan2(move_vert, move_hori);

            if (Mathf.Abs(move_hori) + Mathf.Abs(move_vert) > 0)
            {
                current_speed = start_speed;
            }
            else
            {
                current_speed = 0;
            }
        }
    }

    private void FixedUpdate()
    {
        float move_amount = current_speed * Time.fixedDeltaTime;
        self_rbody.AddForce(new Vector2(Mathf.Cos(current_direction) * move_amount, Mathf.Sin(current_direction) * move_amount));
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == tag_harmful)
        {
            // End current game
            ref_GC_script.EndRound();
        }
    }
}
