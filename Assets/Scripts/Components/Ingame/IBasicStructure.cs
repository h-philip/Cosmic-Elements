using System.Collections;
using System.Collections.Generic;

namespace Components
{
    public interface IBasicStructure : IComponent
    {
        // TODO: Create IStorageSomething and move the properties there
        public double FoodCapacity { get; }
        public double WaterCapacity { get; }
        public double IronCapacity { get; }
        public double CopperCapacity { get; }
        public double RareMineralsCapacity { get; }
        public double EnergyCapacity { get; }
    }
}