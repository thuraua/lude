namespace Data
{
    public class Car
    {
        public int CarId { get; set; }
        public string CarName { get; set; }
        public int Bestand { get; set; }

        public Car(int id, string name, int bestand)
        {
            CarId = id;
            CarName = name;
            Bestand = bestand;
        }

        public override string ToString()
        {
            return "{" + CarId + ", " + CarName + ", " + Bestand + "}";
        }
    }
}