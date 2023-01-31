
using Gliese.Models;

public class IndexViewModel
{
    public List<ArticleTable> Range = new List<ArticleTable>();
    public int Count = 0;
    public int CurrentPage = 1;
    public int StartPage = 0;
    public int EndPage = 0;
    public int PrevPage = 0;
    public int NextPage = 0;
    public int MaxPage = 0;
}