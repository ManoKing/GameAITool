using System;
using Unity.LEGO.UI;
using UnityEngine;

namespace Unity.LEGO.Behaviours.Triggers
{
    public class InputTrigger : SensoryTrigger
    {
        public enum Type
        {
            Up,
            Left,
            Down,
            Right,
            Jump,
            Fire1,
            Fire2,
            Fire3,
            OtherKey,
            AnyKey
        }

        [SerializeField, Tooltip("The input to detect.")]
        Type m_Type = Type.OtherKey;

        enum Key
        {
            A = KeyCode.A,
            B = KeyCode.B,
            C = KeyCode.C,
            D = KeyCode.D,
            E = KeyCode.E,
            F = KeyCode.F,
            G = KeyCode.G,
            H = KeyCode.H,
            I = KeyCode.I,
            J = KeyCode.J,
            K = KeyCode.K,
            L = KeyCode.L,
            M = KeyCode.M,
            N = KeyCode.N,
            O = KeyCode.O,
            P = KeyCode.P,
            Q = KeyCode.Q,
            R = KeyCode.R,
            S = KeyCode.S,
            T = KeyCode.T,
            U = KeyCode.U,
            V = KeyCode.V,
            W = KeyCode.W,
            X = KeyCode.X,
            Y = KeyCode.Y,
            Z = KeyCode.Z,
        }

        [SerializeField, Tooltip("The key to detect.")]
        Key m_OtherKey = Key.E;

        public enum Trigger
        {
            WhenPressed,
            WhenReleased,
            WhilePressed,
            WhileReleased
        }

        [SerializeField, Tooltip("Trigger on input pressed.\nor\nOn input released.\nor\nWhile input pressed.\nor\nWhile input released.")]
        Trigger m_Trigger = Trigger.WhenPressed;

        public enum Enable
        {
            Always,
            WhenPlayerIsNearby,
            WhenBricksAreNearby,
            WhenTagIsNearby
        }

        [SerializeField, Tooltip("Always enable input.\nor\nEnable input when the player is nearby.\nor\nEnable input when any brick is nearby.\nor\nEnable when a tag is nearby.")]
        Enable m_Enable = Enable.WhenPlayerIsNearby;

        [SerializeField, Tooltip("The distance in LEGO modules.")]
        int m_Distance = 20;

        [SerializeField, Tooltip("Show input prompt.")]
        bool m_ShowPrompt = true;

        [SerializeField]
        GameObject m_InputPromptPrefab = default;

        InputPrompt m_InputPrompt;
        bool m_PromptActive = true;
        string m_PromptLabel;

        enum Direction
        {
            Positive,
            Negative
        }

        const float k_AxisDeadzone = 0.05f; // Axis value must be above this threshold to be detected.
        bool m_InputHeld;
        int m_ConditionMetCount;

        protected override void Reset()
        {
            base.Reset();

            m_IconPath = "Assets/LEGO/Gizmos/LEGO Behaviour Icons/Input Trigger.png";
        }

        protected void OnValidate()
        {
            m_Distance = Mathf.Max(1, m_Distance);
        }

        protected override void Start()
        {
            base.Start();

            if (m_Type == Type.AnyKey)
            {
                m_ShowPrompt = false;
            }

            if (IsPlacedOnBrick())
            {
                if (m_Enable != Enable.Always)
                {
                    // Apply the correct sensing, given when to enable the input.
                    switch(m_Enable)
                    {
                        case Enable.WhenPlayerIsNearby:
                            {
                                m_Sense = Sense.Player;
                                break;
                            }
                        case Enable.WhenBricksAreNearby:
                            {
                                m_Sense = Sense.Bricks;
                                break;
                            }
                        case Enable.WhenTagIsNearby:
                            {
                                m_Sense = Sense.Tag;
                                break;
                            }
                    }

                    var colliderComponentToClone = gameObject.AddComponent<SphereCollider>();
                    colliderComponentToClone.center = m_ScopedPivotOffset;
                    colliderComponentToClone.radius = 0.0f;
                    colliderComponentToClone.enabled = false;

                    var sensoryCollider = LEGOBehaviourCollider.Add<SensoryCollider>(colliderComponentToClone, m_ScopedBricks, m_Distance * LEGOHorizontalModule);
                    SetupSensoryCollider(sensoryCollider);

                    Destroy(colliderComponentToClone);
                }
                else
                {
                    m_Distance = int.MaxValue;
                }
            }
        }

        void Update()
        {
            if (m_ShowPrompt && !m_InputPrompt)
            {
                SetupPrompt();
            }

            if (m_Repeat || !m_AlreadyTriggered)
            {
                if (m_Enable == Enable.Always || m_ActiveColliders.Count > 0)
                {
                    var visible = IsVisible() && AdditionalConditionsMet();
                    UpdatePrompt(visible);

                    if (CheckInput())
                    {
                        ConditionMet();
                        m_ConditionMetCount++;

                        if (m_Trigger >= Trigger.WhilePressed)
                        {
                            m_AlreadyTriggered = false;
                        }

                        if (m_ShowPrompt)
                        {
                            m_InputPrompt.Input(m_PromptLabel, m_Distance, m_Repeat, visible);
                        }
                    }
                    else if (m_Trigger >= Trigger.WhilePressed)
                    {
                        if (m_ConditionMetCount > 0)
                        {
                            m_AlreadyTriggered = true;
                        }
                    }
                }
                else
                {
                    UpdatePrompt(false);
                }
            }
        }

