import * as _React from "react";
declare const React: typeof _React;

import { IPlaylists } from "./Type";

export interface IPlaylistBoxProps {
  playlists: IPlaylists[];
  onCalculate: (e: any) => any;
  onDelete: (e: any) => any;
}

export class PlaylistBox extends React.PureComponent<IPlaylistBoxProps, any> {
  render() {
    if (this.props.playlists.length === 0) {
      return <p>将歌单json文件拖入此页面。</p>;
    } else {
      return [
        <form
          key="expression"
          className="expression"
          onSubmit={this.props.onCalculate}
        >
          <input
            type="search"
            name="calculate_expression"
            placeholder="在此输入计算表达式，例如 0x1"
            title="可以输入数字+-x括号进行并集差集交集运算，可以一次性输入多个表达式，半角逗号分割"
            pattern="[\d\s,;\(\)x\+\-、*X]*"
          />
          <input
            type="submit"
            value="计算"
            className="pure-button pure-button-primary"
          />
        </form>,
        <details open key="details">
          <summary>歌单列表</summary>
          <table
            onClick={this.props.onDelete}
            className="pure-table pure-table-striped pure-table-horizontal"
          >
            <thead>
              <tr>
                <th>#</th>
                <th>封面</th>
                <th>歌单名</th>
                <th>创建者</th>
                <th>歌曲数</th>
                <th>创建时间</th>
                <th data-action="clear">清空</th>
              </tr>
            </thead>
            <tbody>
              {this.props.playlists.map((p, index) => (
                <tr key={p.id}>
                  <td>{index}</td>
                  <td>
                    <img src={p.coverImgUrl + "?param=50y50"} alt="" />
                  </td>
                  <td>
                    <a
                      href={"https://music.163.com/playlist?id=" + p.id}
                      target="_blank"
                    >
                      {p.name}
                    </a>
                  </td>
                  <td>
                    <a
                      href={"https://music.163.com/user/" + p.creator.userId}
                      target="_blank"
                    >
                      {p.creator.nickname}
                    </a>
                  </td>
                  <td>{p.tracks.length}</td>
                  <td>{new Date(p.createTime).toLocaleString()}</td>
                  <td>
                    <span data-action="delete" data-key={index}>
                      X
                    </span>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </details>
      ];
    }
  }
}
