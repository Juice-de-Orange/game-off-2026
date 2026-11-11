using System;
using UnityEngine;

namespace GameOff.Core
{
    /// <summary>
    /// P128: the one saved run, as JSON under one key (PlayerPrefs; WebGL keeps it in IndexedDB). Saves at most once per morning
    /// and never again once the run has ended (an ending deletes it). Storage is passed in, so tests run without PlayerPrefs.
    /// DayManager holds one per run; the menu only asks Load / HasSave.
    /// </summary>
    public class SaveSlot
    {
        public const string Key = "kestrel9.run";

        private readonly Func<string> _read;
        private readonly Action<string> _write;
        private readonly Action _delete;
        private int _savedDay = -1;
        private bool _ended;

        public SaveSlot(Func<string> read, Action<string> write, Action delete)
        {
            _read = read;
            _write = write;
            _delete = delete;
        }

        /// <summary>The slot in PlayerPrefs (PlayerPrefs is only touched when a method is called).</summary>
        public static SaveSlot InPlayerPrefs() => new SaveSlot(
            () => PlayerPrefs.GetString(Key, ""),
            json => { PlayerPrefs.SetString(Key, json); PlayerPrefs.Save(); },
            () => { PlayerPrefs.DeleteKey(Key); PlayerPrefs.Save(); });

        /// <summary>The saved run, or null if there is none or it can't be read (broken, older format).</summary>
        public SaveData Load()
        {
            string json = _read();
            if (string.IsNullOrWhiteSpace(json)) return null;
            try
            {
                var data = JsonUtility.FromJson<SaveData>(json);
                return data != null && data.version == SaveData.CurrentVersion && data.day >= 1 ? data : null;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Save] the saved run can't be read, ignored: {e.Message}");
                return null;
            }
        }

        /// <summary>The menu shows CONTINUE only then.</summary>
        public bool HasSave => Load() != null;

        /// <summary>Saves the morning of this day once; returns false if this morning was already saved or the run is over.</summary>
        public bool SaveMorning(int day, Func<SaveData> capture)
        {
            if (_ended || day == _savedDay) return false;
            var data = capture();
            if (data == null) return false;
            _write(JsonUtility.ToJson(data));
            _savedDay = day;
            return true;
        }

        /// <summary>An ending (game over, ticket home, the last day): the save is gone and this run saves no more.</summary>
        public void EndRun()
        {
            _ended = true;
            Delete();
        }

        /// <summary>Removes the saved run (START over an existing save, or an ending).</summary>
        public void Delete() => _delete();
    }
}
