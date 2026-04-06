namespace NzbDrone.Core.Download.Clients.RQBit.ResponseModels;

// https://github.com/ikatson/rqbit/blob/946ad3625892f4f40dde3d0e6bbc3030f68a973c/crates/librqbit/src/torrent_state/mod.rs#L65
public enum TorrentState
{
    Initializing = 0,
    Paused = 1,
    Live = 2,
    Error = 3,
    Invalid = 4
}
