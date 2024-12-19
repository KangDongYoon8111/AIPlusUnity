using System.Collections;
using UnityEngine;

public class NGLExample : MonoBehaviour
{
    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // 마우스 왼쪽 버튼 클릭 감지
        {
            // 클릭한 위치가 화면 왼쪽 1/3 영역이라면,
            if(Input.mousePosition.x < Screen.width / 3)
            {
                // 스크린샷 저장 Coroutine 호출
                StartCoroutine(TakeScreenshotAndSave());
            }
            else
            {
                // 갤러리 작업 중이면 종료
                if (NativeGallery.IsMediaPickerBusy()) return;

                // 클릭한 위치가 화면 중앙 1/3 영역이라면,
                if(Input.mousePosition.x < Screen.width * 2 / 3)
                {
                    PickImage(512); // 이미지를 갤러리에서 가져오기
                }
                else
                {
                    PickVideo(); // 비디오를 갤러리에서 가져오기
                }
            }
        }
    }

    // 비동기 권한 요청 메서드 :
    // 지정된 권한 유형(permissionType) 및 미디어 유형(mediaType)에 대해 권한 요청
    private async void RequestPermissionAsynchronously(NativeGallery.PermissionType permissionType, NativeGallery.MediaType mediaType)
    {
        NativeGallery.Permission permission = await NativeGallery.RequestPermissionAsync(permissionType, mediaType); // 권한 요청
        Debug.Log("Permission result : " + permission); // 권한 결과 출력
    }

    // Coroutine으로 스크린샷을 찍고 갤러리에 저장
    private IEnumerator TakeScreenshotAndSave()
    {
        yield return new WaitForEndOfFrame(); // 프레임 끝날 때까지 대기

        // 현재 화면 캡처를 위한 텍스처 생성
        Texture2D ss = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        ss.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0); // 화면 픽셀 읽기
        ss.Apply(); // 텍스처 적용

        // 갤러리에 이미지를 저장하고 저장 경로를 콜백으로 반환받음
        NativeGallery.Permission permission = NativeGallery.SaveImageToGallery(
            ss,
            "GalleryTest", // 앨범 이름
            "Image.png", // 파일 이름
            (success, path) => Debug.Log("Media save result : " + success + " " + path) // 저장 결과와 경로 출력
            );

        // 저장 권한 출력
        Debug.Log("Permission result : " + permission);

        Destroy(ss); // 텍스처 메모리 해제
    }

    // 갤러리에서 이미지를 선택하고 로드
    private void PickImage(int maxSize)
    {
        // 이미지를 갤러리에서 가져오는 작업 처리
        NativeGallery.Permission permission = NativeGallery.GetImageFromGallery((path) =>
        {
            Debug.Log("Image path : " + path); // 선택된 이미지 경로 출력

            if(path != null) // 이미지 경로가 유효하다면
            {
                Texture2D texture = NativeGallery.LoadImageAtPath(path, maxSize); // 텍스처 로드
                if(texture == null) // 로드 실패 시 로그 출력
                {
                    Debug.Log("Couldn't load texture from " + path);
                    return;
                }

                // 텍스처를 화면에 표시하기 위해 Quad 생성
                GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
                // 카메라 앞에 배치
                quad.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 2.5f;
                // 카메라 방향으로 정렬
                quad.transform.forward = Camera.main.transform.forward;
                // 텍스처 비율에 맞게 조정
                quad.transform.localScale = new Vector3(1f, texture.height / (float)texture.width, 1f);

                Material material = quad.GetComponent<Renderer>().material;
                if (!material.shader.isSupported) // Shader가 지원되지 않으면 대체 Shader로 설정
                {
                    material.shader = Shader.Find("Legacy Shaders/Diffuse");
                }

                material.mainTexture = texture; // 텍스쳐 설정

                Destroy(quad, 5f); // 5초 후 Quad 삭제
                Destroy(texture, 5f); // 5초 후 텍스처 삭제
            }
        });
        Debug.Log("Permission result : " + permission); // 권한 결과 출력
    }

    // 갤러리에서 비디오를 선택하고 재생
    private void PickVideo()
    {
        // 비디오를 갤러리에서 가져오는 작업 처리
        NativeGallery.Permission permission = NativeGallery.GetVideoFromGallery((path) =>
        {
            Debug.Log("Video path : " + path); // 선택된 비디오 경로 출력

            if (path != null) // 비디오 경로가 유효하다면
            {
                Handheld.PlayFullScreenMovie("file://" + path); // 비디오 풀스크린 재생
            }
        }, "Select a video");

        Debug.Log("Permission result : " + permission); // 권한 결과 출력
    }

    // 갤러리에서 이미지 또는 비디오 선택
    private void PickImageOrVideo()
    {
        if (NativeGallery.CanSelectMultipleMediaTypesFromGallery()) // 복합 미디어 선택 가능 여부 확인
        {
            NativeGallery.Permission permission = NativeGallery.GetMixedMediaFromGallery((path) =>
            {
                Debug.Log("Media path : " + path); // 선택된 미디어 경로 출력

                if (path != null) // 경로가 유효하다면
                {
                    // 선택된 파일의 미디어 유형 확인
                    switch (NativeGallery.GetMediaTypeOfFile(path))
                    {
                        case NativeGallery.MediaType.Image: 
                            Debug.Log("Picked image"); // 이미지 선택됨
                            break;
                        case NativeGallery.MediaType.Video: 
                            Debug.Log("Picked video"); // 비디오 선택됨
                            break;
                        default: 
                            Debug.Log("Probably picked something else"); // 기타 파일 선택됨
                            break;
                    }
                }
            }, NativeGallery.MediaType.Image | NativeGallery.MediaType.Video, "Select an image or video");

            Debug.Log("Permission result : " + permission); // 권한 결과 출력
        }
    }
}
