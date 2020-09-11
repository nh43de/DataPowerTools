using System;

namespace DataPowerTools.DataStructures
{
    public class ItemsNotOneToOneException : Exception
    {
        public ItemsNotOneToOneException(string mappingItem)
            : base($"Error creating mapping; items are not one-to-one. Duplicate entry: {mappingItem}")
        {
            DuplicateEntry = mappingItem;
        }

        public ItemsNotOneToOneException()
            : base($"Error creating mapping; items are not one-to-one.")
        {
        }

        public string DuplicateEntry { get; private set; } = "";
    }
}