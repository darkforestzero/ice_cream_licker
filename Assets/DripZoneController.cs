using UnityEngine;

public class DripZoneController : MonoBehaviour
{
    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // For mouse clicks or touch on the screen
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit)) // For 3D objects
            {
                if (hit.transform.IsChildOf(transform)) // Check if the hit object is a child of this GameObject
                {
                    Debug.Log(hit.transform.name + " was tapped!");
                    // Here you can call a method or trigger an event based on the child tapped
                    // Example: hit.transform.GetComponent<YourChildScript>()?.YourMethod();
                }
            }
        }
    }
}