namespace InventoryManager
{
    internal class ItemDefinition
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int? StackMax { get; set; }

        public string Comment { get; set; }
    }
}
