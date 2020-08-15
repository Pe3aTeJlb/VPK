using UnityEngine;

public class FloatUiSystemTrigger : MonoBehaviour
{
    private Transform model;
    void Start()
    {
        model = GetComponentInParent<FloatMenuController>().Model;
    }

    void Update()
    {
        transform.localRotation = model.rotation;
    }
}
