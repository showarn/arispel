using UnityEngine;
using System.Collections.Generic;

namespace ArisMonsterTrucks
{
    public sealed class MonsterTruckVehicle : MonoBehaviour
    {
        public bool IsPlayer { get; private set; }
        public Rigidbody2D Body { get; private set; }
        public float Progress => Body == null ? 0f : Body.position.x;

        private Rigidbody2D rearWheel;
        private Rigidbody2D frontWheel;
        private WheelJoint2D rearJoint;
        private WheelJoint2D frontJoint;
        private CircleCollider2D rearWheelCollider;
        private CircleCollider2D frontWheelCollider;
        private Collider2D bodyCollider;
        private float requestedThrottle;
        private float lastSafeX;
        private float upsideDownTime;
        private float wheelieAngle;
        private bool controlsEnabled;
        private bool loopRide;
        private bool loopCompleted;
        private float loopRideTime;
        private float loopPreviousAngle;
        private float loopAngleTravel;
        private float loopBodyStartRotation;
        private float loopGuideAngle;
        private RaceDirector director;
        private AudioSource engineAudio;
        private TrailRenderer rainbowTrail;
        private float boostTime;
        private bool loopBoostActive;
        private SpriteRenderer[] visualRenderers;
        private readonly Dictionary<SpriteRenderer, Color> originalColors = new();
        private float visualAlpha = 1f;

        private const float MotorSpeed = 1240f;
        private const float MotorTorque = 1280f;
        private const float LoopCenterX = ColorTrackBuilder.LoopCenterX;
        private const float LoopCenterY = ColorTrackBuilder.LoopCenterY;
        private Vector2 rearWheelOffset;
        private Vector2 frontWheelOffset;

        public static MonsterTruckVehicle Create(
            string objectName,
            Vector2 start,
            bool isPlayer,
            RaceDirector raceDirector
        )
        {
            GameObject root = new(objectName);
            MonsterTruckVehicle vehicle = root.AddComponent<MonsterTruckVehicle>();
            vehicle.IsPlayer = isPlayer;
            vehicle.director = raceDirector;
            vehicle.Build(start);
            return vehicle;
        }

        public void SetControlsEnabled(bool enabled)
        {
            controlsEnabled = enabled;
            if (!enabled)
            {
                requestedThrottle = 0f;
                DisableMotor(rearJoint);
                DisableMotor(frontJoint);
            }
        }

        public void SetThrottle(float value)
        {
            requestedThrottle = Mathf.Clamp01(value);
        }

        public void ActivateBoost(bool isLoopBooster = false)
        {
            loopBoostActive = isLoopBooster;
            boostTime = isLoopBooster ? 5.8f : 2f;

            // Ge hela fordonsriggen samma kontrollerade fart. En stor impuls enbart på
            // karossen belastar WheelJoint2D-lederna och kan skjuta isär bilen.
            float minimumSpeed = isLoopBooster ? 37.5f : 16.5f;
            SetMinimumForwardSpeed(Body, minimumSpeed);
            SetMinimumForwardSpeed(rearWheel, minimumSpeed);
            SetMinimumForwardSpeed(frontWheel, minimumSpeed);

            if (rainbowTrail != null)
            {
                rainbowTrail.Clear();
                rainbowTrail.emitting = true;
            }
            engineAudio?.PlayOneShot(RuntimeArt.BoostSound(), 0.7f);
        }

        public void SetOverlapFade(bool overlapping)
        {
            float targetAlpha = overlapping ? 0.42f : 1f;
            visualAlpha = Mathf.MoveTowards(visualAlpha, targetAlpha, Time.deltaTime * 3.6f);

            if (visualRenderers == null)
            {
                return;
            }

            foreach (SpriteRenderer renderer in visualRenderers)
            {
                if (renderer == null || !originalColors.TryGetValue(renderer, out Color original))
                {
                    continue;
                }

                original.a = visualAlpha;
                renderer.color = original;
            }
        }

        public void Rescue(float trackHeight)
        {
            Vector2 bodyPosition = new(Mathf.Max(-7f, lastSafeX), trackHeight + 3.4f);
            Body.position = bodyPosition;
            Body.rotation = 0f;
            Body.linearVelocity = Vector2.zero;
            Body.angularVelocity = 0f;

            PlaceWheel(rearWheel, bodyPosition + rearWheelOffset);
            PlaceWheel(frontWheel, bodyPosition + frontWheelOffset);
            upsideDownTime = 0f;
            wheelieAngle = 0f;
        }

