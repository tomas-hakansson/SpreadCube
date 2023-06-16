namespace SpreadCube_Core
{
    public class Cell
    {
        public Guid Id { get; }
        public string TextContent { get; set; }

        public Cell()
        {
            Id = Guid.NewGuid();
            TextContent = string.Empty;
        }

        public void SetTextContent(string text) =>
            TextContent = text;
    }
}