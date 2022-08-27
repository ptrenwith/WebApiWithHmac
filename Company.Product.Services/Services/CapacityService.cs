using Company.Product.Services.Interfaces;

namespace Company.Product.Services.Services
{
    public class CapacityService : ICapacityService
    {
        public int CalculateDamCapacityFromElevation(int[] elevation)
        {
            bool openSide = false;
            int damCapacity = 0;
            int potentialCapacity = 0;
            int maxElevation = elevation[0];
            int pivot = 0;

            for (int i = 1; i < elevation.Length; i++)
            {
                if (elevation[i] < maxElevation)
                {
                    if (!openSide)
                    {
                        openSide = true;
                        potentialCapacity = 0;
                    }

                    potentialCapacity += maxElevation - elevation[i];
                }
                else
                {
                    if (openSide)
                    {
                        pivot = i;
                        openSide = false;
                    }
                    maxElevation = elevation[i];
                    damCapacity += potentialCapacity;
                    potentialCapacity = 0;
                }

            }

            if (openSide)
            {
                potentialCapacity = 0;
                maxElevation = elevation[elevation.Length - 1];
                for (int i = elevation.Length - 1; i > pivot - 1; i--)
                {
                    if (elevation[i] < maxElevation)
                    {
                        if (!openSide)
                        {
                            openSide = true;
                            potentialCapacity = 0;
                        }

                        potentialCapacity += maxElevation - elevation[i];
                    }
                    else
                    {
                        openSide = false;
                        damCapacity += potentialCapacity;
                        potentialCapacity = 0;
                        maxElevation = elevation[i];
                    }
                }
            }

            return damCapacity;
        }
    }
}
