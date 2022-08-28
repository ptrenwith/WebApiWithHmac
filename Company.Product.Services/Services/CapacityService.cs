using Company.Product.Models.Internal;
using Company.Product.Services.Interfaces;

namespace Company.Product.Services.Services
{
    public class CapacityService : ICapacityService
    {
        public int CalculateDamCapacityFromElevation(int[] elevation)
        {
            DamCapacity damCapacity = new DamCapacity(elevation);

            for (int i = 1; i < elevation.Length; i++)
            {
                damCapacity.Direction = Directions.Forwards;
                EvaluatePotentialCapacity(damCapacity, i);
            }

            // if Open = true it means the end of the array has been reached and the calculation is incomplete, 
            // do the calculation again but start from the back of the array working back to the pivot value.
            if (damCapacity.Open)
            {
                damCapacity.PotentialCapacity = 0;
                damCapacity.MaxElevation = elevation[elevation.Length - 1];
                for (int i = elevation.Length - 1; i > damCapacity.Pivot - 1; i--)
                {
                    damCapacity.Direction = Directions.Backwards;
                    EvaluatePotentialCapacity(damCapacity, i);
                }
            }

            return damCapacity.TotalCapacity;
        }

        private void EvaluatePotentialCapacity(DamCapacity damCapacity, int index)
        {
            if (damCapacity.ElevationMap[index] < damCapacity.MaxElevation)
            {
                if (!damCapacity.Open)
                {
                    // this space can contain water.
                    damCapacity.Open = true;
                    damCapacity.PotentialCapacity = 0;
                }

                damCapacity.PotentialCapacity += damCapacity.MaxElevation - damCapacity.ElevationMap[index];
            }
            else
            {
                // we have reached a point in the array where we know the potentialCapacity is part of the actual capacity.
                if (damCapacity.Direction.Equals(Directions.Forwards) && damCapacity.Open)
                {
                    damCapacity.Pivot = index;
                }
                damCapacity.Open = false;
                damCapacity.MaxElevation = damCapacity.ElevationMap[index];
                damCapacity.TotalCapacity += damCapacity.PotentialCapacity;
                damCapacity.PotentialCapacity = 0;
            }
        }
    }
}
