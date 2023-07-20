using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.UI;

public class TooltipSystem : MonoBehaviour
{
    public static TooltipSystem instance;
    private void Awake()
    {
        instance = this;
        Hide();
    }

    public Tooltip tooltip;
    public InputSystemUIInputModule input;

    public static Vector2 MousePosition
    {
        get
        {
            if (instance == null)
                return Vector2.zero;
            else
            {
                if (instance.input == null)
                    instance.input = FindObjectOfType<InputSystemUIInputModule>();
                if (instance.input == null)
                    throw new System.Exception("No InputSystemUIInputModule in scene!");
                return instance.input.point.action.ReadValue<Vector2>();
            }
        }
    }

    public static void Show(string content, string header = "")
    {
        instance.tooltip.gameObject.SetActive(true);
        instance.tooltip.Set(header, content);
    }

    public static void Hide()
    {
        instance.tooltip.gameObject.SetActive(false);
    }
}
