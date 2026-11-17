using System;
using GameOff.Day;
using GameOff.Player;
using UnityEngine;
using UnityEngine.AI;

namespace GameOff.Station
{
    /// <summary>
    /// P148: one or two guards walk fixed rounds on the station's NavMesh (hall and motel corridor; the hangar floor), stand at every
    /// waypoint for a few seconds looking around, and say a short line when spoken to ([E]). They are out during the shift and the
    /// evening (curfew too), gone in the morning, at night and while the menu is up. They never stop or check the player (they wait
    /// while the player stands in their way) and have nothing to do with the arrest guard at the counter (P104).
    /// Doors: each guard is a NavMesh agent with a trigger body, so like a traveler (SlidingDoor) a public door opens for him while
    /// staff doors, locked doors and the closed lock doors are carved out of his way. The rounds themselves pass no door.
    /// Cheap: the figures exist only on duty, the Animator skips the bones off screen (CullUpdateTransforms), no avoidance, nothing
    /// allocated per frame.
    /// </summary>
    public class GuardRounds : MonoBehaviour
    {
        /// <summary>One guard's round: waypoints on the floor, walked in order and round again, each with the facing he stands in.</summary>
        [Serializable]
        public class Route
        {
            public string name;
            [Tooltip("Waypoints on the floor, walked in order and round again.")]
            public Vector3[] points = new Vector3[0];
            [Tooltip("Facing while standing at each waypoint (degrees, 0 = north, 90 = east); same order as the points.")]
            public float[] yaws = new float[0];
        }

        /// <summary>The skinned figures cost in the browser; the rounds are made for this many.</summary>
        public const int MaxGuards = 2;
        /// <summary>Closer than this to the waypoint counts as there (m).</summary>
        public const float ArriveMeters = 0.25f;
        private const float GiveWayHysteresis = 0.3f; // walks on only once the player is this much further away again
        private const float SettleSeconds = 0.25f;    // right after a new destination the remaining distance isn't known yet
        // P157 (#48): where the way bends more than this he stops and turns on the spot, and walks on once he faces it again.
        private const float TurnOnSpotDegrees = 50f, WalkOnDegrees = 12f;
        private static readonly int Idle = Animator.StringToHash("Idle");
        private static readonly int Walk = Animator.StringToHash("Walk");

        [SerializeField] private DayManager day;
        [SerializeField] private FirstPersonController player;
        [SerializeField] private GuardTuning tuning;
        [Tooltip("PF_NPC_Guard, the figure of the arrest guard (P104), with Idle and Walk.")]
        [SerializeField] private GameObject guardModel;
        [Tooltip("One guard per route, at most two.")]
        [SerializeField] private Route[] routes = new Route[0];

        private Guard[] _guards;
        private Transform _player;
        private bool _onDuty;
        private int _shownDay; // P155: the day the guards were last placed
        private readonly System.Collections.Generic.HashSet<string> _warned = new System.Collections.Generic.HashSet<string>();

        private class Guard
        {
            public Route Route;
            public GameObject Body;
            public Transform Transform;
            public NavMeshAgent Agent;
            public Animator Animator;
            public int Target;           // the waypoint he walks to or stands at
            public bool Standing;
            public float StandTime, StandSeconds, BaseYaw;
            public float WalkTime, Stuck;
            public bool GivingWay;
            public bool TurningOnSpot;   // P157: stopped, turning toward the way before walking on
            public bool Talking;
            public float TalkUntil;
            public int State;            // animator state last crossfaded to
        }

        /// <summary>Guards are out from the shift to the end of the evening of a running day (not in the menu, the morning or at night).</summary>
        public static bool OnDuty(int day, DayManager.Phase phase) =>
            day >= 1 && (phase == DayManager.Phase.Shift || phase == DayManager.Phase.Evening);

        /// <summary>How long he stands at a waypoint, from a random value 0..1.</summary>
        public static float PauseSeconds(Vector2 range, float random01) => Mathf.Lerp(range.x, range.y, Mathf.Clamp01(random01));

        /// <summary>
        /// Looking around while standing: from the waypoint's facing to one side, over to the other and back, once per pause.
        /// <paramref name="t"/> runs from 0 to <paramref name="seconds"/>.
        /// </summary>
        public static float LookYaw(float baseYaw, float degrees, float t, float seconds) =>
            seconds > 0f ? baseYaw + degrees * Mathf.Sin(2f * Mathf.PI * Mathf.Clamp01(t / seconds)) : baseYaw;

