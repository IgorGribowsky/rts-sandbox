using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// A round button of the command ring: metal frame, picture, gold glow when
    /// what it stands for is happening now (M-023). Press feedback comes from
    /// the :active pseudo state in USS, so it follows the pointer exactly.
    ///
    /// The button only reports clicks. What a click does is decided by the
    /// owner, which calls the same entry point the hot key calls.
    /// </summary>
    public class RingButton : VisualElement
    {
        private const long PulseMilliseconds = 140;

        private readonly VisualElement _glow;
        private readonly VisualElement _frame;
        private readonly VisualElement _icon;
        private readonly Clickable _clickable;

        private bool _interactive = true;

        public event Action Clicked;

        /// <summary>Right button went down on the button: the owner may show a tooltip.</summary>
        public event Action<RingButton> SecondaryPressed;

        /// <summary>Right button came up, wherever the pointer is now.</summary>
        public event Action<RingButton> SecondaryReleased;

        public VisualElement Icon => _icon;

        /// <summary>Extra layers on top of the picture: cooldown, key letter.</summary>
        public VisualElement Overlay { get; }

        public RingButton(string sizeClass)
        {
            AddToClassList("ring-button");
            AddToClassList(sizeClass);

            _glow = new VisualElement { pickingMode = PickingMode.Ignore };
            _glow.AddToClassList("ring-button__glow");

            _frame = new VisualElement { pickingMode = PickingMode.Ignore };
            _frame.AddToClassList("ring-button__frame");

            _icon = new VisualElement { pickingMode = PickingMode.Ignore };
            _icon.AddToClassList("ring-button__icon");

            Overlay = new VisualElement { pickingMode = PickingMode.Ignore };
            Overlay.AddToClassList("ring-button__overlay");

            Add(_glow);
            Add(_frame);
            Add(_icon);
            Add(Overlay);

            _clickable = new Clickable(OnClick);
            this.AddManipulator(_clickable);

            RegisterCallback<PointerDownEvent>(OnPointerDown);
            RegisterCallback<PointerUpEvent>(OnPointerUp);
        }

        /// <summary>
        /// Shown, but a click means nothing: a passive skill, an enemy's button,
        /// gathering with nothing around. Still picks the pointer, so the world
        /// does not get the click and the tooltip still opens.
        /// </summary>
        public bool Interactive
        {
            get => _interactive;
            set
            {
                _interactive = value;
                EnableInClassList("is-inert", !value);
            }
        }

        public void SetActive(bool active)
        {
            EnableInClassList("is-active", active);
        }

        public void SetIcon(Texture2D texture)
        {
            _icon.style.backgroundImage = texture != null ? new StyleBackground(texture) : StyleKeyword.None;
        }

        private void OnClick()
        {
            // A short flash on every press, accepted or not: the player sees the
            // click landed even when it changes nothing.
            AddToClassList("is-pulsed");
            schedule.Execute(() => RemoveFromClassList("is-pulsed")).StartingIn(PulseMilliseconds);

            if (_interactive)
            {
                Clicked?.Invoke();
            }
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (evt.button != 1)
            {
                return;
            }

            this.CapturePointer(evt.pointerId);
            SecondaryPressed?.Invoke(this);
            evt.StopPropagation();
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (evt.button != 1)
            {
                return;
            }

            if (this.HasPointerCapture(evt.pointerId))
            {
                this.ReleasePointer(evt.pointerId);
            }

            SecondaryReleased?.Invoke(this);
            evt.StopPropagation();
        }
    }
}
