using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Behavior_Game_Controller : MonoBehaviour
{
    [SerializeField] private Text ref_text = null;

    //private static Behavior_Game_Controller self_GC; // Singleton

    public void StartRound(int iteration, float best_fitness)
    {
        ref_text.text = "Iteration: " + iteration;
        ref_text.text += "\nBest Fit: " + best_fitness;
    }

    public void EndRound()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void Awake()
    {
        /*
        // Creates only one copy of GC, otherwise not destroying on game load would create duplicates
        if (self_GC == null)
        {
            DontDestroyOnLoad(this.gameObject);

            self_GC = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
        */
    }
}
