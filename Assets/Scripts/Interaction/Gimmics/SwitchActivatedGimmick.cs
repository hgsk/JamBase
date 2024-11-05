using System.Collections.Generic;
// スイッチ連動ギミック
public class SwitchActivatedGimmick : BaseGimmick
{
    public List<Switch> switches;
    public bool requireAllSwitches = true;

    private void Update()
    {
        bool shouldActivate = requireAllSwitches ? 
            switches.TrueForAll(s => s.IsActivated) : 
            switches.Exists(s => s.IsActivated);

        if (shouldActivate)
        {
            Activate();
        }
        else
        {
            Deactivate();
        }
    }
}

