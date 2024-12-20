using System.Reflection.Emit;
using UnityEngine;

public class AICameraGallery : MonoBehaviour
{
    [Header("앨범 이름")]
    public string albumName;

    public RunInferenceModel inference;

    public void Picture(int maxSize)
    {
        // NativaCamera를 통해 사진 촬영 호출
        NativeCamera.Permission permission = NativeCamera.TakePicture((path) => SubPicture(path, maxSize), maxSize);
    }

    private void SubPicture(string path, int maxSize)
    {
        // 사진 촬영 완료 후 콜백 함수 실행
        Debug.Log("@First Image Url : " + path); // 저장된 이미지 경로 출력

        if (path != null) // 경로가 유효하면
        {
            SaveToGallery(path); // 갤러리에 저장

            // 경로에서 이미지 파일을 불러와 Texture2D로 변환
            Texture2D texture = NativeCamera.LoadImageAtPath(path, maxSize);
            inference.ExecuteML(texture, out string label, out float accuracy);

            if (texture == null) // 텍스처를 불러오지 못했을 경우
            {
                Debug.Log("@@@Unable to load texture for that path : " + path);
                return;
            }

            if (UIManager.instance.ImageView(texture, label, accuracy))
            {
                Debug.Log("@@ImageView Success!");
            }
            else
            {
                Debug.Log("@@@UIManager Error!");
            }
            Destroy(texture, 10f); // 5초 후 텍스처 제거
                                  // 프로시저로 생성된 텍스처는 수동으로 제거해야 메모리 누수를 방지 가능
        }
    }

    private void SaveToGallery(string imagePath)
    {
        // 넘겨받은 파일 경로를 이용해 갤러리에 저장

        // 고유 파일 이름 생성 (현재 날짜와 시간 기반)
        string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string fileName = $"Image_{timestamp}.png";

        string albumName = this.albumName; // 갤러리에 표시될 앨범 이름

        // NativeGallery를 사용하여 갤러리에 이미지 저장
        NativeGallery.Permission permission = NativeGallery.SaveImageToGallery(imagePath, albumName, fileName, (success, path) => SubSaveToGallery(success, path));

        if (permission == NativeGallery.Permission.Denied)
        {
            Debug.LogWarning("@@@Permission to save image denied.");
        }
    }

    // 이미지 저장 시도 후 동작 : 콜백 함수 실행
    private void SubSaveToGallery(bool success, string path)
    {
        if (success)
        {
            Debug.Log($"@@Image save success : {path}");
        }
        else
        {
            Debug.Log("@@@Image save Failed!");
        }
    }

    public void PickImage(int maxSize)
    {
        NativeGallery.Permission permission = NativeGallery.GetImageFromGallery((path) => SubPickImage(path, maxSize));
    }

    // 갤러리에서 이미지를 선택 후 동작 : 콜백 함수 실행
    private void SubPickImage(string path, int maxSize)
    {
        Debug.Log("@Gallery Image Url : " + path); // 가져온 이미지 경로 출력

        if (path != null) // 이미지 경로가 유효하다면
        {
            Texture2D texture = NativeGallery.LoadImageAtPath(path, maxSize); // 텍스처 로드
            inference.ExecuteML(texture, out string label, out float accuracy);

            if (texture == null)
            {
                Debug.Log("@@@Unable to load texture for that path : " + path);
                return;
            }

            if (UIManager.instance.ImageView(texture, label, accuracy))
            {
                Debug.Log("@@ImageView Success!");
            }
            else
            {
                Debug.Log("@@@UIManager Error!");
            }
            Destroy(texture, 10f); // 5초 후 텍스처 제거
                                  // 프로시저로 생성된 텍스처는 수동으로 제거해야 메모리 누수를 방지 가능
        }
    }
}
