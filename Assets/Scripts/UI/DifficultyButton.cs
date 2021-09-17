using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class DifficultyButton : MonoBehaviour, IPointerClickHandler
{
    public UnityEvent onClick;

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        Debug.Log(name + " difficulty selected", this);

        onClick.Invoke();

        transform.GetChild(0).transform.gameObject.SetActive(true);

        // activates the "selected" sprite on this button and deactivates on the rest
        for (int i = 0; i < transform.parent.childCount; i++)
        {
            if (transform.parent.GetChild(i) == transform)
                transform.GetChild(0).transform.gameObject.SetActive(true);
            else
                transform.parent.GetChild(i).transform.GetChild(0).gameObject.SetActive(false);
        }
    }
}
