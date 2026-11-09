using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GameOff.Day;
using GameOff.Economy;
using UnityEditor;
using UnityEngine;

namespace GameOff.EditorTools
{
    /// <summary>
    /// P71 balancing: plays the economy for 12 days in several styles (EconomySimulator) and checks Martin's goals (07.11.).
    /// Menu GameOff → Economy Report writes the table to the console and to docs/ECONOMY-REPORT.md.
    /// RULE: every change to money (wage, rent, prices, factors, events, wishes, amounts, ticket price) needs a new run until every goal is ✓.
    /// The balance tests (EconomyBalanceTests) check the same goals.
    /// </summary>
    public static class EconomyReport
    {
        public const int Days = 12;
        /// <summary>P73: how far the endless days are checked.</summary>
        public const int EndlessDays = 30;
        private const string ReportPath = "docs/ECONOMY-REPORT.md";

        public struct Goal
        {
            public string Name, Detail;
            public bool Ok;
        }

        public static EconomySimulator Simulator()
        {
            T Load<T>(string path) where T : Object => AssetDatabase.LoadAssetAtPath<T>(path);
            return new EconomySimulator(
                Load<EconomyTuning>("Assets/_Project/Data/Economy/EconomyTuning.asset"),
                Load<HouseholdTuning>("Assets/_Project/Data/Economy/Household.asset"),
                Load<DaySchedule>("Assets/_Project/Data/Days/DaySchedule.asset"),
                Load<VendorStock>("Assets/_Project/Data/Economy/Stock_Kiosk_Test.asset"),
                Load<VendorStock>("Assets/_Project/Data/Economy/Stock_BlackMarket_Test.asset"),
                confiscation: Load<ConfiscationTuning>("Assets/_Project/Data/Economy/ConfiscationTuning.asset"));
        }

        /// <summary>Martin's goals (07.11.): ticket after ~7 days of good, clever work; slower at the bed; mistakes hurt; neglect breaks the family.</summary>
        public static List<Goal> Goals(EconomySimulator sim)
        {
            var clever = sim.Play(EconomySimulator.Style.Clever, 0, Days);
            var bed = sim.Play(EconomySimulator.Style.Bed, 0, Days);
            var sloppy = sim.Play(EconomySimulator.Style.Clever, 2, Days);
            var skip = sim.Play(EconomySimulator.Style.Skip, 0, 3);
            var rumour = sim.Play(EconomySimulator.Style.Rumour, 0, Days);
            var corrupt = sim.Play(EconomySimulator.Style.Corrupt, 0, Days);
            var pilferer = sim.Play(EconomySimulator.Style.Pilferer, 0, Days);
            var endless = sim.Play(EconomySimulator.Style.Clever, 0, EndlessDays);
            var endlessSloppy = sim.Play(EconomySimulator.Style.Clever, 1, EndlessDays);
            float perUnit = rumour.SpeculatedUnits > 0 ? (float)(rumour.SpeculationProfit - rumour.WishCost) / rumour.SpeculatedUnits : 0f;
            string Day(EconomySimulator.Run r) => r.TicketDay < 0 ? "never" : "day " + r.TicketDay;
            return new List<Goal>
            {
                new Goal { Name = "Clever, no mistakes: ticket on day 6–8", Ok = clever.TicketDay >= 6 && clever.TicketDay <= 8 && !clever.GameOver, Detail = Day(clever) },
                new Goal { Name = "Clever, no mistakes: family never sick", Ok = clever.WorstCondition < Household.Sick, Detail = "worst " + clever.WorstCondition },
                new Goal { Name = "Bed, no mistakes: ticket on day 9–11", Ok = bed.TicketDay >= 9 && bed.TicketDay <= 11 && !bed.GameOver, Detail = Day(bed) },
                new Goal { Name = "2 mistakes a day: no ticket by day 12, or a need skipped", Ok = sloppy.TicketDay < 0 || sloppy.SkippedAnyNeed, Detail = Day(sloppy) + (sloppy.SkippedAnyNeed ? ", skipped a need" : "") },
                new Goal { Name = "Nothing for the family: someone critical by day 3", Ok = skip.WorstCondition >= Household.Critical, Detail = "worst " + skip.WorstCondition },
                new Goal { Name = "Pilfering helps, but the ticket not before day 6", Ok = pilferer.TicketDay >= 6 || pilferer.TicketDay < 0, Detail = Day(pilferer) + $", sold {pilferer.LootSold} CR, fines {pilferer.Fines} CR, customs {pilferer.CustomsCaught}x, penalties {pilferer.Disciplinary}{(pilferer.Discharged ? ", discharged" : "")}" },
                new Goal { Name = "Bribes help, but the ticket not before day 6", Ok = corrupt.TicketDay >= 6 || corrupt.TicketDay < 0, Detail = Day(corrupt) + $", {corrupt.Bribes} CR in envelopes" },
                new Goal { Name = $"Endless days: clever without mistakes lasts {EndlessDays} days, family never sick", Ok = !endless.GameOver && endless.WorstCondition < Household.Sick, Detail = endless.GameOver ? "out" : $"balance {endless.Evenings.Last().Balance}, worst {endless.WorstCondition}" },
                new Goal { Name = $"Endless days: 1 mistake a day still lasts {EndlessDays} days", Ok = !endlessSloppy.GameOver, Detail = endlessSloppy.GameOver ? "out on day " + endlessSloppy.Evenings.Last().Day : $"balance {endlessSloppy.Evenings.Last().Balance}" },
                new Goal { Name = "Rumour pays 5–8 CR per speculated unit (after the wish)", Ok = perUnit >= 5f && perUnit <= 8f, Detail = $"{perUnit:0.0} CR × {rumour.SpeculatedUnits}, wish {rumour.WishCost} CR" },
            };
        }

