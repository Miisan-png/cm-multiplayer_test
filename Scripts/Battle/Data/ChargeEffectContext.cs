
public class ChargeEffectContext
{
    public Monster User;
    public Monster MonsterActivated;
    public Monster Target;
    public BattleGlove UserGlove;
    public BattleGlove TargetGlove;
    public BattleController UserController;
    public BattleController TargetController;
    public int CommonData;

    public ChargeEffectContext(Monster _user,Monster monActivate, Monster _target, BattleGlove UGlove, BattleGlove TGlove, BattleController userC, BattleController targerC)
    {
        User = _user;
        MonsterActivated = monActivate;
        Target = _target;
        UserGlove = UGlove;
        TargetGlove = TGlove;
        UserController = userC;
        TargetController = targerC;
    }
}
