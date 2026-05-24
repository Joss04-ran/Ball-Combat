using DG.Tweening;
using TMPro;
using UnityEngine;

public class DamagePopup : MonoBehaviour
{
    private TextMeshPro textMesh;
    void Awake()
    {
        textMesh = GetComponent<TextMeshPro>();
    }
    public void Setup(string text)
    {
        textMesh.SetText(text);

        float animDur = 1f;
        float moveDis = 1.5f;

        transform.DOMoveY(transform.position.y + moveDis, animDur).SetEase(Ease.OutQuart);

        textMesh.DOFade(0f, animDur)
            .SetEase(Ease.InExpo)
            .OnComplete(() =>
            {
                Destroy(gameObject);
            });
    }
}
