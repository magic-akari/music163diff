export interface IPlaylists {
  id: number;
  name: string;
  coverImgUrl: string;
  tracks: ITrack[];
  createTime: number;
  creator: {
    userId: number;
    nickname: string;
  };
}

export interface ITrack {
  id: number;
  name: string;
  artists: IArtist[];
  album: {
    id: number;
    name: string;
  };
}

export interface IArtist {
  id: number;
  name: string;
}

export interface Comparable {
  id: number;
  name: string;
}
