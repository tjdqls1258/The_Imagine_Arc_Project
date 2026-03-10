using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

public static class WindowUtil
{
    public static ObjectField CreateObjectField(string fieldLabel, Type type, Action<Object> changeValue)
    { 
        ObjectField field = new ObjectField(fieldLabel);
        field.objectType = type;
        field.RegisterValueChangedCallback(value => changeValue?.Invoke(value.newValue));

        return field;
    }

    public static DropdownField DropDown(string lable, List<string> stringName, Action<string> selecteAction)
    {
        DropdownField dropdown = new DropdownField(lable, stringName, 0);

        dropdown.style.width = 150;

        dropdown.RegisterValueChangedCallback(key => selecteAction?.Invoke(key.newValue));

        return dropdown;
    }
}
