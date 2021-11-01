using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PanelDragger : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    private Vector2 lastMousePosition;

    private float z;

    private GameManager _manager;

    void Start() {
         z = transform.position.z;
        //    p("******** Start **********, z is "+z);
        RectTransform rect = GetComponent<RectTransform>();

        if (_manager == null) _manager = GameObject.FindObjectOfType<GameManager>();
        if (_manager != null && _manager.panelPosition != null) {
             rect.position = _manager.panelPosition;
        }
       
        int count = countInside(rect);
        if (count < 4) {
            p("Screen res: " + Screen.width + ", " + Screen.height);
            rect.position = new Vector3(Screen.width/2, Screen.height/2, z);
            p("Not all corners visible: " + count + ", moving to " + rect.position);

        }

    }
    /// <summary>
    /// This method will be called on the start of the mouse drag
    /// </summary>
    /// <param name="eventData">mouse pointer event data</param>
    public void OnBeginDrag(PointerEventData eventData) {
      
        lastMousePosition = eventData.position;
      //  Debug.Log("Begin Drag at "+ lastMousePosition);
    }

    /// <summary>
    /// This method will be called during the mouse drag
    /// </summary>
    /// <param name="eventData">mouse pointer event data</param>
    public void OnDrag(PointerEventData eventData) {
        Vector2 currentMousePosition = eventData.position;
        Vector2 diff = currentMousePosition - lastMousePosition;
        RectTransform rect = GetComponent<RectTransform>();

        Vector3 newPosition = new Vector3(diff.x+rect.position.x, diff.y+rect.position.y, z);
        Vector3 oldPos = rect.position;

        rect.position = newPosition;
        if (IsRectTransformInsideSreen(rect)) {
            rect.position = newPosition;
       //     p("Moving panel to " + newPosition);
            if (_manager == null) _manager = GameObject.FindObjectOfType<GameManager>();
            if (_manager != null) _manager.panelPosition = newPosition;
        }
        else {
            rect.position = oldPos;
       //     p("Moving panel BACK to " + oldPos);
        }
        
        lastMousePosition = currentMousePosition;
    }
    private void p(string s) {
       Debug.Log("PanelDragger: " + s);
    }

    /// <summary>
    /// This method will be called at the end of mouse drag
    /// </summary>
    /// <param name="eventData"></param>
    public void OnEndDrag(PointerEventData eventData) {
      //  Debug.Log("End Drag");
        //Implement your funtionlity here
    }

    /// <summary>
    /// This methods will check is the rect transform is inside the screen or not
    /// </summary>
    /// <param name="rectTransform">Rect Trasform</param>
    /// <returns></returns>
    private bool IsRectTransformInsideSreen(RectTransform rectTransform) {
       
        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);
        int visibleCorners = countInside(rectTransform);
        bool  isInside = visibleCorners > 0;
        return isInside;
    }
    private int countInside(RectTransform rectTransform) {
        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);
        int visibleCorners = 0;
        Rect rect = new Rect(0, 0, Screen.width, Screen.height);
        foreach (Vector3 corner in corners) {
            if (rect.Contains(corner)) {
                visibleCorners++;
            }
        }
       
        return visibleCorners;
    }
}
