using UnityEngine;
using UnityEngine.EventSystems;

public class DoorController : MonoBehaviour, IPointerClickHandler
{
    public Vector3 v;
    private Quaternion originQuaternion;
    public bool isOpen = false;
    public float speed;

    public void Start()
    {
        originQuaternion = transform.localRotation;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        isOpen = !isOpen;
    }


    public void Update()
    {

        if (isOpen)
        {
            transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(v), Time.deltaTime * speed);

        }
        else
        {
            transform.localRotation = Quaternion.Lerp(transform.localRotation, originQuaternion, Time.deltaTime * speed);
        }

    }

}
