using System;
using System.Collections.Generic;
using System.Linq;
using StardewValley;
using StardewValley.Locations;
using StardewWeb;

namespace DailyTasksReport.TaskEngines
{
    class MiscTaskEngine : TaskEngine
    {
        private readonly TaskReportConfig _config;

        private readonly Dictionary<string, string> _tvRecipes =
            Game1.content.Load<Dictionary<string, string>>("Data\\TV\\CookingChannel");

        private string _recipeOfTheDay = "";

        private NPC _birthdayNpc;

        private bool _isTravelingMerchantOpen;
        public bool _wasTravelingMerchantVisited;

        internal MiscTaskEngine(TaskReportConfig config)
        {
            _config = config;
            TaskClass = "Misc";
        }


        public override void Clear()
        {
            _isTravelingMerchantOpen = false;
            _wasTravelingMerchantVisited = false;
            //ModEntry.EventsHelper.Display.MenuChanged -= Display_MenuChanged;

            _birthdayNpc = null;

            _recipeOfTheDay = "";
        }

        public override List<ReportReturnItem> DetailedInfo()
        {
            return new List<ReportReturnItem> { };
        }

        public override List<ReportReturnItem> GeneralInfo()
        {
            List<ReportReturnItem> prItem = new List<ReportReturnItem> { };

            if (!Enabled) return prItem;

            Update();

            if (_config.NewRecipeOnTv && _recipeOfTheDay.Length > 0)
            {

                prItem.Add(new ReportReturnItem { Label = I18n.Tasks_Misc_Queen() });
            }

            if (_config.Birthdays && _birthdayNpc != null)
            {
                prItem.Add(new ReportReturnItem { Label = I18n.Tasks_Misc_Birthday("<a href='/Friends?id=" + _birthdayNpc.name + "'>" + _birthdayNpc.displayName + "</a>") });
            }

            if (_config.TravelingMerchant && _isTravelingMerchantOpen && !_wasTravelingMerchantVisited)
            {
                prItem.Add(new ReportReturnItem { Label = I18n.Tasks_Misc_Merchant() });
            }

            if (Game1.dayOfMonth == 28)
            {
                prItem.Add(new ReportReturnItem { Label = "It's the last day of the season, make sure to harvest all of your applicable crops." });
            }




            return prItem;
        }

        internal override void FirstScan()
        {
            if (Game1.locations.OfType<Forest>().First().travelingMerchantDay)
            {
                // ModEntry.EventsHelper.Display.MenuChanged -= Display_MenuChanged;
                //ModEntry.EventsHelper.Display.MenuChanged += Display_MenuChanged;
            }

            if ((Game1.dayOfMonth % 7 == 0 || Game1.dayOfMonth % 7 == 3) && Game1.stats.DaysPlayed > 5)
            {
                var recipeId = (int)(Game1.stats.DaysPlayed % 224 / 7);

                if (Game1.dayOfMonth % 7 == 3)
                    recipeId = Math.Max(1,
                        1 + new Random((int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame / 2).
                        Next((int)Game1.stats.DaysPlayed % 224) / 7);

                if (_tvRecipes.TryGetValue(recipeId.ToString(), out var value))
                {
                    var key = value.Split('/')[0];
                    if (CraftingRecipe.cookingRecipes.ContainsKey(key) && !Game1.player.knowsRecipe(key))
                        _recipeOfTheDay = key;
                }
            }

            foreach (var location in Game1.locations)
                foreach (var npc in location.characters)
                    if (npc.isBirthday(Game1.currentSeason, Game1.dayOfMonth))
                    {
                        _birthdayNpc = npc;
                        return;
                    }
        }

        private void Update()
        {
            if (Game1.locations.OfType<Forest>().First().travelingMerchantDay)
                _isTravelingMerchantOpen = Game1.timeOfDay < 2000;

            if (_birthdayNpc != null && Game1.player.friendshipData.TryGetValue(_birthdayNpc.Name, out var friendship) &&
                friendship.GiftsToday > 0)
                _birthdayNpc = null;

            if (_recipeOfTheDay.Length > 0 && Game1.player.knowsRecipe(_recipeOfTheDay))
                _recipeOfTheDay = "";
        }
    }
}
