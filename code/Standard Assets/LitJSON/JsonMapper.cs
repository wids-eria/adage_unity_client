#region Header
/**
 * JsonMapper.cs
 *   JSON to .Net object and object to JSON conversions.
 *
 * The authors disclaim copyright to this source code. For more details, see
 * the COPYING file included with this distribution.
 **/
#endregion


using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using UnityEngine;


namespace LitJson
{
	public class ForceSerialization : System.Attribute{}
	public class SkipSerialization : System.Attribute{}
	public class SkipParentSerialization : System.Attribute{}
	
    internal struct PropertyMetadata
    {
        public MemberInfo Info;
        public bool       IsField;
        public Type       Type;
    }


    internal struct ArrayMetadata
    {
        private Type element_type;
        private bool is_array;
        private bool is_list;


        public Type ElementType {
            get {
                if (element_type == null)
                    return typeof (JsonData);

                return element_type;
            }

            set { element_type = value; }
        }

        public bool IsArray {
            get { return is_array; }
            set { is_array = value; }
        }

        public bool IsList {
            get { return is_list; }
            set { is_list = value; }
        }
    }


    internal struct ObjectMetadata
    {
        private Type element_type;
        private bool is_dictionary;

        private IDictionary<string, PropertyMetadata> properties;


        public Type ElementType {
            get {
                if (element_type == null)
                    return typeof (JsonData);

                return element_type;
            }

            set { element_type = value; }
        }

        public bool IsDictionary {
            get { return is_dictionary; }
            set { is_dictionary = value; }
        }

        public IDictionary<string, PropertyMetadata> Properties {
            get { return properties; }
            set { properties = value; }
        }
    }


    internal delegate void ExporterFunc    (object obj, JsonWriter writer);
    public   delegate void ExporterFunc<T> (T obj, JsonWriter writer);

    internal delegate object ImporterFunc                (object input);
    public   delegate TValue ImporterFunc<TJson, TValue> (TJson input);

    public delegate IJsonWrapper WrapperFactory ();


    public class JsonMapper
    {
        #region Fields
        private static int max_nesting_depth;

        private static IFormatProvider datetime_format;

        private static IDictionary<Type, ExporterFunc> base_exporters_table;
        private static IDictionary<Type, ExporterFunc> custom_exporters_table;

        private static IDictionary<Type,
                IDictionary<Type, ImporterFunc>> base_importers_table;
        private static IDictionary<Type,
                IDictionary<Type, ImporterFunc>> custom_importers_table;

        private static IDictionary<Type, ArrayMetadata> array_metadata;
        private static readonly object array_metadata_lock = new System.Object ();

        private static IDictionary<Type,
                IDictionary<Type, MethodInfo>> conv_ops;
        private static readonly object conv_ops_lock = new System.Object ();

        private static IDictionary<Type, ObjectMetadata> object_metadata;
        private static readonly object object_metadata_lock = new System.Object ();

        private static IDictionary<Type,
                IList<PropertyMetadata>> type_properties;
        private static readonly object type_properties_lock = new System.Object ();

        private static JsonWriter      static_writer;
        private static readonly object static_writer_lock = new System.Object ();
        #endregion


        #region Constructors
        static JsonMapper ()
        {
            max_nesting_depth = 100;

            array_metadata = new Dictionary<Type, ArrayMetadata> ();
            conv_ops = new Dictionary<Type, IDictionary<Type, MethodInfo>> ();
            object_metadata = new Dictionary<Type, ObjectMetadata> ();
            type_properties = new Dictionary<Type,
                            IList<PropertyMetadata>> ();

            static_writer = new JsonWriter ();

            datetime_format = DateTimeFormatInfo.InvariantInfo;

            base_exporters_table   = new Dictionary<Type, ExporterFunc> ();
            custom_exporters_table = new Dictionary<Type, ExporterFunc> ();

            base_importers_table = new Dictionary<Type,
                                 IDictionary<Type, ImporterFunc>> ();
            custom_importers_table = new Dictionary<Type,
                                   IDictionary<Type, ImporterFunc>> ();

            RegisterBaseExporters ();
            RegisterBaseImporters ();
        }
        #endregion


