using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;
using LitJson;

[ADAGE.BaseClass]
public class ADAGEData
{
    public static ADAGEData Copy(ADAGEData data)
    {
        Type type = data.GetType();
        ADAGEData newData = (ADAGEData)Activator.CreateInstance(type);

        foreach (PropertyInfo sourcePropertyInfo in type.GetProperties())
        {
            PropertyInfo destPropertyInfo = type.GetProperty(sourcePropertyInfo.Name);

            destPropertyInfo.SetValue(
                newData,
                sourcePropertyInfo.GetValue(data, null),
                null);
        }

        foreach (FieldInfo sourceFieldInfo in type.GetFields())
        {
            FieldInfo destFieldInfo = type.GetField(sourceFieldInfo.Name);

            destFieldInfo.SetValue(
                newData,
                sourceFieldInfo.GetValue(data));
        }

        return newData;
    }


    public string application_name { get; set; }
    public string application_version { get; set; }
    public string adage_version = ADAGE.VERSION;
    public string timestamp { get; set; }
    public string session_token { get; set; }
    public string game_id { get; set; }
    public List<string> ada_base_types { get; set; }
    public string key { get; set; }

    public static ADAGEData CreateFromJSON(string json)
    {
        ADAGEData baseData = LitJson.JsonMapper.ToObject<ADAGEData>(json);

        if (baseData.key != null && baseData.key != "")
        {
            Type theType = ReflectionUtils.FindType(baseData.key);
            if (theType != null)
            {
                //ADAGEData output = Activator.CreateInstance(theType) as ADAGEData;
                //output.Copy(baseData);
                ADAGEData output = ADAGEData.Copy(baseData);

                output.InitFromJSON(json);
                return output;
            }
        }

        return null;
    }

    public ADAGEData()
    {
        this.key = GetType().ToString();
    }

	public virtual string ToJson()
	{
		return JsonMapper.ToJson(this);
	}

    //TODO: Can we reflect here?
    public virtual void InitFromJSON(string input) { }
}
