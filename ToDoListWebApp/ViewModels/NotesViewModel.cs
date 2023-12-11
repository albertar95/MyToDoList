using ToDoListWebApp.Models;

namespace ToDoListWebApp.ViewModels
{
    public class NotesViewModel
    {
        public NoteGroup Group { get; set; }
        public List<Note> Notes { get; set; }
        public string[] bgColor { get; set; } = new string[] { "aquamarine", "burlywood", "lemonchiffon", "azure", "cadetblue", "chartreuse", "lightcoral", "lightsteelblue", "plum", "lightseagreen", "peru", "cornflowerblue", "darkgray", "darkkhaki", "lightblue", "bisque", "violet", "mediumseagreen", "palegreen", "paleturquoise", "tan", "hotpink", "cyan", "thistle", "goldenrod", "darksalmon" };
    }
}