        [MenuItem("GameOff/Economy Report")]
        public static void FromMenu()
        {
            string text = Build();
            File.WriteAllText(ReportPath, text);
            AssetDatabase.Refresh();
            Debug.Log(text);
        }

        public static string Build()
        {
            var sim = Simulator();
            var sb = new StringBuilder();
            sb.AppendLine("# Economy Report (P71)");
            sb.AppendLine();
            sb.AppendLine("Generated by **GameOff → Economy Report** – do not edit by hand.");
            sb.AppendLine("**Every change to money (wage, rent, prices, factors, events, wishes, amounts, ticket price) needs a new run until every goal is ✓.**");
            var h = AssetDatabase.LoadAssetAtPath<HouseholdTuning>("Assets/_Project/Data/Economy/Household.asset");
            var e = AssetDatabase.LoadAssetAtPath<EconomyTuning>("Assets/_Project/Data/Economy/EconomyTuning.asset");
            sb.AppendLine();
            sb.AppendLine($"Start {e.startCredits} CR · wage {h.wagePerTraveler} per correct traveler · penalty {h.penaltyPerCitation} · rent {h.rent} · bed × {h.moneyFactor} · **ticket {h.ticketPrice} CR**");
            sb.AppendLine($"Days after the {sim.StoryDays} story days are drawn from the day pool (P73, seed {EconomySimulator.Seed}).");
            sb.AppendLine();
            sb.AppendLine("## Goals");
            foreach (var g in Goals(sim)) sb.AppendLine($"- {(g.Ok ? "✓" : "✗")} {g.Name} – {g.Detail}");
            sb.AppendLine();
            sb.AppendLine("## Evenings (balance after the bed · family: W well, w weak, S sick, C critical)");
            var runs = new[]
            {
                ("Clever 6/6", sim.Play(EconomySimulator.Style.Clever, 0, Days)),
                ("Rumour 6/6", sim.Play(EconomySimulator.Style.Rumour, 0, Days)),
                ("Corrupt 6/6", sim.Play(EconomySimulator.Style.Corrupt, 0, Days)),
                ("Pilferer 6/6", sim.Play(EconomySimulator.Style.Pilferer, 0, Days)),
                ("Bed 6/6", sim.Play(EconomySimulator.Style.Bed, 0, Days)),
                ("Clever 4/6", sim.Play(EconomySimulator.Style.Clever, 2, Days)),
                ("Bed 4/6", sim.Play(EconomySimulator.Style.Bed, 2, Days)),
                ("Skip 6/6", sim.Play(EconomySimulator.Style.Skip, 0, Days)),
            };
            sb.Append("| Style | Ticket |");
            for (int d = 1; d <= Days; d++) sb.Append($" D{d} |");
            sb.AppendLine();
            sb.Append("|---|---|");
            for (int d = 1; d <= Days; d++) sb.Append("---|");
            sb.AppendLine();
            foreach (var (name, run) in runs)
            {
                sb.Append($"| {name} | {(run.TicketDay < 0 ? "–" : "D" + run.TicketDay)} |");
                for (int d = 1; d <= Days; d++)
                {
                    var ev = run.Evenings.FirstOrDefault(x => x.Day == d);
                    if (ev == null) { sb.Append(" |"); continue; }
                    string fam = new string(ev.Family.Select(c => "WwSC"[Mathf.Clamp(c, 0, 3)]).ToArray());
                    sb.Append(ev.GameOver ? " **OUT** |" : $" {ev.Balance} {fam}{(ev.SkippedNeed ? "!" : "")} |");
                }
                sb.AppendLine();
            }
            sb.AppendLine();
            sb.AppendLine($"Character bonuses and reprimands on the clever path: {runs[0].Item2.Bonus:+0;-0;0} CR.");
            sb.AppendLine("`!` = a need was left out that evening. OUT = the rent could not be paid (game over).");
            return sb.ToString();
        }
    }
}
