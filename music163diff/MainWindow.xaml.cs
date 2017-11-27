using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace music163diff
{
    public class Log
    {
        public string Str { get; set; }
        public Brush Color { get; set; }
    }

    public class Logs : ObservableCollection<Log>
    {
        public Logs()
        {
            Add(new Log
            {
                Str = "[" + DateTime.Now.ToLongTimeString() + "] 来自wws的歌单对比分析工具。ver:" +
                      Application.ResourceAssembly.GetName().Version,
                Color = Brushes.White
            });
        }
    }

    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly string _style = Render.h("style", content: Properties.Resources.style);

        private List<Track> _intersect;

        private Playlist _playlistA;
        private Playlist _playlistB;

        private List<Track> _tracksA;
        private List<Track> _tracksB;

        private List<Album> _albums;
        private List<Album> _albumsA;
        private List<Album> _albumsB;
        private List<Album> _albumsIntersect;
        private List<Album> _albumsRemainA;
        private List<Album> _albumsRemainB;

        private List<Artist> _artists;
        private List<Artist> _artistsA;
        private List<Artist> _artistsB;
        private List<Artist> _artistsIntersect;
        private List<Artist> _artistsRemainA;
        private List<Artist> _artistsRemainB;
        private readonly Logs _logs = new Logs();
        private readonly Regex _reg = new Regex(@"\d{5,}");
        private List<Track> _remainA;
        private List<Track> _remainB;
        private List<Track> _union;

        public MainWindow()
        {
            InitializeComponent();

            button.Click += Button_ClickAsync;
            url_a.GotFocus += TextBox_GotFocus;
            url_b.GotFocus += TextBox_GotFocus;
            url_a.LostFocus += TextBox_LostFocus;
            url_b.LostFocus += TextBox_LostFocus;

            expander.Expanded += Expander_Expanded;
            expander.Collapsed += Expander_Collapsed;

            log_box.ItemsSource = _logs;
        }

        private string Html => "<!DOCTYPE html>" +
                               Render.h("html", content:
                                   Render.h("head", content:
                                       Render.h("meta",
                                           new Dictionary<string, string>
                                           {
                                               ["charset"] = "utf-8"
                                           }) +
                                       Render.h("meta",
                                           new Dictionary<string, string>
                                           {
                                               ["name"] = "viewport",
                                               ["content"] = "width=device-width, initial-scale=1.0"
                                           }) +
                                       Render.h("meta",
                                           new Dictionary<string, string>
                                           {
                                               ["http-equiv"] = "X-UA-Compatible",
                                               ["content"] = "ie=edge"
                                           }) +
                                       Render.h("title",
                                           content: WebUtility.HtmlEncode(
                                               $"{_playlistA.name}-{_playlistB.name} 对比分析结果")) +
                                       _style
                                   ) +
                                   Body
                               );

        private string Body => Render.h("body", content: SameSongs + SameAlbums + SameArtists);


        private string SameSongs => Render.section(new Dictionary<string, string>
        {
            ["class"] = "samesongs"
        }, string.Join("", Render.p(content: $"总歌曲数量：{_union.Count}，相同歌曲数：{_intersect.Count}"), Render.table(
                content: string.Join("",
                    Render.h("thead", content: Render.tr(content: string.Join("",
                        Render.th(content: "歌单"),
                        Render.th(content: "封面"),
                        Render.th(content: "歌曲数")))),
                    Render.h("tbody", content: string.Join("",

                        // 
                        Render.tr(content: string.Join("",
                            Render.td(content: Render.a(new Dictionary<string, string>
                            {
                                ["target"] = "_blank",
                                ["href"] = $"http://music.163.com/song/{_playlistA.id}"
                            }, WebUtility.HtmlEncode(_playlistA.name))),
                            Render.td(
                                content: Render.img(new Dictionary<string, string>
                                {
                                    ["height"] = "140",
                                    ["width"] = "140",
                                    ["src"] = $"{_playlistA.coverImgUrl}?param=140y140"
                                })), Render.td(content: $"{_remainA.Count} + {_intersect.Count}"))),
                        // 
                        Render.tr(content: string.Join(
                            "",
                            Render.td(content: Render.a(new Dictionary<string, string>
                            {
                                ["target"] = "_blank",
                                ["href"] = $"http://music.163.com/song/{_playlistB.id}"
                            }, WebUtility.HtmlEncode(_playlistB.name))),
                            Render.td(content: Render.img(
                                new Dictionary<string, string>
                                {
                                    ["height"] = "140",
                                    ["width"] = "140",
                                    ["src"] = $"{_playlistB.coverImgUrl}?param=140y140"
                                })),
                            Render.td(content: $"{_remainB.Count} + {_intersect.Count}")
                        )))))),
            Render.details(new Dictionary<string, string> {["open"] = "open"}, string.Join("",
                Render.summary(new Dictionary<string, string>
                {
                    ["class"] = "large"
                }, "选曲分析"),
                Render.h("input", new Dictionary<string, string>
                {
                    ["type"] = "checkbox",
                    ["id"] = "show_others",
                    ["checked"] = "checked"
                }),
                Render.h("label", new Dictionary<string, string>
                {
                    ["for"] = "show_others"
                }, "显示歌手和专辑"),
                Render.h("input", new Dictionary<string, string>
                {
                    ["type"] = "checkbox",
                    ["id"] = "only_same"
                }),
                Render.h("label", new Dictionary<string, string>
                {
                    ["for"] = "only_same"
                }, "仅显示相同歌曲"),
                Render.div(new Dictionary<string, string>
                {
                    ["class"] = "playlistContainer"
                }, string.Join("",
                    Table(_tracksA, "A", _playlistA.name),
                    Table(_tracksB, "B", _playlistB.name)
                ))))));


        private string SameAlbums => Render.section(new Dictionary<string, string>
            {
                ["class"] = "samealbums"
            }, Render.details(new Dictionary<string, string> {["open"] = "open"}, string.Join("",
                Render.summary(new Dictionary<string, string> {["class"] = "large"}, $"选曲专辑来源分析(共{_albums.Count}个专辑)"),
                Render.div(new Dictionary<string, string>
                    {
                        ["class"] = "flexbox"
                    },
                    string.Join("",
                        Render.details(new Dictionary<string, string> {["open"] = "open"}, string.Join("",
                            Render.summary(content: $"公共来源专辑:({_albumsIntersect.Count})"),
                            Render.ul(content:
                                string.Join("",
                                    _albumsIntersect.Select(al =>
                                        Render.li(content: Render.a(new Dictionary<string, string>
                                        {
                                            ["target"] = "_blank",
                                            ["href"] = $"http://music.163.com/album/{al.id}"
                                        }, WebUtility.HtmlEncode(al.name))))))
                        )),
                        Render.details(new Dictionary<string, string> {["open"] = "open"}, string.Join("",
                            Render.summary(content: $"仅歌单 A 涉及的专辑: ({_albumsRemainA.Count})"),
                            Render.ul(content:
                                string.Join("",
                                    _albumsRemainA.Select(al =>
                                        Render.li(content: Render.a(new Dictionary<string, string>
                                        {
                                            ["target"] = "_blank",
                                            ["href"] = $"http://music.163.com/album/{al.id}"
                                        }, WebUtility.HtmlEncode(al.name))))))
                        )),
                        Render.details(new Dictionary<string, string> {["open"] = "open"}, string.Join("",
                            Render.summary(content: $"仅歌单 B 涉及的专辑: ({_albumsRemainB.Count})"),
                            Render.ul(content:
                                string.Join("",
                                    _albumsRemainB.Select(al =>
                                        Render.li(content: Render.a(new Dictionary<string, string>
                                        {
                                            ["target"] = "_blank",
                                            ["href"] = $"http://music.163.com/album/{al.id}"
                                        }, WebUtility.HtmlEncode(al.name))))))
                        ))
                    ))
            ))
        );

        private string SameArtists => Render.section(new Dictionary<string, string>
            {
                ["class"] = "sameartists"
            }, Render.details(new Dictionary<string, string> {["open"] = "open"}, string.Join("",
                Render.summary(new Dictionary<string, string> {["class"] = "large"}, $"选曲作者来源分析(共{_artists.Count}位作者)"),
                Render.div(new Dictionary<string, string>
                    {
                        ["class"] = "flexbox"
                    },
                    string.Join("",
                        Render.details(new Dictionary<string, string> {["open"] = "open"}, string.Join("",
                            Render.summary(content: $"公共来源作者:({_artistsIntersect.Count})"),
                            Render.ul(content:
                                string.Join("",
                                    _artistsIntersect.Select(ar =>
                                        Render.li(content: Render.a(new Dictionary<string, string>
                                        {
                                            ["target"] = "_blank",
                                            ["href"] = $"http://music.163.com/artist/{ar.id}"
                                        }, WebUtility.HtmlEncode(ar.name))))))
                        )),
                        Render.details(new Dictionary<string, string> {["open"] = "open"}, string.Join("",
                            Render.summary(content: $"仅歌单 A 涉及的作者: ({_artistsRemainA.Count})"),
                            Render.ul(content:
                                string.Join("",
                                    _artistsRemainA.Select(ar =>
                                        Render.li(content: Render.a(new Dictionary<string, string>
                                        {
                                            ["target"] = "_blank",
                                            ["href"] = $"http://music.163.com/artist/{ar.id}"
                                        }, WebUtility.HtmlEncode(ar.name))))))
                        )),
                        Render.details(new Dictionary<string, string> {["open"] = "open"}, string.Join("",
                            Render.summary(content: $"仅歌单 B 涉及的作者: ({_artistsRemainB.Count})"),
                            Render.ul(content:
                                string.Join("",
                                    _artistsRemainB.Select(ar =>
                                        Render.li(content: Render.a(new Dictionary<string, string>
                                        {
                                            ["target"] = "_blank",
                                            ["href"] = $"http://music.163.com/artist/{ar.id}"
                                        }, WebUtility.HtmlEncode(ar.name))))))
                        ))
                    ))
            ))
        );


        private string Table(List<Track> tracks, string trackNo, string caption)
        {
            return Render.table(new Dictionary<string, string> {["class"] = "playlist " + trackNo},
                string.Join("",
                    Render.h("caption", content: caption),
                    Render.h("thead",
                        content: Render.tr(
                            content: string.Join("",
                                Render.th(content: "#"),
                                Render.th(content: "音乐标题"),
                                Render.th(content: "歌手"),
                                Render.th(content: "专辑")))),
                    Render.h("tbody",
                        content: string.Join("",
                            tracks.Select((t, index) =>
                            {
                                var same = _intersect.Any(i => i.id == t.id);
                                var indexString = (index + 1).ToString();
                                return Render.tr(
                                    new Dictionary<string, string>
                                    {
                                        ["id"] = trackNo + t.id,
                                        ["class"] = same ? "same" : ""
                                    },
                                    string.Join("",
                                        Render.td(content: same
                                            ? Render.a(new Dictionary<string, string>
                                            {
                                                ["href"] = "#" + (trackNo == "A" ? "B" : "A") + t.id
                                            }, indexString)
                                            : indexString),
                                        Render.td(content: Render.a(
                                            new Dictionary<string, string>
                                            {
                                                ["target"] = "_blank",
                                                ["href"] = $"http://music.163.com/song/{t.id}"
                                            }, WebUtility.HtmlEncode(t.name))),
                                        Render.td(content: string.Join(",",
                                            t.ar.Select(ar =>
                                                Render.a(
                                                    new Dictionary<string, string>
                                                    {
                                                        ["target"] = "_blank",
                                                        ["href"] = $"http://music.163.com/artist/{ar.id}"
                                                    }, WebUtility.HtmlEncode(ar.name))))),
                                        Render.td(content: Render.a(
                                            new Dictionary<string, string>
                                            {
                                                ["target"] = "_blank",
                                                ["href"] = $"http://music.163.com/album/{t.al.id}"
                                            }, WebUtility.HtmlEncode(t.al.name)))));
                            })))));
        }

        private void Expander_Collapsed(object sender, RoutedEventArgs e)
        {
            Height -= 50;
        }

        private void Expander_Expanded(object sender, RoutedEventArgs e)
        {
            Height += 50;
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textbox && textbox.Text.Trim().Length == 0)
                textbox.Background = null;
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textbox) textbox.Background = Brushes.White;
        }

        private void Add_Log(string str, Brush color = null)
        {
            if (color == null)
                color = Brushes.White;
            _logs.Add(new Log

            {
                Str = "[" + DateTime.Now.ToLongTimeString() + "] " + str,
                Color = color
            });

            scrollview.ScrollToBottom();
        }

        private async void Button_ClickAsync(object sender, RoutedEventArgs e)
        {
            var mcA = _reg.Match(url_a.Text);
            var mcB = _reg.Match(url_b.Text);

            if (!mcA.Success)
            {
                Add_Log("歌单 A 没有填好", Brushes.Red);
                return;
            }
            if (!mcB.Success)
            {
                Add_Log("歌单 B 没有填好", Brushes.Red);
                return;
            }

            var a = int.Parse(mcA.Value);
            var b = int.Parse(mcB.Value);
            Add_Log($"A={a},B={b}");

            string jsonA;
            string jsonB;

            try
            {
                jsonA = await Get_JsonAsync(a);
                jsonB = await Get_JsonAsync(b);
            }
            catch (Exception err)
            {
                Add_Log(err.Message, Brushes.Red);
                return;
            }


            var resA = JsonConvert.DeserializeObject<ResponseJson>(jsonA);
            var resB = JsonConvert.DeserializeObject<ResponseJson>(jsonB);

            if (resA.code != 200)
            {
                Add_Log("歌单 A 获取失败", Brushes.Red);
                Add_Log(jsonA, Brushes.Red);
                return;
            }
            Add_Log("歌单 A 信息已获取");

            if (resB.code != 200)
            {
                Add_Log("歌单 B 获取失败", Brushes.Red);
                Add_Log(jsonB, Brushes.Red);
                return;
            }

            Add_Log("歌单 B 信息已获取");

            _playlistA = resA.playlist;
            _playlistB = resB.playlist;

            _tracksA = _playlistA.tracks;
            _tracksB = _playlistB.tracks;
            _intersect = _tracksA.Intersect(_tracksB).ToList();
            _remainA = _tracksA.Except(_tracksB).ToList();
            _remainB = _tracksB.Except(_tracksA).ToList();
            _union = _tracksA.Union(_tracksB).ToList();

            Add_Log($"总歌曲数量={_union.Count}");
            Add_Log($"歌单A重复率={_intersect.Count}/{_tracksA.Count}");
            Add_Log($"歌单B重复率={_intersect.Count}/{_tracksB.Count}");
            Add_Log($"相似度={_intersect.Count}/{_union.Count}");

            _albums = _union.Select(tr => tr.al).Distinct().ToList();
            _albumsA = _tracksA.Select(tr => tr.al).Distinct().ToList();
            _albumsB = _tracksB.Select(tr => tr.al).Distinct().ToList();
            _albumsIntersect = _albumsA.Intersect(_albumsB).ToList();
            _albumsRemainA = _albumsA.Except(_albumsIntersect).ToList();
            _albumsRemainB = _albumsB.Except(_albumsIntersect).ToList();

            _artists = _union.SelectMany(tr => tr.ar).Distinct().ToList();
            _artistsA = _tracksA.SelectMany(tr => tr.ar).Distinct().ToList();
            _artistsB = _tracksB.SelectMany(tr => tr.ar).Distinct().ToList();
            _artistsIntersect = _artistsA.Intersect(_artistsB).ToList();
            _artistsRemainA = _artistsA.Except(_artistsIntersect).ToList();
            _artistsRemainB = _artistsB.Except(_artistsIntersect).ToList();

            var sfd = new SaveFileDialog
            {
                Filter = "Html文档(*.html)|*.html"
            };

            if (sfd.ShowDialog() == true)
            {
                Add_Log(sfd.FileName, Brushes.LightGreen);
                File.WriteAllText(sfd.FileName, Html);
            }
        }


        private static async Task<string> Get_JsonAsync(int id)
        {
            var baseAddress = new Uri("http://music.163.com");
            var cookieContainer = new CookieContainer();

            using (var handler = new HttpClientHandler {CookieContainer = cookieContainer})
            using (var client = new HttpClient(handler) {BaseAddress = baseAddress})
            {
                var data = JsonConvert.SerializeObject(
                    new
                    {
                        url = "http://music.163.com/api/v3/playlist/detail",
                        method = "POST",
                        @params = new {id, n = 1000}
                    }
                );

                var values = new Dictionary<string, string>
                {
                    ["eparams"] =
                    AESECB.NetEaseMusic163LinuxEncryptor(
                        data
                    )
                };
                var content = new FormUrlEncodedContent(values);

                client.DefaultRequestHeaders.Add("User-Agent",
                    "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 " +
                    "(KHTML, like Gecko) Chrome/60.0.3112.90 Safari/537.36");

                client.DefaultRequestHeaders.Add("Origin", "orpheus://orpheus");
                cookieContainer.Add(baseAddress, new Cookie("os", "linux"));
                cookieContainer.Add(baseAddress, new Cookie("appver", "1.1.0.1232"));
                cookieContainer.Add(baseAddress, new Cookie("osver", "unkonow"));
                cookieContainer.Add(baseAddress, new Cookie("channel", "release"));

                var response = await client.PostAsync("/api/linux/forward", content);
                return await response.Content.ReadAsStringAsync();
            }
        }
    }
}