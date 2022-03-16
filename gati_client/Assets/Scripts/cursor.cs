using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cursor : MonoBehaviour
{
    [SerializeField] public Texture2D pers_cursor;
    void Start()
    {
        Vector2 cursorOffset = new Vector2(0, 0);
        Cursor.SetCursor(pers_cursor, cursorOffset, CursorMode.Auto);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
