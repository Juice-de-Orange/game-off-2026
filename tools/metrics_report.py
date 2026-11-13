#!/usr/bin/env python3
"""Reads the play metrics (P129, docs/DEPLOY.md) and prints what the playtests show.

Usage: python tools/metrics_report.py <folder or .jsonl files ...> [--since 2026-11-12] [--version abc1234]
Get the files from Geoffrey first: scp 'geoffrey:/opt/kestrel9/metrics/*.jsonl' metrics/   (the folder is in .gitignore)

Sections: visits, players and play time per day, where people come from (?src= in the link, else the referring site),
sessions and play time, how far people get (last day and phase per session = where they stop), mistakes per traveler,
decision times, money and family per day, endings, frame rate per browser, errors.
"""
import argparse
import glob
import json
import math
import os
import statistics
import sys
from collections import Counter, defaultdict
from datetime import datetime

try:
    from zoneinfo import ZoneInfo
    LOCAL = ZoneInfo("Europe/Vienna")
except Exception:  # no time zone data (Windows without tzdata): stay in UTC
    LOCAL = None


# The fields the report reads, by type. The server keeps any short text, number or list of numbers in any field, so a crafted
# event could bring a text where a number belongs; clean() drops such values instead of letting one line break the report.
NUMBERS = ("t", "day", "money", "sec", "fps", "load", "evening", "earned", "spent", "slow")
TEXTS = ("ts", "s", "v", "id", "chosen", "right", "why", "ending", "phase", "msg", "at", "ua", "src", "ref")


def number(value):
    """A finite int or float (numeric text is read as a number); None for anything else, also for true/false."""
    if isinstance(value, bool):
        return None
    if isinstance(value, str):
        try:
            value = float(value)
        except ValueError:
            return None
    try:
        return value if isinstance(value, (int, float)) and math.isfinite(value) else None
    except OverflowError:  # an int too big for a float
        return None


def clean(event):
    """The event with every field the report reads in its type (day as int, family as a list of ints), bad values dropped;
    None if it is no event at all."""
    if not isinstance(event, dict) or not isinstance(event.get("e"), str):
        return None
    for key in NUMBERS:
        if key in event:
            value = number(event[key])
            if value is None:
                del event[key]
            else:
                event[key] = int(value) if key == "day" else value
    for key in TEXTS:
        if key in event and not isinstance(event[key], str):
            del event[key]
    if "family" in event:
        family = event["family"] if isinstance(event["family"], list) else []
        event["family"] = [int(n) for n in (number(c) for c in family) if n is not None]
    return event


def load(paths, since=None, version=None):
    files = []
    for path in paths:
        files += sorted(glob.glob(os.path.join(path, "metrics-*.jsonl"))) if os.path.isdir(path) else [path]
    events = []
    for name in files:
        with open(name, encoding="utf-8") as f:
            for line in f:
                try:
                    event = clean(json.loads(line))
                except (json.JSONDecodeError, RecursionError):
                    continue
                if event is None:
                    continue
                if since and event.get("ts", "") < since:
                    continue
                if version and version not in event.get("v", ""):
                    continue
                events.append(event)
    events.sort(key=lambda e: (e.get("s", ""), e.get("t", 0)))
    return events


def median(values):
    return statistics.median(values) if values else 0


def browser(ua):
    for name in ("Edg", "OPR", "Firefox", "Chrome", "Safari"):
        if name in ua:
            return {"Edg": "Edge", "OPR": "Opera"}.get(name, name)
    return ua[:20] or "?"


def local_day(ts):
    try:
        moment = datetime.strptime(ts, "%Y-%m-%dT%H:%M:%SZ")
    except (TypeError, ValueError):
        return "?"
    if LOCAL is not None:
        from datetime import timezone
        moment = moment.replace(tzinfo=timezone.utc).astimezone(LOCAL)
    return moment.strftime("%Y-%m-%d %a")


def source(visit):
    """Where a session came from: the ?src= of the link, else the referring site, else direct."""
    if visit is None:
        return "? (no visit event)"
    if visit.get("src"):
        return visit["src"]
    ref = visit.get("ref", "")
    if not ref:
        return "direct / app (no referrer)"
    if "kestrel9" in ref:
        return "reload / own page"
    for name in ("reddit", "itch", "github", "google", "discord", "twitter", "x.com", "bsky"):
        if name in ref:
            return name
    return ref


