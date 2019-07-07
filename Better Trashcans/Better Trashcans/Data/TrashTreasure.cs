﻿using BetterTrashcans.Framework;
using StardewValley;

namespace BetterTrashcans.Data
{
    public class TrashTreasure : IWeighted
    {
        public int Id { get; set; }

        public string Name { get; set; }
        
        public double Chance { get; set; }

        public bool Enabled { get; set; }
                
        public int MinAmount { get; set; }

        public int MaxAmount { get; set; }

        public TrashTreasure(int id, string name, double chance, int minAmount = 1, int maxAmount = 1, bool enabled = true)
        {
            this.Id = id;
            this.Name = name;
            this.Chance = chance;
            this.Enabled = enabled;
            this.MinAmount = minAmount;
            this.MaxAmount = maxAmount;
        }

        public bool IsValid(Farmer who)
        {
            return true;
        }

        public double GetWeight()
        {
            return this.Chance;
        }

        public bool GetEnabled()
        {
            return this.Enabled;
        }
    }
}