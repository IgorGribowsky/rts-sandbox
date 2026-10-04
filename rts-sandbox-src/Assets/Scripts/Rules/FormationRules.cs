using System;
using System.Collections.Generic;

namespace RtsSandbox.Rules
{
    /// <summary>One unit as the formation sees it.</summary>
    public readonly struct FormationMember
    {
        public FormationMember(bool isMelee, int rang, int id, float side = 0f)
        {
            IsMelee = isMelee;
            Rang = rang;
            Id = id;
            Side = side;
        }

        public bool IsMelee { get; }

        public int Rang { get; }

        /// <summary>Only to keep the order stable between equal units.</summary>
        public int Id { get; }

        /// <summary>
        /// Where the unit stands now across the order's direction, right is more.
        /// Inside a row the leftmost unit takes the leftmost place, so paths
        /// cross less on the way.
        /// </summary>
        public float Side { get; }
    }

    /// <summary>A place in the formation, in steps: right of the centre and forward of it.</summary>
    public readonly struct FormationPlace
    {
        public FormationPlace(float right, float forward)
        {
            Right = right;
            Forward = forward;
        }

        public float Right { get; }

        public float Forward { get; }
    }

    /// <summary>
    /// Smart formation (T-034, M-003): melee in front of ranged, inside each the
    /// higher Rang in front. Rows are as wide as the square root of the group,
    /// rounded up; ranged units start a row of their own, so no archer stands in
    /// the melee line. The whole block is centred on the point of the order.
    ///
    /// Places come out in steps along the order's direction; turning them and
    /// scaling by the spacing is the caller's job.
    /// </summary>
    public static class FormationRules
    {
        public static FormationPlace[] Arrange(IReadOnlyList<FormationMember> members)
        {
            var count = members.Count;
            var places = new FormationPlace[count];

            if (count == 0)
            {
                return places;
            }

            var order = new int[count];
            for (var i = 0; i < count; i++)
            {
                order[i] = i;
            }

            Array.Sort(order, (a, b) => Compare(members[a], members[b]));

            var columns = (int)Math.Ceiling(Math.Sqrt(count));

            // Rows as lists of member indices, front row first.
            var rows = new List<List<int>>();
            List<int> row = null;
            bool? rowIsMelee = null;

            foreach (var index in order)
            {
                var isMelee = members[index].IsMelee;

                if (row == null || row.Count == columns || rowIsMelee != isMelee)
                {
                    row = new List<int>();
                    rows.Add(row);
                    rowIsMelee = isMelee;
                }

                row.Add(index);
            }

            var middleRow = (rows.Count - 1) / 2f;

            for (var r = 0; r < rows.Count; r++)
            {
                rows[r].Sort((a, b) => members[a].Side != members[b].Side
                    ? members[a].Side.CompareTo(members[b].Side)
                    : members[a].Id.CompareTo(members[b].Id));

                var forward = middleRow - r;
                var middleColumn = (rows[r].Count - 1) / 2f;

                for (var c = 0; c < rows[r].Count; c++)
                {
                    places[rows[r][c]] = new FormationPlace(c - middleColumn, forward);
                }
            }

            return places;
        }

        private static int Compare(FormationMember a, FormationMember b)
        {
            if (a.IsMelee != b.IsMelee)
            {
                return a.IsMelee ? -1 : 1;
            }

            if (a.Rang != b.Rang)
            {
                return b.Rang.CompareTo(a.Rang);
            }

            return a.Id.CompareTo(b.Id);
        }
    }
}
