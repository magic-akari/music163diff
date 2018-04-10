import { PlaylistBox } from "./PlaylistBox.js";
import { ResultBox } from "./ResultBox.js";
import { parse } from "./parser.js";
class App extends React.PureComponent {
    constructor(props) {
        super(props);
        this.onFileInputChange = (e) => {
            this.addJsonFiles(Array.from(e.target.files || []));
        };
        this.addJsonFiles = async (files) => {
            const new_files = await Promise.all(files.map(file => new Promise(resolve => {
                const reader = new FileReader();
                reader.onload = (event) => {
                    resolve(event.target.result);
                };
                reader.readAsText(file);
            })));
            this.setState({
                playlist_json: [
                    ...this.state.playlist_json,
                    ...new_files
                        .map((f) => this.convert(f))
                        .filter(f => f !== false)
                ]
            });
        };
        this.onDelete = (e) => {
            const target = e.target;
            if ("action" in target.dataset) {
                const action = target.dataset.action;
                switch (action) {
                    case "delete":
                        {
                            const key = target.dataset.key;
                            this.setState({
                                playlist_json: this.state.playlist_json.filter((_, i) => i != key)
                            });
                        }
                        break;
                    case "clear":
                        {
                            this.setState({ playlist_json: [] });
                        }
                        break;
                    default:
                        break;
                }
            }
        };
        this.onCalculate = (e) => {
            e.preventDefault();
            const form = e.target;
            const calculate_expression = form.elements["calculate_expression"]
                .value;
            const exp_list = calculate_expression
                .replace(/X\*/, "x")
                .split(/\s*[,;]\s*/);
            const result = [
                ...exp_list
                    .map(exp => parse(exp))
                    .filter((ast) => ast !== false)
                    .map(ast => Object.assign({ name: this.translate(ast) }, this.calculate(ast)))
                    .filter(i => i.name !== false && i.tracks !== false),
                ...this.state.result
            ];
            this.setState({
                result
            });
        };
        this.translate = (ast) => {
            if (typeof ast === "string") {
                if (!!this.state.playlist_json[ast]) {
                    return `「${this.state.playlist_json[ast].name}」`;
                }
                return false;
            }
            const left = this.translate(ast.expr);
            const right = this.translate(ast.factor);
            if (typeof ast.factor === "string") {
                return `${left} ${ast.op} ${right}`;
            }
            else {
                return `${left} ${ast.op} (${right})`;
            }
        };
        this.calculate = (input) => {
            if (typeof input === "string") {
                if (!!this.state.playlist_json[input]) {
                    const tracks = this.state.playlist_json[input].tracks;
                    return { tracks };
                }
                return false;
            }
            const { op, expr, factor } = input;
            const left = this.calculate(expr);
            const right = this.calculate(factor);
            if (left == false || right == false) {
                return false;
            }
            switch (op) {
                case "+":
                    return {
                        tracks: this.union(left.tracks, right.tracks)
                    };
                case "-":
                    return {
                        tracks: this.difference(left.tracks, right.tracks)
                    };
                case "x":
                    return {
                        tracks: this.cross(left.tracks, right.tracks)
                    };
            }
        };
        this.state = { playlist_json: [], result: [] };
    }
    componentDidMount() {
        const body = document.body;
        body.addEventListener("dragover", (e) => e.preventDefault());
        body.addEventListener("drop", (e) => {
            e.preventDefault();
            let files = Array.from(e.dataTransfer.files).filter((f) => f.type === "application/json" || /.json$/.test(f.name));
            if (files.length > 0) {
                this.addJsonFiles(files);
            }
        });
    }
    convert(text) {
        try {
            const json = JSON.parse(text);
            if ("result" in json) {
                return json.result;
            }
            else if ("playlist" in json) {
                let playlist = json.playlist;
                if (playlist.trackCount > 0 && "ar" in playlist.tracks[0]) {
                    for (const tracks of playlist.tracks) {
                        tracks["artists"] = tracks.ar;
                        tracks["album"] = tracks.al;
                        delete tracks.ar;
                        delete tracks.al;
                    }
                    return playlist;
                }
            }
        }
        catch (error) {
            return false;
        }
        return false;
    }
    cross(a, b) {
        return a.filter(ia => b.some(ib => ia.id === ib.id && (ib.id !== 0 && ia.name === ib.name)));
    }
    union(a, b) {
        return [
            ...a,
            ...b.filter(ib => !a.some(ia => ia.id === ib.id && (ia.id !== 0 && ia.name === ib.name)))
        ];
    }
    difference(a, b) {
        return a.filter(ia => !b.some(ib => ia.id === ib.id && (ib.id !== 0 && ia.name === ib.name)));
    }
    render() {
        return [
            React.createElement("label", { key: "label" },
                React.createElement("svg", { xmlns: "http://www.w3.org/2000/svg", width: "24", height: "24", viewBox: "0 0 24 24" },
                    React.createElement("path", { d: "M19 13h-6v6h-2v-6H5v-2h6V5h2v6h6v2z" }),
                    React.createElement("path", { fill: "none", d: "M0 0h24v24H0z" })),
                React.createElement("input", { type: "file", accept: "application/json, .json", multiple: true, onChange: this.onFileInputChange })),
            React.createElement(PlaylistBox, { key: "PlaylistBox", playlists: this.state.playlist_json, onCalculate: this.onCalculate, onDelete: this.onDelete }),
            React.createElement(ResultBox, { key: "ResultBox", result: this.state.result })
        ];
    }
}
ReactDOM.render(React.createElement(App, null), document.querySelector("#app"));
//# sourceMappingURL=App.js.map