def summarize(sessions):
    """One row per session: day (local), source, mobile, loaded, played, minutes."""
    rows = []
    for sid, evs in sessions.items():
        visit = next((e for e in evs if e["e"] == "visit"), None)
        game = [e for e in evs if e["e"] != "visit"]
        rows.append({
            "day": local_day(min(e.get("ts", "") for e in evs)),
            "source": source(visit),
            "mobile": bool(visit and visit.get("mobile")),
            "loaded": bool(game),
            "played": any(e["e"] == "day_start" for e in game),
            "minutes": max((e.get("t", 0) for e in game), default=0) / 60,
        })
    return rows


def funnel(rows, key, label, top):
    groups = defaultdict(list)
    for row in rows:
        groups[key(row)].append(row)
    out = []
    for name, group in sorted(groups.items(), key=lambda g: (g[0] if label == "day" else -len(g[1]))):
        played = [r for r in group if r["played"]]
        out.append((name, len(group), sum(1 for r in group if r["mobile"]), sum(1 for r in group if r["loaded"]), len(played),
                    f"{median([r['minutes'] for r in played]):.0f}", f"{sum(r['minutes'] for r in played) / 60:.1f}"))
    table(out[-top * 4:] if label == "day" else out[:top],
          [label, "visits", "mobile", "game loaded", "played", "min/player (median)", "hours total"])


def table(rows, headers):
    widths = [max(len(str(x)) for x in col) for col in zip(headers, *rows)] if rows else [len(h) for h in headers]
    line = lambda r: "  ".join(str(x).ljust(w) for x, w in zip(r, widths))
    print(line(headers))
    print(line(["-" * w for w in widths]))
    for row in rows:
        print(line(row))


