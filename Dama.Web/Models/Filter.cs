namespace Dama.Web.Models
{
    public class Filter
    {
        public string Name { get; set; }

        public string Category { get; set; }

        public string Priority { get; set; }

        public string Label { get; set; }

        public void ResetValues()
        {
            Name = string.Empty;
            Category = string.Empty;
            Priority = string.Empty;
            Label = string.Empty;
        }
    }
}