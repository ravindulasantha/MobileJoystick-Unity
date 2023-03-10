using UnityEngine;
using UnityEngine.EventSystems;

public class Joystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [SerializeField]
    private JoystickType joystickType = JoystickType.Fixed;

    [SerializeField]
    private AxisOptions axisOptions = AxisOptions.Both;

    [SerializeField]
    private float handleRange = 1;

    [SerializeField]
    private float deadZone = 0;

    [SerializeField]
    private RectTransform background;

    [SerializeField]
    private RectTransform handle;

    [HideInInspector]
    public Vector2 input;

    public enum JoystickType
    {
        Fixed,
        Floating,
        Dynamic
    }
    public enum AxisOptions
    {
        Both,
        Horizontal,
        Vertical
    }

    private RectTransform baseRect;
    private Canvas canvas;
    private Camera cam;


    private void Start()
    {
        GetAllComponents();

        SetJoystickMode(joystickType);
    }

    private void GetAllComponents()
    {
        //Gets all the needed components
        baseRect = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
    }

    public void SetJoystickMode(JoystickType joystickType)
    {
        //Sets the joystick mode and activates/deactivates the visuals depending on the mode
        this.joystickType = joystickType;

        if (joystickType == JoystickType.Fixed)
            background.gameObject.SetActive(true);

        else
            background.gameObject.SetActive(false);
    }


    public void OnPointerDown(PointerEventData eventData)
    {
        //If the joystick type is not fixed, moves the joystick to where the player clicks/touches the screen
        if (joystickType != JoystickType.Fixed)
        {
            background.gameObject.SetActive(true);
            background.anchoredPosition = MoveJoystickToClick(eventData.position);
        }

        //Updates the joystick if the player clicks/touches the joystick
        OnDrag(eventData);
    }

    private Vector2 MoveJoystickToClick(Vector2 screenPosition)
    {
        //Moves the non fixed joystick to where the player clicks/touches the screen
        Vector2 localPoint = Vector2.zero;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(baseRect, screenPosition, cam, out localPoint))
        {
            Vector2 pivotOffset = baseRect.pivot * baseRect.sizeDelta;
            return localPoint - (background.anchorMax * baseRect.sizeDelta) + pivotOffset;
        }
        return Vector2.zero;
    }

    public void OnDrag(PointerEventData eventData)
    {
        //Calculates the inputs and moves the handle depending on the inputs
        cam = null;
        if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
            cam = canvas.worldCamera;

        Vector2 position = RectTransformUtility.WorldToScreenPoint(cam, background.position);
        Vector2 radius = background.sizeDelta / 2;
        input = (eventData.position - position) / (radius * canvas.scaleFactor);

        RestrictInput();
        MoveDynamicJoystick(input.magnitude, input.normalized, radius);
        UpdateInput(input.magnitude, input.normalized);

        handle.anchoredPosition = input * radius * handleRange;
    }

    private void RestrictInput()
    {
        //Restricts the handle movement and the inputs if the joystick is not set to both directions
        if (axisOptions == AxisOptions.Horizontal)
            input = new Vector2(input.x, 0f);

        else if (axisOptions == AxisOptions.Vertical)
            input = new Vector2(0f, input.y);
    }

    private void UpdateInput(float magnitude, Vector2 normalised)
    {
        //Updates the inputs if the handle is dragged more than the dead zone
        if (magnitude > deadZone)
        {
            if (magnitude > 1)
                input = normalised;
        }
        else
        {
            input = Vector2.zero;
        }
    }

    private void MoveDynamicJoystick(float magnitude, Vector2 normalised, Vector2 radius)
    {
        //Moves the joystick if it is set to dynamic and if the handle is dragged at maximum
        if (joystickType == JoystickType.Dynamic && magnitude > handleRange)
        {
            Vector2 difference = normalised * (magnitude - handleRange) * radius;
            background.anchoredPosition += difference;
        }
    }

    public virtual void OnPointerUp(PointerEventData eventData)
    {
        //Deactivates the joystick visuals if its type is not fixed
        if (joystickType != JoystickType.Fixed)
            background.gameObject.SetActive(false);


        //Resets the inputs and the position of the handle
        input = Vector2.zero;
        handle.anchoredPosition = Vector2.zero;
    }
}
