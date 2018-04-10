export class ResultBox extends React.PureComponent {
    tracks_table(tracks) {
        return (React.createElement("table", { className: "pure-table pure-table-striped pure-table-horizontal" },
            React.createElement("thead", null,
                React.createElement("tr", null,
                    React.createElement("th", null, "id"),
                    React.createElement("th", null, "\u97F3\u4E50\u6807\u9898"),
                    React.createElement("th", null, "\u6B4C\u624B"),
                    React.createElement("th", null, "\u4E13\u8F91"))),
            React.createElement("tbody", null, tracks.map(track => (React.createElement("tr", null,
                React.createElement("td", null, track.id),
                React.createElement("td", null,
                    React.createElement("a", { href: `https://music.163.com/song/${track.id}`, target: "_blank" }, track.name)),
                React.createElement("td", null, track.artists.map(artist => (React.createElement("a", { href: `https://music.163.com/artist/${artist.id}`, target: "_blank" }, artist.name)))),
                React.createElement("td", null,
                    React.createElement("a", { href: `https://music.163.com/artist/${track.album.id}`, target: "_blank" }, track.album.name))))))));
    }
    render() {
        if (this.props.result.length === 0) {
            return false;
        }
        return (React.createElement("section", null,
            React.createElement("h2", null,
                "\u8BA1\u7B97\u7ED3\u679C (",
                this.props.result.length,
                ")"),
            React.createElement("ol", null, this.props.result.map(r => (React.createElement("li", null,
                React.createElement("details", { open: true },
                    React.createElement("summary", null,
                        r.name,
                        " = ",
                        r.tracks.length),
                    this.tracks_table(r.tracks))))))));
    }
}
//# sourceMappingURL=ResultBox.js.map