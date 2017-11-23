using Microsoft.Xna.Framework;
using StardewValley;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace DailyTasksReport.Tasks
{
    public class TaskItem<TObject>
    {
        public GameLocation Location { get; set; }
        public Vector2 Position { get; set; }
        public string Name { get; set; }
        public TObject Object { get; set; }
        
        public TaskItem(GameLocation location, Vector2 position, string name, TObject @object)
        {
            Location = location;
            Position = position;
            Name = name;
            Object = @object;
        }
    }
}
