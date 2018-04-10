import * as _React from "react";
import * as _ReactDOM from "react-dom";

declare const React: typeof _React;
declare const ReactDOM: typeof _ReactDOM;

import { PlaylistBox } from "./PlaylistBox.js";
import { ResultBox } from "./ResultBox.js";
import { parse, AST } from "./parser.js";

import { Comparable } from "./Type";

class App extends React.PureComponent<any, any> {
  constructor(props: any) {
    super(props);
    this.state = { playlist_json: [] as any[], result: [] as any[] };
  }

  componentDidMount() {
    const body = document.body;
    body.addEventListener("dragover", (e: DragEvent) => e.preventDefault());
    body.addEventListener("drop", (e: DragEvent) => {
      e.preventDefault();

      let files = Array.from(e.dataTransfer.files).filter(
        (f: File) => f.type === "application/json" || /.json$/.test(f.name)
      );
      if (files.length > 0) {
        this.addJsonFiles(files);
      }
    });
  }

  onFileInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    this.addJsonFiles(Array.from(e.target.files || []));
  };

  addJsonFiles = async (files: File[]) => {
    const new_files = await Promise.all(
      files.map(
        file =>
          new Promise<string>(resolve => {
            const reader = new FileReader();
            reader.onload = (event: any) => {
              resolve(event.target.result);
            };
            reader.readAsText(file);
          })
      )
    );

    this.setState({
      playlist_json: [
        ...this.state.playlist_json,
        ...new_files
          .map((f: string) => this.convert(f))
          .filter(f => f !== false)
      ]
    });
  };

  convert(text: string): object | false {
    try {
      const json = JSON.parse(text);
      if ("result" in json) {
        return json.result;
      } else if ("playlist" in json) {
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
    } catch (error) {
      return false;
    }
    return false;
  }

  onDelete = (e: React.UIEvent<HTMLElement>) => {
    const target = e.target as any;
    if ("action" in target.dataset) {
      const action = target.dataset.action;
      switch (action) {
        case "delete":
          {
            const key = target.dataset.key as string;
            this.setState({
              playlist_json: this.state.playlist_json.filter(
                (_: any, i: string) => i != key
              )
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

  onCalculate = (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    const form = e.target as any;
    const calculate_expression = form.elements["calculate_expression"]
      .value as string;
    const exp_list = calculate_expression
      .replace(/X\*/, "x")
      .split(/\s*[,;]\s*/);

    const result = [
      ...exp_list
        .map(exp => parse(exp))
        .filter((ast): ast is AST | string => ast !== false)
        .map(ast =>
          Object.assign({ name: this.translate(ast) }, this.calculate(ast))
        )
        .filter(i => i.name !== false && i.tracks !== false),
      ...this.state.result
    ];

    this.setState({
      result
    });
  };

  translate = (ast: AST | string): string | false => {
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
    } else {
      return `${left} ${ast.op} (${right})`;
    }
  };

  calculate = (input: AST | string): any => {
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

  cross(a: Comparable[], b: Comparable[]) {
    return a.filter(ia =>
      b.some(ib => ia.id === ib.id && (ib.id !== 0 && ia.name === ib.name))
    );
  }

  union(a: Comparable[], b: Comparable[]) {
    return [
      ...a,
      ...b.filter(
        ib =>
          !a.some(ia => ia.id === ib.id && (ia.id !== 0 && ia.name === ib.name))
      )
    ];
  }

  difference(a: Comparable[], b: Comparable[]) {
    return a.filter(
      ia =>
        !b.some(ib => ia.id === ib.id && (ib.id !== 0 && ia.name === ib.name))
    );
  }

  render() {
    return [
      <label key="label">
        <svg
          xmlns="http://www.w3.org/2000/svg"
          width="24"
          height="24"
          viewBox="0 0 24 24"
        >
          <path d="M19 13h-6v6h-2v-6H5v-2h6V5h2v6h6v2z" />
          <path fill="none" d="M0 0h24v24H0z" />
        </svg>
        <input
          type="file"
          accept="application/json, .json"
          multiple
          onChange={this.onFileInputChange}
        />
      </label>,
      <PlaylistBox
        key="PlaylistBox"
        playlists={this.state.playlist_json}
        onCalculate={this.onCalculate}
        onDelete={this.onDelete}
      />,
      <ResultBox key="ResultBox" result={this.state.result} />
    ];
  }
}

ReactDOM.render(<App />, document.querySelector("#app"));
