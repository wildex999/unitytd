using UnityEngine;
using System.Collections;
using System.IO;
using System;

//Show Local maps

public class TabLocalMaps : SelectMapTab
{
    public string mapPath = "";

    void Start()
    {
        mapPath = MapInfo.getMapsPath();
        if(!collider.enabled)
        {
            fillMapsList();
        }
    }

    void OnClick()
    {
        collider.enabled = false;
        if (currentTab != null)
            currentTab.collider.enabled = true;
        currentTab = this;

        foreach (Transform child in itemsList.transform)
            Destroy(child.gameObject);
     }

    //Get Maps from local storage and fill the list
    void fillMapsList()
    {
        DirectoryInfo dir = getDirectory(mapPath, true);
        FileInfo[] files = dir.GetFiles("*.utdmap");

        //Read the header of files to determine if they are valid and to get metadata
        //TODO: Put this in an async thread to avoid the game hanging with a slow hdd or a lot of maps
        foreach (FileInfo file in files)
        {
            //Get map info
            FileStream fileStream = null;
            try
            {
                fileStream = file.Open(FileMode.Open);
            }
            catch (Exception ex)
            {
                Debug.LogWarning("Unable to get map file " + file.Name + " due to error: " + ex.Message);
                continue; //Ignore any file errors, and just skip the file
            }


            MapInfo info = null;
            try
            {
                info = MapBase.readMapHeader(fileStream);
                info.filename = file.Name;
            }
            catch(Exception ex)
            {
                Debug.LogWarning("Unable to read map file " + file.Name + " due to error: " + ex.Message);
                continue;
            }

            //Add entry to list with map info written out
            GameObject item = NGUITools.AddChild(itemsList, baseMapItemPrefab);

            item.transform.localPosition = new Vector3(item.transform.localPosition.x, item.transform.localPosition.y, -1); //NGUI is a bit picky, we have to move these towards the camera(Z sorting disabled my a**)
            UIDragPanelContents content = item.GetComponent<UIDragPanelContents>();
            content.draggablePanel = itemsPanel;

            UIButtonMessage message = item.GetComponent<UIButtonMessage>();
            message.target = selector.gameObject;
            
            MapItem mapItem = item.GetComponent<MapItem>();
            if (mapItem == null)
            {
                Debug.LogError("Could not get MapItem while creating list!");
                continue;
            }

            itemsList.GetComponent<UIGrid>().Reposition();

            mapItem.mapName.text = info.name;
            mapItem.mapMeta.text = cOwner + info.ownerName + " ||" + cVersion + info.version + " ||" + cRecommendedPlayers + info.recommendedPlayers + " ||" + cSize + info.sizeX + " x " + info.sizeY;
            mapItem.mapInfo = info;
        }
    }


}
