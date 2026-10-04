using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// The production queue of the main selected building, above its hire
    /// cards (M-024): the unit in production first, with a ring of progress,
    /// the waiting ones after it. A click on a slot takes that unit out of the
    /// queue with its price back (M-011).
    ///
    /// Slots are kept, not rebuilt: when a unit is born or cancelled only its
    /// slot leaves, shrinking away, and the rest slide up into its place.
    /// </summary>
    public sealed class HireQueue : IDisposable
    {
        private const long LeaveMilliseconds = 180;
        private const long ProgressTickMilliseconds = 33;

        private readonly ScrollView _row;
        private readonly CommandInput _commands;
        private readonly HudTooltip _tooltip;

        private readonly List<Slot> _slots = new List<Slot>();
        private readonly IVisualElementScheduledItem _tick;

        private UnitProducing _producing;

        // The slot the player has just clicked: when the queue reports a unit
        // gone, this tells which one, since the queue holds types, not entries.
        private int _clickedSlot = -1;

        public HireQueue(ScrollView row, CommandInput commands, HudTooltip tooltip)
        {
            _row = row;
            _commands = commands;
            _tooltip = tooltip;

            _row.mode = ScrollViewMode.Horizontal;
            _row.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
            _row.verticalScrollerVisibility = ScrollerVisibility.Hidden;
            _row.AddManipulator(new DragScroller(_row));

            _tick = _row.schedule.Execute(UpdateProgress).Every(ProgressTickMilliseconds);
            _tick.Pause();

            Bind(null);
        }

        public void Dispose()
        {
            Bind(null);
            _tick.Pause();
        }

        /// <summary>Show this building's queue; null hides the row.</summary>
        public void Bind(UnitProducing producing)
        {
            if (producing == _producing && producing != null)
            {
                return;
            }

            if (_producing != null)
            {
                _producing.QueueChanged -= OnQueueChanged;
            }

            _producing = producing;
            _clickedSlot = -1;
            ClearSlots();

            if (_producing != null)
            {
                _producing.QueueChanged += OnQueueChanged;
                foreach (var type in _producing.Queue)
                {
                    AddSlot(type, false);
                }

                _row.scrollOffset = Vector2.zero;
            }

            Refresh();
        }

        private void OnQueueChanged()
        {
            if (_producing == null)
            {
                return;
            }

            var queue = _producing.Queue;

            if (queue.Count == _slots.Count + 1 && SameOrder(queue, 0, 0, _slots.Count))
            {
                // Joined at the end: the usual hire.
                AddSlot(queue[queue.Count - 1], true);
            }
            else if (queue.Count == _slots.Count - 1)
            {
                // One left: the clicked one, or the first that was born.
                var gone = _clickedSlot >= 0 && _clickedSlot < _slots.Count ? _clickedSlot : 0;
                if (!SameAfterRemoval(queue, gone))
                {
                    gone = FindRemoved(queue);
                }

                if (gone >= 0)
                {
                    RemoveSlot(gone);
                }
                else
                {
                    Rebuild();
                }
            }
            else if (queue.Count != _slots.Count || !SameOrder(queue, 0, 0, queue.Count))
            {
                Rebuild();
            }

            _clickedSlot = -1;
            Refresh();
        }

        private void Rebuild()
        {
            ClearSlots();
            foreach (var type in _producing.Queue)
            {
                AddSlot(type, false);
            }
        }

        private bool SameOrder(IReadOnlyList<UnitTypeData> queue, int queueFrom, int slotsFrom, int count)
        {
            for (var i = 0; i < count; i++)
            {
                if (queue[queueFrom + i] != _slots[slotsFrom + i].Type)
                {
                    return false;
                }
            }

            return true;
        }

        private bool SameAfterRemoval(IReadOnlyList<UnitTypeData> queue, int removed)
        {
            return SameOrder(queue, 0, 0, removed)
                && SameOrder(queue, removed, removed + 1, queue.Count - removed);
        }

        private int FindRemoved(IReadOnlyList<UnitTypeData> queue)
        {
            for (var i = 0; i < _slots.Count; i++)
            {
                if (SameAfterRemoval(queue, i))
                {
                    return i;
                }
            }

            return -1;
        }

        private void AddSlot(UnitTypeData type, bool animate)
        {
            var slot = new Slot(type);
            slot.Button.Clicked += () => OnSlotClicked(slot);
            slot.Button.SecondaryPressed += _ => ShowTooltip(slot);
            slot.Button.SecondaryReleased += _ => _tooltip.Hide();

            if (animate)
            {
                // Starts small and see-through, grows in on the next frame.
                slot.Button.AddToClassList("is-entering");
                slot.Button.schedule.Execute(() => slot.Button.RemoveFromClassList("is-entering")).StartingIn(16);
            }

            _row.Add(slot.Button);
            _slots.Add(slot);
        }

        private void RemoveSlot(int index)
        {
            var slot = _slots[index];
            _slots.RemoveAt(index);

            // Leaves the list at once, so the next change already sees the new
            // order; on the screen it shrinks away and is removed after.
            slot.Button.pickingMode = PickingMode.Ignore;
            slot.Button.AddToClassList("is-leaving");
            slot.Button.schedule.Execute(() => slot.Button.RemoveFromHierarchy()).StartingIn(LeaveMilliseconds);
        }

        private void ClearSlots()
        {
            _tooltip.Hide();
            foreach (var slot in _slots)
            {
                slot.Button.RemoveFromHierarchy();
            }

            _slots.Clear();
            _row.Clear();
        }

        private void OnSlotClicked(Slot slot)
        {
            var index = _slots.IndexOf(slot);
            if (index < 0)
            {
                return;
            }

            _tooltip.Hide();
            _clickedSlot = index;
            _commands.CancelProduction(index);
            _clickedSlot = -1;
        }

        /// <summary>The first slot is the one in production: bigger, with the ring.</summary>
        private void Refresh()
        {
            for (var i = 0; i < _slots.Count; i++)
            {
                _slots[i].SetCurrent(i == 0);
            }

            _row.style.display = _slots.Count > 0 ? DisplayStyle.Flex : DisplayStyle.None;

            if (_producing != null && _slots.Count > 0)
            {
                _tick.Resume();
                UpdateProgress();
            }
            else
            {
                _tick.Pause();
            }
        }

        private void UpdateProgress()
        {
            if (_producing == null || _slots.Count == 0)
            {
                return;
            }

            var current = _slots[0];
            current.Progress.Value = _producing.Progress;
            current.Button.EnableInClassList("is-stalled", _producing.IsStalled);
        }

        private void ShowTooltip(Slot slot)
        {
            var index = _slots.IndexOf(slot);
            var kind = index == 0 ? UiText.InProduction : UiText.Queued;
            _tooltip.Show(slot.Button, slot.Type.DisplayName, kind, UiText.CancelHint,
                Array.Empty<KeyValuePair<string, string>>(), true);
        }

        /// <summary>One unit in the queue: its picture in a round frame.</summary>
        private sealed class Slot
        {
            public UnitTypeData Type { get; }
            public RingButton Button { get; }
            public ProgressRing Progress { get; }

            public Slot(UnitTypeData type)
            {
                Type = type;

                Button = new RingButton("ring-button--small");
                Button.AddToClassList("queue-slot");

                var picture = UnitPreviewRenderer.Get(type);
                if (picture != null)
                {
                    Button.Icon.style.backgroundImage = new StyleBackground(Background.FromRenderTexture(picture));
                }

                Progress = new ProgressRing();
                Progress.AddToClassList("queue-slot__progress");
                Button.Overlay.Add(Progress);

                // A cross on hover says what a click does.
                var cancel = new VisualElement { pickingMode = PickingMode.Ignore };
                cancel.AddToClassList("queue-slot__cancel");
                var stroke1 = new VisualElement { pickingMode = PickingMode.Ignore };
                stroke1.AddToClassList("queue-slot__cancel-stroke");
                var stroke2 = new VisualElement { pickingMode = PickingMode.Ignore };
                stroke2.AddToClassList("queue-slot__cancel-stroke");
                stroke2.AddToClassList("queue-slot__cancel-stroke--back");
                cancel.Add(stroke1);
                cancel.Add(stroke2);
                Button.Overlay.Add(cancel);
            }

            public void SetCurrent(bool current)
            {
                Button.EnableInClassList("is-current", current);
                Progress.style.display = current ? DisplayStyle.Flex : DisplayStyle.None;
                if (!current)
                {
                    Progress.Value = 0f;
                    Button.RemoveFromClassList("is-stalled");
                }
            }
        }
    }
}
