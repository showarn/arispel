using UnityEngine;

namespace MonsterTruckGame.Vehicle;

[DisallowMultipleComponent]
public sealed class MonsterTruckVisualCustomizer : MonoBehaviour
{
    [Header("Required renderers")]
    [SerializeField] private SpriteRenderer bodyRenderer = null!;
    [SerializeField] private SpriteRenderer chassisRenderer = null!;
    [SerializeField] private SpriteRenderer rearWheelRenderer = null!;
    [SerializeField] private SpriteRenderer frontWheelRenderer = null!;

    [Header("Optional renderers")]
    [SerializeField] private SpriteRenderer? decalRenderer;
    [SerializeField] private SpriteRenderer? frontBumperRenderer;
    [SerializeField] private SpriteRenderer? rearBumperRenderer;
    [SerializeField] private SpriteRenderer? roofPartRenderer;

    public void SetBody(Sprite sprite)
    {
        bodyRenderer.sprite = sprite;
    }

    public void SetChassis(Sprite sprite)
    {
        chassisRenderer.sprite = sprite;
    }

    public void SetWheels(Sprite sprite)
    {
        rearWheelRenderer.sprite = sprite;
        frontWheelRenderer.sprite = sprite;
    }

    public void SetDecal(Sprite? sprite)
    {
        ApplyOptionalSprite(decalRenderer, sprite);
    }

    public void SetFrontBumper(Sprite? sprite)
    {
        ApplyOptionalSprite(frontBumperRenderer, sprite);
    }

    public void SetRearBumper(Sprite? sprite)
    {
        ApplyOptionalSprite(rearBumperRenderer, sprite);
    }

    public void SetRoofPart(Sprite? sprite)
    {
        ApplyOptionalSprite(roofPartRenderer, sprite);
    }

    private static void ApplyOptionalSprite(
        SpriteRenderer? renderer,
        Sprite? sprite
    )
    {
        if (renderer == null)
        {
            return;
        }

        renderer.sprite = sprite;
        renderer.enabled = sprite != null;
    }
}
