using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[ExecuteInEditMode]
public class Tooltip : MonoBehaviour
{
    public TMP_Text header;
    public TMP_Text content;

    public LayoutElement clampSizeElement;

    public void Set(string header, string content)
    {
        this.header.text = header;
        this.content.text = content;
        UpdateSize();
    }

    public void UpdateSize()
    {
        float width = Mathf.Max(header.preferredWidth, content.preferredWidth);

        clampSizeElement.enabled = width > clampSizeElement.preferredWidth;
    }

    void Update()
    {
        if (Application.isEditor && !Application.isPlaying)
            UpdateSize();

        Vector2 position = TooltipSystem.MousePosition;
        transform.position = position;
        //InputSystemUIInputModule.point.action.ReadValue<Vector2>();
    }
}
