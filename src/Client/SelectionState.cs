using IronDoctrine.Contracts;
using System.Collections.Generic;
using System.Linq;

namespace IronDoctrine.Client;

internal static class SelectionState
{
    // Snapshots promise no entity order. HUD and input share this stable primary entity.
    public static EntitySnapshot[] Ordered(MatchSnapshot snapshot, HashSet<int> selection) =>
        snapshot.Entities.Where(e => selection.Contains(e.Id)).OrderBy(e => e.Id).ToArray();
}
