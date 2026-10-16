using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace JamKit
{
    /// <summary>
    /// The one place JamKit reads input from. Works with the new Input System and with legacy Input,
    /// so it keeps working whatever "Active Input Handling" a project uses.
    ///
    /// Keyboard: WASD / Arrows move · Space jump (W / Up also jump in the 2D platformer) · Left Shift sprint/dash
    /// E interact · J / Left Ctrl / Left Mouse fire · Mouse look · Esc or P pause · R restart.
    /// Gamepad: Left stick / D-pad move · South (A/✕) jump · West or Right trigger dash · North interact
    /// East (B/○) fire · Right stick look · Start pause · Select restart.
    /// </summary>
    public static class JamInput
    {
        /// <summary>Mouse movement → degrees per pixel at sensitivity 1.</summary>
        public static float MouseDegreesPerPixel = 0.12f;
        /// <summary>Right stick → degrees per second at sensitivity 1.</summary>
        public static float StickDegreesPerSecond = 180f;
        const float StickDeadZone = 0.2f;

        // ------------------------------------------------------------------ Move / Look

        /// <summary>Movement direction, length 0..1 (x = right, y = up/forward).</summary>
        public static Vector2 Move
        {
            get
            {
                Vector2 v = Vector2.zero;
#if ENABLE_INPUT_SYSTEM
                var k = Keyboard.current;
                if (k != null)
                {
                    if (k.aKey.isPressed || k.leftArrowKey.isPressed) v.x -= 1f;
                    if (k.dKey.isPressed || k.rightArrowKey.isPressed) v.x += 1f;
                    if (k.sKey.isPressed || k.downArrowKey.isPressed) v.y -= 1f;
                    if (k.wKey.isPressed || k.upArrowKey.isPressed) v.y += 1f;
                }
                var g = Gamepad.current;
                if (g != null)
                {
                    Vector2 s = g.leftStick.ReadValue();
                    if (s.magnitude > StickDeadZone) v += s;
                    v += g.dpad.ReadValue();
                }
#elif ENABLE_LEGACY_INPUT_MANAGER
                if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) v.x -= 1f;
                if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) v.x += 1f;
                if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) v.y -= 1f;
                if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) v.y += 1f;
                Vector2 axes = new Vector2(LegacyAxis("Horizontal"), LegacyAxis("Vertical"));
                if (axes.magnitude > StickDeadZone && v == Vector2.zero) v = axes;
#endif
                return Vector2.ClampMagnitude(v, 1f);
            }
        }

        /// <summary>Camera look this frame, in degrees (x = yaw, y = pitch, up is positive).</summary>
        public static Vector2 Look
        {
            get
            {
                Vector2 v = Vector2.zero;
#if ENABLE_INPUT_SYSTEM
                var m = Mouse.current;
                if (m != null) v += m.delta.ReadValue() * MouseDegreesPerPixel;
                var g = Gamepad.current;
                if (g != null)
                {
                    Vector2 s = g.rightStick.ReadValue();
                    if (s.magnitude > StickDeadZone) v += s * StickDegreesPerSecond * Time.unscaledDeltaTime;
                }
#elif ENABLE_LEGACY_INPUT_MANAGER
                // Legacy "Mouse X/Y" are already scaled by 0.1 in the default Input Manager.
                v += new Vector2(LegacyAxis("Mouse X"), LegacyAxis("Mouse Y")) * (MouseDegreesPerPixel * 10f);
#endif
                return v;
            }
        }

        // ------------------------------------------------------------------ Buttons

        public static bool JumpPressed
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return Down(Keyboard.current?.spaceKey) || Down(Gamepad.current?.buttonSouth);
#elif ENABLE_LEGACY_INPUT_MANAGER
                return Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.JoystickButton0);
#else
                return false;
#endif
            }
        }

        public static bool JumpHeld
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return Held(Keyboard.current?.spaceKey) || Held(Gamepad.current?.buttonSouth);
#elif ENABLE_LEGACY_INPUT_MANAGER
                return Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.JoystickButton0);
#else
                return false;
#endif
            }
        }

        /// <summary>W or Up arrow pressed (the 2D platformer can use these to jump too).</summary>
        public static bool UpPressed
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return Down(Keyboard.current?.wKey) || Down(Keyboard.current?.upArrowKey);
#elif ENABLE_LEGACY_INPUT_MANAGER
                return Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow);
#else
                return false;
#endif
            }
        }

        public static bool UpHeld
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return Held(Keyboard.current?.wKey) || Held(Keyboard.current?.upArrowKey);
#elif ENABLE_LEGACY_INPUT_MANAGER
                return Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);
#else
                return false;
#endif
            }
        }

        /// <summary>Sprint / dash pressed this frame.</summary>
        public static bool DashPressed
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return Down(Keyboard.current?.leftShiftKey) || Down(Gamepad.current?.buttonWest) || Down(Gamepad.current?.rightTrigger);
#elif ENABLE_LEGACY_INPUT_MANAGER
                return Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.JoystickButton2);
#else
                return false;
#endif
            }
        }

        /// <summary>Sprint / dash held.</summary>
        public static bool SprintHeld
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return Held(Keyboard.current?.leftShiftKey) || Held(Gamepad.current?.buttonWest) || Held(Gamepad.current?.rightTrigger);
#elif ENABLE_LEGACY_INPUT_MANAGER
                return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.JoystickButton2);
#else
                return false;
#endif
            }
        }

        public static bool InteractPressed
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return Down(Keyboard.current?.eKey) || Down(Gamepad.current?.buttonNorth);
#elif ENABLE_LEGACY_INPUT_MANAGER
                return Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.JoystickButton3);
