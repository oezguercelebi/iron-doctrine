using IronDoctrine.Contracts;

namespace IronDoctrine.Client;

internal static class CommandIntent
{
    public static MatchOrder Create(int slot, OrderKind kind, int[] actors, int pickedId = 0,
        WorldPoint position = default, string product = "", bool append = false, int queueIndex = 0)
    {
        // Hover/picking must not turn a ground-point command into an entity-following command.
        // In particular, Build must carry exactly the same point approved by CanPlace.
        int target = kind is OrderKind.Attack or OrderKind.ForceAttack or OrderKind.Repair
            or OrderKind.Gather or OrderKind.Enter or OrderKind.Capture ? pickedId : 0;
        return new MatchOrder(slot, kind, actors, target, position, product, append, queueIndex);
    }
}
