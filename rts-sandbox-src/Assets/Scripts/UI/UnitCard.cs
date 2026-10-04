using Assets.Scripts.Infrastructure.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// A card of a building to put down or a unit to hire (M-024): picture,
    /// name, cost with resource icons, the hot key in the corner. The same card
    /// serves both menus.
    ///
    /// Reports clicks and the right button; what a click does is the owner's
    /// business, and it calls the same entry point the key calls.
    /// </summary>
    public class UnitCard : VisualElement
    {
        private const long PulseMilliseconds = 160;

        private readonly List<(ResourceName Name, int Amount, Label Label)> _costs =
            new List<(ResourceName, int, Label)>();

        public UnitTypeData Type { get; }

        public event Action Clicked;
        public event Action<UnitCard> SecondaryPressed;
        public event Action<UnitCard> SecondaryReleased;

        public UnitCard(UnitTypeData type, string keyLabel, GameResources gameResources)
        {
            Type = type;
            AddToClassList("card");

            var preview = new VisualElement { pickingMode = PickingMode.Ignore };
            preview.AddToClassList("card__preview");
            if (type.Preview != null)
            {
                preview.style.backgroundImage = new StyleBackground(type.Preview);
            }
            Add(preview);

            var name = new Label(type.DisplayName) { pickingMode = PickingMode.Ignore };
            name.AddToClassList("card__name");
            name.AddToClassList("hud-text");
            Add(name);

            var costRow = new VisualElement { pickingMode = PickingMode.Ignore };
            costRow.AddToClassList("card__costs");
            foreach (var cost in type.Stats.ResourceCost.Where(c => c.Amount > 0))
            {
                var resource = gameResources.Resources.FirstOrDefault(r => r.ResourceName == cost.ResourceName);

                var item = new VisualElement { pickingMode = PickingMode.Ignore };
                item.AddToClassList("card__cost");

                var icon = new VisualElement { pickingMode = PickingMode.Ignore };
                icon.AddToClassList("card__cost-icon");
                if (resource?.Icon != null)
                {
                    icon.style.backgroundImage = new StyleBackground(resource.Icon);
                }

                var amount = new Label(cost.Amount.ToString()) { pickingMode = PickingMode.Ignore };
                amount.AddToClassList("card__cost-value");
                amount.AddToClassList("hud-text");

                item.Add(icon);
                item.Add(amount);
                costRow.Add(item);
                _costs.Add((cost.ResourceName, cost.Amount, amount));
            }
            Add(costRow);

            if (!string.IsNullOrEmpty(keyLabel))
            {
                var key = new Label(keyLabel) { pickingMode = PickingMode.Ignore };
                key.AddToClassList("card__key");
                key.AddToClassList("hud-text");
                Add(key);
            }

            this.AddManipulator(new Clickable(OnClick));
            RegisterCallback<PointerDownEvent>(OnPointerDown);
            RegisterCallback<PointerUpEvent>(OnPointerUp);
        }

        /// <summary>
        /// Red numbers for what the player cannot pay right now. The supply
        /// resource counts against the free part of the limit.
        /// </summary>
        public void RefreshAffordability(PlayerResources player, GameResources gameResources)
        {
            var allAffordable = true;

            foreach (var cost in _costs)
            {
                var type = gameResources.Resources.FirstOrDefault(r => r.ResourceName == cost.Name)?.ResourceType;
                var have = player.ResourcesAmount.FirstOrDefault(r => r.ResourceName == cost.Name)?.Amount ?? 0;

                bool enough;
                if (type == ResourceType.SupplyResource)
                {
                    var limit = player.MaxSupplyResourcesAmount.FirstOrDefault(r => r.ResourceName == cost.Name)?.Amount ?? 0;
                    enough = have + cost.Amount <= limit;
                }
                else
                {
                    enough = have >= cost.Amount;
                }

                cost.Label.EnableInClassList("is-missing", !enough);
                allAffordable &= enough;
            }

            EnableInClassList("is-unaffordable", !allAffordable);
        }

        private void OnClick()
        {
            AddToClassList("is-pulsed");
            schedule.Execute(() => RemoveFromClassList("is-pulsed")).StartingIn(PulseMilliseconds);
            Clicked?.Invoke();
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

        /// <summary>The card's numbers for its tooltip.</summary>
        public static List<KeyValuePair<string, string>> TooltipRows(UnitTypeData type)
        {
            var rows = new List<KeyValuePair<string, string>>();

            var cost = string.Join("  ", type.Stats.ResourceCost
                .Where(c => c.Amount > 0)
                .Select(c => c.Amount + " " + c.ResourceName));
            if (!string.IsNullOrEmpty(cost))
            {
                rows.Add(new KeyValuePair<string, string>(UiText.Cost, cost));
            }

            rows.Add(new KeyValuePair<string, string>(UiText.Time, UiText.Seconds(type.Stats.ProducingTime)));
            rows.Add(new KeyValuePair<string, string>(UiText.Health, UiText.Number(type.Stats.MaximumHp)));

            var attacks = type.Behaviours.Contains(Assets.Scripts.GameObjects.UnitBehaviour.UnitBehaviourType.MeleeAttacking)
                || type.Behaviours.Contains(Assets.Scripts.GameObjects.UnitBehaviour.UnitBehaviourType.RangeAttacking)
                || type.Behaviours.Contains(Assets.Scripts.GameObjects.UnitBehaviour.UnitBehaviourType.AutoAttackBuilding);
            if (attacks && type.Stats.Damage > 0f)
            {
                rows.Add(new KeyValuePair<string, string>(UiText.Damage, UiText.Number(type.Stats.Damage)));
                var range = type.Stats.HasRangeAttack ? type.Stats.RangeAttackDistance : type.Stats.MeleeAttackDistance;
                rows.Add(new KeyValuePair<string, string>(UiText.AttackRange, UiText.Number(range)));
            }

            if (type.IsBuildingObject)
            {
                rows.Add(new KeyValuePair<string, string>(UiText.Size,
                    type.Building.GridSize + " × " + type.Building.GridSize));
            }

            var supply = type.Stats.SupplyResourceProduces.Where(s => s.Amount > 0).ToList();
            foreach (var s in supply)
            {
                rows.Add(new KeyValuePair<string, string>(UiText.Supply, "+" + s.Amount + " " + s.ResourceName));
            }

            return rows;
        }
    }
}
