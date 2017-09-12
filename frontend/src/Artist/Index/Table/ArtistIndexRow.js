import PropTypes from 'prop-types';
import React, { Component } from 'react';
import getProgressBarKind from 'Utilities/Series/getProgressBarKind';
import formatBytes from 'Utilities/Number/formatBytes';
import { icons } from 'Helpers/Props';
import IconButton from 'Components/Link/IconButton';
import SpinnerIconButton from 'Components/Link/SpinnerIconButton';
import ProgressBar from 'Components/ProgressBar';
import TagListConnector from 'Components/TagListConnector';
// import CheckInput from 'Components/Form/CheckInput';
import VirtualTableRow from 'Components/Table/VirtualTableRow';
import VirtualTableRowCell from 'Components/Table/Cells/VirtualTableRowCell';
import RelativeDateCellConnector from 'Components/Table/Cells/RelativeDateCellConnector';
import ArtistNameLink from 'Artist/ArtistNameLink';
import EditArtistModalConnector from 'Artist/Edit/EditArtistModalConnector';
import DeleteArtistModal from 'Artist/Delete/DeleteArtistModal';
import ArtistStatusCell from './ArtistStatusCell';
import styles from './ArtistIndexRow.css';

class ArtistIndexRow extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isEditArtistModalOpen: false,
      isDeleteArtistModalOpen: false
    };
  }

  onEditSeriesPress = () => {
    this.setState({ isEditArtistModalOpen: true });
  }

  onEditSeriesModalClose = () => {
    this.setState({ isEditArtistModalOpen: false });
  }

  onDeleteSeriesPress = () => {
    this.setState({
      isEditArtistModalOpen: false,
      isDeleteArtistModalOpen: true
    });
  }

  onDeleteArtistModalClose = () => {
    this.setState({ isDeleteArtistModalOpen: false });
  }

  onUseSceneNumberingChange = () => {
    // Mock handler to satisfy `onChange` being required for `CheckInput`.
    //
  }

  //
  // Render

  render() {
    const {
      style,
      id,
      monitored,
      status,
      artistName,
      nameSlug,
      network,
      qualityProfile,
      languageProfile,
      nextAiring,
      previousAiring,
      added,
      albumCount,
      trackCount,
      trackFileCount,
      totalTrackCount,
      latestSeason,
      path,
      sizeOnDisk,
      tags,
      // useSceneNumbering,
      columns,
      isRefreshingSeries,
      onRefreshArtistPress
    } = this.props;

    const {
      isEditArtistModalOpen,
      isDeleteArtistModalOpen
    } = this.state;

    return (
      <VirtualTableRow style={style}>
        {
          columns.map((column) => {
            const {
              name,
              isVisible
            } = column;

            if (!isVisible) {
              return null;
            }

            if (name === 'status') {
              return (
                <ArtistStatusCell
                  key={name}
                  className={styles[name]}
                  monitored={monitored}
                  status={status}
                  component={VirtualTableRowCell}
                />
              );
            }

            if (name === 'sortName') {
              return (
                <VirtualTableRowCell
                  key={name}
                  className={styles[name]}
                >
                  <ArtistNameLink
                    nameSlug={nameSlug}
                    artistName={artistName}
                  />
                </VirtualTableRowCell>
              );
            }

            if (name === 'network') {
              return (
                <VirtualTableRowCell
                  key={name}
                  className={styles[name]}
                >
                  {network}
                </VirtualTableRowCell>
              );
            }

            if (name === 'qualityProfileId') {
              return (
                <VirtualTableRowCell
                  key={name}
                  className={styles[name]}
                >
                  {qualityProfile.name}
                </VirtualTableRowCell>
              );
            }

            if (name === 'languageProfileId') {
              return (
                <VirtualTableRowCell
                  key={name}
                  className={styles[name]}
                >
                  {languageProfile.name}
                </VirtualTableRowCell>
              );
            }

            if (name === 'nextAiring') {
              return (
                <RelativeDateCellConnector
                  key={name}
                  className={styles[name]}
                  date={nextAiring}
                  component={VirtualTableRowCell}
                />
              );
            }

            if (name === 'previousAiring') {
              return (
                <RelativeDateCellConnector
                  key={name}
                  className={styles[name]}
                  date={previousAiring}
                  component={VirtualTableRowCell}
                />
              );
            }

            if (name === 'added') {
              return (
                <RelativeDateCellConnector
                  key={name}
                  className={styles[name]}
                  date={added}
                  component={VirtualTableRowCell}
                />
              );
            }

            if (name === 'albumCount') {
              return (
                <VirtualTableRowCell
                  key={name}
                  className={styles[name]}
                >
                  {albumCount}
                </VirtualTableRowCell>
              );
            }

            if (name === 'trackProgress') {
              const progress = trackCount ? trackFileCount / trackCount * 100 : 100;

              return (
                <VirtualTableRowCell
                  key={name}
                  className={styles[name]}
                >
                  <ProgressBar
                    progress={progress}
                    kind={getProgressBarKind(status, monitored, progress)}
                    showText={true}
                    text={`${trackFileCount} / ${trackCount}`}
                    title={`${trackFileCount} / ${trackCount} (Total: ${totalTrackCount})`}
                    width={125}
                  />
                </VirtualTableRowCell>
              );
            }

            if (name === 'latestSeason') {
              const seasonStatistics = latestSeason.statistics;
              const progress = seasonStatistics.episodeCount ? seasonStatistics.episodeFileCount / seasonStatistics.episodeCount * 100 : 100;

              return (
                <VirtualTableRowCell
                  key={name}
                  className={styles[name]}
                >
                  <ProgressBar
                    progress={progress}
                    kind={getProgressBarKind(status, monitored, progress)}
                    showText={true}
                    text={`${seasonStatistics.episodeFileCount} / ${seasonStatistics.episodeCount}`}
                    title={`${seasonStatistics.episodeFileCount} / ${seasonStatistics.episodeCount} (Total: ${seasonStatistics.totalEpisodeCount})`}
                    width={125}
                  />
                </VirtualTableRowCell>
              );
            }

            if (name === 'trackCount') {
              return (
                <VirtualTableRowCell
                  key={name}
                  className={styles[name]}
                >
                  {totalTrackCount}
                </VirtualTableRowCell>
              );
            }

            if (name === 'path') {
              return (
                <VirtualTableRowCell
                  key={name}
                  className={styles[name]}
                >
                  {path}
                </VirtualTableRowCell>
              );
            }

            if (name === 'sizeOnDisk') {
              return (
                <VirtualTableRowCell
                  key={name}
                  className={styles[name]}
                >
                  {formatBytes(sizeOnDisk)}
                </VirtualTableRowCell>
              );
            }

            if (name === 'tags') {
              return (
                <VirtualTableRowCell
                  key={name}
                  className={styles[name]}
                >
                  <TagListConnector
                    tags={tags}
                  />
                </VirtualTableRowCell>
              );
            }

            // if (name === 'useSceneNumbering') {
            //   return (
            //     <VirtualTableRowCell
            //       key={name}
            //       className={styles[name]}
            //     >
            //       <CheckInput
            //          className={styles.checkInput}
            //         name="useSceneNumbering"
            //         value={useSceneNumbering}
            //         isDisabled={true}
            //         onChange={this.onUseSceneNumberingChange}
            //       />
            //     </VirtualTableRowCell>
            //   );
            // }

            if (name === 'actions') {
              return (
                <VirtualTableRowCell
                  key={name}
                  className={styles[name]}
                >
                  <SpinnerIconButton
                    name={icons.REFRESH}
                    title="Refresh Artist"
                    isSpinning={isRefreshingSeries}
                    onPress={onRefreshArtistPress}
                  />

                  <IconButton
                    name={icons.EDIT}
                    title="Edit Artist"
                    onPress={this.onEditSeriesPress}
                  />
                </VirtualTableRowCell>
              );
            }

            return null;
          })
        }

        <EditArtistModalConnector
          isOpen={isEditArtistModalOpen}
          artistId={id}
          onModalClose={this.onEditSeriesModalClose}
          onDeleteSeriesPress={this.onDeleteSeriesPress}
        />

        <DeleteArtistModal
          isOpen={isDeleteArtistModalOpen}
          artistId={id}
          onModalClose={this.onDeleteArtistModalClose}
        />
      </VirtualTableRow>
    );
  }
}

