namespace ImageManager.Settings
{
    public class SettingsItemAttribute : Attribute
    {
        public string? DisplayName { get; set; }
        public string? Description { get; set; }

        public SettingsItemAttribute(string? displayName = null, string? description = null)
        {
            DisplayName = displayName;
            Description = description;
        }
    }

    public class HideInSettingsAttribute : Attribute
    {
        public bool Hide { get; set; } = true;
        public HideInSettingsAttribute(bool hide = true)
        {
            Hide = hide;
        }
    }
}
