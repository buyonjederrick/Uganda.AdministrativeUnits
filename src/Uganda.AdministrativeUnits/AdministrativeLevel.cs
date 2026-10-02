namespace Uganda.AdministrativeUnits;

/// <summary>The tier of an <see cref="AdministrativeUnit"/> in Uganda's administrative hierarchy.</summary>
public enum AdministrativeLevel
{
    /// <summary>A district (including the capital city district).</summary>
    District = 1,

    /// <summary>A constituency (county or division) within a district.</summary>
    Constituency = 2,

    /// <summary>A subcounty, town council, division or similar unit within a constituency.</summary>
    Subcounty = 3,

    /// <summary>A parish (or ward) within a subcounty.</summary>
    Parish = 4,

    /// <summary>A village (or cell/zone) within a parish. The lowest level.</summary>
    Village = 5,
}
