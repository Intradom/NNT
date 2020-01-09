using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Behavior_NN : MonoBehaviour
{
    private struct NN_Node
    {
        public List<float> weights;
        public float bias;
    }

    [SerializeField] private int num_hidden_layers = 0;
    [SerializeField] private int hidden_layer_size = 0;

    private static Behavior_NN self_NN; // Singleton
    private List<List<NN_Node>> NN_weights = new List<List<NN_Node>>();
    private int input_size = 0;
    private int output_size = 0;
    private bool proper = false;

    public bool Init(int input_layer_size, int output_layer_size)
    {
        proper = (input_layer_size > 0) && (output_layer_size > 0);
        input_size = input_layer_size;
        output_size = output_layer_size;

        NN_weights.Clear();

        // Input layer to hidden layer
        List<NN_Node> i2h_layer_list = new List<NN_Node>();
        for (int i = 0; i < hidden_layer_size; ++i)
        {
            i2h_layer_list.Add(RandomNode(input_layer_size));
        }
        NN_weights.Add(i2h_layer_list);

        // Hidden layer to hidden layer
        for (int i = 0; i < num_hidden_layers - 1; ++i) // Only necessary if h_layers > 1
        {
            List<NN_Node> h2h_layer_list = new List<NN_Node>();
            for (int j = 0; j < hidden_layer_size; ++j)
            {
                h2h_layer_list.Add(RandomNode(hidden_layer_size));
            }
            NN_weights.Add(h2h_layer_list);
        }

        // Hidden layer to output layer
        List<NN_Node> h2o_layer_list = new List<NN_Node>();
        for (int i = 0; i < output_layer_size; ++i)
        {
            h2o_layer_list.Add(RandomNode(hidden_layer_size));
        }
        NN_weights.Add(h2o_layer_list);

        //DebugWeights();
        return proper;
    }

    public bool FF_Pass(ref float[] input, ref float[] output)
    {
        // Make sure the input/output matches the NN dimensions
        if (input.Length != input_size || output.Length != output_size)
        {
            Debug.Log("FF_Pass input/output size mismatch");
            return false;
        }

        List<float> pass_container = new List<float>(Mathf.Max(hidden_layer_size, Mathf.Max(input_size, output_size)));
        for (int i = 0; i < input_size; ++i)
        {
            pass_container[i] = input[i];
        }

        // Network
        for (int i = 0; i < NN_weights.Count; ++i)
        {
            List<float> old_pass_container = new List<float>(pass_container); // Clones

            // Layer
            for (int j = 0; j < NN_weights[i].Count; ++j)
            {
                pass_container[j] = 0;
                // Node
                for (int k = 0; k < NN_weights[i][j].weights.Count; ++k)
                {
                    pass_container[j] += NN_weights[i][j].weights[k] * old_pass_container[k];
                }
                pass_container[j] += NN_weights[i][j].bias;
            }
        }

        for (int i = 0; i < output_size; ++i)
        {
            output[i] = pass_container[i];
        }

        return true;
    }

    private void DebugWeights()
    {
        // Prints all weights in NN_weights
        Debug.Log("Weights: ");
        for (int i = 0; i < NN_weights.Count; ++i)
        {
            string debug_msg = "";
            for (int j = 0; j < NN_weights[i].Count; ++j)
            {
                // Each node
                debug_msg += "[ ";
                for (int k = 0; k < NN_weights[i][j].weights.Count; ++k)
                {
                    debug_msg += NN_weights[i][j].weights[k] + " ";
                }
                debug_msg += "; " + NN_weights[i][j].bias + " ]";
            }
            Debug.Log(debug_msg);
        }
    }

    private NN_Node RandomNode(int size)
    {
        NN_Node node = new NN_Node();

        List<float> rlist = new List<float>();
        for (int i = 0; i < size; ++i)
        {
            rlist.Add(Random.value * 2 - 1);
        }
        node.weights = rlist;
        node.bias = Random.value * 2 - 1;

        return node;
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
