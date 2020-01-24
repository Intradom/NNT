using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Behavior_NN : MonoBehaviour
{
    private class NN_Node
    {
        public List<float> weights;
        public float bias;
    }

    [SerializeField] private float mutate_chance = 0.05f; // per node, should be a value between 0 and 1
    [SerializeField] private int num_hidden_layers = 0;
    [SerializeField] private int hidden_layer_size = 0;

    private static Behavior_NN self_NN; // Singleton
    private List<List<NN_Node>> NN_weights = new List<List<NN_Node>>();
    private List<List<NN_Node>> NN_weights_best = new List<List<NN_Node>>();
    private float best_fitness = 0;
    private int input_size = 0;
    private int output_size = 0;
    private int iteration = 0;
    private bool proper = false;

    // Getter functions
    public int GetIteration()
    {
        return iteration;
    }

    public float GetBestFit()
    {
        return best_fitness;
    }

    public bool Init(int input_layer_size, int output_layer_size)
    {
        // If NN already has been initialized, no need to reset it
        if (proper)
        {
            return false;
        }

        //Debug.Log("NN Init" + this.gameObject.GetInstanceID());
        proper = (input_layer_size > 0) && (output_layer_size > 0);
        input_size = input_layer_size;
        output_size = output_layer_size;

        NN_weights.Clear();

        // Input layer to hidden layer
        List<NN_Node> i2h_layer_list = new List<NN_Node>();
        List<NN_Node> i2h_layer_list_dup = new List<NN_Node>();
        for (int i = 0; i < hidden_layer_size; ++i)
        {
            i2h_layer_list.Add(RandomNode(input_layer_size));
            i2h_layer_list_dup.Add(RandomNode(input_layer_size));
        }
        NN_weights.Add(i2h_layer_list);
        NN_weights_best.Add(i2h_layer_list_dup);

        // Hidden layer to hidden layer
        for (int i = 0; i < num_hidden_layers - 1; ++i) // Only necessary if h_layers > 1
        {
            List<NN_Node> h2h_layer_list = new List<NN_Node>();
            List<NN_Node> h2h_layer_list_dup = new List<NN_Node>();
            for (int j = 0; j < hidden_layer_size; ++j)
            {
                h2h_layer_list.Add(RandomNode(hidden_layer_size));
                h2h_layer_list_dup.Add(RandomNode(hidden_layer_size));
            }
            NN_weights.Add(h2h_layer_list);
            NN_weights_best.Add(h2h_layer_list_dup);
        }

        // Hidden layer to output layer
        List<NN_Node> h2o_layer_list = new List<NN_Node>();
        List<NN_Node> h2o_layer_list_dup = new List<NN_Node>();
        for (int i = 0; i < output_layer_size; ++i)
        {
            h2o_layer_list.Add(RandomNode(hidden_layer_size));
            h2o_layer_list_dup.Add(RandomNode(hidden_layer_size));
        }
        NN_weights.Add(h2o_layer_list);
        NN_weights_best.Add(h2o_layer_list_dup);

        return proper;
    }

    public bool FF_Pass(ref List<float> input, ref List<float> output)
    {
        //Debug.Log("In: " + input.Count);
        //Debug.Log("Out: " + output.Count);
        // Make sure the input/output matches the NN dimensions
        if (input.Count != input_size || output.Count != output_size)
        {
            Debug.Log("FF_Pass input/output size mismatch");
            return false;
        }

        List<float> pass_container = new List<float>(new float [Mathf.Max(hidden_layer_size, Mathf.Max(input_size, output_size))]);

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

    public void IterationFinish(float fitness)
    {
        // Increment iteration
        ++iteration;

        // Check if new fitness better than best so far: if it is, store current weights as best; if not, restore old weights from best
        if (fitness > best_fitness)
        {
            Debug.Log("Best Fit: " + fitness + " @ iteration " + iteration);
            best_fitness = fitness;

            CopyWeights(NN_weights, NN_weights_best);
        }
        else 
        {
            CopyWeights(NN_weights_best, NN_weights);
        }

        // Mutate weights for next iteration
        MutateWeights();

        //DebugWeights(NN_weights);
        //Debug.Log("------------------------------------------------------------------------------------------------------------------");
    }

    private void DebugWeights(List<List<NN_Node>> weights)
    {
        // Prints all weights in weights
        for (int i = 0; i < weights.Count; ++i)
        {
            string debug_msg = "";
            for (int j = 0; j < weights[i].Count; ++j)
            {
                // Each node
                debug_msg += "[ ";
                for (int k = 0; k < weights[i][j].weights.Count; ++k)
                {
                    debug_msg += weights[i][j].weights[k] + " ";
                }
                debug_msg += "; " + weights[i][j].bias + " ]";
            }
            Debug.Log(debug_msg);
        }
    }

    private void CopyWeights(List<List<NN_Node>> source, List<List<NN_Node>> sink)
    {
        // Performs deep copy from source to sink
        for (int i = 0; i < sink.Count; ++i)
        {
            for (int j = 0; j < sink[i].Count; ++j)
            {
                for (int k = 0; k < sink[i][j].weights.Count; ++k)
                {
                    sink[i][j].weights[k] = source[i][j].weights[k];
                }
                sink[i][j].bias = source[i][j].bias;
            }
        }
    }

    private float NewWeight()
    {
        return Random.value * 2 - 1;
    }

    private NN_Node RandomNode(int size)
    {
        NN_Node node = new NN_Node();

        List<float> rlist = new List<float>();
        for (int i = 0; i < size; ++i)
        {
            rlist.Add(NewWeight());
        }
        node.weights = rlist;
        node.bias = NewWeight();

        return node;
    }

    private float TryMutate(float original_value)
    {
        if (Random.value < mutate_chance)
        {
            return NewWeight();
        }

        return original_value;
    }

    private void MutateWeights()
    {
        // TODO: This function could be optimized by generating a list of _% of the weights and directly indexing those weights to be changed

        // Performs mutation on current weights (restoration should have already happened before this point)
        for (int i = 0; i < NN_weights.Count; ++i)
        {
            for (int j = 0; j < NN_weights[i].Count; ++j)
            {
                for (int k = 0; k < NN_weights[i][j].weights.Count; ++k)
                {
                    NN_weights[i][j].weights[k] = TryMutate(NN_weights[i][j].weights[k]);
                }
                NN_weights[i][j].bias = TryMutate(NN_weights[i][j].bias);
            }
        }
    }

    private void Awake()
    {
        //Debug.Log("NN Awake" + this.GetInstanceID());
        // Creates only one copy of GC, otherwise not destroying on game load would create duplicates
        if (self_NN == null)
        {
            DontDestroyOnLoad(this.gameObject);
            
            self_NN = this;
        }
        else
        {
            //Debug.Log("NN Awake Destroy");
            Destroy(this.gameObject);
        }
    }
}
