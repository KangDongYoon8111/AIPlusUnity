using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

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
    private TextMeshProUGUI classText;

    private void Awake()
    {
        imageView = GameObject.Find("RawImage").GetComponent<RawImage>();
        classText = GameObject.Find("ClassText").GetComponent<TextMeshProUGUI>();
    }

    public bool ImageView(Texture2D texture, string label = "Class", float accuracy = 0f)
    {
        if (imageView == null) return false;
        else
        {
            this.imageView.texture = texture;
            this.classText.text = $"{label}\n{Math.Round(accuracy * 100, 1)}%";
            return true;
        }
    }
}
