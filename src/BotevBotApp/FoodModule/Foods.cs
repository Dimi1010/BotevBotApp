namespace BotevBotApp.FoodModule
{
    internal class Food
    {
        /// <summary>
        /// Gets or sets the food name.
        /// </summary>
        public string Name { get; set; }
    }

    internal class WeightedFood : Food
    {
        /// <summary>
        /// Gets or sets the selection weight for the food.
        /// </summary>
        public int SelectionWeight { get; set; }
    }
}