        /// <summary>The line for [E]: from the curfew pool after 23:00 if it has any, else from the normal pool; null if both are empty.</summary>
        public static string PickLine(string[] lines, string[] curfewLines, bool curfew, float random01)
        {
            var pool = curfew && curfewLines != null && curfewLines.Length > 0 ? curfewLines : lines;
            if (pool == null || pool.Length == 0) return null;
            return pool[Mathf.Clamp((int)(random01 * pool.Length), 0, pool.Length - 1)];
        }

        /// <summary>The player stands in his way: close, at about the same height, and within 60° of where he walks.</summary>
        public static bool Blocks(Vector3 guard, Vector3 forward, Vector3 player, float meters)
        {
            var to = player - guard;
            if (Mathf.Abs(to.y) > 1.5f) return false;
            to.y = 0f;
            forward.y = 0f;
            float distance = to.magnitude;
            if (distance > meters) return false;
            if (distance < 0.01f || forward.sqrMagnitude < 1e-6f) return true;
            return Vector3.Dot(forward.normalized, to) > 0.5f * distance;
        }

        private void Awake()
        {
            if (player != null) _player = player.transform;
            if (day == null || tuning == null || guardModel == null)
                Warn("day, tuning or guard model missing - no rounds (GameOff/Build Guard Rounds into Main)");
        }

        private void Update()
        {
            bool duty = day != null && tuning != null && guardModel != null && OnDuty(day.Day, day.CurrentPhase);
            if (duty != _onDuty) SetOnDuty(duty);
            if (!_onDuty || _guards == null) return;
            float dt = Time.deltaTime;
            for (int i = 0; i < _guards.Length; i++) Tick(_guards[i], dt);
        }

        private void SetOnDuty(bool duty)
        {
            _onDuty = duty;
            if (_guards == null && duty) Spawn();
            if (_guards == null) return;
            // P155: a cancelled night (RETURN in the settlement) puts them back where they were; a new day starts them afresh.
            bool newDay = duty && day.Day != _shownDay;
            if (duty) _shownDay = day.Day;
            foreach (var guard in _guards)
            {
                if (!duty) guard.Body.SetActive(false);
                else Show(guard, newDay);
            }
        }

        private void Spawn()
        {
            int count = 0;
            foreach (var route in routes) if (count < MaxGuards && HasPoints(route)) count++;
            _guards = new Guard[count];
            int g = 0;
            foreach (var route in routes)
            {
                if (g >= count) break;
                if (HasPoints(route)) _guards[g++] = Create(route);
            }
        }

        private static bool HasPoints(Route route) => route != null && route.points != null && route.points.Length > 0;

        private Guard Create(Route route)
        {
            var body = Instantiate(guardModel, route.points[0], Quaternion.identity, transform);
            body.SetActive(false);
            body.name = "Guard_" + route.name;
            var guard = new Guard { Route = route, Body = body, Transform = body.transform };

            guard.Animator = body.GetComponent<Animator>();
            if (guard.Animator != null)
            {
                guard.Animator.applyRootMotion = false;
                // Not CullCompletely: a state change made off screen would be lost (ArrivingGroup).
                guard.Animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
            }

            var agent = body.AddComponent<NavMeshAgent>();
            agent.speed = tuning.walkSpeed;
            agent.angularSpeed = 360f;
            agent.acceleration = 6f;
            agent.stoppingDistance = 0.1f;
            agent.autoBraking = true;
            agent.radius = 0.3f; // as baked (STATION.md)
            agent.height = 1.85f;
            agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance; // two guards in two areas: nothing to avoid
            agent.autoRepath = true;
            guard.Agent = agent;

            // A trigger: the player walks through rather than getting stuck in the 2 m motel corridor; [E] hits triggers.
            var capsule = body.AddComponent<CapsuleCollider>();
            capsule.isTrigger = true;
            capsule.center = new Vector3(0f, 0.92f, 0f);
            capsule.height = 1.85f;
            capsule.radius = 0.3f;
            // Kinematic, so the sliding doors' triggers notice him like a traveler (SlidingDoor: NavMeshAgent + collider).
            var physicsBody = body.AddComponent<Rigidbody>();
            physicsBody.isKinematic = true;
            physicsBody.useGravity = false;

            var talk = body.AddComponent<Interactable>();
            talk.SetPrompt(tuning.prompt);
            talk.OnInteract.AddListener(() => Talk(guard));
            return guard;
        }

