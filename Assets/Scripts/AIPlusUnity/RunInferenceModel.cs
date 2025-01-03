using System.Reflection.Emit;
using Unity.Barracuda;
using UnityEngine;
using UnityEngine.XR;

public class RunInferenceModel : MonoBehaviour
{
    public enum backend { CharpBurst, ComputePrecompiled, PixelShader }

    private const int INPUT_RESOLUTION_Y = 224; // 모델 입력 이미지의 높이
    private const int INPUT_RESOLUTION_X = 224; // 모델 입력 이미지의 너비

    [Header("Model 관련 데이터")]
    public NNModel srcModel; // ONNX 형식의 신경망 모델
    public TextAsset labelsAsset; // 클래스 레이블이 포함된 텍스트 파일
    public backend inferenceBackend = backend.CharpBurst;

    public Material preprocessMaterial; // 이미지 전처리에 사용할 머티리얼

    // 내부필드
    private string[] labels; // 클래스 레이블 배열
    private Model model; // Barracuda로 로드된 모델
    private IWorker engine; // 모델을 실행할 워커
    private RenderTexture targetRT; // 이미지 전처리에 사용할 RenderTexture

    void Start()
    {
        Application.targetFrameRate = 60; // 프레임 속도를 60FPS로 제한
        labels = labelsAsset.text.Split('\n'); // 제공된 텍스트 파일에서 레이블을 파싱
        model = ModelLoader.Load(srcModel); // Barracuda를 사용하여 ONNX 모델 로드
        targetRT = RenderTexture.GetTemporary(INPUT_RESOLUTION_X, INPUT_RESOLUTION_Y, 0, RenderTextureFormat.ARGBHalf);
    }

    public void ExecuteML(Texture2D inputImage, out string label, out float accuracy)
    {
        // IWorker : Unity Barracuda에서 신경망 모델을 실행하는 인터페이스.
        // 모델의 실행(추론)을 관리하며, 입력 데이터를 처리하고 결과를 반환하는 역할
        // 일반적인 사용방법
        // 1. IWorker 생성 : WorkerFactory.CreateWorker를 통해 백엔드와 모델을 결합하여 추론 엔진 생성.
        // 2. 입력 데이터 처리 : 입력 데이터를 Tensor 객체로 변환하여 IWorker에 전달.
        // 3. 추론 실행 : IWorker.Execute(inputTensor)를 호출하여 모델 실행.
        // 4. 출력 결과 확인 : IWorker.PeekOutput() 으로 모델의 출력 값을 가져옴.
        // 5. 해석하여 사용자에게 표시
        // 6. 리소스 정리.

        // 1. IWorker 생성 : 선택된 백엔드에 따라 추론 엔진 생성
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

        // 2. 입력 데이터 처리 : 이미지를 전처리하고 텐서로 변환
        var input = new Tensor(PrepareTextureForInput(inputImage), 3);

        // 3. 추론 실행 : 입력 텐서를 사용하여 모델 추론 실행
        engine.Execute(input);

        // 4. 출력 결과 확인 : 출력 텐서를 가져오기
        var output = engine.PeekOutput();

        // 5. 해석하여 사용자에게 표시 : 가장 높은 확률을 가진 클래스의 인덱스 찾기
        var res = output.ArgMax()[0];
        // 해당 클래스 레이블과 확률 가져오기
        label = labels[res];
        accuracy = output[res];

        // 6. 리소스 정리 : 메모리를 해제하여 리소스 정리
        input.Dispose();
        engine.Dispose();
        Resources.UnloadUnusedAssets();
    }

    // 신경망 모델에 입력하기 위해 이미지 모델이 요구하는 형식에 맞게 변환
    private Texture PrepareTextureForInput(Texture2D src)
    {
        // RenderTexture를 활성화하여 GPU에서 이미지를 처리하기 위한 중간 렌더링 대상
        RenderTexture.active = targetRT;
        // 입력 이미지를 렌더링 대상(targetRT)으로 복사하며, preprocessMaterial을 사용해 정규화 수행
        Graphics.Blit(src, targetRT, preprocessMaterial);

        // 처리된 이미지를 Texture2D로 읽어오기
        var result = new Texture2D(targetRT.width, targetRT.height, TextureFormat.RGBAHalf, false);
        // GPU에 있는 RenderTexture 데이터를 CPU 메모리로 복사하여 Texture2D로 변환
        result.ReadPixels(new Rect(0, 0, targetRT.width, targetRT.height), 0, 0);
        // 적용
        result.Apply();

        // 처리된 Texture2D 반환
        return result;
    }
}
