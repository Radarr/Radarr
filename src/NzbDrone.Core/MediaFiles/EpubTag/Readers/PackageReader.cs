using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Xml.Linq;
using VersOne.Epub.Schema;
using VersOne.Epub.Utils;

namespace VersOne.Epub.Internal
{
    public static class PackageReader
    {
        public static async Task<EpubPackage> ReadPackageAsync(ZipArchive epubArchive, string rootFilePath)
        {
            var rootFileEntry = epubArchive.GetEntry(rootFilePath);
            if (rootFileEntry == null)
            {
                throw new Exception("EPUB parsing error: root file not found in archive.");
            }

            XDocument containerDocument;

            using (var containerStream = rootFileEntry.Open())
            {
                containerDocument = await XmlUtils.LoadDocumentAsync(containerStream).ConfigureAwait(false);
            }

            XNamespace opfNamespace = "http://www.idpf.org/2007/opf";
            var packageNode = containerDocument.Element(opfNamespace + "package");
            var result = new EpubPackage();
            var epubVersionValue = packageNode.Attribute("version").Value;
            EpubVersion epubVersion;
            switch (epubVersionValue)
            {
                case "1.0":
                case "2.0":
                    epubVersion = EpubVersion.EPUB_2;
                    break;
                case "3.0":
                    epubVersion = EpubVersion.EPUB_3_0;
                    break;
                case "3.1":
                    epubVersion = EpubVersion.EPUB_3_1;
                    break;
                default:
                    throw new Exception($"Unsupported EPUB version: {epubVersionValue}.");
            }

            result.EpubVersion = epubVersion;
            var metadataNode = packageNode.Element(opfNamespace + "metadata");
            if (metadataNode == null)
            {
                throw new Exception("EPUB parsing error: metadata not found in the package.");
            }

            var metadata = ReadMetadata(metadataNode, result.EpubVersion);
            result.Metadata = metadata;

            return result;
        }

        private static EpubMetadata ReadMetadata(XElement metadataNode, EpubVersion epubVersion)
        {
            var result = new EpubMetadata
            {
                Titles = new List<string>(),
                Creators = new List<EpubMetadataCreator>(),
                Subjects = new List<string>(),
                Publishers = new List<string>(),
                Contributors = new List<EpubMetadataContributor>(),
                Dates = new List<EpubMetadataDate>(),
                Types = new List<string>(),
                Formats = new List<string>(),
                Identifiers = new List<EpubMetadataIdentifier>(),
                Sources = new List<string>(),
                Languages = new List<string>(),
                Relations = new List<string>(),
                Coverages = new List<string>(),
                Rights = new List<string>(),
                MetaItems = new List<EpubMetadataMeta>()
            };

            foreach (var metadataItemNode in metadataNode.Elements())
            {
                var innerText = metadataItemNode.Value;
                switch (metadataItemNode.GetLowerCaseLocalName())
                {
                    case "title":
                        result.Titles.Add(innerText);
                        break;
                    case "creator":
                        var creator = ReadMetadataCreator(metadataItemNode);
                        result.Creators.Add(creator);
                        break;
                    case "subject":
                        result.Subjects.Add(innerText);
                        break;
                    case "description":
                        result.Description = innerText;
                        break;
                    case "publisher":
                        result.Publishers.Add(innerText);
                        break;
                    case "contributor":
                        var contributor = ReadMetadataContributor(metadataItemNode);
                        result.Contributors.Add(contributor);
                        break;
                    case "date":
                        var date = ReadMetadataDate(metadataItemNode);
                        result.Dates.Add(date);
                        break;
                    case "type":
                        result.Types.Add(innerText);
                        break;
                    case "format":
                        result.Formats.Add(innerText);
                        break;
                    case "identifier":
                        var identifier = ReadMetadataIdentifier(metadataItemNode);
                        result.Identifiers.Add(identifier);
                        break;
                    case "source":
                        result.Sources.Add(innerText);
                        break;
                    case "language":
                        result.Languages.Add(innerText);
                        break;
                    case "relation":
                        result.Relations.Add(innerText);
                        break;
                    case "coverage":
                        result.Coverages.Add(innerText);
                        break;
                    case "rights":
                        result.Rights.Add(innerText);
                        break;
                    case "meta":
                        if (epubVersion == EpubVersion.EPUB_2)
                        {
                            var meta = ReadMetadataMetaVersion2(metadataItemNode);
                            result.MetaItems.Add(meta);
                        }
                        else if (epubVersion == EpubVersion.EPUB_3_0 || epubVersion == EpubVersion.EPUB_3_1)
                        {
                            var meta = ReadMetadataMetaVersion3(metadataItemNode);
                            result.MetaItems.Add(meta);
                        }

                        break;
                }
            }

            return result;
        }

