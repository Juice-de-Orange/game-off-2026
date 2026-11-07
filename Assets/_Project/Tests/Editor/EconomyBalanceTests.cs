using GameOff.EditorTools;
using NUnit.Framework;

namespace GameOff.Tests
{
    /// <summary>
    /// P71: the balance goals (Martin, 07.11.) on the project's real data. If one fails after a change to money (wage, rent, prices,
    /// factors, events, wishes, amounts, ticket price), run GameOff → Economy Report and rebalance until every goal is ✓ again.
    /// </summary>
    public class EconomyBalanceTests
    {
        [Test]
        public void AllBalanceGoalsHold()
        {
            var failed = new System.Collections.Generic.List<string>();
            foreach (var goal in EconomyReport.Goals(EconomyReport.Simulator()))
                if (!goal.Ok) failed.Add($"{goal.Name} ({goal.Detail})");
            Assert.IsEmpty(failed, "Rebalance with GameOff → Economy Report: " + string.Join("; ", failed));
        }
    }
}