        #region Private Methods
        private static void AddArrayMetadata (Type type)
        {
            if (array_metadata.ContainsKey (type))
                return;

            ArrayMetadata data = new ArrayMetadata ();

            data.IsArray = type.IsArray;

            if (type.GetInterface ("System.Collections.IList") != null)
                data.IsList = true;

            foreach (PropertyInfo p_info in type.GetProperties ()) {
                if (p_info.Name != "Item")
                    continue;

                ParameterInfo[] parameters = p_info.GetIndexParameters ();

                if (parameters.Length != 1)
                    continue;

                if (parameters[0].ParameterType == typeof (int))
                    data.ElementType = p_info.PropertyType;
            }

            lock (array_metadata_lock) {
                try {
                    array_metadata.Add (type, data);
                } catch (ArgumentException) {
                    return;
                }
            }
        }

        private static void AddObjectMetadata (Type type)
        {
            if (object_metadata.ContainsKey (type))
                return;

            ObjectMetadata data = new ObjectMetadata ();

            if (type.GetInterface ("System.Collections.IDictionary") != null)
                data.IsDictionary = true;

            data.Properties = new Dictionary<string, PropertyMetadata> ();

            foreach (PropertyInfo p_info in type.GetProperties ()) {
                if (p_info.Name == "Item") {
                    ParameterInfo[] parameters = p_info.GetIndexParameters ();

                    if (parameters.Length != 1)
                        continue;

                    if (parameters[0].ParameterType == typeof (string))
                        data.ElementType = p_info.PropertyType;

                    continue;
                }

                PropertyMetadata p_data = new PropertyMetadata ();
                p_data.Info = p_info;
                p_data.Type = p_info.PropertyType;

                data.Properties.Add (p_info.Name, p_data);
            }

            foreach (FieldInfo f_info in type.GetFields ()) {
                PropertyMetadata p_data = new PropertyMetadata ();
                p_data.Info = f_info;
                p_data.IsField = true;
                p_data.Type = f_info.FieldType;

                data.Properties.Add (f_info.Name, p_data);
            }

            lock (object_metadata_lock) {
                try {
                    object_metadata.Add (type, data);
                } catch (ArgumentException) {
                    return;
                }
            }
        }

        private static void AddTypeProperties (Type type)
        {
            if (type_properties.ContainsKey (type))
                return;
			
			bool inherit = true;
			System.Attribute[] attrs = System.Attribute.GetCustomAttributes(type,typeof(SkipParentSerialization));
			if(attrs.Length > 0)
			{
				foreach (System.Attribute attr in attrs)
		        {
		            if (attr is SkipParentSerialization)
		            {
						inherit = false;
		            }
		        }
			}
			
            IList<PropertyMetadata> props = new List<PropertyMetadata> ();
			
			PropertyInfo[] propsInfo;
			if(inherit)
				propsInfo = type.GetProperties();
			else
				propsInfo = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
				
            foreach (PropertyInfo p_info in propsInfo) {
				
			    if (p_info.Name == "Item")
                    continue;

                PropertyMetadata p_data = new PropertyMetadata ();
                p_data.Info = p_info;
                p_data.IsField = false;
                props.Add (p_data);
            }
						
			FieldInfo[] fieldsInfo;
			if(inherit)
				fieldsInfo = type.GetFields();
			else
				fieldsInfo = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
			
            foreach (FieldInfo f_info in fieldsInfo) {
                PropertyMetadata p_data = new PropertyMetadata ();
                p_data.Info = f_info;
                p_data.IsField = true;

                props.Add (p_data);
            }

            lock (type_properties_lock) {
                try {
                    type_properties.Add (type, props);
                } catch (ArgumentException) {
                    return;
                }
            }
        }

