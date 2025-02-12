using UnityEngine;

public class SetCursorVisibility : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.visible = true;
    }
    public void SetVisibilityOn()
    {
        Cursor.visible = true;
    }

    public void SetVisibilityOff()
    {
        Cursor.visible = false;
    }
}