        public void ParkForFinish()
        {
            requestedThrottle = 0f;
            controlsEnabled = false;
            DisableMotor(rearJoint);
            DisableMotor(frontJoint);
            Body.linearVelocity = Vector2.zero;
            Body.angularVelocity = 0f;
            rearWheel.linearVelocity = Vector2.zero;
            rearWheel.angularVelocity = 0f;
            frontWheel.linearVelocity = Vector2.zero;
            frontWheel.angularVelocity = 0f;
            Body.bodyType = RigidbodyType2D.Kinematic;
            rearWheel.bodyType = RigidbodyType2D.Kinematic;
            frontWheel.bodyType = RigidbodyType2D.Kinematic;
        }

        private void Build(Vector2 start)
        {
            gameObject.name += IsPlayer ? " (DU)" : " (KOMPIS)";
            TruckLayoutData defaults = TruckLayout.CreateDefault();
            TruckPartLayout bodyLayout =
                IsPlayer ? TruckLayout.Get(TruckLayoutPart.Body) : defaults.body;
            TruckPartLayout chassisLayout =
                IsPlayer ? TruckLayout.Get(TruckLayoutPart.Chassis) : defaults.chassis;
            TruckPartLayout rearWheelLayout =
                IsPlayer ? TruckLayout.Get(TruckLayoutPart.RearWheel) : defaults.rearWheel;
            TruckPartLayout frontWheelLayout =
                IsPlayer ? TruckLayout.Get(TruckLayoutPart.FrontWheel) : defaults.frontWheel;
            TruckPartLayout rearSuspensionLayout =
                IsPlayer
                    ? TruckLayout.Get(TruckLayoutPart.RearSuspension)
                    : defaults.rearSuspension;
            TruckPartLayout frontSuspensionLayout =
                IsPlayer
                    ? TruckLayout.Get(TruckLayoutPart.FrontSuspension)
                    : defaults.frontSuspension;

            GameObject bodyObject = new("BodyPhysics");
            bodyObject.transform.SetParent(transform, false);
            bodyObject.transform.position = start;
            Body = bodyObject.AddComponent<Rigidbody2D>();
            Body.mass = 2.8f;
            Body.gravityScale = 3.1f;
            Body.linearDamping = 0.08f;
            Body.angularDamping = 0.45f;
            Body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            Body.interpolation = RigidbodyInterpolation2D.Interpolate;
            Body.centerOfMass = new Vector2(-0.28f, -0.68f);

            BoxCollider2D bodyCollider = bodyObject.AddComponent<BoxCollider2D>();
            bodyCollider.size = new Vector2(4.5f, 1.35f);
            bodyCollider.offset = new Vector2(0f, -0.25f);
            this.bodyCollider = bodyCollider;
            PhysicsMaterial2D bodyMaterial = new("TruckBody")
            {
                friction = 0.3f,
                bounciness = 0.05f
            };
            bodyCollider.sharedMaterial = bodyMaterial;

            const float bodyPixelsPerUnit = 340f;
            Sprite bodySprite = RuntimeArt.LoadSprite(
                IsPlayer
                    ? TruckCustomization.GetSelected(GarageCategory.Body).ResourcePath
                    : "Art/Truck/body_plain",
                bodyPixelsPerUnit
            );
            SpriteRenderer bodyRenderer = CreateSprite(
                "Kaross",
                bodyObject.transform,
                bodySprite,
                Vector3.zero,
                20
            );
            ApplyLayout(
                bodyRenderer.transform,
                bodySprite,
                bodyPixelsPerUnit,
                bodyLayout,
                Vector2.zero
            );
            if (IsPlayer)
            {
                bodyRenderer.color = TruckCustomization.SelectedBodyColor();
                AddSelectedGarageParts(bodyObject.transform);
            }
            else
            {
                bodyRenderer.color = new Color(0.95f, 0.68f, 1f);
            }

            const float chassisPixelsPerUnit = 100f;
            Sprite chassisSprite = RuntimeArt.LoadSprite(
                "Art/Truck/chassis",
                chassisPixelsPerUnit
            );
            SpriteRenderer chassisRenderer = CreateSprite(
                "Chassi",
                bodyObject.transform,
                chassisSprite,
                Vector3.zero,
                10
            );
            ApplyLayout(
                chassisRenderer.transform,
                chassisSprite,
                chassisPixelsPerUnit,
                chassisLayout,
                Vector2.zero
            );

            rearWheelOffset = PreviewPositionToWorld(rearWheelLayout);
            frontWheelOffset = PreviewPositionToWorld(frontWheelLayout);

            const float wheelPixelsPerUnit = 75f;
            Sprite wheelSprite = RuntimeArt.LoadSprite(
                IsPlayer
                    ? TruckCustomization.GetSelected(GarageCategory.Wheels).ResourcePath
                    : "Art/Truck/wheel_glow",
                wheelPixelsPerUnit,
                new Vector2(0.5f, 0.508f)
            );
            rearWheel = CreateWheel(
                "Bakhjul",
                start + rearWheelOffset,
                wheelSprite,
                Body,
                rearWheelOffset,
                PreviewScaleForSprite(rearWheelLayout, wheelSprite, wheelPixelsPerUnit),
                rearWheelLayout.rotation,
                out rearJoint
            );
            frontWheel = CreateWheel(
                "Framhjul",
                start + frontWheelOffset,
                wheelSprite,
                Body,
                frontWheelOffset,
                PreviewScaleForSprite(frontWheelLayout, wheelSprite, wheelPixelsPerUnit),
                frontWheelLayout.rotation,
                out frontJoint
            );
            rearWheelCollider = rearWheel.GetComponent<CircleCollider2D>();
            frontWheelCollider = frontWheel.GetComponent<CircleCollider2D>();
            CreateSuspensionVisual(
                "Bakfjäder",
                bodyObject.transform,
                rearSuspensionLayout
            );
            CreateSuspensionVisual(
                "Framfjäder",
                bodyObject.transform,
                frontSuspensionLayout
            );

            CreateNameBadge(bodyObject.transform);
            SetupEngineAudio(bodyObject);
            SetupRainbowTrail(bodyObject.transform);
            visualRenderers = GetComponentsInChildren<SpriteRenderer>();
            int orderOffset = IsPlayer ? 12 : 0;
            foreach (SpriteRenderer renderer in visualRenderers)
            {
                renderer.sortingOrder += orderOffset;
                originalColors[renderer] = renderer.color;
            }
            lastSafeX = start.x;
        }

