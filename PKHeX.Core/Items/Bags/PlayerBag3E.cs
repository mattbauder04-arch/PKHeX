using System;
using System.Collections.Generic;
using static PKHeX.Core.InventoryType;
namespace PKHeX.Core;

public sealed class PlayerBag3E : PlayerBag, IPlayerBag3
{
    private const int BaseOffset = 0x0498;

    // NOTE FOR POKEMON NULL:
    // The bag has been expanded with additional sections (Consumables, Niche Items, Special, etc.)
    // which shifts the TMs/HMs and other pouches to different offsets.
    // To avoid corrupting the save, we ONLY read/write the Berry pouch.
    // Berry pouch offset and slot count are unchanged from vanilla Emerald.

    public override IReadOnlyList<InventoryPouch3> Pouches { get; } = GetPouches(ItemStorage3E.Instance);
    public override ItemStorage3E Info => ItemStorage3E.Instance;

    private static InventoryPouch3[] GetPouches(ItemStorage3E info) =>
    [
        new(0x2F8, 46, 999, info, Berries),
    ];

    public PlayerBag3E(SAV3E sav) : this(sav.Large[BaseOffset..], sav.SecurityKey) { }

    public PlayerBag3E(ReadOnlySpan<byte> data, uint security)
    {
        UpdateSecurityKey(security);
        Pouches.LoadAll(data);
    }

    public override void CopyTo(SaveFile sav) => CopyTo((SAV3E)sav);

    // Only write back the berry pouch — all other bag sections are left completely untouched.
    public void CopyTo(SAV3E sav) => CopyTo(sav.Large[BaseOffset..]);
    public void CopyTo(Span<byte> data) => Pouches.SaveAll(data);

    public override int GetMaxCount(InventoryType type, int itemIndex)
    {
        return GetMaxCount(type);
    }

    public void UpdateSecurityKey(uint securityKey)
    {
        foreach (var pouch in Pouches)
        {
            if (pouch.Type != PCItems)
                pouch.SecurityKey = securityKey;
        }
    }
}

/// <summary>
/// Encryption interface for player bags that utilize security keys.
/// </summary>
/// <see cref="PlayerBag3E"/>
/// <see cref="PlayerBag3FRLG"/>
public interface IPlayerBag3
{
    /// <summary>
    /// Updates the security key for all pouches that require it.
    /// </summary>
    /// <remarks>
    /// Interior item data is not modified; this only changes the key used for read/write operations.
    /// </remarks>
    /// <param name="securityKey">The new security key to use.</param>
    void UpdateSecurityKey(uint securityKey);
}
