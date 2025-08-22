using System;

public class ChargeEffects
{
    public Element ElementID;
    public int EffectID;
    public Action<ChargeEffectContext> ActivationEffect;
    public Action<ChargeEffectContext> PersistentEffect;
    public Action<ChargeEffectContext> DispelEffect;

    private EffectTiming Timing;
    public EffectTiming timing => Timing;

    private ChargeEffectContext Context;
    public ChargeEffectContext context => Context;

    public Action<ChargeEffectContext> ContextUpdate;

    private int Turns;
    public int turns => Turns;

    public bool FirstTurn = true;

    public ChargeEffects(Action<ChargeEffectContext> effectAction,Action<ChargeEffectContext> persisteffect, Action<ChargeEffectContext> dispelEffect, EffectTiming timing, Element _elementID,int effectid,int turns)
    {
        ActivationEffect = effectAction;
        PersistentEffect = persisteffect;
        DispelEffect = dispelEffect;
        Timing = timing;
        ElementID = _elementID;
        EffectID = effectid;
        Turns = turns;
    }

    public void AssignContext(ChargeEffectContext con)
    {
        Context = con;
        ContextUpdate = new Action<ChargeEffectContext>(c => Context = c);
    }
}
public enum EffectTiming
{
    BeforeRoll,AfterRoll,BeforeSelect,AfterSelect
}
