export class PlaylistBox extends React.PureComponent {
    render() {
        if (this.props.playlists.length === 0) {
            return React.createElement("p", null, "\u5C06\u6B4C\u5355json\u6587\u4EF6\u62D6\u5165\u6B64\u9875\u9762\u3002");
        }
        else {
            return [
                React.createElement("form", { key: "expression", className: "expression", onSubmit: this.props.onCalculate },
                    React.createElement("input", { type: "search", name: "calculate_expression", placeholder: "\u5728\u6B64\u8F93\u5165\u8BA1\u7B97\u8868\u8FBE\u5F0F\uFF0C\u4F8B\u5982 0x1", title: "\u53EF\u4EE5\u8F93\u5165\u6570\u5B57+-x\u62EC\u53F7\u8FDB\u884C\u5E76\u96C6\u5DEE\u96C6\u4EA4\u96C6\u8FD0\u7B97\uFF0C\u53EF\u4EE5\u4E00\u6B21\u6027\u8F93\u5165\u591A\u4E2A\u8868\u8FBE\u5F0F\uFF0C\u534A\u89D2\u9017\u53F7\u5206\u5272", pattern: "[\\d\\s,;\\(\\)x\\+\\-\u3001*X]*" }),
                    React.createElement("input", { type: "submit", value: "\u8BA1\u7B97", className: "pure-button pure-button-primary" })),
                React.createElement("details", { open: true, key: "details" },
                    React.createElement("summary", null, "\u6B4C\u5355\u5217\u8868"),
                    React.createElement("table", { onClick: this.props.onDelete, className: "pure-table pure-table-striped pure-table-horizontal" },
                        React.createElement("thead", null,
                            React.createElement("tr", null,
                                React.createElement("th", null, "#"),
                                React.createElement("th", null, "\u5C01\u9762"),
                                React.createElement("th", null, "\u6B4C\u5355\u540D"),
                                React.createElement("th", null, "\u521B\u5EFA\u8005"),
                                React.createElement("th", null, "\u6B4C\u66F2\u6570"),
                                React.createElement("th", null, "\u521B\u5EFA\u65F6\u95F4"),
                                React.createElement("th", { "data-action": "clear" }, "\u6E05\u7A7A"))),
                        React.createElement("tbody", null, this.props.playlists.map((p, index) => (React.createElement("tr", { key: p.id },
                            React.createElement("td", null, index),
                            React.createElement("td", null,
                                React.createElement("img", { src: p.coverImgUrl + "?param=50y50", alt: "" })),
                            React.createElement("td", null,
                                React.createElement("a", { href: "https://music.163.com/playlist?id=" + p.id, target: "_blank" }, p.name)),
                            React.createElement("td", null,
                                React.createElement("a", { href: "https://music.163.com/user/" + p.creator.userId, target: "_blank" }, p.creator.nickname)),
                            React.createElement("td", null, p.tracks.length),
                            React.createElement("td", null, new Date(p.createTime).toLocaleString()),
                            React.createElement("td", null,
                                React.createElement("span", { "data-action": "delete", "data-key": index }, "X"))))))))
            ];
        }
    }
}
//# sourceMappingURL=PlaylistBox.js.map