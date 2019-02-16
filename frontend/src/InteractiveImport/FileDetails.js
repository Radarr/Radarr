import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { icons } from 'Helpers/Props';
import formatTimeSpan from 'Utilities/Date/formatTimeSpan';
import Icon from 'Components/Icon';
import Link from 'Components/Link/Link';
import DescriptionList from 'Components/DescriptionList/DescriptionList';
import DescriptionListItem from 'Components/DescriptionList/DescriptionListItem';
import DescriptionListItemTitle from 'Components/DescriptionList/DescriptionListItemTitle';
import DescriptionListItemDescription from 'Components/DescriptionList/DescriptionListItemDescription';
import styles from './FileDetails.css';

class FileDetails extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isExpanded: props.isExpanded
    };
  }

  //
  // Listeners

  onExpandPress = () => {
    const {
      isExpanded
    } = this.state;
    this.setState({ isExpanded: !isExpanded });
  }

  //
  // Render

  renderRejections() {
    const {
      rejections
    } = this.props;

    return (
      <span>
        <DescriptionListItemTitle>
          Rejections
        </DescriptionListItemTitle>
        {
          _.map(rejections, (item, key) => {
            return (
              <DescriptionListItemDescription key={key}>
                {item.reason}
              </DescriptionListItemDescription>
            );
          })
        }
      </span>
    );
  }

  render() {
    const {
      filename,
      audioTags,
      rejections
    } = this.props;

    const {
      isExpanded
    } = this.state;

    return (
      <div
        className={styles.fileDetails}
      >
        <div className={styles.header} onClick={this.onExpandPress}>
          <div className={styles.filename}>
            {filename}
          </div>

          <div className={styles.expandButton}>
            <Icon
              className={styles.expandButtonIcon}
              name={isExpanded ? icons.COLLAPSE : icons.EXPAND}
              title={isExpanded ? 'Hide file info' : 'Show file info'}
              size={24}
            />
          </div>
        </div>

        <div>
          {
            isExpanded &&
              <div className={styles.audioTags}>

                <DescriptionList>
                  {
                    audioTags.title !== undefined &&
                    <DescriptionListItem
                      title="Track Title"
                      data={audioTags.title}
                    />
                  }
                  {
                    audioTags.trackNumbers[0] > 0 &&
                      <DescriptionListItem
                        title="Track Number"
                        data={audioTags.trackNumbers[0]}
                      />
                  }
                  {
                    audioTags.discNumber > 0 &&
                      <DescriptionListItem
                        title="Disc Number"
                        data={audioTags.discNumber}
                      />
                  }
                  {
                    audioTags.discCount > 0 &&
                    <DescriptionListItem
                      title="Disc Count"
                      data={audioTags.discCount}
                    />
                  }
                  {
                    audioTags.albumTitle !== undefined &&
                      <DescriptionListItem
                        title="Album"
                        data={audioTags.albumTitle}
                      />
                  }
                  {
                    audioTags.artistTitle !== undefined &&
                      <DescriptionListItem
                        title="Artist"
                        data={audioTags.artistTitle}
                      />
                  }
                  {
                    audioTags.country !== undefined &&
                    <DescriptionListItem
                      title="Country"
                      data={audioTags.country.name}
                    />
                  }
                  {
                    audioTags.year > 0 &&
                    <DescriptionListItem
                      title="Year"
                      data={audioTags.year}
                    />
                  }
                  {
                    audioTags.label !== undefined &&
                    <DescriptionListItem
                      title="Label"
                      data={audioTags.label}
                    />
                  }
                  {
                    audioTags.catalogNumber !== undefined &&
                    <DescriptionListItem
                      title="Catalog Number"
                      data={audioTags.catalogNumber}
                    />
                  }
                  {
                    audioTags.disambiguation !== undefined &&
                    <DescriptionListItem
                      title="Disambiguation"
                      data={audioTags.disambiguation}
                    />
                  }
                  {
                    audioTags.duration !== undefined &&
                    <DescriptionListItem
                      title="Duration"
                      data={formatTimeSpan(audioTags.duration)}
                    />
                  }
                  {
                    audioTags.artistMBId !== undefined &&
                    <Link
                      to={`https://musicbrainz.org/artist/${audioTags.artistMBId}`}
                    >
                      <DescriptionListItem
                        title="MusicBrainz Artist ID"
                        data={audioTags.artistMBId}
                      />
                    </Link>
                  }
                  {
                    audioTags.albumMBId !== undefined &&
                    <Link
                      to={`https://musicbrainz.org/release-group/${audioTags.albumMBId}`}
                    >
                      <DescriptionListItem
                        title="MusicBrainz Album ID"
                        data={audioTags.albumMBId}
                      />
                    </Link>
                  }
                  {
                    audioTags.releaseMBId !== undefined &&
                    <Link
                      to={`https://musicbrainz.org/release/${audioTags.releaseMBId}`}
                    >
                      <DescriptionListItem
                        title="MusicBrainz Release ID"
                        data={audioTags.releaseMBId}
                      />
                    </Link>
                  }
                  {
                    audioTags.recordingMBId !== undefined &&
                    <Link
                      to={`https://musicbrainz.org/recording/${audioTags.recordingMBId}`}
                    >
                      <DescriptionListItem
                        title="MusicBrainz Recording ID"
                        data={audioTags.recordingMBId}
                      />
                    </Link>
                  }
                  {
                    audioTags.trackMBId !== undefined &&
                    <Link
                      to={`https://musicbrainz.org/track/${audioTags.trackMBId}`}
                    >
                      <DescriptionListItem
                        title="MusicBrainz Track ID"
                        data={audioTags.trackMBId}
                      />
                    </Link>
                  }
                  {
                    rejections.length > 0 &&
                      this.renderRejections()
                  }
                </DescriptionList>
              </div>
          }
        </div>
      </div>
    );
  }
}

FileDetails.propTypes = {
  audioTags: PropTypes.object.isRequired,
  filename: PropTypes.string.isRequired,
  rejections: PropTypes.arrayOf(PropTypes.object).isRequired,
  isExpanded: PropTypes.bool
};

export default FileDetails;
