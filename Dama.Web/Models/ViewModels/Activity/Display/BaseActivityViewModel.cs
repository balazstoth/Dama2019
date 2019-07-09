namespace Dama.Web.Models.ViewModels.Activity.Display
{
    public abstract class BaseActivityViewModel
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public string Color { get; set; }

        public string Labels { get; set; }

        public string Category { get; set; }
    }
}