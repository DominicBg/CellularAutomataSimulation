[System.Serializable]
public struct PixelSortingSettings
{
    public enum Order { Luminance, Addition /* RGB, RBG, GRB, GBR, BRG, BGR */ }

    public Order order;
    public Bound bound;
    public bool isDescending;
    public bool yx;
}
