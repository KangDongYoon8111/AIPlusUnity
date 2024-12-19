using UnityEngine;

public class NativeCameraLink : MonoBehaviour
{
    // 싱글턴 접근용 프로터피
    public static NativeCameraLink instance
    {
        get
        {
            if (m_instance == null) m_instance = FindObjectOfType<NativeCameraLink>();
            return m_instance;
        }
    }

    private static NativeCameraLink m_instance; // 실제 싱글턴이 할당될 변수

    [Header("앨범 이름")]
    public string albumName;

    /// <summary>
    /// 사진 촬영 메서드 : 
    /// 카메라를 사용하여 사진을 촬영하고 결과를 처리하는 메서드
    /// </summary>
    /// <param name="maxSize">화면상 미리보기 위한 조정 사이즈</param>
    public void Picture(int maxSize)
    {
        // NativaCamera를 통해 사진 촬영 호출
        NativeCamera.Permission permission = NativeCamera.TakePicture((path) => SubPicture(path, maxSize), maxSize);
    }

    /// <summary>
    /// 이미지 저장 메서드 :
    /// 카메라로 촬영된 이미지를 갤러리로 저장
    /// </summary>
    /// <param name="imagePath">임시저장된 위치값 전달 매개변수</param>
    private void SaveToGallery(string imagePath)
    {
        // 넘겨받은 파일 경로를 이용해 갤러리에 저장

        // 고유 파일 이름 생성 (현재 날짜와 시간 기반)
        string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string fileName = $"Image_{timestamp}.png";

        string albumName = this.albumName; // 갤러리에 표시될 앨범 이름

        // NativeGallery를 사용하여 갤러리에 이미지 저장
        NativeGallery.Permission permission = NativeGallery.SaveImageToGallery(imagePath, albumName, fileName, (success, path) => SubSaveToGallery(success, path));

        if(permission == NativeGallery.Permission.Denied)
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

    // 사진 촬영 시도 후 동작 : 콜백 함수 실행
    private void SubPicture(string path, int maxSize)
    {
        // 사진 촬영 완료 후 콜백 함수 실행
        Debug.Log("@First Image Url : " + path); // 저장된 이미지 경로 출력

        if (path != null) // 경로가 유효하면
        {
            SaveToGallery(path); // 갤러리에 저장

            // 경로에서 이미지 파일을 불러와 Texture2D로 변환
            Texture2D texture = NativeCamera.LoadImageAtPath(path, maxSize);

            if (texture == null) // 텍스처를 불러오지 못했을 경우
            {
                Debug.Log("@@@Unable to load texture for that path : " + path);
                return;
            }

            if (UIManager.instance.ImageView(texture))
            {
                Debug.Log("@@ImageView Success!");
            }
            else
            {
                Debug.Log("@@@UIManager Error!");
            }
            Destroy(texture, 5f); // 5초 후 텍스처 제거
                                  // 프로시저로 생성된 텍스처는 수동으로 제거해야 메모리 누수를 방지 가능
        }
    }
}
