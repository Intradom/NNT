using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Behavior_NN : MonoBehaviour
{
    [SerializeField] private int num_hidden_layers = 0;
    [SerializeField] private int hidden_layer_size = 0;

    private static Behavior_NN self_NN; // Singleton
    private List<List<float>> NN_data = new List<List<float>>();
    private bool proper = false;

    public bool Init(int input_layer_size, int output_layer_size)
    {
        proper = (input_layer_size > 0) && (output_layer_size > 0);

        NN_data.Clear();

        NN_data.Add(RandomList(input_layer_size));
        for (int i = 0; i < num_hidden_layers; ++i)
        {
            NN_data.Add(RandomList(hidden_layer_size));
        }
        NN_data.Add(RandomList(output_layer_size));

        //DebugWeights();

        return proper;
    }

    private void DebugWeights()
    {
        // Prints all weights in NN_data
        Debug.Log("Weights: ");
        for (int i = 0; i < NN_data.Count; ++i)
        {
            string debug_msg = "[ ";
            for (int j = 0; j < NN_data[i].Count; ++j)
            {
                debug_msg += NN_data[i][j] + " ";
            }
            Debug.Log(debug_msg + "]");
        }
    }

    private List<float> RandomList(int size)
    {
        List<float> rlist = new List<float>();
        for (int i = 0; i < size; ++i)
        {
            rlist.Add(Random.value);
        }

        return rlist;
    }

    private void Awake()
    {
        // Creates only one copy of GC, otherwise not destroying on game load would create duplicates
        if (self_NN == null)
        {
            DontDestroyOnLoad(this.gameObject);

            self_NN = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
}