        /// <summary>
        /// Every time he comes on duty (a new shift, or the evening again after a cancelled night, or a new run): at a random
        /// waypoint of his round, standing and looking around.
        /// </summary>
        private void Show(Guard guard, bool newDay)
        {
            var route = guard.Route;
            Vector3 from = guard.Transform.position;
            if (newDay)
            {
                guard.Target = UnityEngine.Random.Range(0, route.points.Length);
                from = route.points[guard.Target];
                guard.BaseYaw = YawAt(route, guard.Target, 0f);
                guard.Transform.rotation = Quaternion.Euler(0f, guard.BaseYaw, 0f);
                guard.Standing = true;
                guard.StandTime = 0f;
                guard.StandSeconds = PauseSeconds(tuning.pauseSeconds, UnityEngine.Random.value);
            }
            guard.Talking = false;
            guard.GivingWay = false;
            if (!NavMesh.SamplePosition(from, out var hit, 1f, NavMesh.AllAreas))
            {
                Warn($"no NavMesh near {from} for {guard.Body.name} - bake the NavMesh (GameOff/Setup Main Lighting + Bake)");
                return;
            }
            guard.Transform.position = hit.position; // the agent binds to the NavMesh where it is enabled
            guard.Body.SetActive(true);
            if (!guard.Agent.isOnNavMesh && !guard.Agent.Warp(hit.position))
            {
                guard.Body.SetActive(false);
                Warn($"{guard.Body.name} could not be placed on the NavMesh at {hit.position}");
                return;
            }
            guard.State = 0; // switched on again, the Animator starts over in its default state
            if (guard.Animator != null) guard.Animator.speed = 1f; // P157: Turn may have left a slow walk pace
            if (newDay && guard.Animator != null && guard.Animator.isActiveAndEnabled)
            {
                guard.Animator.Play(Idle, 0, UnityEngine.Random.value); // out of step with the other one
                guard.State = Idle;
            }
            if (guard.Standing) Stand(guard);
            else Go(guard);
        }

        private void Tick(Guard guard, float dt)
        {
            var agent = guard.Agent;
            if (!guard.Body.activeSelf || !agent.isOnNavMesh) return;

            if (guard.Talking)
            {
                if (Time.time < guard.TalkUntil)
                {
                    if (_player != null) TurnTo(guard, YawTowards(guard.Transform.position, _player.position, guard.Transform.eulerAngles.y), dt);
                    return;
                }
                guard.Talking = false;
                if (guard.Standing) Stand(guard);
                else Go(guard);
            }

            if (guard.Standing)
            {
                guard.StandTime += dt;
                TurnTo(guard, LookYaw(guard.BaseYaw, tuning.lookAroundDegrees, guard.StandTime, guard.StandSeconds), dt);
                if (guard.StandTime >= guard.StandSeconds)
                {
                    guard.Target = (guard.Target + 1) % guard.Route.points.Length;
                    guard.Standing = false;
                    guard.GivingWay = false;
                    Go(guard);
                }
                return;
            }

            float giveWay = tuning.giveWayMeters + (guard.GivingWay ? GiveWayHysteresis : 0f);
            // Where he wants to walk, not where he faces: after a talk he faces the player, and the player stood "in his way" for good.
            var heading = guard.Agent != null && guard.Agent.hasPath ? guard.Agent.steeringTarget - guard.Transform.position : guard.Transform.forward;
            bool blocked = _player != null && player.ControlEnabled && Blocks(guard.Transform.position, heading, _player.position, giveWay);
            if (blocked != guard.GivingWay)
            {
                guard.GivingWay = blocked;
                agent.isStopped = blocked;
                Play(guard, blocked ? Idle : Walk);
            }
            if (blocked)
            {
                guard.Stuck = 0f;
                return;
            }

            guard.WalkTime += dt;
            if (agent.pathPending) return;
            // Arrival first: right at the waypoint the steering target is centimetres away and its angle means nothing.
            if (guard.WalkTime >= SettleSeconds && (agent.pathStatus == NavMeshPathStatus.PathInvalid || agent.remainingDistance <= ArriveMeters))
            {
                Arrive(guard);
                return;
            }
            if (Turn(guard, dt))
            {
                guard.Stuck = 0f;
                return;
            }
            if (guard.WalkTime < SettleSeconds) return;
            guard.Stuck = agent.velocity.sqrMagnitude < 0.01f ? guard.Stuck + dt : 0f;
            if (guard.Stuck >= tuning.stuckSeconds) Arrive(guard); // stands where he got to, then tries the next waypoint
        }

        private void Arrive(Guard guard)
        {
            guard.Standing = true;
            guard.StandTime = 0f;
            guard.StandSeconds = PauseSeconds(tuning.pauseSeconds, UnityEngine.Random.value);
            guard.BaseYaw = YawAt(guard.Route, guard.Target, guard.Transform.eulerAngles.y);
            Stand(guard);
        }

