import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { kinds, sizes } from 'Helpers/Props';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import Label from 'Components/Label';
import QualityProfileNameConnector from 'Settings/Profiles/Quality/QualityProfileNameConnector';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import EpisodeAiringConnector from './EpisodeAiringConnector';
import TrackDetailRow from './TrackDetailRow';
import styles from './EpisodeSummary.css';

class EpisodeSummary extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isRemoveTrackFileModalOpen: false
    };
  }

  //
  // Listeners

  onRemoveTrackFilePress = () => {
    this.setState({ isRemoveTrackFileModalOpen: true });
  }

  onConfirmRemoveTrackFile = () => {
    this.props.onDeleteTrackFile();
    this.setState({ isRemoveTrackFileModalOpen: false });
  }

  onRemoveTrackFileModalClose = () => {
    this.setState({ isRemoveTrackFileModalOpen: false });
  }

  //
  // Render

  render() {
    const {
      qualityProfileId,
      overview,
      releaseDate,
      albumLabel,
      path,
      items,
      size,
      quality,
      qualityCutoffNotMet,
      columns
    } = this.props;

    const hasOverview = !!overview;

    return (
      <div>
        <div>
          <span className={styles.infoTitle}>Releases</span>

          <EpisodeAiringConnector
            releaseDate={releaseDate}
            albumLabel={albumLabel}
          />
        </div>

        <div>
          <span className={styles.infoTitle}>Quality Profile</span>

          <Label
            kind={kinds.PRIMARY}
            size={sizes.MEDIUM}
          >
            <QualityProfileNameConnector
              qualityProfileId={qualityProfileId}
            />
          </Label>
        </div>

        <div className={styles.overview}>
          {
            hasOverview ?
              overview :
              'No album overview.'
          }
        </div>

        <div>
          {
            <div className={styles.episodes}>
              {
                items.length ?
                  <Table
                    columns={columns}
                  >
                    <TableBody>
                      {
                        items.map((item) => {
                          return (
                            <TrackDetailRow
                              key={item.id}
                              columns={columns}
                              {...item}
                            />
                          );
                        })
                      }
                    </TableBody>
                  </Table> :

                  <div className={styles.noEpisodes}>
                    No tracks in this group
                  </div>
              }
            </div>
          }
        </div>

        <ConfirmModal
          isOpen={this.state.isRemoveTrackFileModalOpen}
          kind={kinds.DANGER}
          title="Delete Episode File"
          message={`Are you sure you want to delete '${path}'?`}
          confirmLabel="Delete"
          onConfirm={this.onConfirmRemoveTrackFile}
          onCancel={this.onRemoveTrackFileModalClose}
        />
      </div>
    );
  }
}

EpisodeSummary.propTypes = {
  qualityProfileId: PropTypes.number.isRequired,
  overview: PropTypes.string,
  albumLabel: PropTypes.arrayOf(PropTypes.string),
  releaseDate: PropTypes.string.isRequired,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  columns: PropTypes.arrayOf(PropTypes.object).isRequired,
  path: PropTypes.string,
  size: PropTypes.number,
  quality: PropTypes.object,
  qualityCutoffNotMet: PropTypes.bool,
  onDeleteTrackFile: PropTypes.func.isRequired
};

export default EpisodeSummary;
