namespace Business.Enums
{
    using System.ComponentModel;

    public enum Position
    {
        Mover = 1,
        Driver = 2,
        Office = 3,
        CEO = 4,
        [Description("General Manager")]
        GeneralManager = 5
    }
}