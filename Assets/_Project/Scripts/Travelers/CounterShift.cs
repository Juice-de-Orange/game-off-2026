using GameOff.Audio;
using System;
using System.Collections;
using System.Collections.Generic;
using GameOff.Counter;
using GameOff.Documents;
using UnityEngine;

namespace GameOff.Travelers
{
    /// <summary>
    /// One shift at the counter (P07). Sitting down starts it; the day's travelers come one by one through the drawer.
    /// Decide by stamping the papers (APPROVED / DENIED, the last stamp counts) and handing all of them back through the
    /// drawer, or by the Arrest / Airlock buttons (papers are confiscated). Every decision is judged by the rules; a wrong one
    /// prints a citation at once. Other systems listen to DecisionMade (money, morale: P08) and ShiftEnded (day flow: P09).
    /// </summary>
    public class CounterShift : MonoBehaviour
    {
        [SerializeField] private CounterStation station;
        [SerializeField] private DocumentDesk desk;
        [SerializeField] private DayRules day;
        [Tooltip("Pause with the lock empty, between one traveler being gone and the next one walking in (s).")]
        [SerializeField] private float nextTravelerDelay = 0.5f;
        [Tooltip("Citations per day that are only warnings; from the next one on they carry a penalty (P08).")]
        [SerializeField] private int freeWarnings = 2;

        private int _index = -1;
        private bool _started, _deciding;
        private TravelerData _current;
        private EvaluationResult _evaluation;
        private Verdict? _returnedStamp;
        private int _citations;
        private bool _nextPending;
        private bool _arriving, _deliverPending;

        /// <summary>The traveler at the window, the decision taken, and how it was judged.</summary>
        public event Action<TravelerData, Verdict, Judgement> DecisionMade;
        public event Action<TravelerData> TravelerArrived;
        /// <summary>The counter seat (P87: block it while a character asks a question).</summary>
        public CounterStation Station => station;
        public event Action ShiftEnded;
        /// <summary>A new day's rules are in force (the pinboard changes its notes).</summary>
        public event Action DayBegun;
        /// <summary>The officer sat down for the first time today, just before the first traveler.</summary>
        public event Action ShiftStarted;

        public DayRules Day => day;
        /// <summary>The day's new rule in one line, for the announcement; empty if there is none.</summary>
        public string NewToday { get; private set; } = "";
        public TravelerData Current => _current;
        /// <summary>P97: 0 to 1, share of today's travelers already dealt with.</summary>
        public float Progress => day == null || day.Travelers.Count == 0 ? 0f : Mathf.Clamp01(_index / (float)day.Travelers.Count);
        public int Citations => _citations;
        /// <summary>Citations per day that are only warnings (P10 deducts a penalty for every one beyond).</summary>
        public int FreeWarnings => freeWarnings;

        private void OnEnable()
        {
            station.SeatedChanged += OnSeated;
            station.ActionTriggered += OnAction;
            desk.DocumentsReturned += OnReturned;
            desk.ReturnRefused += OnRefused;
            desk.ReturnCheck = CheckReturn;
            if (_nextPending)
            {
                // Disabled while waiting for the next traveler: pick up where it stopped
                _nextPending = false;
                NextTraveler();
            }
            if (_deliverPending)
            {
                _deliverPending = false;
                if (_current != null) desk.Deliver(_current.Documents);
            }
        }

        private void OnDisable()
        {
            station.SeatedChanged -= OnSeated;
            station.ActionTriggered -= OnAction;
            desk.DocumentsReturned -= OnReturned;
            desk.ReturnRefused -= OnRefused;
            if (desk.ReturnCheck == CheckReturn) desk.ReturnCheck = null;
            if (_deciding)
            {
                // The Next() coroutine dies with the component; remember to continue when enabled again
                StopAllCoroutines();
                _deciding = false;
                _nextPending = true;
            }
            if (_arriving)
            {
                StopAllCoroutines();
                _arriving = false;
                _deliverPending = true;
            }
        }

        /// <summary>Starts a day (P09 will call this each morning); sitting down starts the first traveler.</summary>
        /// <summary>P75: may change the day before it starts (recurring characters in the queue, their wanted entries).</summary>
        public Func<DayRules, DayRules> PlanDay;
        /// <summary>P91: how long the traveler needs to reach the window; the papers come after that.</summary>
        public Func<float> ArrivalDelay;
        /// <summary>P91: how long the last traveler needs to be gone after this decision; the next one comes after that.</summary>
        public Func<Verdict, float> NextDelay;

        public void BeginDay(DayRules newDay, string newToday = "")
        {
            StopAllCoroutines(); // a pending "next traveler" belongs to the old day
            day = PlanDay != null ? PlanDay(newDay) : newDay;
            NewToday = newToday ?? "";
            _index = -1;
            _started = false;
            _deciding = false;
            _nextPending = false;
            _arriving = false;
            _deliverPending = false;
            _current = null;
            _evaluation = null;
            _returnedStamp = null;
            _citations = 0;
            desk.ClearForNewDay();
            DayBegun?.Invoke();
        }

        private void OnSeated(bool seated)
        {
            if (!seated || _started || day == null) return;
            _started = true;
            ShiftStarted?.Invoke();
            NextTraveler();
        }