        private static MethodInfo GetConvOp (Type t1, Type t2)
        {
            lock (conv_ops_lock) {
                if (! conv_ops.ContainsKey (t1))
                    conv_ops.Add (t1, new Dictionary<Type, MethodInfo> ());
            }

            if (conv_ops[t1].ContainsKey (t2))
                return conv_ops[t1][t2];

            MethodInfo op = t1.GetMethod (
                "op_Implicit", new Type[] { t2 });

            lock (conv_ops_lock) {
                try {
                    conv_ops[t1].Add (t2, op);
                } catch (ArgumentException) {
                    return conv_ops[t1][t2];
                }
            }

            return op;
        }

        private static object ReadValue (Type inst_type, JsonReader reader)
        {
            reader.Read ();

            if (reader.Token == JsonToken.ArrayEnd)
                return null;

            if (reader.Token == JsonToken.Null) {

                if (! inst_type.IsClass)
                    throw new JsonException (String.Format (
                            "Can't assign null to an instance of type {0}",
                            inst_type));

                return null;
            }

			/*UnityEngine.Debug.Log (inst_type.ToString());
			UnityEngine.Debug.Log (reader.Token);*/
				
            if (reader.Token == JsonToken.Double ||
                reader.Token == JsonToken.Int ||
                reader.Token == JsonToken.Long ||
                reader.Token == JsonToken.String ||
                reader.Token == JsonToken.Boolean) {

                Type json_type = reader.Value.GetType ();

				/*Debug.Log (json_type);
				Debug.Log (inst_type);*/

                if (inst_type.IsAssignableFrom (json_type))
                    return reader.Value;

                // If there's a custom importer that fits, use it
                if (custom_importers_table.ContainsKey (json_type) &&
                    custom_importers_table[json_type].ContainsKey (
                        inst_type)) {

                    ImporterFunc importer =
                        custom_importers_table[json_type][inst_type];

                    return importer (reader.Value);
                }

                // Maybe there's a base importer that works
                if (base_importers_table.ContainsKey (json_type) &&
                    base_importers_table[json_type].ContainsKey (
                        inst_type)) {

                    ImporterFunc importer =
                        base_importers_table[json_type][inst_type];

                    return importer (reader.Value);
                }

                // Maybe it's an enum
                if (inst_type.IsEnum)
                    return Enum.ToObject (inst_type, reader.Value);

                // Try using an implicit conversion operator
                MethodInfo conv_op = GetConvOp (inst_type, json_type);

                if (conv_op != null)
                    return conv_op.Invoke (null,
                                           new object[] { reader.Value });

                // No luck
                throw new JsonException (String.Format (
                        "Can't assign value '{0}' (type {1}) to type {2}",
                        reader.Value, json_type, inst_type));
            }

            object instance = null;

