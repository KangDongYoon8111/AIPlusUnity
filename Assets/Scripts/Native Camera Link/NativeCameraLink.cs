using UnityEngine;

public class NativeCameraLink : MonoBehaviour
{
    // �̱��� ���ٿ� ��������
    public static NativeCameraLink instance
    {
        get
        {
            if (m_instance == null) m_instance = FindObjectOfType<NativeCameraLink>();
            return m_instance;
        }
    }

    private static NativeCameraLink m_instance; // ���� �̱����� �Ҵ�� ����

    [Header("�ٹ� �̸�")]
    public string albumName;

    /// <summary>
    /// ���� �Կ� �޼��� : 
    /// ī�޶� ����Ͽ� ������ �Կ��ϰ� ����� ó���ϴ� �޼���
    /// </summary>
    /// <param name="maxSize">ȭ��� �̸����� ���� ���� ������</param>
    public void Picture(int maxSize)
    {
        // NativaCamera�� ���� ���� �Կ� ȣ��
        NativeCamera.Permission permission = NativeCamera.TakePicture((path) => SubPicture(path, maxSize), maxSize);
    }

    /// <summary>
    /// �̹��� ���� �޼��� :
    /// ī�޶�� �Կ��� �̹����� �������� ����
    /// </summary>
    /// <param name="imagePath">�ӽ������ ��ġ�� ���� �Ű�����</param>
    private void SaveToGallery(string imagePath)
    {
        // �Ѱܹ��� ���� ��θ� �̿��� �������� ����

        // ���� ���� �̸� ���� (���� ��¥�� �ð� ���)
        string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string fileName = $"Image_{timestamp}.png";

        string albumName = this.albumName; // �������� ǥ�õ� �ٹ� �̸�

        // NativeGallery�� ����Ͽ� �������� �̹��� ����
        NativeGallery.Permission permission = NativeGallery.SaveImageToGallery(imagePath, albumName, fileName, (success, path) => SubSaveToGallery(success, path));

        if(permission == NativeGallery.Permission.Denied)
        {
            Debug.LogWarning("@@@Permission to save image denied.");
        }
    }

    // �̹��� ���� �õ� �� ���� : �ݹ� �Լ� ����
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

    // ���� �Կ� �õ� �� ���� : �ݹ� �Լ� ����
    private void SubPicture(string path, int maxSize)
    {
        // ���� �Կ� �Ϸ� �� �ݹ� �Լ� ����
        Debug.Log("@First Image Url : " + path); // ����� �̹��� ��� ���

        if (path != null) // ��ΰ� ��ȿ�ϸ�
        {
            SaveToGallery(path); // �������� ����

            // ��ο��� �̹��� ������ �ҷ��� Texture2D�� ��ȯ
            Texture2D texture = NativeCamera.LoadImageAtPath(path, maxSize);

            if (texture == null) // �ؽ�ó�� �ҷ����� ������ ���
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
