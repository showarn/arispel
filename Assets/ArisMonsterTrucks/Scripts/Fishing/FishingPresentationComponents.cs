using UnityEngine;
using UnityEngine.UI;

namespace ArisMonsterTrucks.Fishing
{
    public sealed class SwimmingFishView : MonoBehaviour
    {
        private RectTransform rect;
        private RectTransform waterArea;
        private float speed;
        private float verticalCenter;
        private float phase;
        private bool movingRight;

        public void Initialize(
            RectTransform area,
            Sprite sprite,
            float swimSpeed,
            float startX,
            float startY,
            bool goesRight
        )
        {
            rect = GetComponent<RectTransform>();
            waterArea = area;
            speed = Mathf.Max(22f, swimSpeed * 58f);
            verticalCenter = startY;
            phase = Random.Range(0f, Mathf.PI * 2f);
            movingRight = goesRight;
            rect.anchoredPosition = new Vector2(startX, startY);
            Image image = GetComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = true;
            rect.localScale = new Vector3(goesRight ? -1f : 1f, 1f, 1f);
        }

        public void SetFish(Sprite sprite, float swimSpeed)
        {
            Image image = GetComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = true;
            speed = Mathf.Max(22f, swimSpeed * 58f);
        }

        private void Update()
        {
            if (rect == null || waterArea == null)
            {
                return;
            }

            float delta = Time.unscaledDeltaTime;
            Vector2 position = rect.anchoredPosition;
            position.x += (movingRight ? 1f : -1f) * speed * delta;
            position.y = verticalCenter
                + Mathf.Sin(Time.unscaledTime * 0.75f + phase) * 22f;

            float halfWidth = waterArea.rect.width * 0.5f;
            if (movingRight && position.x > halfWidth + 150f)
            {
                position.x = -halfWidth - 150f;
                verticalCenter = Random.Range(-430f, -225f);
            }
            else if (!movingRight && position.x < -halfWidth - 150f)
            {
                position.x = halfWidth + 150f;
                verticalCenter = Random.Range(-430f, -225f);
            }
            rect.anchoredPosition = position;
        }
    }

    public sealed class FishingConfettiParticle : MonoBehaviour
    {
        private RectTransform rect;
        private Graphic graphic;
        private Vector2 velocity;
        private float remaining;
        private float duration;

        public void Launch(Vector2 origin, Vector2 initialVelocity, Color color)
        {
            rect ??= GetComponent<RectTransform>();
            graphic ??= GetComponent<Graphic>();
            rect.anchoredPosition = origin;
            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;
            velocity = initialVelocity;
            duration = Random.Range(0.9f, 1.4f);
            remaining = duration;
            graphic.color = color;
            gameObject.SetActive(true);
        }

        private void Update()
        {
            float delta = Time.unscaledDeltaTime;
            remaining -= delta;
            rect.anchoredPosition += velocity * delta;
            velocity += Vector2.down * 500f * delta;
            rect.Rotate(0f, 0f, 220f * delta);
            Color color = graphic.color;
            color.a = Mathf.Clamp01(remaining / 0.35f);
            graphic.color = color;
            if (remaining <= 0f)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
