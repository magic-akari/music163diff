import * as _React from "react";
declare const React: typeof _React;

import { ITrack } from "./Type";

type IResultBoxProps = Array<{
  name: string;
  tracks: ITrack[];
}>;

export class ResultBox extends React.PureComponent<
  { result: IResultBoxProps },
  any
> {
  tracks_table(tracks: ITrack[]) {
    return (
      <table className="pure-table pure-table-striped pure-table-horizontal">
        <thead>
          <tr>
            <th>id</th>
            <th>音乐标题</th>
            <th>歌手</th>
            <th>专辑</th>
          </tr>
        </thead>
        <tbody>
          {tracks.map(track => (
            <tr>
              <td>{track.id}</td>
              <td>
                <a
                  href={`https://music.163.com/song/${track.id}`}
                  target="_blank"
                >
                  {track.name}
                </a>
              </td>
              <td>
                {(track.artists as any[]).map(artist => (
                  <a
                    href={`https://music.163.com/artist/${artist.id}`}
                    target="_blank"
                  >
                    {artist.name}
                  </a>
                ))}
              </td>
              <td>
                <a
                  href={`https://music.163.com/artist/${track.album.id}`}
                  target="_blank"
                >
                  {track.album.name}
                </a>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    );
  }

  render() {
    if (this.props.result.length === 0) {
      return false;
    }
    return (
      <section>
        <h2>计算结果 ({this.props.result.length})</h2>
        <ol>
          {this.props.result.map(r => (
            <li>
              <details open>
                <summary>
                  {r.name}
                  {" = "}
                  {r.tracks.length}
                </summary>
                {this.tracks_table(r.tracks)}
              </details>
            </li>
          ))}
        </ol>
      </section>
    );
  }
}
