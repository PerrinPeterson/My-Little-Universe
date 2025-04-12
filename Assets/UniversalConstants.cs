using UnityEngine;

[ExecuteInEditMode]
public class UniversalConstants : MonoBehaviour
{
    public float GravitationalConstant = 0.000674f;




    private void OnValidate()
    {
        //Call the OnValidate function of all gravity objects
        gravityObject[] gravityObjects = FindObjectsByType<gravityObject>(FindObjectsSortMode.None);
        foreach (var gravityObject in gravityObjects)
        {
            gravityObject.OnValidate();
        }

    }
}