ArtistIndexRow.propTypes = {
  style: PropTypes.object.isRequired,
  id: PropTypes.number.isRequired,
  monitored: PropTypes.bool.isRequired,
  status: PropTypes.string.isRequired,
  artistName: PropTypes.string.isRequired,
  nameSlug: PropTypes.string.isRequired,
  network: PropTypes.string,
  qualityProfile: PropTypes.object.isRequired,
  languageProfile: PropTypes.object.isRequired,
  nextAiring: PropTypes.string,
  previousAiring: PropTypes.string,
  added: PropTypes.string,
  albumCount: PropTypes.number.isRequired,
  trackCount: PropTypes.number,
  trackFileCount: PropTypes.number,
  totalTrackCount: PropTypes.number,
  latestSeason: PropTypes.object,
  path: PropTypes.string.isRequired,
  sizeOnDisk: PropTypes.number,
  tags: PropTypes.arrayOf(PropTypes.number).isRequired,
  // useSceneNumbering: PropTypes.bool.isRequired,
  columns: PropTypes.arrayOf(PropTypes.object).isRequired,
  isRefreshingSeries: PropTypes.bool.isRequired,
  onRefreshArtistPress: PropTypes.func.isRequired
};

ArtistIndexRow.defaultProps = {
  trackCount: 0,
  trackFileCount: 0
};

export default ArtistIndexRow;
