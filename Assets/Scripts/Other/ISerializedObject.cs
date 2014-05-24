using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

//Interface for objects which can be saved and loaded to a binary stream(File or network)
//Inside the Unity editor any prefab with a component(C# script root object) which implements this interface, will be given a unique ID for that prefab.
//This ID will be given through a call to setUniqueId(), and MUST be stored on a public variable, for it to be serialized.

//This ID will be used to identify which prefab to instantiate and handle the saving/loading of binary data

public interface ISerializedObject
{
    //Set unique ID for prefab, MUST be stored on a public variable
    void setUniqueId(int id);
    int getUniqueId();

    void writeToStream(BinaryWriter stream);
    void readFromStream(BinaryReader stream);

}
