using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ScrollSnap : MonoBehaviour
{
    public ScrollRect scrollRect;
    public RectTransform content;
    public RectTransform viewport; // Reference to viewport
    public float snapSpeed = 10f; // Adjust for faster/slower snapping

    private List<float> snapPositions = new List<float>();
    private bool isSnapping = false;

    void Start()
    {
        CalculateSnapPositions();
    }

    void Update()
    {
        // Recalculate positions if the number of children changes
        if (snapPositions.Count != content.childCount)
        {
            CalculateSnapPositions();
        }
    }

    void CalculateSnapPositions()
    {
        snapPositions.Clear();
        int childCount = content.childCount;

        for (int i = 0; i < childCount; i++)
        {
            RectTransform child = content.GetChild(i) as RectTransform;

            // Convert top of child to content space
            float childTopY = child.localPosition.y - (child.rect.height * 0.5f);
            snapPositions.Add(childTopY);
        }
    }

    public void OnEndDrag()
    {
        if (isSnapping || snapPositions.Count == 0) return;

        float currentY = content.localPosition.y; // Current content Y position
        float closestPos = snapPositions[0];
        float minDistance = Mathf.Abs(currentY - closestPos);

        // Find the closest position
        foreach (float pos in snapPositions)
        {
            float distance = Mathf.Abs(currentY - pos);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestPos = pos;
            }
        }

        // Adjust target position so the selected character's top aligns with the viewport top
        float viewportTopY = viewport.rect.height * 0.5f;
        float targetY = closestPos - viewportTopY;

        StartCoroutine(SmoothSnap(targetY));
    }

    IEnumerator SmoothSnap(float targetY)
    {
        isSnapping = true;
        Vector2 startPos = content.localPosition;
        Vector2 targetPos = new Vector2(content.localPosition.x, targetY);

        float time = 0;
        while (time < 1)
        {
            time += Time.deltaTime * snapSpeed;
            content.localPosition = Vector2.Lerp(startPos, targetPos, time);
            yield return null;
        }

        content.localPosition = targetPos;
        isSnapping = false;
    }
}
