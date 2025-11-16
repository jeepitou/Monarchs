using System.Collections.Generic;
using Monarchs.Logic;

public static class TraitListExtension
{
    public static CardTrait GetTrait(this List<CardTrait> traits, string id)
    {
        return traits.Find(t => t.id == id);
    }

    public static void SetTrait(this List<CardTrait> traits, string id, int value)
    {
        var trait = traits.GetTrait(id);
        if (trait != null)
            trait.value = value;
        else
            traits.Add(new CardTrait(id, value));
    }

    public static void AddTrait(this List<CardTrait> traits, string id, int value)
    {
        var trait = traits.GetTrait(id);
        if (trait != null)
            trait.value += value;
        else
            traits.SetTrait(id, value);
    }

    public static void RemoveTrait(this List<CardTrait> traits, string id)
    {
        traits.RemoveAll(t => t.id == id);
    }

    public static bool HasTrait(this List<CardTrait> traits, string id)
    {
        return traits.GetTrait(id) != null;
    }
}