        private void AddSelectedGarageParts(Transform bodyTransform)
        {
            TruckPartLayout decalLayout = TruckLayout.Get(TruckLayoutPart.Decal);
            GarageItemDefinition decal = TruckCustomization.GetSelected(GarageCategory.Decals);
            if (!string.IsNullOrEmpty(decal.ResourcePath))
            {
                const float decalPixelsPerUnit = 85f;
                Sprite decalSprite = RuntimeArt.LoadSprite(
                    decal.ResourcePath,
                    decalPixelsPerUnit
                );
                SpriteRenderer decalRenderer = CreateSprite(
                    "Monterad dekal",
                    bodyTransform,
                    decalSprite,
                    new Vector3(0f, 0f, -0.05f),
                    22
                );
                ApplyLayout(
                    decalRenderer.transform,
                    decalSprite,
                    decalPixelsPerUnit,
                    decalLayout,
                    new Vector2(0f, -0.05f)
                );
            }

            foreach (
                GarageItemDefinition accessory in TruckCustomization.GetEquippedAccessories()
            )
            {
                AddAccessory(bodyTransform, accessory);
            }
        }

        private static void AddAccessory(
            Transform bodyTransform,
            GarageItemDefinition accessory
        )
        {
            TruckPartLayout accessoryLayout = TruckLayout.Get(
                TruckLayoutPart.Accessory,
                accessory.Id
            );
            GarageAccessoryMount mount = GarageAccessoryMounts.Get(accessory.Id);

            TruckPartLayout accessoryDefault = TruckLayout.CreateDefault().accessory;
            float accessoryLayoutScale =
                accessoryLayout.width / Mathf.Max(1f, accessoryDefault.width);
            TruckPartLayout effectiveAccessory = new(
                mount.PreviewPosition.x + accessoryLayout.x - accessoryDefault.x,
                mount.PreviewPosition.y + accessoryLayout.y - accessoryDefault.y,
                mount.PreviewSize.x * accessoryLayoutScale,
                mount.PreviewSize.y * accessoryLayoutScale,
                accessoryLayout.rotation
            );
            Sprite accessorySprite = RuntimeArt.LoadSprite(
                accessory.ResourcePath,
                mount.PixelsPerUnit
            );
            SpriteRenderer accessoryRenderer = CreateSprite(
                "Monterat tillbehör",
                bodyTransform,
                accessorySprite,
                Vector3.zero,
                mount.SortingOrder
            );
            ApplyLayout(
                accessoryRenderer.transform,
                accessorySprite,
                mount.PixelsPerUnit,
                effectiveAccessory,
                new Vector2(0f, mount.RuntimeDepth)
            );
            if (mount.MirrorHorizontally)
            {
                Vector3 mirroredScale = accessoryRenderer.transform.localScale;
                mirroredScale.x = -Mathf.Abs(mirroredScale.x);
                accessoryRenderer.transform.localScale = mirroredScale;
            }
            accessoryRenderer.transform.localRotation = Quaternion.Euler(
                0f,
                0f,
                accessoryLayout.rotation
            );
        }