def report(events, top=15):
    sessions = defaultdict(list)
    for e in events:
        sessions[e.get("s", "?")].append(e)
    if not sessions:
        print("No events.")
        return

    stamps = [e["ts"] for e in events if e.get("ts")]
    print(f"# Play metrics: {len(sessions)} sessions, {len(events)} events, "
          f"{min(stamps, default='?')[:10]} to {max(stamps, default='?')[:10]}\n")

    rows = summarize(sessions)
    print("## Per day" + (" (Vienna time)" if LOCAL else " (UTC)"))
    print("Visits = page opened, played = started a day (day 1, or a saved day with CONTINUE). A reload counts as a new visit.")
    funnel(rows, lambda r: r["day"], "day", top)
    print("\n## Where players come from")
    funnel(rows, lambda r: r["source"], "source", top)
    print()

    # Play time and where people stop
    minutes, last_place, played = [], Counter(), 0
    for evs in sessions.values():
        minutes.append(max(e.get("t", 0) for e in evs) / 60)
        days = [e for e in evs if e["e"] in ("day_start", "beat", "traveler", "day_end") and e.get("day", 0) > 0]
        if days:
            played += 1
        last = evs[-1]
        if all(e["e"] == "visit" for e in evs):
            last_place["left while loading"] += 1
        elif any(e["e"] == "run_end" for e in evs):
            last_place["run finished"] += 1
        else:
            last_place[f"day {last.get('day', 0)} {last.get('phase', last['e'])}" if last.get("day") else "menu / loading"] += 1
    print("## Sessions")
    print(f"- started a day: {played} of {len(sessions)}")
    print(f"- minutes per session: median {median(minutes):.1f}, max {max(minutes):.1f}")
    load_times = [e["load"] for e in events if e["e"] == "session" and "load" in e]
    print(f"- load time until the game runs: median {median(load_times):.1f} s\n")

    print("## Where sessions end (last event)")
    table(last_place.most_common(top), ["place", "sessions"])

    # The days each session actually started (day_start), not 1..best: CONTINUE from a save starts later than day 1.
    reached, starters = Counter(), 0
    for evs in sessions.values():
        started = {e["day"] for e in evs if e["e"] == "day_start" and e.get("day", 0) > 0}
        starters += bool(started)
        reached.update(started)
    if reached:
        print("\n## Days reached (sessions that started this day)")
        table([(d, reached[d], f"{100 * reached[d] / starters:.0f}%") for d in sorted(reached)],
              ["day", "sessions", "of sessions that started a day"])

    # Counter decisions
    travelers = [e for e in events if e["e"] == "traveler"]
    if travelers:
        print("\n## Counter decisions")
        wrong = sum(1 for e in travelers if not e.get("ok"))
        print(f"- {len(travelers)} decisions, {wrong} wrong ({100 * wrong / len(travelers):.0f}%), "
              f"median {median([e.get('sec', 0) for e in travelers]):.1f} s per traveler\n")
        per = defaultdict(list)
        for e in travelers:
            per[e.get("id", "?")].append(e)
        rows = []
        for tid, evs in per.items():
            bad = [e for e in evs if not e.get("ok")]
            chosen = Counter(e.get("chosen") for e in bad).most_common(1)
            rows.append((tid, evs[0].get("day", ""), evs[0].get("right", ""), len(evs), f"{100 * len(bad) / len(evs):.0f}%",
                         f"{median([e.get('sec', 0) for e in evs]):.0f}", chosen[0][0] if chosen else ""))
        rows.sort(key=lambda r: (-float(r[4][:-1]), -r[3]))
        print("Travelers decided wrong most often (min. 2 decisions):")
        table([r for r in rows if r[3] >= 2][:top], ["traveler", "day", "right", "n", "wrong", "s", "wrong pick"])
        reasons = Counter(e.get("why", "")[:90] for e in travelers if not e.get("ok") and e.get("why"))
        if reasons:
            print("\nCitations printed most often:")
            table(reasons.most_common(top), ["citation", "n"])

    # Days: money, family, evening
    ends = [e for e in events if e["e"] == "day_end"]
    if ends:
        print("\n## Day ends (median per day)")
        per_day = defaultdict(list)
        for e in ends:
            per_day[e.get("day", 0)].append(e)
        rows = []
        for d in sorted(per_day):
            evs = per_day[d]
            sick = [sum(1 for c in e.get("family", []) if c >= 2) for e in evs]
            rows.append((d, len(evs), median([e.get("money", 0) for e in evs]), median([e.get("earned", 0) for e in evs]),
                         median([e.get("spent", 0) for e in evs]), f"{median(sick):.1f}",
                         f"{median([e.get('evening', 0) for e in evs]) / 60:.1f}",
                         f"{100 * sum(1 for e in evs if e.get('curfew')) / len(evs):.0f}%"))
        table(rows, ["day", "n", "money", "earned", "spent", "sick+", "evening min", "curfew"])
        shifts = [e for e in events if e["e"] == "shift_end"]
        if shifts:
            print(f"\nShift length: median {median([e.get('sec', 0) for e in shifts]) / 60:.1f} min")

    endings = Counter(f"{e.get('ending') or '?'} (day {e.get('day')})" for e in events if e["e"] == "run_end")
    if endings:
        print("\n## Endings")
        table(endings.most_common(top), ["ending", "n"])

    # Performance
    ua_of = {e.get("s", "?"): browser(e.get("ua", "")) for e in events if e["e"] == "session"}
    beats = [e for e in events if e["e"] == "beat" and not e.get("paused")]
    if beats:
        print("\n## Frame rate (beats every 30 s, pause left out)")
        per = defaultdict(list)
        for e in beats:
            per[(ua_of.get(e.get("s", "?"), "?"), e.get("phase", "?"))].append(e)
        rows = [(b, p, len(evs), f"{median([e.get('fps', 0) for e in evs]):.0f}",
                 f"{min(e.get('fps', 0) for e in evs):.0f}", f"{median([e.get('slow', 0) for e in evs]):.0f}")
                for (b, p), evs in sorted(per.items())]
        table(rows, ["browser", "phase", "beats", "fps median", "fps min", "slow frames/30s"])

    errors = Counter((e.get("msg", "")[:100], e.get("at", "")[:60]) for e in events if e["e"] == "error")
    if errors:
        print("\n## Errors (each counted once per session)")
        table([(m, a, n) for (m, a), n in errors.most_common(top)], ["message", "at", "sessions"])


def main():
    parser = argparse.ArgumentParser(description=__doc__.splitlines()[0])
    parser.add_argument("paths", nargs="+", help="folder with metrics-*.jsonl or single files")
    parser.add_argument("--since", help="only events from this UTC date/time on, e.g. 2026-11-12")
    parser.add_argument("--version", help="only this build (part of the version, e.g. the commit)")
    parser.add_argument("--top", type=int, default=15, help="rows per table")
    args = parser.parse_args()
    events = load(args.paths, args.since, args.version)
    report(events, args.top)
    return 0


if __name__ == "__main__":
    sys.exit(main())
