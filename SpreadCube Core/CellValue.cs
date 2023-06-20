namespace SpreadCube_Core
{
    public class CellValue
    {
        public Guid Id { get; }
        public string TextContent { get; set; }

        public CellValue()
        {
            Id = Guid.NewGuid();
            TextContent = string.Empty;
        }

        public void SetTextContent(string text) =>
            TextContent = text;

        public override bool Equals(object? obj) =>
            obj is CellValue cell && Id.Equals(cell.Id);

        public override int GetHashCode() =>
            HashCode.Combine(Id);
    }
}