using UnityEngine;

public class GameTipsButton : MonoBehaviour
{
    public int maxID = 10;
    
    public void OnClick()
    {
        var sourceID = "GameTipsButton_";
        string id = "";
        string trans = "";
        do
        {
            id = Random.Range(0, maxID).ToString();
        } while (!Methods.HasLocalText(sourceID + id, out trans) || trans == "");
        
        UIManager.Instance.CenteredInform((sourceID + id));
    }
}