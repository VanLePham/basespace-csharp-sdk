﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Illumina.BaseSpace.SDK.ServiceModels;
using Illumina.BaseSpace.SDK.Types;
using ServiceStack.Text;

namespace Illumina.BaseSpace.SDK.Deserialization
{
    public static class PropertyDeserializer
    {
        public static PropertyItemsResourceList JsonToPropertyItemsResourceList(string jsonString)
        {
            var json = JsonObject.Parse(jsonString);
            var ret = new PropertyItemsResourceList()
            {
                DisplayedCount = json["DisplayedCount"].To<int?>(),
                TotalCount = json["TotalCount"].To<int?>(),
                Limit = json["Limit"].To<int>(),
                Offset = json["Offset"].To<int>(),
                SortBy = json["SortBy"].To<PropertyItemsSortByParameters?>(),
                SortDir = json["SortDir"].To<SortDirection?>()
            };

            ret.Type = json["Type"];
            var simpleType = ret.Type.Replace(Property.TYPE_LIST_SUFFIX, String.Empty);

            switch (simpleType)
            {
                case Property.TYPE_STRING:
                    ret.Items =
                        json.ArrayObjects("Items")
                            .Select(
                                itemj =>
                                new PropertyItem()
                                {
                                    Id = itemj["Id"],
                                    Content = new PropertyContentLiteral(simpleType, itemj["Content"])
                                })
                            .ToArray();

                    break;
                default:
                    ret.Items =
                        json.ArrayObjects("Items")
                            .Select(
                                itemj =>
                                new PropertyItem()
                                {
                                    Id = itemj["Id"],
                                    Content = DeserializePropertyReference(simpleType, itemj.ToJson())
                                })
                            .ToArray();
                    break;
            }
            return ret; 
        }

        public static Property JsonToProperty(string jsonString)
        {
            var json = JsonObject.Parse(jsonString);
            var property = new Property()
            {
                Description = json.Get("Description"),
                Href = json.Get<Uri>("Href"),
                Type = json.Get("Type"),
                Name = json.Get("Name"),
                HrefItems = json.Get<Uri>("HrefItems"),
                ItemsDisplayedCount = json.Get<int?>("ItemsDisplayedCount"),
                ItemsTotalCount = json.Get<int?>("ItemsTotalCount")
            };

            var simpleType = property.GetSimpleType();
            if (json.ContainsKey("Content"))
            {
                switch (simpleType)
                {
                    case Property.TYPE_STRING:
                        property.Content = new PropertyContentLiteral(property.Type, json.Get("Content"));
                        break;
                    default:
                        property.Content = DeserializePropertyReference(simpleType, json.Child("Content"));
                        break;
                }
            }

            if (json.ContainsKey("Items"))
            {
                switch (simpleType)
                {
                    case Property.TYPE_STRING:
                        property.Items = json.Get<string[]>("Items").Select(i => new PropertyContentLiteral(simpleType, i)).ToArray();
                        break;
                    default:
                        property.Items = json.ArrayObjects("Items").Select(itemj => DeserializePropertyReference(simpleType, itemj.ToJson())).Where(x => x != null).ToArray();
                        break;
                }
            }

            return property;
        }

        public static IPropertyContent DeserializePropertyReference(string type, string json)
        {
            IPropertyContent ret = null;
            if (String.IsNullOrEmpty(type) || String.IsNullOrEmpty(json))
            {
                return null;
            }
            switch (type)
            {
                case Property.TYPE_PROJECT:
                    ret = JsonSerializer.DeserializeFromString<ProjectCompact>(json);
                    break;
                case Property.TYPE_APPRESULT:
                    ret = JsonSerializer.DeserializeFromString<AppResultCompact>(json);
                    break;
                case Property.TYPE_SAMPLE:
                    ret = JsonSerializer.DeserializeFromString<SampleCompact>(json);
                    break;
                case Property.TYPE_RUN:
                    ret = JsonSerializer.DeserializeFromString<RunCompact>(json);
                    break;
                case Property.TYPE_APPSESSION:
                    ret = JsonSerializer.DeserializeFromString<AppSessionCompact>(json);
                    break;
                case Property.TYPE_FILE:
                    ret = JsonSerializer.DeserializeFromString<FileCompact>(json);
                    break;
            }
            return ret;
        }
    }
}
