namespace TauManager.Models
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class Announcement
    {
        /* 
            This class is overengineered on purpose.
            I intend to create a full-scale notification system
            where announcements will be stored in the DB and the
            officers will be able to create them for specific people
            or for the entire syndicate. For now this class will
            only be used as a viewmodel, but I intend to make it
            a full-featured ORM class soonish.
        */
        // The styles correspond directly to Bootstrap primary styles.
        public enum AnnouncementStyle : byte { Primary, Secondary, Success, Warning, Danger, Info, Light, Dark };
        public int Id { get; set; }
        public int? FromId { get; set; }
        public Player From { get; set; }
        public int? ToId { get; set; }
        public Player To { get; set; }
        public string Text { get; set; }
        public AnnouncementStyle Style { get; set; }
    }
}