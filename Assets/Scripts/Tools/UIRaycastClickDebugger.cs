using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIRaycastClickDebugger : MonoBehaviour
{
    public GraphicRaycaster uiRaycaster; // El GraphicRaycaster que detectará los clics en la UI
    public EventSystem eventSystem; // El EventSystem que manejará los eventos de entrada

    void Start()
    {
        // Obtener los componentes si no están asignados
        if (uiRaycaster == null)
        {
            uiRaycaster = GetComponent<GraphicRaycaster>();
        }

        if (eventSystem == null)
        {
            eventSystem = EventSystem.current;
        }
    }

    void Update()
    {
        // Detectar si se ha hecho clic con el botón izquierdo del ratón
        if (Input.GetMouseButtonDown(0))
        {
            // Crear una lista de resultados para almacenar los elementos que intersecta el raycast
            PointerEventData pointerEventData = new PointerEventData(eventSystem);
            pointerEventData.position = Input.mousePosition;

            List<RaycastResult> results = new List<RaycastResult>();

            // Hacer el raycast en la UI
            uiRaycaster.Raycast(pointerEventData, results);

            // Verificar si ha colisionado con algún elemento de la UI
            if (results.Count > 0)
            {
                // Mostrar en consola el nombre del primer objeto de UI con el que ha colisionado
                Debug.Log("Colisionaste con el objeto UI: " + results[0].gameObject.name);
            }
        }
    }
}