        /// <summary>Stops the agent; the facing is his own now (looking around, toward the player).</summary>
        private void Stand(Guard guard)
        {
            var agent = guard.Agent;
            if (agent.isOnNavMesh)
            {
                agent.isStopped = true;
                agent.ResetPath();
            }
            agent.updateRotation = false;
            Play(guard, Idle);
        }

        /// <summary>Walks to the current target. P157: he turns himself (Turn): first on the spot toward the way, then along it.</summary>
        private void Go(Guard guard)
        {
            var agent = guard.Agent;
            guard.WalkTime = 0f;
            guard.Stuck = 0f;
            guard.TurningOnSpot = true; // until Turn sees him facing the way
            agent.updateRotation = false;
            if (agent.isOnNavMesh)
            {
                agent.isStopped = true;
                agent.SetDestination(guard.Route.points[guard.Target]);
            }
            Play(guard, Idle);
        }

        /// <summary>
        /// P157 (#48): faces the way himself instead of the agent turning him while it already moves (that slid him sideways round
        /// corners). Where the way bends sharply he stops and turns on the spot; the walk cycle runs at his real speed, so the feet
        /// don't skate while he speeds up or slows down. True while he turns on the spot.
        /// </summary>
        private bool Turn(Guard guard, float dt)
        {
            var agent = guard.Agent;
            float yaw = guard.Transform.eulerAngles.y;
            float want = agent.hasPath ? YawTowards(guard.Transform.position, agent.steeringTarget, yaw) : yaw;
            bool onSpot = TurnsOnSpot(Mathf.DeltaAngle(yaw, want), guard.TurningOnSpot);
            guard.Transform.rotation = Quaternion.RotateTowards(guard.Transform.rotation, Quaternion.Euler(0f, want, 0f),
                tuning.turnDegreesPerSecond * (onSpot ? 1f : 2f) * dt);
            guard.TurningOnSpot = onSpot;
            agent.isStopped = onSpot;
            Play(guard, onSpot ? Idle : Walk);
            if (!onSpot && guard.Animator != null)
                guard.Animator.speed = Mathf.Clamp(agent.velocity.magnitude / Mathf.Max(0.1f, tuning.walkSpeed), 0.4f, 1.2f);
            return onSpot;
        }

        /// <summary>P157: turning on the spot starts above TurnOnSpotDegrees off the way and ends below WalkOnDegrees.</summary>
        public static bool TurnsOnSpot(float degreesOff, bool turningNow) =>
            Mathf.Abs(degreesOff) > (turningNow ? WalkOnDegrees : TurnOnSpotDegrees);

        /// <summary>[E]: he stops, turns to the player and says one line in the hint bar (like the evening guests, P101).</summary>
        private void Talk(Guard guard)
        {
            if (!_onDuty || day == null || day.UI == null) return;
            string line = PickLine(tuning.lines, tuning.curfewLines, day.IsCurfew, UnityEngine.Random.value);
            if (!string.IsNullOrWhiteSpace(line)) day.UI.ShowHint($"{tuning.speakerName}: \"{line}\"", tuning.lineSeconds);
            guard.Talking = true;
            guard.TalkUntil = Time.time + tuning.talkSeconds;
            var agent = guard.Agent;
            if (agent.isOnNavMesh) agent.isStopped = true;
            agent.updateRotation = false;
            Play(guard, Idle);
        }

        private void TurnTo(Guard guard, float yaw, float dt) =>
            guard.Transform.rotation = Quaternion.RotateTowards(guard.Transform.rotation, Quaternion.Euler(0f, yaw, 0f), tuning.turnDegreesPerSecond * dt);

        private static float YawTowards(Vector3 from, Vector3 to, float fallback)
        {
            var d = to - from;
            d.y = 0f;
            return d.sqrMagnitude > 1e-4f ? Mathf.Atan2(d.x, d.z) * Mathf.Rad2Deg : fallback;
        }

        private static float YawAt(Route route, int index, float fallback) =>
            route.yaws != null && index >= 0 && index < route.yaws.Length ? route.yaws[index] : fallback;

        private void Play(Guard guard, int state)
        {
            if (guard.State == state || guard.Animator == null || !guard.Animator.isActiveAndEnabled) return;
            guard.Animator.speed = 1f; // Turn sets the walk cycle's pace while walking
            guard.Animator.CrossFade(state, tuning.animationBlendSeconds);
            guard.State = state;
        }

        private void Warn(string message)
        {
            if (!_warned.Add(message)) return; // each problem once, but every different one
            Debug.LogWarning("[GuardRounds] " + message);
        }
    }
}