            if (reader.Token == JsonToken.ArrayStart) {

                AddArrayMetadata (inst_type);
                ArrayMetadata t_data = array_metadata[inst_type];

                if (! t_data.IsArray && ! t_data.IsList)
                    throw new JsonException (String.Format (
                            "Type {0} can't act as an array",
                            inst_type));

                IList list;
                Type elem_type;

                if (! t_data.IsArray) {
                    list = (IList) Activator.CreateInstance (inst_type);
                    elem_type = t_data.ElementType;
                } else {
                    list = new ArrayList ();
                    elem_type = inst_type.GetElementType ();
                }

                while (true) {
					object item;

					if (custom_importers_table.ContainsKey (typeof(JsonReader)) &&
					    custom_importers_table[typeof(JsonReader)].ContainsKey (
						elem_type)) 
					{	
						reader.Read();
						if(reader.Token == JsonToken.ArrayEnd)
							break;

						ImporterFunc importer =
							custom_importers_table[typeof(JsonReader)][elem_type];
												
						item = importer (reader);
					}
					else
					{
                    	item = ReadValue (elem_type, reader);
					}

                    if (item == null && reader.Token == JsonToken.ArrayEnd)
                        break;

                    list.Add (item);
                }

                if (t_data.IsArray) {
                    int n = list.Count;
                    instance = Array.CreateInstance (elem_type, n);

                    for (int i = 0; i < n; i++)
                        ((Array) instance).SetValue (list[i], i);
                } else
                    instance = list;

            } 
			else if (reader.Token == JsonToken.ObjectStart) 
			{	
				if (custom_importers_table.ContainsKey (typeof(string)) &&
				    custom_importers_table[typeof(string)].ContainsKey (
					inst_type)) {
					
					ImporterFunc importer =
						custom_importers_table[typeof(string)][inst_type];

					return importer (reader.Value);
				}

				System.Type init_type = inst_type;
				if(inst_type.IsAbstract)
				{
					Dictionary<string, System.Type> childTypes = ReflectionUtils.GetChildTypes(inst_type);	
					while(reader.Read() && reader.Token != JsonToken.PropertyName);

					if(childTypes.ContainsKey(reader.Value.ToString()))
					{
                        inst_type = childTypes[reader.Value.ToString()];
					}
					else
					{
						throw new JsonException (String.Format ("Attempting to deserialize JSON of type '{0}'. However, the type '{1}' doesn't inherit from the abstract type '{2}'", inst_type, reader.Value, inst_type));
					}
					
					while(reader.Read() && reader.Token != JsonToken.ObjectStart);
				}

				//Debug.Log ("This is my type: " + inst_type);
                AddObjectMetadata (inst_type);
                ObjectMetadata t_data = object_metadata[inst_type];
				             
				instance = Activator.CreateInstance (inst_type);
			
                while (true) 
				{
                    reader.Read ();

                    if (reader.Token == JsonToken.ObjectEnd)
					{
						if(init_type != inst_type)
							while(reader.Read() && reader.Token != JsonToken.ObjectEnd);
				
						break;
					}

                    string property = (string) reader.Value;
				
                    if (t_data.Properties.ContainsKey (property)) 
					{
                        PropertyMetadata prop_data =
                            t_data.Properties[property];

                        if (prop_data.IsField) {
                            ((FieldInfo) prop_data.Info).SetValue (
                                instance, ReadValue (prop_data.Type, reader));
                        } else {
                            PropertyInfo p_info =
                                (PropertyInfo) prop_data.Info;

                            if (p_info.CanWrite)
                                p_info.SetValue (
                                    instance,
                                    ReadValue (prop_data.Type, reader),
                                    null);
                            else
                                ReadValue (prop_data.Type, reader);
                        }

                    } 
					else 
					{
                        if (! t_data.IsDictionary) {

                            if (! reader.SkipNonMembers) {
                                throw new JsonException (String.Format (
                                        "The type {0} doesn't have the " +
                                        "property '{1}'",
                                        inst_type, property));
                            } else {
                                ReadSkip (reader);
                                continue;
                            }
                        }

						Type[] arguments = ((IDictionary) instance).GetType().GetGenericArguments();
						Type keyType = arguments[0];

						if(keyType == typeof(System.Int32))
						{
							((IDictionary) instance).Add (
								int.Parse(property), ReadValue (
								arguments[1], reader));
						}
						else
						{
							((IDictionary) instance).Add (
								property, ReadValue (
								t_data.ElementType, reader));
						}
                    }
                }

            }

            return instance;
        }

        private static IJsonWrapper ReadValue (WrapperFactory factory,
                                               JsonReader reader)
        {
            reader.Read ();

            if (reader.Token == JsonToken.ArrayEnd ||
                reader.Token == JsonToken.Null)
                return null;

            IJsonWrapper instance = factory ();

            if (reader.Token == JsonToken.String) {
                instance.SetString ((string) reader.Value);
                return instance;
            }

            if (reader.Token == JsonToken.Double) {
                instance.SetDouble ((double) reader.Value);
                return instance;
            }

            if (reader.Token == JsonToken.Int) {
                instance.SetInt ((int) reader.Value);
                return instance;
            }

            if (reader.Token == JsonToken.Long) {
                instance.SetLong ((long) reader.Value);
                return instance;
            }

            if (reader.Token == JsonToken.Boolean) {
                instance.SetBoolean ((bool) reader.Value);
                return instance;
            }

            if (reader.Token == JsonToken.ArrayStart) {
                instance.SetJsonType (JsonType.Array);

                while (true) {
                    IJsonWrapper item = ReadValue (factory, reader);
                    if (item == null && reader.Token == JsonToken.ArrayEnd)
                        break;

                    ((IList) instance).Add (item);
                }
            }
            else if (reader.Token == JsonToken.ObjectStart) {
                instance.SetJsonType (JsonType.Object);

                while (true) {
                    reader.Read ();

                    if (reader.Token == JsonToken.ObjectEnd)
                        break;

                    string property = (string) reader.Value;

                    ((IDictionary) instance)[property] = ReadValue (
                        factory, reader);
                }

            }

            return instance;
        }