        private void FixedUpdate()
        {
            if (Body == null)
            {
                return;
            }

            float throttle = controlsEnabled ? requestedThrottle : 0f;
            bool boosted = boostTime > 0f;

            if (
                !loopCompleted
                && !loopRide
                && director != null
                && director.TrackHasLoop
                && throttle > 0.05f
                && Body.position.x > 73.2f
                && Body.position.x < 79f
                && Body.position.y < 5.5f
            )
            {
                loopRide = true;
                loopRideTime = 0f;
                loopGuideAngle = -90f;
                Vector2 entryRadial = Body.position - new Vector2(LoopCenterX, LoopCenterY);
                loopPreviousAngle = Mathf.Atan2(entryRadial.y, entryRadial.x) * Mathf.Rad2Deg;
                loopAngleTravel = 0f;
                loopBodyStartRotation = Body.rotation;
                SetLoopGravity(0f);
                director?.EnterLoop(this);
            }

            float drivenThrottle = boosted ? Mathf.Max(0.72f, throttle) : throttle;
            ApplyMotor(rearJoint, drivenThrottle, boosted);
            ApplyMotor(frontJoint, drivenThrottle, boosted);

            if (loopRide)
            {
                ApplyPhysicalLoopAssistance();
            }
            else
            {
                bool grounded = IsGrounded();
                if (grounded)
                {
                    float wheelSlope = Mathf.Atan2(
                        frontWheel.position.y - rearWheel.position.y,
                        frontWheel.position.x - rearWheel.position.x
                    ) * Mathf.Rad2Deg;
                    float speed = Mathf.Max(0f, Body.linearVelocity.x);
                    float flatGround = 1f - Mathf.InverseLerp(4f, 20f, Mathf.Abs(wheelSlope));
                    float highSpeedReduction = 1f - Mathf.InverseLerp(16f, 25f, speed);
                    float requestedWheelie = throttle
                        * flatGround
                        * Mathf.InverseLerp(1.5f, 8f, speed)
                        * highSpeedReduction
                        * 8.5f;
                    wheelieAngle = Mathf.MoveTowards(
                        wheelieAngle,
                        requestedWheelie,
                        Time.fixedDeltaTime * (requestedWheelie > wheelieAngle ? 12f : 24f)
                    );
                    float targetAngle = Mathf.Clamp(wheelSlope, -18f, 23f) + wheelieAngle;
                    float angleError = Mathf.DeltaAngle(Body.rotation, targetAngle);
                    float stabilizingTorque = angleError * 7.2f - Body.angularVelocity * 1.75f;
                    Body.AddTorque(stabilizingTorque, ForceMode2D.Force);

                    float slopeRadians = wheelSlope * Mathf.Deg2Rad;
                    Vector2 uphillTangent = new(
                        Mathf.Cos(slopeRadians),
                        Mathf.Sin(slopeRadians)
                    );
                    float climbAssist = Mathf.InverseLerp(3f, 24f, wheelSlope);
                    Body.AddForce(
                        uphillTangent
                            * throttle
                            * Mathf.Lerp(4f, 15f, climbAssist),
                        ForceMode2D.Force
                    );
                }
                else
                {
                    wheelieAngle = Mathf.MoveTowards(
                        wheelieAngle,
                        0f,
                        Time.fixedDeltaTime * 28f
                    );
                    Body.AddTorque(
                        throttle * 0.22f - Body.angularVelocity * 0.16f,
                        ForceMode2D.Force
                    );
                }

                LimitNormalDrivingVelocity(grounded);
                ClampNormalDrivingRotation();
            }

            Body.angularVelocity = Mathf.Clamp(
                Body.angularVelocity,
                loopRide ? -260f : -145f,
                loopRide ? 260f : 145f
            );
        }

        private void Update()
        {
            if (Body == null)
            {
                return;
            }

            if (boostTime > 0f)
            {
                boostTime -= Time.deltaTime;
                if (boostTime <= 0f && rainbowTrail != null)
                {
                    loopBoostActive = false;
                    rainbowTrail.emitting = false;
                }
            }

            if (!HasFinitePhysicsState())
            {
                float safeHeight = director == null ? 0f : director.TrackHeightAt(lastSafeX);
                Rescue(safeHeight);
                UpdateEngineAudio();
                return;
            }

            float currentTrackHeight = director == null
                ? 0f
                : director.TrackHeightAt(Body.position.x);
            if (
                Body.position.y > currentTrackHeight + 0.4f
                && Body.position.y < currentTrackHeight + 6f
                && Mathf.Abs(Mathf.DeltaAngle(Body.rotation, 0f)) < 50f
            )
            {
                lastSafeX = Mathf.Max(lastSafeX, Body.position.x - 0.5f);
            }

            bool tipped = !loopRide
                && IsGrounded()
                && Mathf.Abs(Mathf.DeltaAngle(Body.rotation, 0f)) > 105f;
            upsideDownTime = tipped ? upsideDownTime + Time.deltaTime : 0f;

            bool fellThroughTrack = !loopRide && Body.position.y < currentTrackHeight - 3.2f;
            bool flewOutOfBounds = !loopRide && Body.position.y > currentTrackHeight + 20f;
            if (fellThroughTrack || flewOutOfBounds || upsideDownTime > 1.8f)
            {
                float height = director == null ? 0f : director.TrackHeightAt(lastSafeX);
                Rescue(height);
            }

            UpdateEngineAudio();
        }

