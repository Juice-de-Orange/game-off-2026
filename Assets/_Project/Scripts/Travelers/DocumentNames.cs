using GameOff.Documents;

namespace GameOff.Travelers
{
    /// <summary>Readable English names of document types (bulletins, citations).</summary>
    public static class DocumentNames
    {
        public static string Of(DocumentType type) => type switch
        {
            DocumentType.IdCard => "ID card",
            DocumentType.EntryPermit => "entry permit",
            DocumentType.CargoManifest => "cargo manifest",
            DocumentType.Citation => "citation",
            _ => type.ToString(),
        };
    }
}