        private static void ReadSkip (JsonReader reader)
        {
            ToWrapper (
                delegate { return new JsonMockWrapper (); }, reader);
        }

        private static void RegisterBaseExporters ()
        {
            base_exporters_table[typeof (byte)] =
                delegate (object obj, JsonWriter writer) {
                    writer.Write (Convert.ToInt32 ((byte) obj));
                };

            base_exporters_table[typeof (char)] =
                delegate (object obj, JsonWriter writer) {
                    writer.Write (Convert.ToString ((char) obj));
                };

            base_exporters_table[typeof (DateTime)] =
                delegate (object obj, JsonWriter writer) {
                    writer.Write (Convert.ToString ((DateTime) obj,
                                                    datetime_format));
                };

            base_exporters_table[typeof (decimal)] =
                delegate (object obj, JsonWriter writer) {
                    writer.Write ((decimal) obj);
                };

            base_exporters_table[typeof (sbyte)] =
                delegate (object obj, JsonWriter writer) {
                    writer.Write (Convert.ToInt32 ((sbyte) obj));
                };

            base_exporters_table[typeof (short)] =
                delegate (object obj, JsonWriter writer) {
                    writer.Write (Convert.ToInt32 ((short) obj));
                };

            base_exporters_table[typeof (ushort)] =
                delegate (object obj, JsonWriter writer) {
                    writer.Write (Convert.ToInt32 ((ushort) obj));
                };

            base_exporters_table[typeof (uint)] =
                delegate (object obj, JsonWriter writer) {
                    writer.Write (Convert.ToUInt64 ((uint) obj));
                };

            base_exporters_table[typeof (ulong)] =
                delegate (object obj, JsonWriter writer) {
                    writer.Write ((ulong) obj);
                };

            base_exporters_table[typeof (float)] =
                delegate (object obj, JsonWriter writer) {
                    writer.Write ((float) obj);
                };
        }

        private static void RegisterBaseImporters ()
        {
            ImporterFunc importer;

            importer = delegate (object input) {
                return Convert.ToByte ((int) input);
            };
            RegisterImporter (base_importers_table, typeof (int),
                              typeof (byte), importer);

            importer = delegate (object input) {
                return Convert.ToUInt64 ((int) input);
            };
            RegisterImporter (base_importers_table, typeof (int),
                              typeof (ulong), importer);

            importer = delegate (object input) {
                return Convert.ToSByte ((int) input);
            };
            RegisterImporter (base_importers_table, typeof (int),
                              typeof (sbyte), importer);

            importer = delegate (object input) {
                return Convert.ToInt16 ((int) input);
            };
            RegisterImporter (base_importers_table, typeof (int),
                              typeof (short), importer);

            importer = delegate (object input) {
                return Convert.ToUInt16 ((int) input);
            };
            RegisterImporter (base_importers_table, typeof (int),
                              typeof (ushort), importer);

            importer = delegate (object input) {
                return Convert.ToUInt32 ((int) input);
            };
            RegisterImporter (base_importers_table, typeof (int),
                              typeof (uint), importer);

            importer = delegate (object input) {
                return Convert.ToSingle ((int) input);
            };
            RegisterImporter (base_importers_table, typeof (int),
                              typeof (float), importer);

            importer = delegate (object input) {
                return Convert.ToDouble ((int) input);
            };
            RegisterImporter (base_importers_table, typeof (int),
                              typeof (double), importer);

            importer = delegate (object input) {
                return Convert.ToDecimal ((double) input);
            };
            RegisterImporter (base_importers_table, typeof (double),
                              typeof (decimal), importer);


            importer = delegate (object input) {
                return Convert.ToUInt32 ((long) input);
            };
            RegisterImporter (base_importers_table, typeof (long),
                              typeof (uint), importer);

            importer = delegate (object input) {
                return Convert.ToChar ((string) input);
            };
            RegisterImporter (base_importers_table, typeof (string),
                              typeof (char), importer);

            importer = delegate (object input) {
                return Convert.ToDateTime ((string) input, datetime_format);
            };
            RegisterImporter (base_importers_table, typeof (string),
                              typeof (DateTime), importer);

            importer = delegate (object input) {
                return Convert.ToInt32 (input);
            };
            RegisterImporter (base_importers_table, typeof (string),
                              typeof (Int32), importer);

            importer = delegate (object input) {
                return Convert.ToSingle (input);
            };
            RegisterImporter (base_importers_table, typeof (string),
                              typeof (float), importer);

            importer = delegate (object input) {
                return Convert.ToSingle(input);
            };
            RegisterImporter (base_importers_table, typeof (double),
                              typeof (float), importer);
        }

