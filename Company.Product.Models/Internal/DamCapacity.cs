namespace Company.Product.Models.Internal
{
    public class DamCapacity
    {
        public DamCapacity(int[] elevation)
        {
            ElevationMap = elevation;
            MaxElevation = elevation[0];
            Open = false;
            TotalCapacity = 0;
            PotentialCapacity = 0;
            Pivot = 0;
        }

        public int[] ElevationMap { get; set; }

        public int TotalCapacity { get; set; }

        public int PotentialCapacity { get; set; }

        public int MaxElevation { get; set; }

        public int Pivot { get; set; }

        public bool Open { get; set; }

        public Directions Direction { get; set; }
    }
}
