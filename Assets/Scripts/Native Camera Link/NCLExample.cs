using UnityEngine;

public class NCLExample : MonoBehaviour
{
    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // ���콺 ���� ��ư Ŭ�� ����
        {
            // ī�޶� �̹� ��� ���̶�� �� �̻� �۾����� ����
            if (NativeCamera.IsCameraBusy()) return;

            // ȭ�� Ŭ��(��ġ)�� �� ��ġ�� ȭ���� �����̴ٸ�,
            if (Input.mousePosition.x < Screen.width / 2)
            {
                // ���� �Կ� �޼��� �ߵ�. �ִ� ũ��� 512px�� ����
                TakePicture(512);
            }
            else // ȭ�� Ŭ��(��ġ)�� �� ��ġ�� ȭ���� �������̴ٸ�,
            {
                // ���� ��ȭ �޼��� �ߵ�.
                RecordVideo();
            }
        }
    }

    // �񵿱�� ī�޶� ������ ��û�ϴ� �޼���
    // ���� �Ǵ� ���� ������ ��û�� �� ���
    private async void RequestPermissionAsynchronously(bool isPicturePermission)
    {
        // NativeCamera�� ���� ��û �񵿱� �޼��带 ȣ��
        NativeCamera.Permission permission = await NativeCamera.RequestPermissionAsync(isPicturePermission);
        // ���� ��û ����� ����� �α׷� ���
        Debug.Log("Permission result: " + permission);
    }
    /* DK Chack Point : �񵿱��?
    * 1. �񵿱�(asynchronous:���̽�ũ���ʽ�)�� �۾��� ���ÿ� ó���ϰų� �۾��� �Ϸ�� ������ ��ٸ��� �ʰ� �ٸ� �۾��� ��� ������ �� �ִ� ���α׷��� ����Դϴ�.
    * 2. ���� : ����� ���������� ������ �ֹ��� ��, ������ ��������� �ƹ��͵� ���� �ʴ� �Ϳ� ���� / �񵿱�� ������ ���� ������ �ٸ� �ൿ(����Ʈ���� ����ϰų�, å�� �д� ��)�� �ϴ� �Ϳ� �����մϴ�.
    * 3. async Ű���� : ���������� Ű����ν� �ش� �޼���(�Լ�)�� �񵿱�� ������ ������ ��Ÿ���ϴ�.
    * 4. await Ű���� : �񵿱� �۾� �Ʒ�������(NativeCamera.RequestPermissionAsync)�� �ϷḦ ��ٸ��ϴ�.
    * 5. ���� ���� : RequestPermissionAsync �޼���� �񵿱� �޼����, ī�޶� ������ ��û�մϴ�. ���� ��û ����� ��ȯ�Ǹ� permission ������ ����� �����մϴ�.
    */

    // ī�޶� ����Ͽ� ������ �Կ��ϰ� ����� ó���ϴ� �޼���
    private void TakePicture(int maxSize)
    {
        // NativeCamera�� ���� ���� �Կ� ȣ��
        NativeCamera.Permission permission = NativeCamera.TakePicture((path) =>
        {
            // ���� �Կ� �Ϸ� �� �ݹ� �Լ� ����
            Debug.Log("Image path : " + path); // ����� �̹��� ��� ���

            if (path != null) // ��ΰ� ��ȿ�ϸ�
            {
                // ��ο��� �̹��� ������ �ҷ��� Texture2D�� ��ȯ
                Texture2D texture = NativeCamera.LoadImageAtPath(path, maxSize);
                if (texture == null) // �ؽ�ó�� �ҷ����� ������ ���
                {
                    Debug.Log("Couldn't load texture from " + path);
                    return;
                }

                // ĸó�� �̹����� ȭ�鿡 ǥ���ϱ� ���� Quad ����
                GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
                // ī�޶� �� 2.5m ��ġ�� ����
                quad.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 2.5f;
                // Quad�� ī�޶� ���ϵ��� ����
                quad.transform.forward = Camera.main.transform.forward;
                // �̹��� ������ �°� �����ϸ�
                quad.transform.localScale = new Vector3(1f, texture.height / (float)texture.width, 1f);

                // Quad�� �ؽ�ó�� ����
                Material material = quad.GetComponent<Renderer>().material;
                // ��� ������ ���̴��� ������, �⺻ ���̴��� ��ü
                if (!material.shader.isSupported) material.shader = Shader.Find("Legacy Shaders/Diffuse");

                material.mainTexture = texture; // �ؽ�ó �Ҵ�

                Destroy(quad, 5f); // 5�� �� Quad ����
                Destroy(texture, 5f); // 5�� �� �ؽ�ó ����
                // ���ν����� ������ �ؽ�ó�� �������� �����ؾ� �޸� ������ ���� ����
            }
        }, maxSize // maxSize�� �̹����� �ִ� ũ�⸦ �ȼ� ������ ����
        );

        // ���� ��û ����� ����� �α׷� ���
        Debug.Log("Permission result: " + permission);
    }
    /* DK Chack Point : �ݹ��̶�?
    * 1. �ݹ�(callback)�� Ư�� �۾��� �Ϸ�Ǿ��� �� ����� �ڵ峪 �Լ��� �ǹ��մϴ�.
    *    �Ϲ������� �ݹ��� �޼���(�Լ�)�� ���ڷ� ���޵Ǹ�, �۾��� ���� �� ȣ��˴ϴ�.
    * 2. ���� : 1) �������� ������ �ֹ��ϰ� ��ȭ��ȣ�� ����� "������ �غ�Ǹ� �����ּ���." ��� ��û�� ��Ȳ����, 2) ������ �غ�Ǿ��� �� ���������� ��ȭ�� �ϴ� ���� �ݹ� �Դϴ�.
    * 3. ���� ���� : TakePicture �޼���(�Լ�)�� �ݹ��� ����ϸ�, (path) => { ... } �� �۾�(���� �Կ�)�� �Ϸ��ϸ� ȣ��˴ϴ�.
    *               �̶� path ���ڴ� �۾��� ���(�Կ��� ������ ���)�� �ݹ� �Լ��� ���޵˴ϴ�.
    */

    // ī�޶� ����Ͽ� ������ ��ȭ�ϰ� ����� ó���ϴ� �޼���
    private void RecordVideo()
    {
        // NativeCamera�� ���� ���� ��ȭ ȣ��
        NativeCamera.Permission permission = NativeCamera.RecordVideo((path) =>
        {
            // ���� ��ȭ �Ϸ� �� �ݹ� �Լ� ����
            Debug.Log("Video path: " + path); // ����� ���� ��� ���

            if (path != null) // ��ΰ� ��ȿ�ϸ�
            {
                // ������ ��ü ȭ������ ���
                Handheld.PlayFullScreenMovie("file://" + path);
            }
        });
        // ���� ��û ����� ����� �α׷� ���
        Debug.Log("Permission result: " + permission);
    }
}
