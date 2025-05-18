using UnityEngine;
using Unity.Sentis;

public class ModelClassifier : MonoBehaviour
{
    public ModelAsset modelAsset;
    public float[] results;

    private Worker worker;

    // Define scaler means and stds (from training)
    private readonly float[] means = {2.85158444f, -2.09365348f,  1.06248185f, -0.55495541f, 0.81567667f};
    private readonly float[] stds  = {0.56171271f, 0.64174425f, 0.32367693f, 0.15503442f, 0.08138782f};

    void Start()
    {
        Model model = ModelLoader.Load(modelAsset);

        // set up input and output tensors
        FunctionalGraph graph = new FunctionalGraph();
        FunctionalTensor[] inputs = graph.AddInputs(model);
        FunctionalTensor[] outputs = Functional.Forward(model, inputs);

        Model runtimeModel = graph.Compile(outputs);
        worker = new Worker(runtimeModel, BackendType.GPUCompute);
    }

    public int PredictStep(float[] rawInput)
    {
        // scaling rawinputs using the means and stds
        float[] scaled = ScaleInput(rawInput);

        using Tensor<float> inputTensor = new Tensor<float>(new TensorShape(1, scaled.Length), scaled);

        // tell worker to run the inputTensors through the model
        worker.Schedule(inputTensor);

        // peek the output
        using Tensor<float> outputTensor = worker.PeekOutput() as Tensor<float>;
        float[] predictions = outputTensor.DownloadToArray();
        Debug.Log("Model Output: " + string.Join(", ", predictions));
        return ArgMax(predictions);
    }

    private float[] ScaleInput(float[] input)
    {
        float[] scaled = new float[input.Length];
        for (int i = 0; i < input.Length; i++)
        {
            scaled[i] = (input[i] - means[i]) / stds[i];
        }
        return scaled;
    }

    private int ArgMax(float[] array)
    {
        // returns the value that has highest propability according to model
        int maxIndex = 0;
        float maxValue = array[0];
        for (int i = 1; i < array.Length; i++)
        {
            if (array[i] > maxValue)
            {
                maxValue = array[i];
                maxIndex = i;
            }
        }
        return maxIndex;
    }

    void OnDestroy()
    {
        worker?.Dispose();
    }
}