        private void SetupEngineAudio(GameObject bodyObject)
        {
            engineAudio = bodyObject.AddComponent<AudioSource>();
            engineAudio.clip = RuntimeArt.EngineLoop();
            engineAudio.loop = true;
            engineAudio.playOnAwake = false;
            engineAudio.spatialBlend = 0f;
            engineAudio.dopplerLevel = 0f;
            engineAudio.mute = false;
            engineAudio.priority = IsPlayer ? 20 : 180;
            engineAudio.ignoreListenerPause = true;
            engineAudio.pitch = IsPlayer ? 0.82f : 0.86f;
            engineAudio.volume = IsPlayer ? 0.12f : 0.012f;
            engineAudio.Play();
        }

        private void UpdateEngineAudio()
        {
            if (engineAudio == null)
            {
                return;
            }

            float audibleThrottle = controlsEnabled ? requestedThrottle : 0f;
            float targetPitch = Mathf.Lerp(0.82f, 1.42f, audibleThrottle)
                + (boostTime > 0f ? 0.18f : 0f);
            float targetVolume = IsPlayer
                ? Mathf.Lerp(0.12f, 0.29f, audibleThrottle)
                : Mathf.Lerp(0.012f, 0.035f, audibleThrottle);

            engineAudio.pitch = Mathf.MoveTowards(
                engineAudio.pitch,
                targetPitch,
                Time.deltaTime * 1.8f
            );
            engineAudio.volume = Mathf.MoveTowards(
                engineAudio.volume,
                targetVolume,
                Time.deltaTime * 0.45f
            );
        }

