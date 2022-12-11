
using Gliese.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Markdig;

namespace Gliese.Models
{

    [Table("articles")]
    [PrimaryKey(nameof(Pk))]
    public class ArticleModel
    {
        [Column("pk")]
        public string Pk { get; set; } = "";
        [Column("title")]
        public string Title { get; set; } = "";
        [Column("body")]
        public string Body { get; set; } = "";
        [Column("create_time")]
        public DateTime CreateTime { get; set; } = DateTime.MinValue;
        [Column("update_time")]
        public DateTime UpdateTime { get; set; } = DateTime.MinValue;
        [Column("creator")]
        public string Creator { get; set; } = "";
        [Column("keywords")]
        public string? Keywords { get; set; } = "";
        [Column("description")]
        public string? Description { get; set; } = "";
        [Column("mark_lang")]
        public int MarkLang { get; set; } = 0;
        [Column("status")]
        public int Status { get; set; } = 0;
        [Column("mark_text")]
        public string? MarkText { get; set; } = "";
        [Column("cover")]
        public string? Cover { get; set; } = "";

        public ArticleReadViewModel ToViewModel()
        {
            var tocList = new List<TocItem>();

            tocList.Add(new TocItem
            {
                Title = this.Title,
                Header = 0,
            });
            var bodyHtmlBuilder = new StringBuilder();

            if (this.MarkLang == 1)
            {
                if (this.MarkText != null)
                {
                    var result = Markdown.ToHtml(this.MarkText);

                    bodyHtmlBuilder.Append(result);
                }
            }
            else
            {
                var deserialized = JsonConvert.DeserializeObject(this.Body);
                var bodyHtml = buildBody(tocList, deserialized);
                //Console.WriteLine("xxxx");
                bodyHtmlBuilder.Append(bodyHtml);
            }
            var viewModel = new ArticleReadViewModel();
            viewModel.Article = this;
            if (this.Keywords != null)
                viewModel.KeywordsList = this.Keywords.Split(",");

            //var bodyHtml = bodyHtmlBuilder.ToString();
            viewModel.BodyHtml = bodyHtmlBuilder.ToString();

            return viewModel;
        }

        string buildBody(List<TocItem> tocList, object? node)
        {
            if (node == null)
                return "";
            var token = node as JToken;
            if (token == null)
                return "";
            var children = token["children"] as JArray;
            if (children == null)
                return "";
            var sb = new StringBuilder();
            foreach (JObject item in children)
            {
                var nodeStr = buildNode(tocList, item);
                sb.Append(nodeStr);
            }

            return sb.ToString();
        }

        string buildNode(List<TocItem> tocList, JObject node)
        {
            var nodeName = ((string?)node["name"]);
            if (nodeName == null)
                return "";
            switch (nodeName)
            {
                case "paragraph":
                    return buildParagraph(tocList, node);
                case "header":
                    return buildHeader(tocList, node);
                case "code-block":
                    return buildCodeBlock(node);
            }

            return "";
        }

        string buildParagraph(List<TocItem> tocList, JObject node)
        {
            var children = node["children"] as JArray;
            if (children == null)
                return "";

            var sb = new StringBuilder();
            sb.Append("<p>");
            foreach (JObject item in children)
            {
                var nodeStr = buildText(item);
                sb.Append(nodeStr);
            }
            sb.Append("</p>");

            return sb.ToString();
        }
        string buildText(JObject node)
        {
            var text = ((string?)node["text"]);
            if (text == null)
                return "";

            return text;
        }
        string buildHeader(List<TocItem> tocList, JObject node)
        {
            var header = ((int?)node["header"]);
            if (header == null)
                return "";

            var children = node["children"] as JArray;
            if (children == null)
                return "";

            var sb = new StringBuilder();
            sb.Append($"<h{header}>");
            var headerTitle = "";
            foreach (JObject item in children)
            {
                var nodeStr = buildText(item);
                headerTitle += nodeStr;
            }
            tocList.Add(new TocItem
            {
                Title = headerTitle,
                Header = (int)header,
            });
            sb.Append(headerTitle);
            sb.Append($"</h{header}>");

            return sb.ToString();
        }
        string buildCodeBlock(JObject node)
        {
            var language = ((string?)node["language"]);
            if (language == null)
                return "";

            var children = node["children"] as JArray;
            if (children == null)
                return "";

            var sb = new StringBuilder();
            sb.Append($"<pre class='code' data-lang='{language}'><code>");
            foreach (JObject item in children)
            {
                var nodeStr = buildText(item);
                sb.Append(nodeStr);
            }
            sb.Append($"</code></pre>");

            return sb.ToString();
        }

    }

    public class TocItem
    {
        public string Title = "";
        public int Header = 0;
    }

    public class ArticleReadViewModel
    {
        public ArticleModel Article = new ArticleModel();
        public string[] KeywordsList = new string[0];

        public string BodyHtml = "";
    }


}
