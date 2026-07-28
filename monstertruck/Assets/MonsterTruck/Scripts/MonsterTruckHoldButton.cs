using UnityEngine;
using UnityEngine.EventSystems;

namespace MonsterTruckGame.Vehicle;

public enum MonsterTruckButtonAction
{
    Forward,
    Reverse,
    Brake,
    RotateLeft,
    RotateRight
}

[DisallowMultipleComponent]
public sealed class MonsterTruckHoldButton :
    MonoBehaviour,
    IPointerDownHandler,
    IPointerUpHandler,
    IPointerExitHandler
{
    [SerializeField] private MonsterTruckController2D controller = null!;
    [SerializeField] private MonsterTruckButtonAction action;

    public void OnPointerDown(PointerEventData eventData)
    {
        SetPressedState(true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        SetPressedState(false);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetPressedState(false);
    }

    private void OnDisable()
    {
        SetPressedState(false);
    }

    private void SetPressedState(bool pressed)
    {
        switch (action)
        {
            case MonsterTruckButtonAction.Forward:
                controller.SetThrottle(pressed ? 1f : 0f);
                break;

            case MonsterTruckButtonAction.Reverse:
                controller.SetThrottle(pressed ? -1f : 0f);
                break;

            case MonsterTruckButtonAction.Brake:
                controller.SetBrake(pressed);
                break;

            case MonsterTruckButtonAction.RotateLeft:
                controller.SetAirControl(pressed ? -1f : 0f);
                break;

            case MonsterTruckButtonAction.RotateRight:
                controller.SetAirControl(pressed ? 1f : 0f);
                break;

            default:
                Debug.LogError(
                    $"Unsupported button action: {action}",
                    this
                );
                break;
        }
    }
}
