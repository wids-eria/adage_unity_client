using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using LitJson;

static public class ADAGEJsonConversion
{
	public static void Init()
	{
		/*** Exporter Functions ***/
		ExporterFunc<Color> colorExporter = new ExporterFunc<Color>(ColorToJson);
		JsonMapper.RegisterExporter<Color>(colorExporter);
		
		ExporterFunc<Vector2> vector2Exporter = new ExporterFunc<Vector2>(Vector2ToJson);
		JsonMapper.RegisterExporter<Vector2>(vector2Exporter);
		
		ExporterFunc<Vector3> vector3Exporter = new ExporterFunc<Vector3>(Vector3ToJson);
		JsonMapper.RegisterExporter<Vector3>(vector3Exporter);
		
		ExporterFunc<Quaternion> quaternionExporter = new ExporterFunc<Quaternion>(QuaternionToJson);
		JsonMapper.RegisterExporter<Quaternion>(quaternionExporter);

		ExporterFunc<ADAGEUploadWrapper> wrapperExporter = new ExporterFunc<ADAGEUploadWrapper>(WrapperToJson);
		JsonMapper.RegisterExporter<ADAGEUploadWrapper>(wrapperExporter);
		
		/*** Importer Functions ***/
		//ImporterFunc<string, List<ADAGEData>> adageDataListImporter = new ImporterFunc<string, List<ADAGEData>>(JsonToADAGEDataList);
		//JsonMapper.RegisterImporter<string, List<ADAGEData>>(adageDataListImporter);

		//ImporterFunc<JsonReader, ADAGEData> adageDataImporter = new ImporterFunc<JsonReader, ADAGEData>(JsonToADAGEData);
		//JsonMapper.RegisterImporter<JsonReader, ADAGEData>(adageDataImporter);
		
		//ImporterFunc<string, ADAGETrackerResult> adageDataTrackerImporter = new ImporterFunc<string, ADAGETrackerResult>(JsonToADAGETrackerResult);
		//JsonMapper.RegisterImporter<string, ADAGETrackerResult>(adageDataTrackerImporter);
	}

	/*static ADAGETrackerResult JsonToADAGETrackerResult(string input)
	{
		Debug.Log ("tracker results in");

		JsonData data = JsonMapper.ToObject(input);
		return new ADAGETrackerResult();
	}

	static ADAGEData JsonToADAGEData(JsonReader reader)
	{
		Debug.Log ("converting AD:");

		Dictionary<string, object> values = new Dictionary<string, object>();
		string currentValue = "";
		//JsonReader reader = new JsonReader(input);
		while(reader.Read())
		{
			if(reader.Token == JsonToken.ObjectEnd)
				break;

			if(reader.Token == JsonToken.PropertyName)
			{
				currentValue = (string) reader.Value;

				JsonData data = JsonMapper.ToObject(reader);

				values.Add(currentValue, JsonMapper.ToObject(reader));
				Debug.Log (values[currentValue].GetType());
			}
		}

		return new ADAGEData();
	}

	static List<ADAGEData> JsonToADAGEDataList(string input) 
	{
		Debug.Log ("converting");
		return new List<ADAGEData>();
	}*/

	static void QuaternionToJson(Quaternion quat, JsonWriter writer)
	{	
		writer.WriteObjectStart();
			writer.WritePropertyName("x");
			writer.Write (quat.x.ToString());
			writer.WritePropertyName("y");
			writer.Write (quat.y.ToString());
			writer.WritePropertyName("z");
			writer.Write (quat.z.ToString());
			writer.WritePropertyName("w");
			writer.Write (quat.w.ToString());
		writer.WriteObjectEnd();
	}

	static void Vector2ToJson(Vector2 vector, JsonWriter writer)
	{	
		writer.WriteObjectStart();
			writer.WritePropertyName("x");
			writer.Write (vector.x.ToString());
			writer.WritePropertyName("y");
			writer.Write (vector.y.ToString());
		writer.WriteObjectEnd();
	}

	static void Vector3ToJson(Vector3 vector, JsonWriter writer)
	{	
		writer.WriteObjectStart();
			writer.WritePropertyName("x");
			writer.Write (vector.x.ToString());
			writer.WritePropertyName("y");
			writer.Write (vector.y.ToString());
			writer.WritePropertyName("z");
			writer.Write (vector.z.ToString());
		writer.WriteObjectEnd();
	}
	
	static void ColorToJson(Color color, JsonWriter writer)
	{
		writer.WriteObjectStart();
			writer.WritePropertyName("r");
			writer.Write (color.r.ToString());
			writer.WritePropertyName("g");
			writer.Write (color.g.ToString());
			writer.WritePropertyName("b");
			writer.Write (color.b.ToString());
			writer.WritePropertyName("a");
			writer.Write (color.a.ToString());
		writer.WriteObjectEnd();
	}

	static void WrapperToJson(ADAGEUploadWrapper wrapper, JsonWriter writer)
	{
		writer.WriteObjectStart();
		{
			writer.WritePropertyName("data");
			writer.WriteArrayStart();
			{
				for(int i = 0; i < wrapper.data.Count; i++)
				{
					writer.TextWriter.Write(wrapper.data[i]);
					if(i < wrapper.data.Count - 1)
						writer.TextWriter.Write (",");
				}
			}
			writer.WriteArrayEnd();
		}
		writer.WriteObjectEnd();
	}
}