        private void SetupRainbowTrail(Transform bodyTransform)
        {
            GameObject trailObject = new("Regnbåge från avgasröret");
            trailObject.transform.SetParent(bodyTransform, false);
            trailObject.transform.localPosition = new Vector3(-2.55f, -0.15f, 0f);

            rainbowTrail = trailObject.AddComponent<TrailRenderer>();
            rainbowTrail.material = RuntimeArt.SpriteMaterial();
            rainbowTrail.time = 1.15f;
            rainbowTrail.minVertexDistance = 0.08f;
            rainbowTrail.startWidth = 0.82f;
            rainbowTrail.endWidth = 0.05f;
            rainbowTrail.numCapVertices = 5;
            rainbowTrail.numCornerVertices = 4;
            rainbowTrail.sortingOrder = 7;
            rainbowTrail.emitting = false;

            Gradient gradient = new();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(RuntimeArt.Hex("#FF3B5C"), 0f),
                    new GradientColorKey(RuntimeArt.Hex("#FFB52E"), 0.18f),
                    new GradientColorKey(RuntimeArt.Hex("#FFE94A"), 0.34f),
                    new GradientColorKey(RuntimeArt.Hex("#52E66D"), 0.5f),
                    new GradientColorKey(RuntimeArt.Hex("#48CFFF"), 0.68f),
                    new GradientColorKey(RuntimeArt.Hex("#5868FF"), 0.84f),
                    new GradientColorKey(RuntimeArt.Hex("#D45CFF"), 1f)
                },
                new[]
                {
                    new GradientAlphaKey(0.95f, 0f),
                    new GradientAlphaKey(0.82f, 0.62f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            rainbowTrail.colorGradient = gradient;
        }

        private void ApplyPhysicalLoopAssistance()
        {
            loopRideTime += Time.fixedDeltaTime;
            Vector2 center = new(LoopCenterX, LoopCenterY);
            const float degreesPerSecond = 430f;
            const float bodyOrbitRadius = 5.05f;
            loopGuideAngle += degreesPerSecond * Time.fixedDeltaTime;
            float radians = loopGuideAngle * Mathf.Deg2Rad;
            Vector2 radialDirection = new(Mathf.Cos(radians), Mathf.Sin(radians));
            Vector2 tangent = new(-radialDirection.y, radialDirection.x);
            Vector2 bodyTarget = center + radialDirection * bodyOrbitRadius;
            float bodyAngle = loopGuideAngle + 90f;

            Body.position = bodyTarget;
            Body.rotation = bodyAngle;
            Body.linearVelocity = tangent * 37f;
            Body.angularVelocity = degreesPerSecond;

            Vector2 rearOffset = RotateOffset(rearWheelOffset, bodyAngle);
            Vector2 frontOffset = RotateOffset(frontWheelOffset, bodyAngle);
            rearWheel.position = bodyTarget + rearOffset;
            frontWheel.position = bodyTarget + frontOffset;
            rearWheel.linearVelocity = tangent * 37f;
            frontWheel.linearVelocity = tangent * 37f;
            rearWheel.angularVelocity = -820f;
            frontWheel.angularVelocity = -820f;

            if (loopGuideAngle >= 270f)
            {
                CompleteGuidedLoop();
            }
        }

        private void CompleteGuidedLoop()
        {
            loopRide = false;
            loopCompleted = true;
            SetLoopGravity(3.1f);
            Vector2 exitBodyPosition = new(81.5f, 3.05f);
            Body.position = exitBodyPosition;
            Body.rotation = 0f;
            PlaceWheel(rearWheel, exitBodyPosition + rearWheelOffset);
            PlaceWheel(frontWheel, exitBodyPosition + frontWheelOffset);
            SetMinimumForwardSpeed(Body, 30f);
            SetMinimumForwardSpeed(rearWheel, 30f);
            SetMinimumForwardSpeed(frontWheel, 30f);
            director?.ExitLoop(this);
            StopBoost();
        }

        private void SetLoopGravity(float value)
        {
            Body.gravityScale = value;
            rearWheel.gravityScale = value;
            frontWheel.gravityScale = value;
        }

        private static Vector2 RotateOffset(Vector2 offset, float degrees)
        {
            float radians = degrees * Mathf.Deg2Rad;
            float cosine = Mathf.Cos(radians);
            float sine = Mathf.Sin(radians);
            return new Vector2(
                offset.x * cosine - offset.y * sine,
                offset.x * sine + offset.y * cosine
            );
        }

        private void StopBoost()
        {
            boostTime = 0f;
            loopBoostActive = false;
            if (rainbowTrail != null)
            {
                rainbowTrail.emitting = false;
            }
        }

        private static void ApplySpeedBasedLoopGrip(
            Rigidbody2D rigidbody,
            Vector2 center,
            Vector2 referenceTangent
        )
        {
            Vector2 radial = rigidbody.position - center;
            float radius = radial.magnitude;
            if (radius < 0.01f)
            {
                return;
            }

            float tangentSpeed = Mathf.Abs(Vector2.Dot(rigidbody.linearVelocity, referenceTangent));
            float centripetalAcceleration = tangentSpeed * tangentSpeed / Mathf.Max(4.8f, radius);
            float gripFactor = Mathf.InverseLerp(4.5f, 10.5f, tangentSpeed);
            Vector2 inward = -radial.normalized;
            rigidbody.AddForce(
                inward * rigidbody.mass * centripetalAcceleration * gripFactor * 0.62f,
                ForceMode2D.Force
            );

            if (rigidbody != null && rigidbody.mass < 1f)
            {
                const float wheelContactRadius = ColorTrackBuilder.LoopRadius - 1.12f;
                float contactError = Mathf.Clamp(
                    wheelContactRadius - radius,
                    -1.35f,
                    1.35f
                );
                rigidbody.AddForce(
                    radial.normalized
                        * contactError
                        * 285f
                        * Mathf.Lerp(0.45f, 1f, gripFactor),
                    ForceMode2D.Force
                );
            }
        }

        private void ClampNormalDrivingRotation()
        {
            float signedAngle = Mathf.DeltaAngle(0f, Body.rotation);
            float clampedAngle = Mathf.Clamp(signedAngle, -20f, 29f);

            if (Mathf.Approximately(signedAngle, clampedAngle))
            {
                return;
            }

            Body.rotation = clampedAngle;
            if (
                (clampedAngle >= 29f && Body.angularVelocity > 0f)
                || (clampedAngle <= -20f && Body.angularVelocity < 0f)
            )
            {
                Body.angularVelocity = 0f;
            }
        }

        private static void SetMinimumForwardSpeed(Rigidbody2D rigidbody, float minimumSpeed)
        {
            if (rigidbody == null)
            {
                return;
            }

            Vector2 velocity = rigidbody.linearVelocity;
            velocity.x = Mathf.Max(velocity.x, minimumSpeed);
            velocity.y = Mathf.Clamp(velocity.y, -5f, 8f);
            rigidbody.linearVelocity = velocity;
        }

        private void LimitNormalDrivingVelocity(bool grounded)
        {
            Vector2 bodyVelocity = Body.linearVelocity;
            float maximumForwardSpeed = loopBoostActive
                ? 39f
                : boostTime > 0f
                    ? 27f
                    : 23.5f;
            bodyVelocity.x = Mathf.Clamp(bodyVelocity.x, -7f, maximumForwardSpeed);
            bodyVelocity.y = Mathf.Clamp(bodyVelocity.y, grounded ? -12f : -17f, 16f);
            Body.linearVelocity = bodyVelocity;

            LimitWheelTranslation(rearWheel);
            LimitWheelTranslation(frontWheel);
        }

        private void LimitWheelTranslation(Rigidbody2D wheel)
        {
            if (wheel == null)
            {
                return;
            }

            Vector2 velocity = wheel.linearVelocity;
            float maximumForwardSpeed = loopBoostActive
                ? 40f
                : boostTime > 0f
                    ? 28f
                    : 24.5f;
            velocity.x = Mathf.Clamp(velocity.x, -8f, maximumForwardSpeed);
            velocity.y = Mathf.Clamp(velocity.y, -18f, 17f);
            wheel.linearVelocity = velocity;
        }

        private bool HasFinitePhysicsState()
        {
            return IsFinite(Body.position)
                && IsFinite(Body.linearVelocity)
                && IsFinite(Body.rotation)
                && IsFinite(Body.angularVelocity)
                && IsFinite(rearWheel.position)
                && IsFinite(frontWheel.position);
        }

        private static bool IsFinite(Vector2 value)
        {
            return IsFinite(value.x) && IsFinite(value.y);
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }

        private Rigidbody2D CreateWheel(
            string wheelName,
            Vector2 worldPosition,
            Sprite sprite,
            Rigidbody2D connectedBody,
            Vector2 connectedAnchor,
            float visualAndColliderScale,
            float startRotation,
            out WheelJoint2D joint
        )
        {
            GameObject wheelObject = new(wheelName);
            wheelObject.transform.SetParent(transform, true);
            wheelObject.transform.position = worldPosition;

            SpriteRenderer renderer = wheelObject.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = 30;
            wheelObject.transform.localScale = Vector3.one * visualAndColliderScale;
            wheelObject.transform.rotation = Quaternion.Euler(0f, 0f, startRotation);

            Rigidbody2D wheelBody = wheelObject.AddComponent<Rigidbody2D>();
            wheelBody.mass = 0.65f;
            wheelBody.gravityScale = 3.1f;
            wheelBody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            wheelBody.interpolation = RigidbodyInterpolation2D.Interpolate;

            CircleCollider2D collider = wheelObject.AddComponent<CircleCollider2D>();
            collider.radius = 1.05f;
            collider.sharedMaterial = new PhysicsMaterial2D("MonsterGrip")
            {
                friction = 1.48f,
                bounciness = 0.03f
            };

            joint = wheelObject.AddComponent<WheelJoint2D>();
            joint.connectedBody = connectedBody;
            joint.autoConfigureConnectedAnchor = false;
            joint.anchor = Vector2.zero;
            joint.connectedAnchor = connectedAnchor;

            JointSuspension2D suspension = joint.suspension;
            suspension.angle = 90f;
            suspension.frequency = 7.1f;
            suspension.dampingRatio = 0.82f;
            joint.suspension = suspension;

            DistanceJoint2D travelLimiter = wheelObject.AddComponent<DistanceJoint2D>();
            travelLimiter.connectedBody = connectedBody;
            travelLimiter.autoConfigureConnectedAnchor = false;
            travelLimiter.anchor = Vector2.zero;
            travelLimiter.connectedAnchor = connectedAnchor;
            travelLimiter.distance = 0.46f;
            travelLimiter.maxDistanceOnly = true;
            travelLimiter.enableCollision = false;
            return wheelBody;
        }

        private static void ApplyLayout(
            Transform target,
            Sprite sprite,
            float pixelsPerUnit,
            TruckPartLayout value,
            Vector2 depth
        )
        {
            Vector2 position = PreviewPositionToWorld(value);
            target.localPosition = new Vector3(
                position.x + depth.x,
                position.y,
                depth.y
            );
            float scale = PreviewScaleForSprite(value, sprite, pixelsPerUnit);
            target.localScale = new Vector3(scale, scale, 1f);
            target.localRotation = Quaternion.Euler(0f, 0f, value.rotation);
        }

        private static Vector2 PreviewPositionToWorld(TruckPartLayout value)
        {
            return new Vector2(
                value.x / TruckLayout.PreviewUnitsPerWorldUnit,
                value.y / TruckLayout.PreviewUnitsPerWorldUnit
                    + TruckLayout.PreviewWorldOriginY
            );
        }

        private static float PreviewScaleForSprite(
            TruckPartLayout value,
            Sprite sprite,
            float pixelsPerUnit
        )
        {
            if (sprite == null)
            {
                return 1f;
            }

            float uiPixelsPerTexturePixel = Mathf.Min(
                value.width / Mathf.Max(1f, sprite.rect.width),
                value.height / Mathf.Max(1f, sprite.rect.height)
            );
            return Mathf.Clamp(
                uiPixelsPerTexturePixel
                    * pixelsPerUnit
                    / TruckLayout.PreviewUnitsPerWorldUnit,
                0.2f,
                3f
            );
        }

        private bool IsGrounded()
        {
            return (rearWheelCollider != null && rearWheelCollider.IsTouchingLayers())
                || (frontWheelCollider != null && frontWheelCollider.IsTouchingLayers())
                || (bodyCollider != null && bodyCollider.IsTouchingLayers());
        }

        private void CreateSuspensionVisual(
            string objectName,
            Transform parent,
            TruckPartLayout layout
        )
        {
            GameObject springObject = new(objectName);
            springObject.transform.SetParent(parent, false);
            SpriteRenderer renderer = springObject.AddComponent<SpriteRenderer>();
            const float pixelsPerUnit = 105f;
            renderer.sprite = RuntimeArt.LoadSprite(
                "Art/Truck/suspension_spring",
                pixelsPerUnit
            );
            renderer.sortingOrder = 15;
            ApplyLayout(
                springObject.transform,
                renderer.sprite,
                pixelsPerUnit,
                layout,
                Vector2.zero
            );
        }

        private void CreateNameBadge(Transform parent)
        {
            GameObject badge = new(IsPlayer ? "DU-märke" : "KOMPIS-märke");
            badge.transform.SetParent(parent, false);
            badge.transform.localPosition = new Vector3(0f, 2.1f, 0f);

            string badgeText = IsPlayer && !string.IsNullOrEmpty(PlayerProfile.Username)
                ? PlayerProfile.Username.ToUpperInvariant()
                : IsPlayer ? "DU" : "KOMPIS";
            bool longName = badgeText.Length > 9;
            SpriteRenderer bubble = badge.AddComponent<SpriteRenderer>();
            bubble.sprite = RuntimeArt.RoundedRectangleSprite(
                IsPlayer ? "PlayerBadge" : "NpcBadge",
                RuntimeArt.Hex("#3B226E"),
                IsPlayer ? RuntimeArt.Hex("#FFD84A") : RuntimeArt.Hex("#F58BFF"),
                IsPlayer ? 310 : 200,
                86,
                28,
                8
            );
            bubble.sortingOrder = 50;
            badge.transform.localScale = new Vector3(
                IsPlayer ? 0.6f : 0.55f,
                IsPlayer ? 0.6f : 0.55f,
                1f
            );

            GameObject labelObject = new("Text");
            labelObject.transform.SetParent(badge.transform, false);
            labelObject.transform.localPosition = new Vector3(0f, 0f, -0.1f);
            TextMesh label = labelObject.AddComponent<TextMesh>();
            label.text = badgeText;
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.fontSize = IsPlayer ? (longName ? 27 : 34) : 30;
            label.characterSize = 0.08f;
            label.color = RuntimeArt.Hex("#3B226E");
            label.fontStyle = FontStyle.Bold;
            MeshRenderer meshRenderer = label.GetComponent<MeshRenderer>();
            meshRenderer.sortingOrder = 51;
        }

        private static SpriteRenderer CreateSprite(
            string objectName,
            Transform parent,
            Sprite sprite,
            Vector3 localPosition,
            int sortingOrder
        )
        {
            GameObject child = new(objectName);
            child.transform.SetParent(parent, false);
            child.transform.localPosition = localPosition;
            SpriteRenderer renderer = child.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = sortingOrder;
            return renderer;
        }

        private static void ApplyMotor(WheelJoint2D joint, float throttle, bool boosted)
        {
            if (joint == null)
            {
                return;
            }

            if (throttle <= 0.01f)
            {
                joint.useMotor = false;
                return;
            }

            JointMotor2D motor = joint.motor;
            motor.motorSpeed = (boosted ? MotorSpeed * 2.05f : MotorSpeed) * throttle;
            motor.maxMotorTorque = boosted ? MotorTorque * 2f : MotorTorque;
            joint.motor = motor;
            joint.useMotor = true;
        }

        private static void DisableMotor(WheelJoint2D joint)
        {
            if (joint != null)
            {
                joint.useMotor = false;
            }
        }

        private static void PlaceWheel(Rigidbody2D wheel, Vector2 position)
        {
            wheel.position = position;
            wheel.rotation = 0f;
            wheel.linearVelocity = Vector2.zero;
            wheel.angularVelocity = 0f;
        }

    }
}
