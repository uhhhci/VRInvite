using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

/*
 * Beinhaltet die Positions- und Rotationsdaten vom Tracking.
 */
public struct CamTrackingData
{
    public Vector3 _position;
    public Vector3 _rotation;


    public CamTrackingData(Vector3 position, Vector3 rotation)
    {
        _position = position;
        _rotation = rotation;
    }

    public byte[] ToArray()
    {
        var stream = new MemoryStream();
        var writer = new BinaryWriter(stream); 

        writer.Write(_position.x);
        writer.Write(_position.y);
        writer.Write(_position.z);

        writer.Write(_rotation.x);
        writer.Write(_rotation.y);
        writer.Write(_rotation.z);

        return stream.ToArray();
    }

    //For Server! In same struct script
    public static CamTrackingData FromArray(byte[] bytes)
    {
        var reader = new BinaryReader(new MemoryStream(bytes));

        var newStruct = default(CamTrackingData);

        newStruct._position.x = reader.ReadSingle();
        newStruct._position.y = reader.ReadSingle();
        newStruct._position.z = reader.ReadSingle();

        newStruct._rotation.x = reader.ReadSingle();
        newStruct._rotation.y = reader.ReadSingle();
        newStruct._rotation.z = reader.ReadSingle();

        return newStruct;
    }
}






