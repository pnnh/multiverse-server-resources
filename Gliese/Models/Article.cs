
using Gliese.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Markdig;
using Gliese.Utils;

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
                var bodyHtml = ArticleUtils.BuildBody(tocList, deserialized);
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
 
    }


    public class ArticleReadViewModel
    {
        public ArticleModel Article = new ArticleModel();
        public string[] KeywordsList = new string[0];

        public string BodyHtml = ""; 
    }


}
