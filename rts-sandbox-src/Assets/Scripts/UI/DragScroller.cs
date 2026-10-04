using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// Scrolls a horizontal ScrollView by dragging it with the left button, as
    /// a finger would (M-024). A press that has not moved yet is still a click
    /// on the card under it; once the pointer has gone further than a few
    /// pixels it is a drag, the scroller takes the pointer, and the card's
    /// click is cancelled.
    /// </summary>
    public class DragScroller : PointerManipulator
    {
        private const float DragThreshold = 8f;

        private readonly ScrollView _scrollView;

        private int _pointerId = -1;
        private Vector2 _pressPosition;
        private float _pressOffset;
        private bool _dragging;

        public DragScroller(ScrollView scrollView)
        {
            _scrollView = scrollView;
            target = scrollView;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            // Trickle down: the scroller sees the press before the card does,
            // without taking it away from the card.
            target.RegisterCallback<PointerDownEvent>(OnPointerDown, TrickleDown.TrickleDown);
            target.RegisterCallback<PointerMoveEvent>(OnPointerMove, TrickleDown.TrickleDown);
            target.RegisterCallback<PointerUpEvent>(OnPointerUp, TrickleDown.TrickleDown);
            target.RegisterCallback<PointerCaptureOutEvent>(OnCaptureOut);
            target.RegisterCallback<WheelEvent>(OnWheel, TrickleDown.TrickleDown);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<PointerDownEvent>(OnPointerDown, TrickleDown.TrickleDown);
            target.UnregisterCallback<PointerMoveEvent>(OnPointerMove, TrickleDown.TrickleDown);
            target.UnregisterCallback<PointerUpEvent>(OnPointerUp, TrickleDown.TrickleDown);
            target.UnregisterCallback<PointerCaptureOutEvent>(OnCaptureOut);
            target.UnregisterCallback<WheelEvent>(OnWheel, TrickleDown.TrickleDown);
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (evt.button != 0)
            {
                return;
            }

            _pointerId = evt.pointerId;
            _pressPosition = evt.position;
            _pressOffset = _scrollView.scrollOffset.x;
            _dragging = false;
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (evt.pointerId != _pointerId)
            {
                return;
            }

            var dx = evt.position.x - _pressPosition.x;

            if (!_dragging)
            {
                if (Mathf.Abs(dx) < DragThreshold)
                {
                    return;
                }

                _dragging = true;
                target.CapturePointer(_pointerId);
            }

            var offset = _scrollView.scrollOffset;
            offset.x = _pressOffset - dx;
            _scrollView.scrollOffset = offset;
            evt.StopPropagation();
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (evt.pointerId != _pointerId)
            {
                return;
            }

            if (_dragging)
            {
                evt.StopPropagation();
            }

            if (target.HasPointerCapture(_pointerId))
            {
                target.ReleasePointer(_pointerId);
            }

            _pointerId = -1;
            _dragging = false;
        }

        private void OnCaptureOut(PointerCaptureOutEvent evt)
        {
            _pointerId = -1;
            _dragging = false;
        }

        /// <summary>The wheel scrolls the row sideways instead of zooming the camera.</summary>
        private void OnWheel(WheelEvent evt)
        {
            var offset = _scrollView.scrollOffset;
            offset.x += evt.delta.y * 40f;
            _scrollView.scrollOffset = offset;
            evt.StopPropagation();
        }
    }
}
