using System.Collections;
using UnityEngine;

public class NGLExample : MonoBehaviour
{
    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // ���콺 ���� ��ư Ŭ�� ����
        {
            // Ŭ���� ��ġ�� ȭ�� ���� 1/3 �����̶��,
            if(Input.mousePosition.x < Screen.width / 3)
            {
                // ��ũ���� ���� Coroutine ȣ��
                StartCoroutine(TakeScreenshotAndSave());
            }
            else
            {
                // ������ �۾� ���̸� ����
                if (NativeGallery.IsMediaPickerBusy()) return;

                // Ŭ���� ��ġ�� ȭ�� �߾� 1/3 �����̶��,
                if(Input.mousePosition.x < Screen.width * 2 / 3)
                {
                    PickImage(512); // �̹����� ���������� ��������
                }
                else
                {
                    PickVideo(); // ������ ���������� ��������
                }
            }
        }
    }

    // �񵿱� ���� ��û �޼��� :
    // ������ ���� ����(permissionType) �� �̵�� ����(mediaType)�� ���� ���� ��û
    private async void RequestPermissionAsynchronously(NativeGallery.PermissionType permissionType, NativeGallery.MediaType mediaType)
    {
        NativeGallery.Permission permission = await NativeGallery.RequestPermissionAsync(permissionType, mediaType); // ���� ��û
        Debug.Log("Permission result : " + permission); // ���� ��� ���
    }

    // Coroutine���� ��ũ������ ��� �������� ����
    private IEnumerator TakeScreenshotAndSave()
    {
        yield return new WaitForEndOfFrame(); // ������ ���� ������ ���

        // ���� ȭ�� ĸó�� ���� �ؽ�ó ����
        Texture2D ss = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        ss.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0); // ȭ�� �ȼ� �б�
        ss.Apply(); // �ؽ�ó ����

        // �������� �̹����� �����ϰ� ���� ��θ� �ݹ����� ��ȯ����
        NativeGallery.Permission permission = NativeGallery.SaveImageToGallery(
            ss,
            "GalleryTest", // �ٹ� �̸�
            "Image.png", // ���� �̸�
            (success, path) => Debug.Log("Media save result : " + success + " " + path) // ���� ����� ��� ���
            );

        // ���� ���� ���
        Debug.Log("Permission result : " + permission);

        Destroy(ss); // �ؽ�ó �޸� ����
    }

    // ���������� �̹����� �����ϰ� �ε�
    private void PickImage(int maxSize)
    {
        // �̹����� ���������� �������� �۾� ó��
        NativeGallery.Permission permission = NativeGallery.GetImageFromGallery((path) =>
        {
            Debug.Log("Image path : " + path); // ���õ� �̹��� ��� ���

            if(path != null) // �̹��� ��ΰ� ��ȿ�ϴٸ�
            {
                Texture2D texture = NativeGallery.LoadImageAtPath(path, maxSize); // �ؽ�ó �ε�
                if(texture == null) // �ε� ���� �� �α� ���
                {
                    Debug.Log("Couldn't load texture from " + path);
                    return;
                }

                // �ؽ�ó�� ȭ�鿡 ǥ���ϱ� ���� Quad ����
                GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
                // ī�޶� �տ� ��ġ
                quad.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 2.5f;
                // ī�޶� �������� ����
                quad.transform.forward = Camera.main.transform.forward;
                // �ؽ�ó ������ �°� ����
                quad.transform.localScale = new Vector3(1f, texture.height / (float)texture.width, 1f);

                Material material = quad.GetComponent<Renderer>().material;
                if (!material.shader.isSupported) // Shader�� �������� ������ ��ü Shader�� ����
                {
                    material.shader = Shader.Find("Legacy Shaders/Diffuse");
                }

                material.mainTexture = texture; // �ؽ��� ����

                Destroy(quad, 5f); // 5�� �� Quad ����
                Destroy(texture, 5f); // 5�� �� �ؽ�ó ����
            }
        });
        Debug.Log("Permission result : " + permission); // ���� ��� ���
    }

    // ���������� ������ �����ϰ� ���
    private void PickVideo()
    {
        // ������ ���������� �������� �۾� ó��
        NativeGallery.Permission permission = NativeGallery.GetVideoFromGallery((path) =>
        {
            Debug.Log("Video path : " + path); // ���õ� ���� ��� ���

            if (path != null) // ���� ��ΰ� ��ȿ�ϴٸ�
            {
                Handheld.PlayFullScreenMovie("file://" + path); // ���� Ǯ��ũ�� ���
            }
        }, "Select a video");

        Debug.Log("Permission result : " + permission); // ���� ��� ���
    }

    // ���������� �̹��� �Ǵ� ���� ����
    private void PickImageOrVideo()
    {
        if (NativeGallery.CanSelectMultipleMediaTypesFromGallery()) // ���� �̵�� ���� ���� ���� Ȯ��
        {
            NativeGallery.Permission permission = NativeGallery.GetMixedMediaFromGallery((path) =>
            {
                Debug.Log("Media path : " + path); // ���õ� �̵�� ��� ���

                if (path != null) // ��ΰ� ��ȿ�ϴٸ�
                {
                    // ���õ� ������ �̵�� ���� Ȯ��
                    switch (NativeGallery.GetMediaTypeOfFile(path))
                    {
                        case NativeGallery.MediaType.Image: 
                            Debug.Log("Picked image"); // �̹��� ���õ�
                            break;
                        case NativeGallery.MediaType.Video: 
                            Debug.Log("Picked video"); // ���� ���õ�
                            break;
                        default: 
                            Debug.Log("Probably picked something else"); // ��Ÿ ���� ���õ�
                            break;
                    }
                }
            }, NativeGallery.MediaType.Image | NativeGallery.MediaType.Video, "Select an image or video");

            Debug.Log("Permission result : " + permission); // ���� ��� ���
        }
    }
}
