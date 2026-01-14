using UnityEngine;

public class PositionInVR : MonoBehaviour
{
    public Vector3 position = new Vector3(-1.5f, 1.5f, 2f);
    public Vector3 rotation = new Vector3(0f, 15f, 0f);

    private void Start()
    {
        transform.localPosition = position;
        transform.localEulerAngles = rotation;
    }
}