        bool CheckInput()
        {
            switch (m_Type)
            {
                case Type.Up:
                    return CheckAxis("Vertical", Direction.Positive);
                case Type.Down:
                    return CheckAxis("Vertical", Direction.Negative);
                case Type.Right:
                    return CheckAxis("Horizontal", Direction.Positive);
                case Type.Left:
                    return CheckAxis("Horizontal", Direction.Negative);
                case Type.Jump:
                    return CheckButton("Jump");
                case Type.Fire1:
                    return CheckButton("Fire1");
                case Type.Fire2:
                    return CheckButton("Fire2");
                case Type.Fire3:
                    return CheckButton("Fire3");
                case Type.OtherKey:
                    return CheckKey((KeyCode)m_OtherKey);
                case Type.AnyKey:
                    return CheckAnyKey();
                default:
                    return false;
            }
        }

        bool CheckAxis(string axisName, Direction axisDirection)
        {
            var result = false;

            var axisRaw = Input.GetAxisRaw(axisName);
            var axisAbsoluteValue = axisDirection == Direction.Positive ? Mathf.Max(0f, axisRaw) : Mathf.Abs(Mathf.Min(axisRaw, 0f));

            switch (m_Trigger)
            {
                case Trigger.WhenPressed:
                    result = axisAbsoluteValue > k_AxisDeadzone && !m_InputHeld;
                    break;
                case Trigger.WhenReleased:
                    result = axisAbsoluteValue <= k_AxisDeadzone && m_InputHeld;
                    break;
                case Trigger.WhilePressed:
                    result = axisAbsoluteValue > k_AxisDeadzone;
                    break;
                case Trigger.WhileReleased:
                    result = axisAbsoluteValue <= k_AxisDeadzone;
                    break;
            }

            m_InputHeld = axisAbsoluteValue > k_AxisDeadzone;

            return result;
        }

        bool CheckButton(string buttonName)
        {
            switch (m_Trigger)
            {
                case Trigger.WhenPressed:
                    return Input.GetButtonDown(buttonName);
                case Trigger.WhenReleased:
                    return Input.GetButtonUp(buttonName);
                case Trigger.WhilePressed:
                    return Input.GetButton(buttonName);
                case Trigger.WhileReleased:
                    return !Input.GetButton(buttonName);
                default:
                    return false;
            }
        }

        bool CheckKey(KeyCode key)
        {
            switch (m_Trigger)
            {
                case Trigger.WhenPressed:
                    return Input.GetKeyDown(key);
                case Trigger.WhenReleased:
                    return Input.GetKeyUp(key);
                case Trigger.WhilePressed:
                    return Input.GetKey(key);
                case Trigger.WhileReleased:
                    return !Input.GetKey(key);
                default:
                    return false;
            }
        }

        bool CheckAnyKey()
        {
            switch (m_Trigger)
            {
                case Trigger.WhenPressed:
                    return Input.anyKeyDown;
                case Trigger.WhenReleased:
                    var keyReleased = !Input.anyKey && m_InputHeld;
                    m_InputHeld = Input.anyKey;
                    return keyReleased;
                case Trigger.WhilePressed:
                    return Input.anyKey;
                case Trigger.WhileReleased:
                    return !Input.anyKey;
                default:
                    return false;
            }
        }

        void SetupPrompt()
        {
            // Create prompt label.
            var label = m_Type <= Type.Fire3 ? Enum.GetName(typeof(Type), m_Type) : m_OtherKey.ToString();
            m_PromptLabel = m_Type >= Type.Fire1 && m_Type <= Type.Fire3 ? label.Insert(4, " ") : label;

            PromptPlacementHandler promptHandler = null;

            // Check if there is already an existing prompt in the scope.
            foreach (var brick in m_ScopedBricks)
            {
                if (brick.GetComponent<PromptPlacementHandler>())
                {
                    promptHandler = brick.GetComponent<PromptPlacementHandler>();
                }

                var inputTriggers = brick.GetComponents<InputTrigger>();

                foreach (var inputTrigger in inputTriggers)
                {
                    if (inputTrigger.m_InputPrompt)
                    {
                        m_InputPrompt = inputTrigger.m_InputPrompt;
                        break;
                    }
                }
            }

            var activeFromStart = (m_Enable == Enable.Always || m_ActiveColliders.Count > 0) && IsVisible();

            // Create a new prompt if none was found.
            if (!m_InputPrompt)
            {
                if (promptHandler == null)
                {
                    promptHandler = gameObject.AddComponent<PromptPlacementHandler>();
                }

                var go = Instantiate(m_InputPromptPrefab, promptHandler.transform);
                m_InputPrompt = go.GetComponent<InputPrompt>();

                // Get the current scoped bounds - might be different than the initial scoped bounds.
                var scopedBounds = GetScopedBounds(m_ScopedBricks, out _, out _);
                promptHandler.AddInstance(go, scopedBounds, PromptPlacementHandler.PromptType.InputPrompt, activeFromStart);
            }

            // Add this Input Trigger to the prompt.
            m_InputPrompt.AddLabel(m_PromptLabel, activeFromStart, m_Distance, promptHandler);
        }

        void UpdatePrompt(bool active)
        {
            if (m_ShowPrompt)
            {
                if (m_PromptActive != active)
                {
                    m_PromptActive = active;

                    if (active)
                    {
                        m_InputPrompt.Activate(m_PromptLabel);
                    }
                    else
                    {
                        m_InputPrompt.Deactivate(m_PromptLabel, m_Distance);
                    }
                }
            }
        }

        void OnDestroy()
        {
            if (m_InputPrompt)
            {
                UpdatePrompt(false);
            }
        }
    }
}
