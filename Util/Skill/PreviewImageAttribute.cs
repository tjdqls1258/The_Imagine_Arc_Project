using UnityEngine;

//[PreviewImage] 
public class PreviewImageAttribute : PropertyAttribute
{
    public float PreviewHeight;

    public PreviewImageAttribute(float height = 64f)
    {
        PreviewHeight = height;
    }
}