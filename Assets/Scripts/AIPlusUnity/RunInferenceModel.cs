using System.Reflection.Emit;
using Unity.Barracuda;
using UnityEngine;
using UnityEngine.XR;

public class RunInferenceModel : MonoBehaviour
{
    public enum backend { CharpBurst, ComputePrecompiled, PixelShader }

    private const int INPUT_RESOLUTION_Y = 224; // �� �Է� �̹����� ����
    private const int INPUT_RESOLUTION_X = 224; // �� �Է� �̹����� �ʺ�

    [Header("Model ���� ������")]
    public NNModel srcModel; // ONNX ������ �Ű�� ��
    public TextAsset labelsAsset; // Ŭ���� ���̺��� ���Ե� �ؽ�Ʈ ����
    public backend inferenceBackend = backend.CharpBurst;

    public Material preprocessMaterial; // �̹��� ��ó���� ����� ��Ƽ����

    // �����ʵ�
    private string[] labels; // Ŭ���� ���̺� �迭
    private Model model; // Barracuda�� �ε�� ��
    private IWorker engine; // ���� ������ ��Ŀ
    private RenderTexture targetRT; // �̹��� ��ó���� ����� RenderTexture

    void Start()
    {
        Application.targetFrameRate = 60; // ������ �ӵ��� 60FPS�� ����
        labels = labelsAsset.text.Split('\n'); // ������ �ؽ�Ʈ ���Ͽ��� ���̺��� �Ľ�
        model = ModelLoader.Load(srcModel); // Barracuda�� ����Ͽ� ONNX �� �ε�
        targetRT = RenderTexture.GetTemporary(INPUT_RESOLUTION_X, INPUT_RESOLUTION_Y, 0, RenderTextureFormat.ARGBHalf);
    }

    public void ExecuteML(Texture2D inputImage, out string label, out float accuracy)
    {
        // IWorker : Unity Barracuda���� �Ű�� ���� �����ϴ� �������̽�.
        // ���� ����(�߷�)�� �����ϸ�, �Է� �����͸� ó���ϰ� ����� ��ȯ�ϴ� ����
        // �Ϲ����� �����
        // 1. IWorker ���� : WorkerFactory.CreateWorker�� ���� �鿣��� ���� �����Ͽ� �߷� ���� ����.
        // 2. �Է� ������ ó�� : �Է� �����͸� Tensor ��ü�� ��ȯ�Ͽ� IWorker�� ����.
        // 3. �߷� ���� : IWorker.Execute(inputTensor)�� ȣ���Ͽ� �� ����.
        // 4. ��� ��� Ȯ�� : IWorker.PeekOutput() ���� ���� ��� ���� ������.
        // 5. �ؼ��Ͽ� ����ڿ��� ǥ��
        // 6. ���ҽ� ����.

        // 1. IWorker ���� : ���õ� �鿣�忡 ���� �߷� ���� ����
        switch (inferenceBackend)
        {
            case backend.CharpBurst:
                engine = WorkerFactory.CreateWorker(WorkerFactory.Type.CSharpBurst, model);
                break;
            case backend.ComputePrecompiled:
                engine = WorkerFactory.CreateWorker(WorkerFactory.Type.ComputePrecompiled, model);
                break;
            case backend.PixelShader:
                engine = WorkerFactory.CreateWorker(WorkerFactory.Type.PixelShader, model);
                break;
            default:
                Debug.Log("Invalid backend selection.");
                break;
        }

        // 2. �Է� ������ ó�� : �̹����� ��ó���ϰ� �ټ��� ��ȯ
        var input = new Tensor(PrepareTextureForInput(inputImage), 3);

        // 3. �߷� ���� : �Է� �ټ��� ����Ͽ� �� �߷� ����
        engine.Execute(input);

        // 4. ��� ��� Ȯ�� : ��� �ټ��� ��������
        var output = engine.PeekOutput();

        // 5. �ؼ��Ͽ� ����ڿ��� ǥ�� : ���� ���� Ȯ���� ���� Ŭ������ �ε��� ã��
        var res = output.ArgMax()[0];
        // �ش� Ŭ���� ���̺�� Ȯ�� ��������
        label = labels[res];
        accuracy = output[res];

        // 6. ���ҽ� ���� : �޸𸮸� �����Ͽ� ���ҽ� ����
        input.Dispose();
        engine.Dispose();
        Resources.UnloadUnusedAssets();
    }

    // �Ű�� �𵨿� �Է��ϱ� ���� �̹��� ���� �䱸�ϴ� ���Ŀ� �°� ��ȯ
    private Texture PrepareTextureForInput(Texture2D src)
    {
        // RenderTexture�� Ȱ��ȭ�Ͽ� GPU���� �̹����� ó���ϱ� ���� �߰� ������ ���
        RenderTexture.active = targetRT;
        // �Է� �̹����� ������ ���(targetRT)���� �����ϸ�, preprocessMaterial�� ����� ����ȭ ����
        Graphics.Blit(src, targetRT, preprocessMaterial);

        // ó���� �̹����� Texture2D�� �о����
        var result = new Texture2D(targetRT.width, targetRT.height, TextureFormat.RGBAHalf, false);
        // GPU�� �ִ� RenderTexture �����͸� CPU �޸𸮷� �����Ͽ� Texture2D�� ��ȯ
        result.ReadPixels(new Rect(0, 0, targetRT.width, targetRT.height), 0, 0);
        // ����
        result.Apply();

        // ó���� Texture2D ��ȯ
        return result;
    }
}
