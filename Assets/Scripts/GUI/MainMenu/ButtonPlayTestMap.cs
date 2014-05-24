using UnityEngine;
using System.Collections;

public class ButtonPlayTestMap : MonoBehaviour {

    MapSelector selector = null;

    void OnClick()
    {
        //Show map selector
        selector = MapSelector.createMapSelector(transform.parent.gameObject, MapSelectorType.Open);
        selector.SelectMapEvent += OnMapSelected;
    }

    void OnMapSelected(MapInfo map)
    {
        if (map == null)
            return;

        GameManager gameManager = GameManager.createGameManager();
        gameManager.loadMapOnSceneChange(map);

        Application.LoadLevel(1);
    }
}
