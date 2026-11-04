namespace GameOff.Documents
{
    /// <summary>The document kinds a traveler can hand over (P06). Each has its own prefab layout.</summary>
    public enum DocumentType
    {
        IdCard,
        EntryPermit,
        CargoManifest,
        /// <summary>Citation slip from the counter printer after a wrong decision (P07). Not part of a traveler's papers.</summary>
        Citation
    }
}
