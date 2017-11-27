using System.Collections.Generic;
using System.Linq;

namespace music163diff
{
    static class Render
    {
        public static string h(string name, Dictionary<string, string> attrbutes = null, string content = null)
        {
            var attrbutesString =
                attrbutes == null ? "" : string.Join(" ", attrbutes.Select(i => $"{i.Key}=\"{i.Value}\""));

            return $"<{name} {attrbutesString}>{content}</{name}>";
        }

        public static string section(Dictionary<string, string> attrbutes = null, string content = null) =>
            h("section", attrbutes, content);

        public static string div(Dictionary<string, string> attrbutes = null, string content = null) =>
            h("div", attrbutes, content);

        public static string p(Dictionary<string, string> attrbutes = null, string content = null) =>
            h("p", attrbutes, content);

        public static string a(Dictionary<string, string> attrbutes = null, string content = null) =>
            h("a", attrbutes, content);

        public static string img(Dictionary<string, string> attrbutes = null, string content = null) =>
            h("img", attrbutes, content);

        public static string span(Dictionary<string, string> attrbutes = null, string content = null) =>
            h("span", attrbutes, content);

        public static string table(Dictionary<string, string> attrbutes = null, string content = null) =>
            h("table", attrbutes, content);

        public static string th(Dictionary<string, string> attrbutes = null, string content = null) =>
            h("th", attrbutes, content);

        public static string tr(Dictionary<string, string> attrbutes = null, string content = null) =>
            h("tr", attrbutes, content);

        public static string td(Dictionary<string, string> attrbutes = null, string content = null) =>
            h("td", attrbutes, content);

        public static string ul(Dictionary<string, string> attrbutes = null, string content = null) =>
            h("ul", attrbutes, content);

        public static string li(Dictionary<string, string> attrbutes = null, string content = null) =>
            h("li", attrbutes, content);

        public static string details(Dictionary<string, string> attrbutes = null, string content = null) =>
            h("details", attrbutes, content);

        public static string summary(Dictionary<string, string> attrbutes = null, string content = null) =>
            h("summary", attrbutes, content);
    }
}