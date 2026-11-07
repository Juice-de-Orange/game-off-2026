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
    /// home at reference price × moneyFactor (HouseholdTuning). The deliveries are cleared after CONFIRM.
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
            }
            if (shift != null)
            {
                shift.DayBegun -= OnDayBegun;
                shift.DecisionMade -= OnDecision;
            }
        }

        private void OnDayBegun() => _travelersToday = _correctToday = 0;

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
        private int Cost(int need) => Tuning.MoneyCost((FamilyNeed)need);
        /// <summary>P13: the item that came home by drone for this need tonight, or null.</summary>
        private ItemData Delivered(int need) => Economy.Delivery.CoveringItem(Tuning.TagOf((FamilyNeed)need));
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
            _open = true;
            _returned = false;
            root.SetActive(true);
            Refresh();
            while (_open) yield return null;
            root.SetActive(false);
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
            Economy.Household.Settle(Met(Food), Met(Heat), Met(Medicine));
            Debug.Log($"[Settlement] day {day}: wage {Wage}, penalty {Penalty}, rent {Tuning.rent}, " +
                      $"food {Met(Food)}, heat {Met(Heat)}, medicine {Met(Medicine)} (drone: {Economy.Delivery.Items.Count} items) → balance {wallet.Balance}");
            Economy.Delivery.Clear();
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
            if (!CanPayRent)
                sb.AppendLine("\n<color=#E2463F>You can't pay the rent.</color>\n<color=#8B9096>RETURN and sell something – or CONFIRM and lose your room.</color>");
            income.text = sb.ToString();

            int available = AfterFixed - ChosenCosts();
            for (int i = 0; i < 3; i++)
            {
                var delivered = Delivered(i);
                if (delivered != null)
                {
                    needButtons[i].interactable = false;
                    needLabels[i].text = $"<color=#9BE05A>[X]  {NeedNames[i]}  ·  by drone ({delivered.DisplayName})</color>" +
                                         (NeedAvailable(i) ? "" : "  (nobody sick)");
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
                string now = Name(e.Household.Condition(i)), then = Name(next[i]);
                string arrow = next[i] > e.Household.Condition(i) ? "<color=#E2463F>" + then + "</color>"
                    : next[i] < e.Household.Condition(i) ? "<color=#9BE05A>" + then + "</color>" : then;
                fb.AppendLine($"{m.name} ({m.role})  ·  {now} → {arrow}");
            }
            family.text = fb.ToString();

            int after = AfterFixed - ChosenCosts();
            balance.text = $"BALANCE  {e.Format(e.Wallet.Balance)}  →  " +
                           (after < 0 ? $"<color=#E2463F>{e.Format(after)}</color>" : e.Format(after));
        }

        private static string ExtraCitations(int n) => n == 1 ? "1 extra citation" : $"{n} extra citations";

        private string Line(string label, int amount) =>
            $"{label}<pos=78%>{(amount >= 0 ? "+" : "−")}{Economy.Format(Mathf.Abs(amount))}";

        private string Name(int condition)
        {
            var names = Tuning.conditionNames;
            return names.Length == 0 ? condition.ToString() : names[Mathf.Clamp(condition, 0, names.Length - 1)];
        }
    }
}
