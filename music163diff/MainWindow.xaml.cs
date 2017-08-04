using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace music163diff
{

    public class Log
    {
        public string str { get; set; }
        public Brush color { get; set; }
    }

    public class Logs : ObservableCollection<Log>
    {
        public Logs()
        {
            Add(new Log()
            {
                str = "[" + DateTime.Now.ToLongTimeString() + "] 来自wws的歌单对比分析工具。ver:" + Application.ResourceAssembly.GetName().Version.ToString(),
                color = Brushes.White
            });
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Regex reg = new Regex(@"\d{5,}");
        private Logs logs = new Logs();

        private static Func<string, string> api = (string id) => $"http://music.163.com/api/playlist/detail?id={id}";

        private Result result_a;
        private Result result_b;

        private List<Track> tracks_a;
        private List<Track> tracks_b;

        private List<Track> intersect;
        private List<Track> remain_a;
        private List<Track> remain_b;
        private List<Track> union;

        private List<Track> intersect_pos_a;
        private List<Track> intersect_pos_b;

        private List<Track> remain_a_end;
        private List<Track> remain_b_end;

        private List<Album> albums;
        private List<Album> albums_a;
        private List<Album> albums_b;
        private List<Album> albums_intersect;
        private List<Album> albums_remain_a;
        private List<Album> albums_remain_b;

        private List<Artist> artists;
        private List<Artist> artists_a;
        private List<Artist> artists_b;
        private List<Artist> artists_intersect;
        private List<Artist> artists_remain_a;
        private List<Artist> artists_remain_b;

        private const string style = @"<style>
table {
  border-spacing: 0;
  border-collapse: collapse;
  width: 100%;
}
tr {
  background-color: #fff;
  border-top: 1px solid #c6cbd1;
}
tr:nth-child(2n) {
  background-color: #f6f8fa;
}
th {
  padding: 6px 13px;
  border: 1px solid #dfe2e5;
}
td {
  padding: 6px 13px;
  border: 1px solid #dfe2e5;
}
a {
  color: #111;
  text-decoration: none;
}
a:hover {
  color: #0366d6;
  text-decoration: underline;
}

section {
  border: 1px solid blue;
  border-radius: 5px;
  padding: 1rem;
  margin: 1rem;
}

ul {
  list-style-type: none;
  padding: 0;
}

li {
  padding: 6px 13px;
  border: 1px solid #dfe2e5;
  border-top: 1px solid #c6cbd1;
}

li:nth-child(2n) {
  background-color: #f6f8fa;
}
summary {
  font-size: 2rem;
}
img{
  width: 100px;
  height: 100px;
}
</style>
";

        private string Html
        {
            get => "<!DOCTYPE html><html><head><meta charset=\"utf-8\">" +
                @"" +
                $"<title>{result_a.name}-{result_b.name} 对比分析结果</title></head><body>" +
                style +
                $"<section>{Part1}" + $"{Part2}" + $"{(intersect_pos_a.Count == 0 ? "" : Part3) }" + $"{Part4}" + $"{Part5}" + "</section>" +
                $"<section>{Part6}</section>" +
                $"<section>{Part7}</section>" +
                "</body></html>";
        }

        private string Part1
        {
            get => $"<p>总歌曲数:{union.Count}，相同歌曲数:{intersect.Count}</p>" +
                "<table>" +
                "<tr><th>歌单</th><th>封面</th><th>歌曲数</th><th>仅此歌单拥有的歌曲数</th></tr>" +
                $"<tr><td>A({result_a.name})</td><td><img height=\"100\" width=\"100\" src=\"{result_a.coverImgUrl}\"></td><td>{tracks_a.Count}</td><td>{remain_a.Count}</td></tr>" +
                $"<tr><td>B({result_b.name})</td><td><img height=\"100\" width=\"100\" src=\"{result_b.coverImgUrl}\"></td><td>{tracks_b.Count}</td><td>{remain_b.Count}</td></tr>" +
                "</table>";
        }

        private string Part2
        {
            get => "<details open=\"open\">" +
                $"<summary>完全相同的歌曲:({intersect.Count})</summary>" +
                "<table>" +
                $"{table(intersect)}" +
                "</table>" +
                "</details>";

        }

        private string Part3
        {
            get => "<details open=\"open\">" +
                $"<summary>仅歌名相同的歌曲:({intersect_pos_a.Count})</summary>" +
                "<table>" +
                $"{table(intersect_pos_a)}" +
                "</table>" +
                "<table>" +
                $"{table(intersect_pos_b)}" +
                "</table>" +
                "</details>";

        }
        private string Part4
        {
            get => "<details open=\"open\">" +
                $"<summary>仅歌单 A 中存在的歌曲:({remain_a_end.Count})</summary>" +
                "<table>" +
                $"{table(remain_a_end)}" +
                "</table>" +
                "</details>";

        }
        private string Part5
        {
            get => "<details open=\"open\">" +
                $"<summary>仅歌单 B 中存在的歌曲:({remain_b_end.Count})</summary>" +
                "<table>" +
                $"{table(remain_b_end)}" +
                "</table>" +
                "</details>";

        }

        private string Part6
        {
            get => "<details open=\"open\">" +
                $"<summary>选曲专辑来源分析(共{albums.Count}个专辑)</summary>" +
                $"<p>公共来源专辑:({albums_intersect.Count})</p>" +
                "<ul>" +
                String.Join("", albums_intersect.Select(al => $"<li><a target=\"_blank\" href=\"http://music.163.com/album/{al.id}\">{al.name}</a></li>").ToArray()) +
                "</ul>" +
                $"<p>仅歌单 A 涉及的专辑:({albums_remain_a.Count})</p>" +
                "<ul>" +
                String.Join("", albums_remain_a.Select(al => $"<li><a target=\"_blank\" href=\"http://music.163.com/album/{al.id}\">{al.name}</a></li>").ToArray()) +
                "</ul>" +
                $"<p>仅歌单 B 涉及的专辑:({albums_remain_b.Count})</p>" +
                "<ul>" +
                String.Join("", albums_remain_b.Select(al => $"<li><a target=\"_blank\" href=\"http://music.163.com/album/{al.id}\">{al.name}</a></li>").ToArray()) +
                "</ul>" +
                "</details>";
        }

        private string Part7
        {
            get => "<details open=\"open\">" +
                $"<summary>选曲作者来源分析(共{artists.Count}位作者)</summary>" +
                $"<p>公共作者:({artists_intersect.Count})</p>" +
                 "<ul>" +
               String.Join("", artists_intersect.Select(ar => $"<li><a target=\"_blank\" href=\"http://music.163.com/artist/{ar.id}\">{ar.name}</a></li>").ToArray()) +
                 "</ul>" +
                 $"<p>仅歌单 A 涉及的作者:({artists_remain_a.Count})</p>" +
                 "<ul>" +
               String.Join("", artists_remain_a.Select(ar => $"<li><a target=\"_blank\" href=\"http://music.163.com/artist/{ar.id}\">{ar.name}</a></li>").ToArray()) +
                 "</ul>" +
                 $"<p>仅歌单 B 涉及的作者:({artists_remain_b.Count})</p>" +
                 "<ul>" +
               String.Join("", artists_remain_b.Select(ar => $"<li><a target=\"_blank\" href=\"http://music.163.com/artist/{ar.id}\">{ar.name}</a></li>").ToArray()) +
                 "</ul>" +
                "</details>";
        }

        private static Func<List<Track>, string> table = (List<Track> tracks) =>
        "<tr><th>歌名</th><th>作者</th><th>专辑</th></tr>" +
        String.Join("", tracks.Select(tr => $"<tr><td><a target=\"_blank\" href=\"http://music.163.com/song/{tr.id}\">{tr.name}</a></td>" +
        $"<td>{String.Join(",", tr.artists.Select(ar => $"<a target=\"_blank\" href=\"http://music.163.com/artist/{ar.id}\">{ar.name}</a>").ToArray())}</td>" +
        $"<td><a target=\"_blank\" href=\"http://music.163.com/album/{tr.album.id}\">{tr.album.name}</td>" +
        "</tr>").ToArray());

        public MainWindow()
        {
            InitializeComponent();

            button.Click += Button_Click;
            url_a.GotFocus += TextBox_GotFocus;
            url_b.GotFocus += TextBox_GotFocus;
            url_a.LostFocus += TextBox_LostFocus;
            url_b.LostFocus += TextBox_LostFocus;

            expander.Expanded += Expander_Expanded;
            expander.Collapsed += Expander_Collapsed;

            log_box.ItemsSource = logs;

        }

        private void Expander_Collapsed(object sender, RoutedEventArgs e)
        {
            this.Height -= 50;
        }

        private void Expander_Expanded(object sender, RoutedEventArgs e)
        {
            this.Height += 50;
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textbox = sender as TextBox;
            if (textbox.Text.Trim().Length == 0)
            {
                textbox.Background = null;
            }
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var textbox = sender as TextBox;
            textbox.Background = Brushes.White;
        }

        private void Add_Log(string str, Brush color = null)
        {
            if (color == null)
            {
                color = Brushes.White;
            }

            logs.Add(new Log()
            {
                str = "[" + DateTime.Now.ToLongTimeString() + "] " + str,
                color = color
            });

            scrollview.ScrollToBottom();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var mc_a = reg.Match(url_a.Text);
            var mc_b = reg.Match(url_b.Text);
            if (!mc_a.Success)
            {
                Add_Log("歌单 A 没有填好", Brushes.Red);
                return;
            }
            if (!mc_b.Success)
            {
                Add_Log("歌单 B 没有填好", Brushes.Red);
                return;
            }

            var a = mc_a.Value;
            var b = mc_b.Value;

            Add_Log($"A={a},B={b}");
            var json_a = string.Empty;
            var json_b = string.Empty;

            try
            {
                json_a = Get_Json(a);
                json_b = Get_Json(b);

            }
            catch (Exception err)
            {
                Add_Log(err.Message, Brushes.Red);
                return;
            }


            var res_a = JsonConvert.DeserializeObject<ResponseJson>(json_a);
            var res_b = JsonConvert.DeserializeObject<ResponseJson>(json_b);
            if (res_a.code != 200)
            {
                Add_Log("歌单 A 获取失败", Brushes.Red);
                Add_Log(json_a, Brushes.Red);
                return;
            }
            Add_Log("歌单 A 信息已获取");

            if (res_b.code != 200)
            {
                Add_Log("歌单 B 获取失败", Brushes.Red);
                Add_Log(json_b, Brushes.Red);
                return;
            }
            Add_Log("歌单 B 信息已获取");

            result_a = res_a.result;
            result_b = res_b.result;

            tracks_a = result_a.tracks;
            tracks_b = result_b.tracks;

            intersect = tracks_a.Intersect(tracks_b).ToList();
            remain_a = tracks_a.Except(tracks_b).ToList();
            remain_b = tracks_b.Except(tracks_a).ToList();
            union = tracks_a.Union(tracks_b).ToList();

            intersect_pos_a = remain_a.Where(r_a => remain_b.Select(r_b => r_b.name.ToLower()).Contains(r_a.name.ToLower())).OrderBy(t => t.name).ToList();
            intersect_pos_b = remain_b.Where(r_b => remain_a.Select(r_a => r_a.name.ToLower()).Contains(r_b.name.ToLower())).OrderBy(t => t.name).ToList();

            remain_a_end = remain_a.Except(intersect_pos_a).ToList();
            remain_b_end = remain_b.Except(intersect_pos_b).ToList();

            Add_Log($"总歌曲数量={union.Count}");
            Add_Log($"歌单A重复率={intersect.Count }/{ tracks_a.Count}~{(intersect.Count + intersect_pos_a.Count)}/{ tracks_a.Count}");
            Add_Log($"歌单B重复率={intersect.Count }/{ tracks_b.Count}~{(intersect.Count + intersect_pos_b.Count) }/{ tracks_b.Count}");
            Add_Log($"相似度={intersect.Count }/{ union.Count}~{ (intersect.Count + intersect_pos_a.Count) }/{ union.Count}");

            albums = union.Select(tr => tr.album).Distinct().ToList();
            albums_a = tracks_a.Select(tr => tr.album).Distinct().ToList();
            albums_b = tracks_b.Select(tr => tr.album).Distinct().ToList();
            albums_intersect = albums_a.Intersect(albums_b).ToList();
            albums_remain_a = albums_a.Except(albums_intersect).ToList();
            albums_remain_b = albums_b.Except(albums_intersect).ToList();

            artists = union.SelectMany(tr => tr.artists).Distinct().ToList();
            artists_a = tracks_a.SelectMany(tr => tr.artists).Distinct().ToList();
            artists_b = tracks_b.SelectMany(tr => tr.artists).Distinct().ToList();
            artists_intersect = artists_a.Intersect(artists_b).ToList();
            artists_remain_a = artists_a.Except(artists_intersect).ToList();
            artists_remain_b = artists_b.Except(artists_intersect).ToList();


            var sfd = new Microsoft.Win32.SaveFileDialog()
            {
                Filter = "Html文档(*.html)|*.html"
            };

            if (sfd.ShowDialog() == true)
            {
                Add_Log(sfd.FileName, Brushes.LightGreen);
                File.WriteAllText(sfd.FileName, Html);
            }
        }




        private static string Get_Json(string id)
        {
            var request = WebRequest.Create(api(id)) as HttpWebRequest;
            request.ContentType = "application/json; charset=utf-8";
            var response = request.GetResponse() as HttpWebResponse;

            var result = string.Empty;
            using (Stream responseStream = response.GetResponseStream())
            {
                StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                return reader.ReadToEnd();
            }
        }
    }
}
