using UnityEditor;
using UnityEngine;

public static class RectTransformExtensions
{
    [MenuItem("CONTEXT/RectTransform/Set Anchors to Match Size (Maintain Visual)")]
    private static void SetAnchorsToMatchSize(MenuCommand command)
    {
        RectTransform rectTransform = (RectTransform)command.context;

        Undo.RecordObject(rectTransform, "Set Anchors to Match Size");

        RectTransform parentRectTransform = rectTransform.parent as RectTransform;

        if (parentRectTransform != null)
        {
            // Salva as posições atuais
            Vector2 originalPosition = rectTransform.anchoredPosition;
            Vector2 originalSize = rectTransform.rect.size;

            // Calcula os novos anchors
            Vector2 anchorMin = new Vector2(
                rectTransform.anchorMin.x + rectTransform.offsetMin.x / parentRectTransform.rect.width,
                rectTransform.anchorMin.y + rectTransform.offsetMin.y / parentRectTransform.rect.height
            );

            Vector2 anchorMax = new Vector2(
                rectTransform.anchorMax.x + rectTransform.offsetMax.x / parentRectTransform.rect.width,
                rectTransform.anchorMax.y + rectTransform.offsetMax.y / parentRectTransform.rect.height
            );

            // Aplica os novos anchors
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;

            // Restaura o tamanho visual
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.anchoredPosition = Vector2.zero;
        }
        else
        {
            Debug.LogWarning("RectTransform does not have a parent RectTransform. Anchors cannot be adjusted.");
        }
    }

    [MenuItem("CONTEXT/RectTransform/Set Anchors to Object Center")]
    private static void SetAnchorsToObjectCenter(MenuCommand command)
    {
        RectTransform rectTransform = (RectTransform)command.context;

        Undo.RecordObject(rectTransform, "Set Anchors to Object Center");

        RectTransform parentRectTransform = rectTransform.parent as RectTransform;
        if (parentRectTransform == null)
        {
            Debug.LogWarning("RectTransform does not have a parent RectTransform. Anchors cannot be adjusted.");
            return;
        }

        // Salva a posição original
        Vector2 originalAnchoredPosition = rectTransform.anchoredPosition;

        // Calcula a posição central relativa ao pai
        Vector2 anchorCenter = new Vector2(
            rectTransform.anchorMin.x + (rectTransform.offsetMin.x + rectTransform.rect.width / 2) / parentRectTransform.rect.width,
            rectTransform.anchorMin.y + (rectTransform.offsetMin.y + rectTransform.rect.height / 2) / parentRectTransform.rect.height
        );

        // Aplica os novos anchors centralizados
        rectTransform.anchorMin = anchorCenter;
        rectTransform.anchorMax = anchorCenter;

        // Mantém o visual ajustando a posição e o pivot
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero;
        //rectTransform.anchoredPosition = originalAnchoredPosition;
    }

}