        private static void RegisterImporter (
            IDictionary<Type, IDictionary<Type, ImporterFunc>> table,
            Type json_type, Type value_type, ImporterFunc importer)
        {
            if (! table.ContainsKey (json_type))
                table.Add (json_type, new Dictionary<Type, ImporterFunc> ());

            table[json_type][value_type] = importer;
        }

        private static void WriteValue (object obj, JsonWriter writer,
                                        bool writer_is_private,
                                        int depth)
        {		
            if (depth > max_nesting_depth)
                throw new JsonException (
                    String.Format ("Max allowed object depth reached while " +
                                   "trying to export from type {0}{1}",
                                   obj.GetType (), obj));

			//Null handler
            if (obj == null) {
                writer.Write (null);
                return;
            }
			
			Type obj_type = obj.GetType ();

            // See if there's a custom exporter for the object FIRST as in BEFORE ANYTHING ELSE!!! Love, Mark
            if (custom_exporters_table.ContainsKey (obj_type)) {
                ExporterFunc exporter = custom_exporters_table[obj_type];
                exporter (obj, writer);

                return;
            }
			
            if (obj is IJsonWrapper) {
                if (writer_is_private)
                    writer.TextWriter.Write (((IJsonWrapper) obj).ToJson ());
                else
                    ((IJsonWrapper) obj).ToJson (writer);

                return;
            }
			
			/*** Check Base Types ***/

			if(obj.GetType().ToString() == "System.MonoType")
			{
				writer.Write (obj.ToString());
				return;
			}

            if (obj is String) {
                writer.Write ((string) obj);
                return;
            }

            if (obj is Double) {
                writer.Write ((double) obj);
                return;
            }

            if (obj is Int32) {
                writer.Write ((int) obj);
                return;
            }

            if (obj is Boolean) {
                writer.Write ((bool) obj);
                return;
            }

            if (obj is Int64) {
                writer.Write ((long) obj);
                return;
            }

            if (obj is Array) {
                writer.WriteArrayStart ();

                foreach (object elem in (Array) obj)
                    WriteValue (elem, writer, writer_is_private, depth + 1);

                writer.WriteArrayEnd ();

                return;
            }

            if (obj is IList) {
				Type valueType = obj.GetType().GetGenericArguments()[0];
				writer.WriteArrayStart ();
                foreach (object elem in (IList) obj)
					if(!valueType.IsAbstract)
						WriteValue (elem, writer, writer_is_private, depth + 1);
					else
					{
						writer.WriteObjectStart();
						writer.WritePropertyName(elem.GetType().ToString());
	                    WriteValue (elem, writer, writer_is_private, depth + 1);						
						writer.WriteObjectEnd();						
					}
                writer.WriteArrayEnd ();

                return;
            }

            if (obj is IDictionary) {
                writer.WriteObjectStart ();
				IDictionary dict = (IDictionary) obj;

				Type curType = obj.GetType();
				bool isDict = typeof(IDictionary).IsAssignableFrom(curType);
				while(isDict)
				{
					isDict = typeof(IDictionary).IsAssignableFrom(curType.BaseType);
					if(isDict)
						curType = curType.BaseType;
				}

				Type valueType = curType.GetGenericArguments()[1];
                foreach (DictionaryEntry entry in dict) {
					//This next line means we can't have anything but base types as keys. Love, Mark
					//writer.WritePropertyName (entry.Key.ToString());
					if(IsBaseType(entry.Key))
						writer.WritePropertyName (entry.Key.ToString());
					else
					{
						JsonWriter newWriter = new JsonWriter();
						JsonMapper.ToJson(entry.Key, newWriter);
						//string key = writer.JsonToString(newWriter.ToString());
						string key = newWriter.ToString();
						writer.WritePropertyName(key);
					}

					if(!valueType.IsAbstract)
					{
	                    WriteValue (entry.Value, writer, writer_is_private, depth + 1);
					}
					else
					{
						//Creates a second layer that stores the child type key of the object for decoding
						writer.WriteObjectStart();
						if(entry.Value != null)
							writer.WritePropertyName(entry.Value.GetType().ToString());
						else
							writer.WritePropertyName("null");

						WriteValue (entry.Value, writer, writer_is_private, depth + 1);						
						writer.WriteObjectEnd();
					}
                }
                writer.WriteObjectEnd ();

                return;
            }
						
            /*Type obj_type = obj.GetType ();

            // See if there's a custom exporter for the object
            if (custom_exporters_table.ContainsKey (obj_type)) {
                ExporterFunc exporter = custom_exporters_table[obj_type];
                exporter (obj, writer);

                return;
            }*/

            // If not, maybe there's a base exporter
            if (base_exporters_table.ContainsKey (obj_type)) {
                ExporterFunc exporter = base_exporters_table[obj_type];
                exporter (obj, writer);

                return;
            }

            // Last option, let's see if it's an enum
            if (obj is Enum) {
                Type e_type = Enum.GetUnderlyingType (obj_type);

                if (e_type == typeof (long)
                    || e_type == typeof (uint)
                    || e_type == typeof (ulong))
                    writer.Write ((ulong) obj);
                else
                    writer.Write ((int) obj);

                return;
            }

            // Okay, so it looks like the input should be exported as an
            // object
				
	        AddTypeProperties (obj_type);
            IList<PropertyMetadata> props = type_properties[obj_type];
			
            writer.WriteObjectStart ();
            foreach (PropertyMetadata p_data in props) 
			{
				bool skip = false;
				bool force = false;
				System.Object[] attrs = p_data.Info.GetCustomAttributes(false);
				if(attrs.Length > 0)
				{
					for(int j = 0; j < attrs.Length; j++)
					{
						if(attrs[j].GetType() == typeof(SkipSerialization))
						{
							skip = true;
							break;
						}

						if(attrs[j].GetType() == typeof(ForceSerialization))
						{
							force = true;
							break;
						}
					}
				}
			
				if(skip)
					continue;

				if (p_data.IsField) 
				{
					FieldInfo f_info = ((FieldInfo) p_data.Info);
					if(f_info.GetValue(obj) == null && !force)
						continue;

                    writer.WritePropertyName (p_data.Info.Name);
					if(f_info.FieldType.IsAbstract)
					{
						writer.WriteObjectStart();
						writer.WritePropertyName(f_info.GetValue(obj).GetType().ToString());
						depth++;
					}
                    
					WriteValue (f_info.GetValue (obj),
                                writer, writer_is_private, depth + 1);
					
					if(f_info.FieldType.IsAbstract)
					{
            			writer.WriteObjectEnd ();
					}
                }
                else {
                    PropertyInfo p_info = (PropertyInfo) p_data.Info;
					
                    if (p_info.CanRead) 
					{
						object propertyValue = GetPropertyValue(obj, p_info);

						if(propertyValue == null && !force)
							continue;

                        writer.WritePropertyName (p_data.Info.Name);
						
						if(p_info.PropertyType.IsAbstract)
						{
							writer.WriteObjectStart();
							//writer.WritePropertyName(p_info.GetValue(obj, null).GetType().ToString());
							writer.WritePropertyName(propertyValue.GetType().ToString());
							depth++;
						}
						
						//WriteValue (p_info.GetValue (obj, null), writer, writer_is_private, depth + 1); 
						WriteValue (propertyValue, writer, writer_is_private, depth + 1);
						
						if(p_info.PropertyType.IsAbstract)
						{
	            			writer.WriteObjectEnd ();
						}
                    }
                }
            }
            writer.WriteObjectEnd ();
        }
        #endregion

