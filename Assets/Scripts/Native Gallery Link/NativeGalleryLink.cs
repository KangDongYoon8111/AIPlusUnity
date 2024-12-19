using UnityEngine;

public class NativeGalleryLink : MonoBehaviour
{
    // �̱��� ���ٿ� ������Ƽ
    public static NativeGalleryLink instance
    {
        get
        {
            if (m_instance == null) m_instance = FindObjectOfType<NativeGalleryLink>();
            return m_instance;            
        }
    }

    private static NativeGalleryLink m_instance; // ���� �̱����� �Ҵ�� ����

    /// <summary>
    /// ������ �̹��� ���� �޼��� :
    /// �������� �����Ͽ� �̹����� ��ġ���� �������� ó���ϴ� �޼���
    /// </summary>
    /// <param name="maxSize">ȭ��� �̸����� ���� ���� ������</param>
    public void PickImage(int maxSize)
    {
        NativeGallery.Permission permission = NativeGallery.GetImageFromGallery((path) => SubPickImage(path, maxSize));
    }

    // ���������� �̹����� ���� �� ���� : �ݹ� �Լ� ����
    private void SubPickImage(string path, int maxSize)
    {
        Debug.Log("@Gallery Image Url : " + path); // ������ �̹��� ��� ���

        if(path != null) // �̹��� ��ΰ� ��ȿ�ϴٸ�
        {
            Texture2D texture = NativeGallery.LoadImageAtPath(path, maxSize); // �ؽ�ó �ε�
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
            Destroy(texture, 5f); // 5�� �� �ؽ�ó ����
                                  // ���ν����� ������ �ؽ�ó�� �������� �����ؾ� �޸� ������ ���� ����
        }
    }
}
