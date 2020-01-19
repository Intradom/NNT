using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Behavior_Player : MonoBehaviour
{
    [SerializeField] private GameObject ref_GC = null;
    [SerializeField] private GameObject ref_NN = null;
    [SerializeField] private string tag_harmful = null;
    [SerializeField] private string layer_mask_harmful = null;
    [SerializeField] private float start_speed = 0;
    [SerializeField] private int direction_check_rays = 16;
    [SerializeField] private bool player_control = false;
    [SerializeField] private bool draw_rays = false;

    private Rigidbody2D self_rbody = null;
    private Behavior_Game_Controller ref_GC_script = null;
    private Behavior_NN ref_NN_script = null;
    private float current_speed = 0;
    private float current_direction = 0; // Radians

    private const int NN_OUTPUTS = 2; // direction and speed

    private void NeuralFeed()
    {
        List<float> ray_hit_dists = new List<float>(direction_check_rays);
        List<float> NN_out = new List<float>(new float[NN_OUTPUTS]);

        for (int i = 0; i < direction_check_rays; ++i)
        {
            float ray_dir = ((Mathf.PI * 2) / direction_check_rays) * i;
            Vector2 dir_vec = new Vector2(Mathf.Cos(ray_dir), Mathf.Sin(ray_dir));

            RaycastHit2D hit = Physics2D.Raycast(transform.position, dir_vec, Mathf.Infinity, LayerMask.GetMask(layer_mask_harmful));
            ray_hit_dists.Add(hit.distance);

            if (draw_rays && hit.collider)
            {
                // Rays only show up in Scene view
                Debug.DrawRay(transform.position, dir_vec * hit.distance, Color.yellow);
            }
        }

        ref_NN_script.FF_Pass(ref ray_hit_dists, ref NN_out);

        // Order doesn't matter, as long as it's consistent, values returned are (-1, 1) so they need to be scaled
        current_speed = NN_out[0] * start_speed;
        current_direction = NN_out[1] * (Mathf.PI * 2);
    }

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
        //Debug.Log("C_Spd: " + current_speed);
        //Debug.Log("C_Dir: " + current_direction);

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
        else
        {
            NeuralFeed();
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
