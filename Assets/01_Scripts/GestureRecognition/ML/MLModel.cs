using Unity.Barracuda;
using UnityEngine;

public class MLModel : MonoBehaviour
{
    public NNModel modelAsset;
    public TextAsset labelsFile;

    private IWorker worker;
    private string[] labels;

    [System.Serializable]
    private class Wrapper
    {
        public string[] array;
    }

    void Start()
    {
        var model = ModelLoader.Load(modelAsset);
        worker = WorkerFactory.CreateWorker(WorkerFactory.Type.Auto, model);

        labels = JsonUtility.FromJson<Wrapper>("{\"array\":" + labelsFile.text + "}").array;

        Debug.Log("Model + Labels loaded");
    }

    public (string label, float confidence) Predict(float[] input)
    {
        using (Tensor tensor = new Tensor(1, 128, input))
        {
            worker.Execute(tensor);

            using (Tensor output = worker.PeekOutput())
            {
                float[] logits = output.ToReadOnlyArray();
                float[] probs = Softmax(logits);

                int bestIndex = 0;
                float bestValue = probs[0];

                for (int i = 1; i < probs.Length; i++)
                {
                    if (probs[i] > bestValue)
                    {
                        bestValue = probs[i];
                        bestIndex = i;
                    }
                }

                return (labels[bestIndex], bestValue);
            }
        }
    }

    private float[] Softmax(float[] logits)
    {
        float max = logits[0];

        for (int i = 1; i < logits.Length; i++)
            if (logits[i] > max)
                max = logits[i];

        float sum = 0f;
        float[] exp = new float[logits.Length];

        for (int i = 0; i < logits.Length; i++)
        {
            exp[i] = Mathf.Exp(logits[i] - max);
            sum += exp[i];
        }

        for (int i = 0; i < exp.Length; i++)
            exp[i] /= sum;

        return exp;
    }
}