		private static bool IsBaseType(object obj)
		{
			if(obj.GetType().ToString() == "System.MonoType") {
				return true;
			}
			
			if (obj is String) {
				return true;
			}
			
			if (obj is Double) {
				return true;
			}
			
			if (obj is Int32) {
				return true;
			}
			
			if (obj is Boolean) {
				return true;
			}
			
			if (obj is Int64) {
				return true;
			}

			return false;
		}

		private static object GetPropertyValue(object target, PropertyInfo info)
		{
			MethodInfo getter = info.GetGetMethod();
			return getter.Invoke(target, null);
		}

        public static string ToJson (object obj)
        {
            lock (static_writer_lock) {
                static_writer.Reset ();

                WriteValue (obj, static_writer, true, 0);

                return static_writer.ToString ();
            }
        }

        public static void ToJson (object obj, JsonWriter writer)
        {
            WriteValue (obj, writer, false, 0);
        }

        public static JsonData ToObject (JsonReader reader)
        {
            return (JsonData) ToWrapper (
                delegate { return new JsonData (); }, reader);
        }

        public static JsonData ToObject (TextReader reader)
        {
            JsonReader json_reader = new JsonReader (reader);

            return (JsonData) ToWrapper (
                delegate { return new JsonData (); }, json_reader);
        }

