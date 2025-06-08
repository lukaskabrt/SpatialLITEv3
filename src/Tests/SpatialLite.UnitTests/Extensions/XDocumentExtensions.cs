using System.Xml.Linq;

namespace SpatialLite.UnitTests.Extensions;

/// <summary>
/// Extends XDocument with correct DeepEquals function
/// </summary>
public static class XDocumentExtensions
{
    public static bool DeepEqualsWithNormalization(XDocument doc1, XDocument doc2)
    {
        if (doc1.Root == null || doc2.Root == null)
        {
            return false;
        }

        return XMLCompare(doc1.Root, doc2.Root);
    }

    public static bool XMLCompare(XElement ele1, XElement ele2)
    {
        if (ele1.HasAttributes)
        {
            if (ele1.Attributes().Count() != ele2.Attributes().Count())
            {
                return false;
            }

            foreach (var attr in ele1.Attributes())
            {
                if (ele2.Attribute(attr.Name.LocalName) == null)
                {
                    return false;
                }

                if (!attr.Value.Equals(ele2.Attribute(attr.Name.LocalName)?.Value, StringComparison.CurrentCultureIgnoreCase))
                {
                    return false;
                }
            }
        }

        if (ele1.HasElements)
        {
            if (ele1.Elements().Count() != ele2.Elements().Count())
            {
                return false;
            }

            for (var i = 0; i <= ele1.Elements().Count() - 1; i++)
            {
                if (XMLCompare(ele1.Elements().ElementAt(i), ele2.Elements().ElementAt(i)) == false)
                {
                    return false;
                }
            }
        }

        return true;
    }
}
