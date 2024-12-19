using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    // �̱��� ���ٿ� ������Ƽ
    public static UIManager instance
    {
        get
        {
            if(m_instance == null) m_instance = FindObjectOfType<UIManager>();
            return m_instance;
        }
    }

    private static UIManager m_instance; // ���� �̱����� �Ҵ�� ����

    private RawImage imageView;

    private void Awake()
    {
        imageView = GameObject.Find("RawImage").GetComponent<RawImage>();
    }

    public bool ImageView(Texture2D texture)
    {
        if (imageView == null) return false;
        else
        {
            this.imageView.texture = texture;
            return true;
        }
    }
}
