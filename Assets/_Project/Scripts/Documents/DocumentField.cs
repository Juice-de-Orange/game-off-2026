using System;

namespace GameOff.Documents
{
    /// <summary>
    /// One value printed on a document. The key matches a text object "Field_&lt;key&gt;" in the document prefab and is what
    /// the checking logic (P07) compares across documents: name, species, origin, id, purpose, duration, expires, issued,
    /// ship, destination, cargo.
    /// </summary>
    [Serializable]
    public struct DocumentField
    {
        public string key;
        public string value;

        public DocumentField(string key, string value)
        {
            this.key = key;
            this.value = value;
        }
    }
}