        public static JsonData ToObject (string json)
        {
            return (JsonData) ToWrapper (
                delegate { return new JsonData (); }, json);
        }

        public static T ToObject<T> (JsonReader reader)
        {
            return (T) ReadValue (typeof (T), reader);
        }

        public static T ToObject<T> (TextReader reader)
        {
            JsonReader json_reader = new JsonReader (reader);

            return (T) ReadValue (typeof (T), json_reader);
        }

        public static T ToObject<T> (string json)
        {
            JsonReader reader = new JsonReader (json);

            return (T) ReadValue (typeof (T), reader);
        }

        public static IJsonWrapper ToWrapper (WrapperFactory factory,
                                              JsonReader reader)
        {
            return ReadValue (factory, reader);
        }

        public static IJsonWrapper ToWrapper (WrapperFactory factory,
                                              string json)
        {
            JsonReader reader = new JsonReader (json);

            return ReadValue (factory, reader);
        }

        public static void RegisterExporter<T> (ExporterFunc<T> exporter)
        {
            ExporterFunc exporter_wrapper =
                delegate (object obj, JsonWriter writer) {
                    exporter ((T) obj, writer);
                };

            custom_exporters_table[typeof (T)] = exporter_wrapper;
        }

        public static void RegisterImporter<TJson, TValue> (
            ImporterFunc<TJson, TValue> importer)
        {
            ImporterFunc importer_wrapper =
                delegate (object input) {
                    return importer ((TJson) input);
                };

            RegisterImporter (custom_importers_table, typeof (TJson),
                              typeof (TValue), importer_wrapper);
        }

        public static void UnregisterExporters ()
        {
            custom_exporters_table.Clear ();
        }

        public static void UnregisterImporters ()
        {
            custom_importers_table.Clear ();
        }
    }
}
