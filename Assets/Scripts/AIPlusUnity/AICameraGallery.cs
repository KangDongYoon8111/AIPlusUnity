using System.Reflection.Emit;
using UnityEngine;

public class AICameraGallery : MonoBehaviour
{
    [Header("�ٹ� �̸�")]
    public string albumName;

    public RunInferenceModel inference;

    public void Picture(int maxSize)
    {
        // NativaCamera�� ���� ���� �Կ� ȣ��
        NativeCamera.Permission permission = NativeCamera.TakePicture((path) => SubPicture(path, maxSize), maxSize);
    }

    private void SubPicture(string path, int maxSize)
    {
        // ���� �Կ� �Ϸ� �� �ݹ� �Լ� ����
        Debug.Log("@First Image Url : " + path); // ����� �̹��� ��� ���

        if (path != null) // ��ΰ� ��ȿ�ϸ�
        {
            SaveToGallery(path); // �������� ����

            // ��ο��� �̹��� ������ �ҷ��� Texture2D�� ��ȯ
            Texture2D texture = NativeCamera.LoadImageAtPath(path, maxSize);
            inference.ExecuteML(texture, out string label, out float accuracy);

            if (texture == null) // �ؽ�ó�� �ҷ����� ������ ���
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
            Destroy(texture, 10f); // 5�� �� �ؽ�ó ����
                                  // ���ν����� ������ �ؽ�ó�� �������� �����ؾ� �޸� ������ ���� ����
        }
    }

    private void SaveToGallery(string imagePath)
    {
        // �Ѱܹ��� ���� ��θ� �̿��� �������� ����

        // ���� ���� �̸� ���� (���� ��¥�� �ð� ���)
        string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string fileName = $"Image_{timestamp}.png";

        string albumName = this.albumName; // �������� ǥ�õ� �ٹ� �̸�

        // NativeGallery�� ����Ͽ� �������� �̹��� ����
        NativeGallery.Permission permission = NativeGallery.SaveImageToGallery(imagePath, albumName, fileName, (success, path) => SubSaveToGallery(success, path));

        if (permission == NativeGallery.Permission.Denied)
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

    public void PickImage(int maxSize)
    {
        NativeGallery.Permission permission = NativeGallery.GetImageFromGallery((path) => SubPickImage(path, maxSize));
    }

    // ���������� �̹����� ���� �� ���� : �ݹ� �Լ� ����
    private void SubPickImage(string path, int maxSize)
    {
        Debug.Log("@Gallery Image Url : " + path); // ������ �̹��� ��� ���

        if (path != null) // �̹��� ��ΰ� ��ȿ�ϴٸ�
        {
            Texture2D texture = NativeGallery.LoadImageAtPath(path, maxSize); // �ؽ�ó �ε�
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
            Destroy(texture, 10f); // 5�� �� �ؽ�ó ����
                                  // ���ν����� ������ �ؽ�ó�� �������� �����ؾ� �޸� ������ ���� ����
        }
    }
}