        private static EpubMetadataCreator ReadMetadataCreator(XElement metadataCreatorNode)
        {
            var result = new EpubMetadataCreator();
            foreach (var metadataCreatorNodeAttribute in metadataCreatorNode.Attributes())
            {
                var attributeValue = metadataCreatorNodeAttribute.Value;
                switch (metadataCreatorNodeAttribute.GetLowerCaseLocalName())
                {
                    case "role":
                        result.Role = attributeValue;
                        break;
                    case "file-as":
                        result.FileAs = attributeValue;
                        break;
                }
            }

            result.Creator = metadataCreatorNode.Value;
            return result;
        }

        private static EpubMetadataContributor ReadMetadataContributor(XElement metadataContributorNode)
        {
            var result = new EpubMetadataContributor();
            foreach (var metadataContributorNodeAttribute in metadataContributorNode.Attributes())
            {
                var attributeValue = metadataContributorNodeAttribute.Value;
                switch (metadataContributorNodeAttribute.GetLowerCaseLocalName())
                {
                    case "role":
                        result.Role = attributeValue;
                        break;
                    case "file-as":
                        result.FileAs = attributeValue;
                        break;
                }
            }

            result.Contributor = metadataContributorNode.Value;
            return result;
        }

        private static EpubMetadataDate ReadMetadataDate(XElement metadataDateNode)
        {
            var result = new EpubMetadataDate();
            var eventAttribute = metadataDateNode.Attribute(metadataDateNode.Name.Namespace + "event");
            if (eventAttribute != null)
            {
                result.Event = eventAttribute.Value;
            }

            result.Date = metadataDateNode.Value;

            return result;
        }

        private static EpubMetadataIdentifier ReadMetadataIdentifier(XElement metadataIdentifierNode)
        {
            var result = new EpubMetadataIdentifier();
            foreach (var metadataIdentifierNodeAttribute in metadataIdentifierNode.Attributes())
            {
                var attributeValue = metadataIdentifierNodeAttribute.Value;
                switch (metadataIdentifierNodeAttribute.GetLowerCaseLocalName())
                {
                    case "id":
                        result.Id = attributeValue;
                        break;
                    case "opf:scheme":
                        result.Scheme = attributeValue;
                        break;
                }
            }

            result.Identifier = metadataIdentifierNode.Value;
            return result;
        }

        private static EpubMetadataMeta ReadMetadataMetaVersion2(XElement metadataMetaNode)
        {
            var result = new EpubMetadataMeta();
            foreach (var metadataMetaNodeAttribute in metadataMetaNode.Attributes())
            {
                var attributeValue = metadataMetaNodeAttribute.Value;
                switch (metadataMetaNodeAttribute.GetLowerCaseLocalName())
                {
                    case "name":
                        result.Name = attributeValue;
                        break;
                    case "content":
                        result.Content = attributeValue;
                        break;
                }
            }

            return result;
        }

        private static EpubMetadataMeta ReadMetadataMetaVersion3(XElement metadataMetaNode)
        {
            var result = new EpubMetadataMeta();
            foreach (var metadataMetaNodeAttribute in metadataMetaNode.Attributes())
            {
                var attributeValue = metadataMetaNodeAttribute.Value;
                switch (metadataMetaNodeAttribute.GetLowerCaseLocalName())
                {
                    case "id":
                        result.Id = attributeValue;
                        break;
                    case "refines":
                        result.Refines = attributeValue;
                        break;
                    case "property":
                        result.Property = attributeValue;
                        break;
                    case "scheme":
                        result.Scheme = attributeValue;
                        break;
                }
            }

            result.Content = metadataMetaNode.Value;
            return result;
        }
    }
}