#else
                return false;
#endif
            }
        }

        public static bool FirePressed
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return Down(Keyboard.current?.jKey) || Down(Keyboard.current?.leftCtrlKey) ||
                       Down(Mouse.current?.leftButton) || Down(Gamepad.current?.buttonEast);
#elif ENABLE_LEGACY_INPUT_MANAGER
                return Input.GetKeyDown(KeyCode.J) || Input.GetKeyDown(KeyCode.LeftControl) ||
                       Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.JoystickButton1);
#else
                return false;
#endif
            }
        }

        public static bool FireHeld
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return Held(Keyboard.current?.jKey) || Held(Keyboard.current?.leftCtrlKey) ||
                       Held(Mouse.current?.leftButton) || Held(Gamepad.current?.buttonEast);
#elif ENABLE_LEGACY_INPUT_MANAGER
                return Input.GetKey(KeyCode.J) || Input.GetKey(KeyCode.LeftControl) ||
                       Input.GetMouseButton(0) || Input.GetKey(KeyCode.JoystickButton1);
#else
                return false;
#endif
            }
        }

        /// <summary>Esc / P / Start, or the given extra key.</summary>
        public static bool PausePressed(KeyCode extraKey = KeyCode.None)
        {
#if ENABLE_INPUT_SYSTEM
            if (Down(Keyboard.current?.escapeKey) || Down(Keyboard.current?.pKey) || Down(Gamepad.current?.startButton)) return true;
#elif ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.JoystickButton7)) return true;
#endif
            return extraKey != KeyCode.None && GetKeyDown(extraKey);
        }

        /// <summary>The given key (R by default) or Select.</summary>
        public static bool RestartPressed(KeyCode key = KeyCode.R)
        {
#if ENABLE_INPUT_SYSTEM
            if (Down(Gamepad.current?.selectButton)) return true;
#elif ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetKeyDown(KeyCode.JoystickButton6)) return true;
#endif
            return key != KeyCode.None && GetKeyDown(key);
        }

        /// <summary>Any confirm-style press (Jump, Interact, Fire, Enter). Used to advance dialogue.</summary>
        public static bool AdvancePressed
        {
            get
            {
                if (JumpPressed || InteractPressed || FirePressed) return true;
                return GetKeyDown(KeyCode.Return);
            }
        }

        // ------------------------------------------------------------------ KeyCode bridge

        /// <summary>KeyCode check that works with both input backends (letters, digits, F-keys and common keys).</summary>
        public static bool GetKeyDown(KeyCode code)
        {
#if ENABLE_INPUT_SYSTEM
            var k = Keyboard.current;
            if (k == null) return false;
            Key key = ToKey(code);
            return key != Key.None && k[key].wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKeyDown(code);
#else
            return false;
#endif
        }

        public static bool GetKey(KeyCode code)
        {
#if ENABLE_INPUT_SYSTEM
            var k = Keyboard.current;
            if (k == null) return false;
            Key key = ToKey(code);
            return key != Key.None && k[key].isPressed;
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKey(code);
#else
            return false;
#endif
        }

#if ENABLE_INPUT_SYSTEM
        static bool Down(UnityEngine.InputSystem.Controls.ButtonControl b) => b != null && b.wasPressedThisFrame;
        static bool Held(UnityEngine.InputSystem.Controls.ButtonControl b) => b != null && b.isPressed;

        static Key ToKey(KeyCode code)
        {
            if (code >= KeyCode.A && code <= KeyCode.Z) return Key.A + (code - KeyCode.A);
            if (code >= KeyCode.Alpha0 && code <= KeyCode.Alpha9)
                return code == KeyCode.Alpha0 ? Key.Digit0 : Key.Digit1 + (code - KeyCode.Alpha1);
            if (code >= KeyCode.F1 && code <= KeyCode.F12) return Key.F1 + (code - KeyCode.F1);
            switch (code)
            {
                case KeyCode.Space: return Key.Space;
                case KeyCode.Return: return Key.Enter;
                case KeyCode.KeypadEnter: return Key.NumpadEnter;
                case KeyCode.Escape: return Key.Escape;
                case KeyCode.Tab: return Key.Tab;
                case KeyCode.Backspace: return Key.Backspace;
                case KeyCode.LeftShift: return Key.LeftShift;
                case KeyCode.RightShift: return Key.RightShift;
                case KeyCode.LeftControl: return Key.LeftCtrl;
                case KeyCode.RightControl: return Key.RightCtrl;
                case KeyCode.LeftAlt: return Key.LeftAlt;
                case KeyCode.RightAlt: return Key.RightAlt;
                case KeyCode.UpArrow: return Key.UpArrow;
                case KeyCode.DownArrow: return Key.DownArrow;
                case KeyCode.LeftArrow: return Key.LeftArrow;
                case KeyCode.RightArrow: return Key.RightArrow;
                case KeyCode.Backslash: return Key.Backslash;
                case KeyCode.Slash: return Key.Slash;
                case KeyCode.Period: return Key.Period;
                case KeyCode.Comma: return Key.Comma;
                case KeyCode.Minus: return Key.Minus;
                case KeyCode.Equals: return Key.Equals;
                default: return Key.None;
            }
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER && !ENABLE_INPUT_SYSTEM
        static float LegacyAxis(string name)
        {
            try { return Input.GetAxisRaw(name); }
            catch (System.ArgumentException) { return 0f; } // axis removed from the Input Manager
        }
#endif
    }
}
