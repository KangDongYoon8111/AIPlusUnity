using UnityEngine;

public class NativeGalleryLink : MonoBehaviour
{
    // 싱글턴 접근용 프로퍼티
    public static NativeGalleryLink instance
    {
        get
        {
            if (m_instance == null) m_instance = FindObjectOfType<NativeGalleryLink>();
            return m_instance;            
        }
    }

    private static NativeGalleryLink m_instance; // 실제 싱글턴이 할당될 변수

    /// <summary>
    /// 갤러리 이미지 선택 메서드 :
    /// 갤러리에 접근하여 이미지의 위치값을 가져오고 처리하는 메서드
    /// </summary>
    /// <param name="maxSize">화면상 미리보기 위한 조정 사이즈</param>
    public void PickImage(int maxSize)
    {
        NativeGallery.Permission permission = NativeGallery.GetImageFromGallery((path) => SubPickImage(path, maxSize));
    }

    // 갤러리에서 이미지를 선택 후 동작 : 콜백 함수 실행
    private void SubPickImage(string path, int maxSize)
    {
        Debug.Log("@Gallery Image Url : " + path); // 가져온 이미지 경로 출력

        if(path != null) // 이미지 경로가 유효하다면
        {
            Texture2D texture = NativeGallery.LoadImageAtPath(path, maxSize); // 텍스처 로드
            if(texture == null)
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
