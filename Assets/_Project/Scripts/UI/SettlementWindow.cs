using System.Collections;
using System.Text;
using GameOff.Day;
using GameOff.Economy;
using GameOff.Travelers;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace GameOff.UI
{
    /// <summary>
    /// Nightly settlement (P10), like a household book: wage for the travelers of the day, penalty for citations beyond the
    /// free warnings, rent (always paid), then food, heating and medicine to tick off, the family's condition before → after,
    /// and the balance before → after. CONFIRM books everything in the Wallet and applies the night to the Household.
    /// P13: a need covered by a drone delivery (PlayerEconomy.Delivery, by tag) is already ticked and free; otherwise money goes
    /// home at reference price × moneyFactor (HouseholdTuning).
    /// P131: the deliveries are the family's stock at home. CONFIRM uses up one item per covered need (medicine only while
    /// someone is sick), perishable leftovers spoil (a line in the morning letter), the rest stays for the next nights.
    /// The FAMILY box ends with the stock now and after tonight.
    /// Plugs into DayManager.NightStep (after "SLEEP", before the fade) and LetterAddendum (family lines in the morning letter).
    /// </summary>
    public class SettlementWindow : MonoBehaviour
    {
        private const int Food = 0, Heat = 1, Medicine = 2;
        private static readonly string[] NeedNames = { "FOOD", "HEATING", "MEDICINE" };

        [SerializeField] private DayManager dayManager;
        [SerializeField] private CounterShift shift;
        [SerializeField] private GameObject root;
        [SerializeField] private TMP_Text title;
        [SerializeField] private TMP_Text income;
        [SerializeField] private Button[] needButtons = new Button[3];
        [SerializeField] private TMP_Text[] needLabels = new TMP_Text[3];
        [SerializeField] private TMP_Text family;
        [SerializeField] private TMP_Text balance;
        [SerializeField] private Button confirmButton;
        [Tooltip("Back to the evening without booking anything (Esc does the same).")]
        [SerializeField] private Button returnButton;

        private readonly bool[] _paid = new bool[3];
        private int _travelersToday;
        private int _correctToday;
        private bool _open;
        private bool _returned;
        /// <summary>P109: 23:00 has struck this evening (DayManager.IsCurfew is false in the night phase this window runs in).</summary>
        private readonly System.Collections.Generic.List<string> _customsLines = new System.Collections.Generic.List<string>();

        private static PlayerEconomy Economy => PlayerEconomy.Instance;
        private HouseholdTuning Tuning => Economy != null ? Economy.HouseholdTuning : null;

        private void Awake()
        {
            for (int i = 0; i < needButtons.Length; i++)
            {
                int need = i;
                needButtons[i].onClick.AddListener(() => Toggle(need));
            }
            confirmButton.onClick.AddListener(() => _open = false);
            if (returnButton != null) returnButton.onClick.AddListener(Return);
            root.SetActive(false);
        }

        // The pause menu ignores Esc while the player has no control, so here it means RETURN.
        private void Update()
        {
            if (_open && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) Return();
        }

        private void Return()
        {
            _returned = true;
            _open = false;
        }

        private void OnEnable()
        {
            if (dayManager != null)
            {
                dayManager.NightStep = Run;
                dayManager.LetterAddendum = () => Economy != null ? Economy.MorningReport() : "";
                dayManager.FamilyInTrouble = () => Economy != null && Economy.Household != null && Economy.Household.AnySick;
                dayManager.LetterOverride = () => Economy != null ? Economy.LossLetter() : null;
            }
            if (shift != null)
            {
                shift.DayBegun += OnDayBegun;
                shift.DecisionMade += OnDecision;
            }
        }

        private void OnDisable()
        {
            if (dayManager != null)
            {
                dayManager.NightStep = null;
                dayManager.LetterAddendum = null;
                dayManager.FamilyInTrouble = null;
                dayManager.LetterOverride = null;
            }
            if (shift != null)
            {
                shift.DayBegun -= OnDayBegun;
                shift.DecisionMade -= OnDecision;
            }
        }

        private void OnDayBegun()
        {
            _travelersToday = _correctToday = 0;
        }

        // P70: only correctly decided travelers are paid (Martin, 06.11.); wrong ones bring nothing.
        private void OnDecision(TravelerData traveler, Verdict chosen, Judgement judgement)
        {
            _travelersToday++;
            if (judgement.Correct) _correctToday++;
        }

        // ---------- Numbers ----------

        private int Wage => Payroll.Wage(_correctToday, Tuning);
        private int Citations => shift != null ? shift.Citations : 0;
        private int FreeWarnings => shift != null ? shift.FreeWarnings : 0;
        private int Penalty => Payroll.Penalty(Citations, FreeWarnings, Tuning);
        /// <summary>P69: the need shrinks with the family.</summary>
        private int Cost(int need)
        {
            int full = Tuning.MoneyCost((FamilyNeed)need);
            var family = Economy.Household;
            return family.Alive == family.Count ? full : Market.Rounded(full * family.Alive / (float)family.Count);
        }
        /// <summary>P131: the item at home that covers each need tonight (a different one per need), null where none does or the need isn't due.</summary>
        private ItemData[] FromHome()
        {
            var tags = new ItemTag[3];
            for (int i = 0; i < 3; i++) tags[i] = NeedAvailable(i) ? Tuning.TagOf((FamilyNeed)i) : null;
            return Economy.Delivery.Covering(tags, Perishable);
        }
        private static ItemTag Perishable => Economy.Tuning != null ? Economy.Tuning.perishableTag : null;
        /// <summary>P131: the tags of all needs, due or not: only items with one of them stay in the stock.</summary>
        private ItemTag[] NeedTags => new[] { Tuning.TagOf(FamilyNeed.Food), Tuning.TagOf(FamilyNeed.Heat), Tuning.TagOf(FamilyNeed.Medicine) };
        /// <summary>P13: the item at home that covers this need tonight, or null.</summary>
        private ItemData Delivered(int need) => FromHome()[need];
        private bool Covered(int need) => Delivered(need) != null;
        private bool Met(int need) => _paid[need] || Covered(need);
        private int Bribe => Economy.BribeTonight; // P66: the barkeeper's envelope for letting his man through
        private int AfterFixed => Economy.Wallet.Balance + Wage + Bribe - Penalty - Tuning.rent;
        /// <summary>The rent is the one thing that must be paid; without it CONFIRM ends the run.</summary>
        private bool CanPayRent => AfterFixed >= 0;

        private int ChosenCosts()
        {
            int sum = 0;
            for (int i = 0; i < 3; i++) if (_paid[i]) sum += Cost(i);
            return sum;
        }

        private bool NeedAvailable(int need) => need != Medicine || Economy.Household.AnySick;

        // ---------- Flow ----------

        /// <summary>Shows the settlement for the day that ends and waits until CONFIRM.</summary>
        public IEnumerator Run(int day)
        {
            if (Economy == null || Tuning == null) { Debug.LogWarning("[Settlement] No PlayerEconomy or HouseholdTuning – skipped."); yield break; }
            title.text = $"DAY {day}  ·  SETTLEMENT";
            // Nothing is preselected: the player decides what the family gets (Martin, 06.11.). Drone deliveries are already ticked.
            for (int i = 0; i < 3; i++) _paid[i] = false;
            InspectDrone(); // P110: before the needs are read, so kept items no longer cover one
            _open = true;
            _returned = false;
            root.SetActive(true);
            Refresh();
            while (_open) yield return null;
            root.SetActive(false);
            if (Discharged)
            {
                // P110: drone customs reached the disciplinary limit; the night ends with the discharge, nothing is booked.
                dayManager.GameOver(Confiscation.DischargedTitle, Confiscation.Instance.DischargedBody);
                yield break;
            }
            if (_returned)
            {
                dayManager.CancelNight(); // nothing booked, the evening goes on
                yield break;
            }
            if (!CanPayRent)
            {
                // Placeholder game over (Martin, 06.11.): without the rent there is no room 103 and no way home.
                Debug.Log($"[Settlement] day {day}: rent {Tuning.rent} can't be paid ({AfterFixed + Tuning.rent} CR available) – game over");
                dayManager.GameOver("STRANDED", "You lost everything and are stranded in space.");
                yield break;
            }
            Book(day);
            if (Economy.FamilyBroken) dayManager.GameOver(Tuning.familyLostTitle, Economy.FamilyLostBody());
        }

        private void Toggle(int need)
        {
            if (Covered(need)) return;
            if (_paid[need]) _paid[need] = false;
            else if (NeedAvailable(need) && ChosenCosts() + Cost(need) <= AfterFixed) _paid[need] = true;
            Refresh();
        }

        private void Book(int day)
        {
            var wallet = Economy.Wallet;
            if (Wage > 0) wallet.Add(Wage, $"Wage · {_correctToday} of {_travelersToday} correct");
            if (Bribe > 0) wallet.Add(Bribe, "Envelope from the bar · " + Economy.BribeFrom);
            Economy.BribeTonight = 0;
            if (Penalty > 0) wallet.Charge(Penalty, "Penalty · " + ExtraCitations(Payroll.PenalizedCitations(Citations, FreeWarnings)));
            wallet.Charge(Tuning.rent, "Rent · room 103");
            for (int i = 0; i < 3; i++)
                if (_paid[i]) wallet.Charge(Cost(i), "Family · " + NeedNames[i].ToLowerInvariant());
            // Read before the night: medicine is only due while someone is sick.
            bool food = Met(Food), heat = Met(Heat), medicine = Met(Medicine);
            var fromHome = FromHome();
            Economy.Household.Settle(food, heat, medicine);
            // P131: one item per covered need is used up, perishable leftovers go bad, items for no need leave, the rest stays at home.
            var home = Economy.Delivery;
            var spoiled = home.Night(fromHome, NeedTags, Perishable);
            if (spoiled.Count > 0) Economy.AddLetterLine("Spoiled at home: " + HomeDelivery.Summary(spoiled) + ".");
            Debug.Log($"[Settlement] day {day}: wage {Wage}, penalty {Penalty}, rent {Tuning.rent}, " +
                      $"food {food}, heat {heat}, medicine {medicine} (used from home: {HomeDelivery.Summary(fromHome)}; " +
                      $"left: {home.Summary()}; spoiled: {HomeDelivery.Summary(spoiled)}) → balance {wallet.Balance}");
            _customsLines.Clear();
        }

        // ---------- P110: customs on the drone ----------

        private static bool Discharged =>
            Confiscation.Instance != null && Confiscation.Instance.Tuning != null &&
            Confiscation.Instance.Conduct.Discharged(Confiscation.Instance.Tuning);

        /// <summary>
        /// Inspects what the player sent since the last inspection. Reopening after RETURN inspects only what was sent since,
        /// so the same shipment is never rolled twice.
        /// </summary>
        private void InspectDrone()
        {
            var confiscation = Confiscation.Instance;
            if (confiscation == null || confiscation.Tuning == null) return;
            var t = confiscation.Tuning;
            var result = DroneCustoms.Inspect(Economy.Delivery, confiscation.Conduct, t, Random.value);
            if (!result.Caught) return;
            string items = DroneCustoms.ItemList(result.Lost);
            string line = t.droneCaughtLine.Replace("{items}", items) +
                          $"\nDISCIPLINARY PENALTY {confiscation.Conduct.Disciplinary} OF {t.disciplinaryLimit}.";
            if (Discharged) line += "\nYou are discharged from service.";
            _customsLines.Add(line);
            Economy.AddLetterLine(t.droneLetterLine.Replace("{items}", items));
            Debug.Log($"[Customs] drone inspected: kept {items}, disciplinary {confiscation.Conduct.Disciplinary}");
        }

        private void Refresh()
        {
            var e = Economy;
            var t = Tuning;
            var sb = new StringBuilder();
            sb.AppendLine(Line($"Wage  ·  {_correctToday} of {_travelersToday} correct × {t.wagePerTraveler}", Wage));
            int penalized = Payroll.PenalizedCitations(Citations, FreeWarnings);
            // Short enough to stay left of the amount column (B06); the free warnings are explained on the citation itself.
            if (penalized > 0) sb.AppendLine(Line("Penalty  ·  " + ExtraCitations(penalized), -Penalty));
            if (Bribe > 0) sb.AppendLine(Line("Envelope from the bar", Bribe));
            sb.AppendLine(Line("Rent  ·  room 103", -t.rent));
            foreach (var line in _customsLines) sb.AppendLine($"\n<color=#E2463F>{line}</color>");
            if (!CanPayRent) sb.AppendLine(RentWarning(dayManager != null && dayManager.CurfewStruck));
            income.text = sb.ToString();

            int available = AfterFixed - ChosenCosts();
            var fromHome = FromHome();
            for (int i = 0; i < 3; i++)
            {
                var delivered = fromHome[i];
                if (delivered != null)
                {
                    needButtons[i].interactable = false;
                    needLabels[i].text = $"<color=#9BE05A>[X]  {NeedNames[i]}  ·  from home ({delivered.DisplayName})</color>";
                    continue;
                }
                bool can = NeedAvailable(i) && (_paid[i] || Cost(i) <= available);
                needButtons[i].interactable = can || _paid[i];
                string box = _paid[i] ? "[X]" : "[ ]";
                string note = !NeedAvailable(i) ? "  (nobody sick)" : !can && !_paid[i] ? "  (not enough)" : "";
                needLabels[i].text = $"{box}  {NeedNames[i]}  −{e.Format(Cost(i))}{note}";
            }

            var next = e.Household.Preview(Met(Food), Met(Heat), Met(Medicine));
            var fb = new StringBuilder("FAMILY\n");
            for (int i = 0; i < e.Household.Count && i < t.members.Count; i++)
            {
                var m = t.members[i];
                if (e.Household.IsDead(i))
                {
                    fb.AppendLine($"<color=#8B9096>{m.name} ({m.role})  ·  {t.deadName}</color>");
                    continue;
                }
                string now = Name(e.Household.Condition(i)), then = Name(next[i]);
                string arrow = next[i] > e.Household.Condition(i) ? "<color=#E2463F>" + then + "</color>"
                    : next[i] < e.Household.Condition(i) ? "<color=#9BE05A>" + then + "</color>" : then;
                fb.AppendLine($"{m.name} ({m.role})  ·  {now} → {arrow}");
            }
            fb.Append(HomeLine(fromHome));
            family.text = fb.ToString();

            int after = AfterFixed - ChosenCosts();
            balance.text = $"BALANCE  {e.Format(e.Wallet.Balance)}  →  " +
                           (after < 0 ? $"<color=#E2463F>{e.Format(after)}</color>" : e.Format(after));
        }

        /// <summary>P131: the stock at home now and what stays after tonight (the night played on a copy, so it matches Book).</summary>
        private string HomeLine(ItemData[] fromHome)
        {
            var home = Economy.Delivery;
            // One line under the family (the box has room for about two more): what is at home, what is left tomorrow.
            if (home.Items.Count == 0) return "<color=#8B9096>AT HOME  nothing</color>";
            var tomorrow = home.Copy();
            var spoils = tomorrow.Night(fromHome, NeedTags, Perishable);
            string after = tomorrow.Items.Count > 0 ? tomorrow.Summary() : "nothing";
            if (spoils.Count > 0) after += $"  <color=#E2463F>({HomeDelivery.Summary(spoils)} spoils)</color>";
            return $"AT HOME  {home.Summary()}  ·  after tonight: {after}";
        }

        /// <summary>The rent can't be paid: before the curfew RETURN and selling still help; after it (P109) the trading is closed for tonight.</summary>
        public static string RentWarning(bool curfew) =>
            "\n<color=#E2463F>You can't pay the rent.</color>\n<color=#8B9096>" +
            (curfew ? "Trading is closed for tonight. CONFIRM and lose your room." : "RETURN and sell something – or CONFIRM and lose your room.") +
            "</color>";

        private static string ExtraCitations(int n) => n == 1 ? "1 extra citation" : $"{n} extra citations";

        private string Line(string label, int amount) =>
            $"{label}<pos=78%>{(amount >= 0 ? "+" : "−")}{Economy.Format(Mathf.Abs(amount))}";

        private string Name(int condition)
        {
            if (condition == Household.Dead) return Tuning.deadName;
            var names = Tuning.conditionNames;
            return names.Length == 0 ? condition.ToString() : names[Mathf.Clamp(condition, 0, names.Length - 1)];
        }
    }
}
