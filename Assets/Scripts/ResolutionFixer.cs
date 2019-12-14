using UnityEngine;

public class ResolutionFixer : MonoBehaviour
{
    public float orthographicSize = 5.441571f;
    public float aspect = 0.5625f;

    void Start()
    {
        // check to see if any adjusting needs to be done
        float windowaspect = (float)Screen.width / (float)Screen.height;

        if (windowaspect < aspect)
        {
            Debug.Log(windowaspect);

            Camera.main.projectionMatrix = Matrix4x4.Ortho(
                -orthographicSize * aspect, orthographicSize * aspect,
                -orthographicSize, orthographicSize,
                GetComponent<Camera>().nearClipPlane, GetComponent<Camera>().farClipPlane);
        }
    }
}