        private void NextTraveler()
        {
            _index++;
            _returnedStamp = null;
            // Broken content is skipped loudly instead of freezing the counter (the validator reports it too)
            while (_index < day.Travelers.Count && (day.Travelers[_index] == null || day.Travelers[_index].Documents.Count == 0))
            {
                Debug.LogError($"[Shift] {day.name}: traveler slot {_index} is empty or has no papers - skipped.", day);
                _index++;
            }
            if (_index >= day.Travelers.Count)
            {
                _current = null;
                station.ShowNotice("End of shift. No more travelers today.", 5f);
                ShiftEnded?.Invoke();
                return;
            }
            _current = day.Travelers[_index];
            _evaluation = RuleEvaluator.Evaluate(_current, day);
            TravelerArrived?.Invoke(_current);
            float walk = ArrivalDelay != null ? ArrivalDelay() : 0f;
            if (walk > 0f) StartCoroutine(DeliverAfter(walk));
            else desk.Deliver(_current.Documents);
        }

        private IEnumerator DeliverAfter(float seconds)
        {
            _arriving = true;
            yield return new WaitForSeconds(seconds);
            _arriving = false;
            desk.Deliver(_current.Documents);
        }

        private void OnAction(CounterAction action)
        {
            if (_current == null || _deciding || _arriving) return;
            // Stamps are read from the traveler's papers when they are handed back (a stamp on a citation slip doesn't count)
            if (action != CounterAction.Arrest && action != CounterAction.Airlock) return;
            if (desk.IsBusy) return; // papers still on their way in
            desk.Confiscate();
            if (action == CounterAction.Airlock) AudioManager.Play(s => s.airlockFire); // only when it really happens
            else AudioManager.Play(s => s.buttonArrest);
            Decide(action == CounterAction.Arrest ? Verdict.Arrest : Verdict.Airlock);
        }

        /// <summary>The drawer only goes back with a decision stamped and all of the traveler's papers in it.</summary>
        private string CheckReturn(IReadOnlyList<DocumentView> inDrawer, IReadOnlyList<DocumentView> onDesk)
        {
            if (_current == null || !HasTravelerPapers(inDrawer)) return null; // only slips: just send them away
            if (HasTravelerPapers(onDesk)) return "Hand back all of the traveler's papers.";
            _returnedStamp = LatestStamp(inDrawer);
            if (_returnedStamp == null) return "Stamp the papers first: APPROVED or DENIED.";
            return null;
        }

        private void OnReturned(IReadOnlyList<DocumentData> returned)
        {
            if (_current == null || _deciding || _returnedStamp == null) return;
            bool travelerPapers = false;
            foreach (var d in returned)
                if (d != null && d.Type != DocumentType.Citation) travelerPapers = true;
            if (travelerPapers) Decide(_returnedStamp.Value);
        }

        private void OnRefused(string reason) => station.ShowNotice(reason);

        private void Decide(Verdict chosen)
        {
            _deciding = true;
            var traveler = _current;
            var judgement = DecisionJudge.Judge(_evaluation, chosen);
            Debug.Log($"[Shift] {traveler.DisplayName}: chose {chosen}, correct {judgement.CorrectVerdict} → " +
                      (judgement.Correct ? "OK" : judgement.Reason));
            if (!judgement.Correct) PrintCitation(judgement);
            _current = null;
            DecisionMade?.Invoke(traveler, chosen, judgement);
            StartCoroutine(Next(chosen));
        }

        private IEnumerator Next(Verdict chosen)
        {
            yield return new WaitForSeconds((NextDelay != null ? NextDelay(chosen) : 0f) + nextTravelerDelay);
            _deciding = false;
            NextTraveler();
        }

        private void PrintCitation(Judgement judgement)
        {
            _citations++;
            string penalty = _citations <= freeWarnings ? $"WARNING {_citations} OF {freeWarnings}" : "PENALTY - DEDUCTED FROM WAGE";
            desk.Print(DocumentData.CreateRuntime(DocumentType.Citation, new List<DocumentField>
            {
                new DocumentField("id", $"C-{_citations:00}"),
                new DocumentField("date", day.Date),
                new DocumentField("reason", judgement.Reason.ToUpperInvariant()),
                new DocumentField("penalty", penalty),
            }));
        }

        /// <summary>The most recent APPROVED/DENIED stamp on the traveler's papers, or null if none is stamped.</summary>
        private static Verdict? LatestStamp(IReadOnlyList<DocumentView> views)
        {
            StampTarget latest = null;
            foreach (var v in views)
            {
                if (v == null || v.Type == DocumentType.Citation) continue;
                var target = v.GetComponent<StampTarget>();
                if (target != null && target.IsStamped && (latest == null || target.StampOrder > latest.StampOrder)) latest = target;
            }
            if (latest == null) return null;
            return latest.LastStamp == CounterAction.Approve ? Verdict.Admit : Verdict.Deny;
        }

        private static bool HasTravelerPapers(IReadOnlyList<DocumentView> views)
        {
            foreach (var v in views)
                if (v != null && v.Type != DocumentType.Citation) return true;
            return false;
        }
    }
}
