﻿using System;
using System.Collections.Generic;
using System.Linq;
using MrCMS.Entities.Documents;
using MrCMS.Entities.Documents.Web;

namespace MrCMS.Helpers
{
    public static class DocumentTypeHelper
    {
        private static readonly List<DocumentTypeDefinition> _documentTypeDefinitions;

        static DocumentTypeHelper()
        {
            _documentTypeDefinitions = GetDocumentTypeDefinitions();
        }

        public static IEnumerable<DocumentTypeDefinition> WebpageDocumentTypeDefinitions
        {
            get { return DocumentTypeDefinitions.Where(x => x.Type != null && x.Type.IsSubclassOf(typeof(Webpage))); }
        }

        public static List<DocumentTypeDefinition> DocumentTypeDefinitions
        {
            get { return _documentTypeDefinitions; }
        }

        private static List<DocumentTypeDefinition> GetDocumentTypeDefinitions()
        {
            var list = new List<DocumentTypeDefinition>();

            foreach (var attribs in TypeHelper.GetAllConcreteTypesAssignableFrom<Document>().Select(type =>
                                                                    type.GetCustomAttributes(typeof(DocumentTypeDefinition), false))
                                                            .Where(attribs => attribs.Length > 0))
            {
                list.AddRange(attribs.Cast<DocumentTypeDefinition>());
            }
            return list.OrderBy(x => x.DisplayOrder).ToList();
        }

        public static Type GetTypeByName(string name)
        {
            return DocumentTypeDefinitions.Where(x => x.TypeName == name).Select(x => x.Type).FirstOrDefault();
        }

        public static string GetIconClass(Document document)
        {
            var documentTypeDefinition =
                DocumentTypeDefinitions.FirstOrDefault(x => document.GetType().Name.StartsWith(x.TypeName));

            return documentTypeDefinition != null ? documentTypeDefinition.IconClass : null;
        }

        public static DocumentTypeDefinition GetDefinitionByType(Type getType)
        {
            return DocumentTypeDefinitions.FirstOrDefault(x => x.Type.Name == getType.Name);
        }

        public static DocumentTypeDefinition GetDefinition(this Document document)
        {
            return DocumentTypeDefinitions.FirstOrDefault(x => x.Type.Name == document.DocumentType);
        }

        public static IEnumerable<DocumentTypeDefinition> GetValidWebpageDocumentTypes(this Webpage parent)
        {
            if (parent == null)
                return WebpageDocumentTypeDefinitions.Where(definition => !definition.RequiresParent);

            var documentTypeDefinition = WebpageDocumentTypeDefinitions.FirstOrDefault(definition => definition.TypeName == parent.Unproxy().GetType().Name);

            if (documentTypeDefinition == null) return Enumerable.Empty<DocumentTypeDefinition>();

            switch (documentTypeDefinition.ChildrenListType)
            {
                case ChildrenListType.BlackList:
                    return
                        WebpageDocumentTypeDefinitions.Except(
                            documentTypeDefinition.ChildrenList.Select(GetDefinitionByType)).Where(
                                def => !def.AutoBlacklist);
                case ChildrenListType.WhiteList:
                    return documentTypeDefinition.ChildrenList.Select(GetDefinitionByType);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static int? GetMaxChildNodes(this Document document)
        {
            var documentTypeDefinition = document.GetDefinition();
            return documentTypeDefinition != null && documentTypeDefinition.MaxChildNodes > 0
                       ? documentTypeDefinition.MaxChildNodes
                       : (int?) null;
        }
    }
}