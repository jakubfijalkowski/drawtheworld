using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using FLib;

#if WINRT
using Windows.UI;
#else
using System.Windows.Media;
#endif

namespace DrawTheWorld.Core.UserData.Loaders
{
	static class XmlLoader
	{
		public static Pack Load(TextReader input, Guid id)
		{
			XDocument file = null;
			try
			{
				file = XDocument.Load(input);
			}
			catch (Exception ex)
			{
				throw new PackLoadException("Cannot load pack file - the file is corrupted.", ex);
			}

			var root = file.Root;
			if (root.Name != "pack")
				throw new PackLoadException("Invalid root element.");

			var version = root.GetAttributeValue("version");
			if (version == null || (version != "1" && version != "2"))
				throw new PackLoadException("File has invalid version or version is missing.");

			int versionAsInt = int.Parse(version);

			if (versionAsInt > 1 && id == default(Guid))
			{
				var packIdAttr = root.GetAttributeValue("id");
				if (string.IsNullOrWhiteSpace(packIdAttr) || !Guid.TryParse(packIdAttr, out id))
					id = Guid.NewGuid();
			}

			var name = root.Element("names").ParseTranslations("name");
			var description = root.Element("descriptions").ParseTranslations("desc");
			var author = root.GetElementValue("author");
			var authorsPage = root.GetElementValue("authorsPage");
			var boardsList = root.Element("boards");
			var boards = boardsList != null ? boardsList.Descendants("board").Select(b => b.ParseBoard(id, versionAsInt)).ToList() : new List<BoardData>();

			return new Pack(id, boards, name, description, author, authorsPage);
		}

		public static void Save(Pack pack, Stream output)
		{
			var root = new XElement("pack",
				new XAttribute("version", "2"),
				new XAttribute("id", pack.Id.ToString()),
				pack.Name.ToXml("names", "name"),
				pack.Description.ToXml("descriptions", "desc"),
				new XElement("author", pack.Author),
				new XElement("authorsPage", pack.AuthorsPage),
				new XElement("boards", pack.Boards.Select(b => b.ToXml()))
				);
			new XDocument(root).Save(output, SaveOptions.OmitDuplicateNamespaces | SaveOptions.DisableFormatting);
		}

		private static BoardData ParseBoard(this XElement element, Guid packId, int version)
		{
			if (element.Name != "board")
				throw new PackLoadException("Invalid element '{0}' in the boards list.".FormatWith(element.Name));

			var id = default(Guid);
			if (version > 1)
			{
				var idAttribute = element.Attribute("id");
				if (idAttribute == null || !Guid.TryParse(idAttribute.Value, out id))
					throw new PackLoadException("Cannot parse id of the board. Element is missing or value is invalid.");
			}
			else
			{
				id = Guid.NewGuid();
			}

			var name = element.Element("names").ParseTranslations("name");
			var width = element.ParseUIntAttribute("width");
			var height = element.ParseUIntAttribute("height");
			var palette = element.Element("palette").ParsePalette();
			var boardData = element.GetElementValue("data");
			Color?[] data = null;

			if (boardData != null)
			{
				var elements = boardData.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
				if (elements.Length != width * height)
					throw new PackLoadException("Invalid data size.");

				data = elements
					.Select(e => int.Parse(e))
					.Select(e => e == 0 ? (Color?)null : palette[e])
					.ToArray();
			}
			else
				data = new Color?[width * height];

			return new BoardData(packId, id, name, new Size(width, height), data);
		}

		private static Dictionary<int, Color> ParsePalette(this XElement element)
		{
			if (element == null)
				return new Dictionary<int, Color>();

			var colors = element
				.Descendants("color")
				.ToDictionary(
					c => c.ParseUIntAttribute("value", 1),
					c => Color.FromArgb(255, (byte)c.ParseUIntAttribute("R", 0, 255), (byte)c.ParseUIntAttribute("G", 0, 255), (byte)c.ParseUIntAttribute("B", 0, 255)));
			return colors;
		}

		private static Translations ParseTranslations(this XElement element, string elementName)
		{
			if (element == null)
				return new Translations();

			var translations = element
				.Descendants(elementName)
				.ToDictionary(e => e.GetAttributeValue("lang"), e => e.Value);
			return new Translations(translations);
		}

		private static string GetAttributeValue(this XElement root, XName name)
		{
			var e = root.Attribute(name);
			return e != null ? e.Value : null;
		}

		private static string GetElementValue(this XElement root, XName name)
		{
			var e = root.Element(name);
			return e != null ? e.Value : null;
		}

		private static int ParseUIntAttribute(this XElement root, XName name, int min = 0, int max = int.MaxValue)
		{
			var e = root.GetAttributeValue(name);
			if (e == null)
				throw new PackLoadException("Missing '{0}' attribute.".FormatWith(name.LocalName));
			int ret = 0;
			if (!int.TryParse(e, out ret) || ret < min || ret > max)
				throw new PackLoadException("Cannot parse attribute '{0}'.".FormatWith(name.LocalName));
			return ret;
		}

		private static XElement ToXml(this Translations trans, XName elementName, XName childName)
		{
			return
				new XElement(
					elementName,
					from t in trans
					select new XElement(childName, new XAttribute("lang", t.Key), t.Value)
					);
		}

		private static XElement ToXml(this BoardData board)
		{
			var palette = board.Data
				.Where(d => d.HasValue)
				.Distinct()
				.Select((d, i) => Tuple.Create(d.Value, i + 1))
				.ToDictionary(t => t.Item1, t => t.Item2);

			return
				new XElement("board",
					new XAttribute("id", board.Id.ToString()),
					new XAttribute("width", board.Size.Width),
					new XAttribute("height", board.Size.Height),
					board.Name.ToXml("names", "name"),
					palette.ToXml(),
					board.Data.ToXml(palette)
				);
		}

		private static XElement ToXml(this Dictionary<Color, int> palette)
		{
			return
				new XElement("palette",
					from e in palette
					select new XElement("color",
						new XAttribute("value", e.Value),
						new XAttribute("R", e.Key.R),
						new XAttribute("G", e.Key.G),
						new XAttribute("B", e.Key.B)
					));
		}

		private static XElement ToXml(this IReadOnlyList<Color?> data, Dictionary<Color, int> palette)
		{
			return new XElement("data",
				string.Join(" ", data.Select(c => c.HasValue ? palette[c.Value].ToString() : "0"))
				);
		}
	}
}
