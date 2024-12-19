using UnityEngine;

public class NCLExample : MonoBehaviour
{
    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // 마우스 왼쪽 버튼 클릭 감지
        {
            // 카메라가 이미 사용 중이라면 더 이상 작업하지 않음
            if (NativeCamera.IsCameraBusy()) return;

            // 화면 클릭(터치)시 그 위치가 화면의 왼쪽이다면,
            if (Input.mousePosition.x < Screen.width / 2)
            {
                // 사진 촬영 메서드 발동. 최대 크기는 512px로 제한
                TakePicture(512);
            }
            else // 화면 클릭(터치)시 그 위치가 화면의 오른쪽이다면,
            {
                // 비디오 녹화 메서드 발동.
                RecordVideo();
            }
        }
    }

    // 비동기로 카메라 권한을 요청하는 메서드
    // 사진 또는 비디오 권한을 요청할 때 사용
    private async void RequestPermissionAsynchronously(bool isPicturePermission)
    {
        // NativeCamera의 권한 요청 비동기 메서드를 호출
        NativeCamera.Permission permission = await NativeCamera.RequestPermissionAsync(isPicturePermission);
        // 권한 요청 결과를 디버그 로그로 출력
        Debug.Log("Permission result: " + permission);
    }
    /* DK Chack Point : 비동기란?
    * 1. 비동기(asynchronous:에이싱크러너스)는 작업을 동시에 처리하거나 작업이 완료될 때까지 기다리지 않고 다른 작업을 계속 진행할 수 있는 프로그래밍 방식입니다.
    * 2. 비유 : 동기는 음식점에서 음식을 주문한 후, 음식이 나오기까지 아무것도 하지 않는 것에 비해 / 비동기는 음식이 나올 때까지 다른 행동(스마트폰을 사용하거나, 책을 읽는 등)을 하는 것에 비유합니다.
    * 3. async 키워드 : 한정자이자 키워드로써 해당 메서드(함수)가 비동기로 동작할 것임을 나타냅니다.
    * 4. await 키워드 : 비동기 작업 아래에서는(NativeCamera.RequestPermissionAsync)의 완료를 기다립니다.
    * 5. 최종 정리 : RequestPermissionAsync 메서드는 비동기 메서드로, 카메라 권한을 요청합니다. 권한 요청 결과가 반환되면 permission 변수에 결과를 저장합니다.
    */

    // 카메라를 사용하여 사진을 촬영하고 결과를 처리하는 메서드
    private void TakePicture(int maxSize)
    {
        // NativeCamera를 통해 사진 촬영 호출
        NativeCamera.Permission permission = NativeCamera.TakePicture((path) =>
        {
            // 사진 촬영 완료 후 콜백 함수 실행
            Debug.Log("Image path : " + path); // 저장된 이미지 경로 출력

            if (path != null) // 경로가 유효하면
            {
                // 경로에서 이미지 파일을 불러와 Texture2D로 변환
                Texture2D texture = NativeCamera.LoadImageAtPath(path, maxSize);
                if (texture == null) // 텍스처를 불러오지 못했을 경우
                {
                    Debug.Log("Couldn't load texture from " + path);
                    return;
                }

                // 캡처된 이미지를 화면에 표시하기 위해 Quad 생성
                GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
                // 카메라 앞 2.5m 위치에 생성
                quad.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 2.5f;
                // Quad가 카메라를 향하도록 설정
                quad.transform.forward = Camera.main.transform.forward;
                // 이미지 비율에 맞게 스케일링
                quad.transform.localScale = new Vector3(1f, texture.height / (float)texture.width, 1f);

                // Quad에 텍스처를 적용
                Material material = quad.GetComponent<Renderer>().material;
                // 사용 가능한 세이더가 없으면, 기본 세이더로 대체
                if (!material.shader.isSupported) material.shader = Shader.Find("Legacy Shaders/Diffuse");

                material.mainTexture = texture; // 텍스처 할당

                Destroy(quad, 5f); // 5초 후 Quad 제거
                Destroy(texture, 5f); // 5초 후 텍스처 제거
                // 프로시저로 생성된 텍스처는 수동으로 제거해야 메모리 누수를 방지 가능
            }
        }, maxSize // maxSize는 이미지의 최대 크기를 픽셀 단위로 제한
        );

        // 권한 요청 결과를 디버그 로그로 출력
        Debug.Log("Permission result: " + permission);
    }
    /* DK Chack Point : 콜백이란?
    * 1. 콜백(callback)은 특정 작업이 완료되었을 때 실행될 코드나 함수를 의미합니다.
    *    일반적으로 콜백은 메서드(함수)의 인자로 전달되며, 작업이 끝난 후 호출됩니다.
    * 2. 비유 : 1) 음직점에 음식을 주문하고 전화번호를 남기며 "음식이 준비되면 연락주세요." 라고 요청한 상황에서, 2) 음식이 준비되었을 때 음식점에서 전화를 하는 것이 콜백 입니다.
    * 3. 최종 정리 : TakePicture 메서드(함수)는 콜백을 사용하며, (path) => { ... } 는 작업(사진 촬영)을 완료하면 호출됩니다.
    *               이때 path 인자는 작업의 결과(촬영한 사진의 경로)가 콜백 함수에 전달됩니다.
    */

    // 카메라를 사용하여 비디오를 녹화하고 결과를 처리하는 메서드
    private void RecordVideo()
    {
        // NativeCamera를 통해 비디오 녹화 호출
        NativeCamera.Permission permission = NativeCamera.RecordVideo((path) =>
        {
            // 비디오 녹화 완료 후 콜백 함수 실행
            Debug.Log("Video path: " + path); // 저장된 비디오 경로 출력

            if (path != null) // 경로가 유효하면
            {
                // 비디오를 전체 화면으로 재생
                Handheld.PlayFullScreenMovie("file://" + path);
            }
        });
        // 권한 요청 결과를 디버르 로그로 출력
        Debug.Log("Permission result: " + permission);
    }
}
