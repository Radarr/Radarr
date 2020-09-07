import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { Tab, TabList, TabPanel, Tabs } from 'react-tabs';
import TextTruncate from 'react-text-truncate';
import AuthorPoster from 'Author/AuthorPoster';
import DeleteAuthorModal from 'Author/Delete/DeleteAuthorModal';
import EditAuthorModalConnector from 'Author/Edit/EditAuthorModalConnector';
import AuthorHistoryTable from 'Author/History/AuthorHistoryTable';
import BookFileEditorTable from 'BookFile/Editor/BookFileEditorTable';
import HeartRating from 'Components/HeartRating';
import Icon from 'Components/Icon';
import Label from 'Components/Label';
import IconButton from 'Components/Link/IconButton';
import Link from 'Components/Link/Link';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import MonitorToggleButton from 'Components/MonitorToggleButton';
import PageContent from 'Components/Page/PageContent';
import PageContentBodyConnector from 'Components/Page/PageContentBodyConnector';
import PageToolbar from 'Components/Page/Toolbar/PageToolbar';
import PageToolbarButton from 'Components/Page/Toolbar/PageToolbarButton';
import PageToolbarSection from 'Components/Page/Toolbar/PageToolbarSection';
import PageToolbarSeparator from 'Components/Page/Toolbar/PageToolbarSeparator';
import Popover from 'Components/Tooltip/Popover';
import Tooltip from 'Components/Tooltip/Tooltip';
import { align, icons, kinds, sizes, tooltipPositions } from 'Helpers/Props';
import InteractiveSearchFilterMenuConnector from 'InteractiveSearch/InteractiveSearchFilterMenuConnector';
import InteractiveSearchTable from 'InteractiveSearch/InteractiveSearchTable';
import OrganizePreviewModalConnector from 'Organize/OrganizePreviewModalConnector';
import RetagPreviewModalConnector from 'Retag/RetagPreviewModalConnector';
import QualityProfileNameConnector from 'Settings/Profiles/Quality/QualityProfileNameConnector';
import fonts from 'Styles/Variables/fonts';
import formatBytes from 'Utilities/Number/formatBytes';
import stripHtml from 'Utilities/String/stripHtml';
import selectAll from 'Utilities/Table/selectAll';
import toggleSelected from 'Utilities/Table/toggleSelected';
import InteractiveImportModal from '../../InteractiveImport/InteractiveImportModal';
import AuthorAlternateTitles from './AuthorAlternateTitles';
import AuthorDetailsLinks from './AuthorDetailsLinks';
import AuthorDetailsSeasonConnector from './AuthorDetailsSeasonConnector';
import AuthorDetailsSeriesConnector from './AuthorDetailsSeriesConnector';
import AuthorTagsConnector from './AuthorTagsConnector';
import styles from './AuthorDetails.css';

const defaultFontSize = parseInt(fonts.defaultFontSize);
const lineHeight = parseFloat(fonts.lineHeight);

function getFanartUrl(images) {
  const fanartImage = _.find(images, { coverType: 'fanart' });
  if (fanartImage) {
    // Remove protocol
    return fanartImage.url.replace(/^https?:/, '');
  }
}

function getExpandedState(newState) {
  return {
    allExpanded: newState.allSelected,
    allCollapsed: newState.allUnselected,
    expandedState: newState.selectedState
  };
}

class AuthorDetails extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isOrganizeModalOpen: false,
      isRetagModalOpen: false,
      isEditAuthorModalOpen: false,
      isDeleteAuthorModalOpen: false,
      isInteractiveImportModalOpen: false,
      allExpanded: false,
      allCollapsed: false,
      expandedState: {},
      selectedTabIndex: 0
    };
  }

  //
  // Listeners

  onOrganizePress = () => {
    this.setState({ isOrganizeModalOpen: true });
  }

  onOrganizeModalClose = () => {
    this.setState({ isOrganizeModalOpen: false });
  }

  onRetagPress = () => {
    this.setState({ isRetagModalOpen: true });
  }

  onRetagModalClose = () => {
    this.setState({ isRetagModalOpen: false });
  }

  onInteractiveImportPress = () => {
    this.setState({ isInteractiveImportModalOpen: true });
  }

  onInteractiveImportModalClose = () => {
    this.setState({ isInteractiveImportModalOpen: false });
  }

  onEditAuthorPress = () => {
    this.setState({ isEditAuthorModalOpen: true });
  }

  onEditAuthorModalClose = () => {
    this.setState({ isEditAuthorModalOpen: false });
  }

  onDeleteAuthorPress = () => {
    this.setState({
      isEditAuthorModalOpen: false,
      isDeleteAuthorModalOpen: true
    });
  }

  onDeleteAuthorModalClose = () => {
    this.setState({ isDeleteAuthorModalOpen: false });
  }

  onExpandAllPress = () => {
    const {
      allExpanded,
      expandedState
    } = this.state;

    this.setState(getExpandedState(selectAll(expandedState, !allExpanded)));
  }

  onExpandPress = (bookId, isExpanded) => {
    this.setState((state) => {
      const convertedState = {
        allSelected: state.allExpanded,
        allUnselected: state.allCollapsed,
        selectedState: state.expandedState
      };

      const newState = toggleSelected(convertedState, [], bookId, isExpanded, false);

      return getExpandedState(newState);
    });
  }

  //
  // Render

  render() {
    const {
      id,
      authorName,
      ratings,
      path,
      statistics,
      qualityProfileId,
      monitored,
      status,
      overview,
      links,
      images,
      alternateTitles,
      tags,
      isSaving,
      isRefreshing,
      isSearching,
      isFetching,
      isPopulated,
      booksError,
      bookFilesError,
      hasBooks,
      hasMonitoredBooks,
      hasSeries,
      series,
      hasBookFiles,
      previousAuthor,
      nextAuthor,
      onMonitorTogglePress,
      onRefreshPress,
      onSearchPress
    } = this.props;

    const {
      bookFileCount,
      sizeOnDisk
    } = statistics;

    const {
      isOrganizeModalOpen,
      isRetagModalOpen,
      isEditAuthorModalOpen,
      isDeleteAuthorModalOpen,
      isInteractiveImportModalOpen,
      allExpanded,
      allCollapsed,
      expandedState,
      selectedTabIndex
    } = this.state;

    const continuing = status === 'continuing';

    let bookFilesCountMessage = 'No book files';

    if (bookFileCount === 1) {
      bookFilesCountMessage = '1 book file';
    } else if (bookFileCount > 1) {
      bookFilesCountMessage = `${bookFileCount} book files`;
    }

    let expandIcon = icons.EXPAND_INDETERMINATE;

    if (allExpanded) {
      expandIcon = icons.COLLAPSE;
    } else if (allCollapsed) {
      expandIcon = icons.EXPAND;
    }

    return (
      <PageContent title={authorName}>
        <PageToolbar>
          <PageToolbarSection>
            <PageToolbarButton
              label="Refresh & Scan"
              iconName={icons.REFRESH}
              spinningName={icons.REFRESH}
              title="Refresh information and scan disk"
              isSpinning={isRefreshing}
              onPress={onRefreshPress}
            />

            <PageToolbarButton
              label="Search Monitored"
              iconName={icons.SEARCH}
              isDisabled={!monitored || !hasMonitoredBooks || !hasBooks}
              isSpinning={isSearching}
              title={hasMonitoredBooks ? undefined : 'No monitored books for this author'}
              onPress={onSearchPress}
            />

            <PageToolbarSeparator />

            <PageToolbarButton
              label="Preview Rename"
              iconName={icons.ORGANIZE}
              isDisabled={!hasBookFiles}
              onPress={this.onOrganizePress}
            />

            {/* <PageToolbarButton */}
            {/*   label="Preview Retag" */}
            {/*   iconName={icons.RETAG} */}
            {/*   isDisabled={!hasBookFiles} */}
            {/*   onPress={this.onRetagPress} */}
            {/* /> */}

            <PageToolbarButton
              label="Manual Import"
              iconName={icons.INTERACTIVE}
              onPress={this.onInteractiveImportPress}
            />

            <PageToolbarSeparator />

            <PageToolbarButton
              label="Edit"
              iconName={icons.EDIT}
              onPress={this.onEditAuthorPress}
            />

            <PageToolbarButton
              label="Delete"
              iconName={icons.DELETE}
              onPress={this.onDeleteAuthorPress}
            />
          </PageToolbarSection>

          <PageToolbarSection alignContent={align.RIGHT}>
            <PageToolbarButton
              label={allExpanded ? 'Collapse All' : 'Expand All'}
              iconName={expandIcon}
              onPress={this.onExpandAllPress}
            />
          </PageToolbarSection>
        </PageToolbar>

        <PageContentBodyConnector innerClassName={styles.innerContentBody}>
          <div className={styles.header}>
            <div
              className={styles.backdrop}
              style={{
                backgroundImage: `url(${getFanartUrl(images)})`
              }}
            >
              <div className={styles.backdropOverlay} />
            </div>

            <div className={styles.headerContent}>
              <AuthorPoster
                className={styles.poster}
                images={images}
                size={250}
                lazy={false}
              />

              <div className={styles.info}>
                <div className={styles.titleRow}>
                  <div className={styles.titleContainer}>
                    <div className={styles.toggleMonitoredContainer}>
                      <MonitorToggleButton
                        className={styles.monitorToggleButton}
                        monitored={monitored}
                        isSaving={isSaving}
                        size={40}
                        onPress={onMonitorTogglePress}
                      />
                    </div>

                    <div className={styles.title}>
                      {authorName}
                    </div>

                    {
                      !!alternateTitles.length &&
                        <div className={styles.alternateTitlesIconContainer}>
                          <Popover
                            anchor={
                              <Icon
                                name={icons.ALTERNATE_TITLES}
                                size={20}
                              />
                            }
                            title="Alternate Titles"
                            body={<AuthorAlternateTitles alternateTitles={alternateTitles} />}
                            position={tooltipPositions.BOTTOM}
                          />
                        </div>
                    }
                  </div>

                  <div className={styles.authorNavigationButtons}>
                    <IconButton
                      className={styles.authorNavigationButton}
                      name={icons.ARROW_LEFT}
                      size={30}
                      title={`Go to ${previousAuthor.authorName}`}
                      to={`/author/${previousAuthor.titleSlug}`}
                    />

                    <IconButton
                      className={styles.authorNavigationButton}
                      name={icons.ARROW_UP}
                      size={30}
                      title={'Go to author listing'}
                      to={'/'}
                    />

                    <IconButton
                      className={styles.authorNavigationButton}
                      name={icons.ARROW_RIGHT}
                      size={30}
                      title={`Go to ${nextAuthor.authorName}`}
                      to={`/author/${nextAuthor.titleSlug}`}
                    />
                  </div>
                </div>

                <div className={styles.details}>
                  <div>
                    <HeartRating
                      rating={ratings.value}
                      iconSize={20}
                    />
                  </div>
                </div>

                <div className={styles.detailsLabels}>
                  <Label
                    className={styles.detailsLabel}
                    size={sizes.LARGE}
                  >
                    <Icon
                      name={icons.FOLDER}
                      size={17}
                    />

                    <span className={styles.path}>
                      {path}
                    </span>
                  </Label>

                  <Label
                    className={styles.detailsLabel}
                    title={bookFilesCountMessage}
                    size={sizes.LARGE}
                  >
                    <Icon
                      name={icons.DRIVE}
                      size={17}
                    />

                    <span className={styles.sizeOnDisk}>
                      {
                        formatBytes(sizeOnDisk)
                      }
                    </span>
                  </Label>

                  <Label
                    className={styles.detailsLabel}
                    title="Quality Profile"
                    size={sizes.LARGE}
                  >
                    <Icon
                      name={icons.PROFILE}
                      size={17}
                    />

                    <span className={styles.qualityProfileName}>
                      {
                        <QualityProfileNameConnector
                          qualityProfileId={qualityProfileId}
                        />
                      }
                    </span>
                  </Label>

                  <Label
                    className={styles.detailsLabel}
                    size={sizes.LARGE}
                  >
                    <Icon
                      name={monitored ? icons.MONITORED : icons.UNMONITORED}
                      size={17}
                    />

                    <span className={styles.qualityProfileName}>
                      {monitored ? 'Monitored' : 'Unmonitored'}
                    </span>
                  </Label>

                  <Label
                    className={styles.detailsLabel}
                    title={continuing ? 'More books are expected' : 'No additional books are expected'}
                    size={sizes.LARGE}
                  >
                    <Icon
                      name={continuing ? icons.AUTHOR_CONTINUING : icons.AUTHOR_ENDED}
                      size={17}
                    />

                    <span className={styles.qualityProfileName}>
                      {continuing ? 'Continuing' : 'Deceased'}
                    </span>
                  </Label>

                  <Tooltip
                    anchor={
                      <Label
                        className={styles.detailsLabel}
                        size={sizes.LARGE}
                      >
                        <Icon
                          name={icons.EXTERNAL_LINK}
                          size={17}
                        />

                        <span className={styles.links}>
                          Links
                        </span>
                      </Label>
                    }
                    tooltip={
                      <AuthorDetailsLinks
                        links={links}
                      />
                    }
                    kind={kinds.INVERSE}
                    position={tooltipPositions.BOTTOM}
                  />

                  {
                    !!tags.length &&
                      <Tooltip
                        anchor={
                          <Label
                            className={styles.detailsLabel}
                            size={sizes.LARGE}
                          >
                            <Icon
                              name={icons.TAGS}
                              size={17}
                            />

                            <span className={styles.tags}>
                              Tags
                            </span>
                          </Label>
                        }
                        tooltip={<AuthorTagsConnector authorId={id} />}
                        kind={kinds.INVERSE}
                        position={tooltipPositions.BOTTOM}
                      />

                  }
                </div>
                <div className={styles.overview}>
                  <TextTruncate
                    line={Math.floor(125 / (defaultFontSize * lineHeight))}
                    text={stripHtml(overview)}
                  />
                </div>
              </div>
            </div>
          </div>

          <div className={styles.contentContainer}>
            {
              !isPopulated && !booksError && !bookFilesError &&
                <LoadingIndicator />
            }

            {
              !isFetching && booksError &&
                <div>Loading books failed</div>
            }

            {
              !isFetching && bookFilesError &&
                <div>Loading book files failed</div>
            }

            {
              isPopulated &&
                <Tabs selectedIndex={this.state.tabIndex} onSelect={(tabIndex) => this.setState({ selectedTabIndex: tabIndex })}>
                  <TabList
                    className={styles.tabList}
                  >
                    <Tab
                      className={styles.tab}
                      selectedClassName={styles.selectedTab}
                    >
                      Books
                    </Tab>

                    <Tab
                      className={styles.tab}
                      selectedClassName={styles.selectedTab}
                    >
                      Series
                    </Tab>

                    <Tab
                      className={styles.tab}
                      selectedClassName={styles.selectedTab}
                    >
                      History
                    </Tab>

                    <Tab
                      className={styles.tab}
                      selectedClassName={styles.selectedTab}
                    >
                      Search
                    </Tab>

                    <Tab
                      className={styles.tab}
                      selectedClassName={styles.selectedTab}
                    >
                      Files
                    </Tab>

                    {
                      selectedTabIndex === 3 &&
                        <div className={styles.filterIcon}>
                          <InteractiveSearchFilterMenuConnector
                            type="author"
                          />
                        </div>
                    }
                  </TabList>

                  <TabPanel>
                    <AuthorDetailsSeasonConnector
                      authorId={id}
                      isExpanded={true}
                      onExpandPress={this.onExpandPress}
                    />
                  </TabPanel>

                  <TabPanel>
                    {
                      isPopulated && hasSeries &&
                        <div>
                          {
                            series.map((item) => {
                              return (
                                <AuthorDetailsSeriesConnector
                                  key={item.id}
                                  seriesId={item.id}
                                  authorId={id}
                                  isExpanded={expandedState[item.id]}
                                  onExpandPress={this.onExpandPress}
                                />
                              );
                            })
                          }
                        </div>
                    }
                  </TabPanel>

                  <TabPanel>
                    <AuthorHistoryTable
                      authorId={id}
                    />
                  </TabPanel>

                  <TabPanel>
                    <InteractiveSearchTable
                      type="author"
                      authorId={id}
                    />
                  </TabPanel>

                  <TabPanel>
                    <BookFileEditorTable
                      authorId={id}
                    />
                  </TabPanel>
                </Tabs>
            }

          </div>

          <div className={styles.metadataMessage}>
            Missing or too many books? Modify or create a new
            <Link to='/settings/profiles'> Metadata Profile </Link>
            or manually
            <Link to={`/add/search?term=${encodeURIComponent(authorName)}`}> Search </Link>
            for new items!
          </div>

          <OrganizePreviewModalConnector
            isOpen={isOrganizeModalOpen}
            authorId={id}
            onModalClose={this.onOrganizeModalClose}
          />

          <RetagPreviewModalConnector
            isOpen={isRetagModalOpen}
            authorId={id}
            onModalClose={this.onRetagModalClose}
          />

          <EditAuthorModalConnector
            isOpen={isEditAuthorModalOpen}
            authorId={id}
            onModalClose={this.onEditAuthorModalClose}
            onDeleteAuthorPress={this.onDeleteAuthorPress}
          />

          <DeleteAuthorModal
            isOpen={isDeleteAuthorModalOpen}
            authorId={id}
            onModalClose={this.onDeleteAuthorModalClose}
          />

          <InteractiveImportModal
            isOpen={isInteractiveImportModalOpen}
            authorId={id}
            folder={path}
            allowAuthorChange={false}
            showFilterExistingFiles={true}
            showImportMode={false}
            onModalClose={this.onInteractiveImportModalClose}
          />
        </PageContentBodyConnector>
      </PageContent>
    );
  }
}

AuthorDetails.propTypes = {
  id: PropTypes.number.isRequired,
  authorName: PropTypes.string.isRequired,
  ratings: PropTypes.object.isRequired,
  path: PropTypes.string.isRequired,
  statistics: PropTypes.object.isRequired,
  qualityProfileId: PropTypes.number.isRequired,
  monitored: PropTypes.bool.isRequired,
  status: PropTypes.string.isRequired,
  overview: PropTypes.string,
  links: PropTypes.arrayOf(PropTypes.object).isRequired,
  images: PropTypes.arrayOf(PropTypes.object).isRequired,
  alternateTitles: PropTypes.arrayOf(PropTypes.string).isRequired,
  tags: PropTypes.arrayOf(PropTypes.number).isRequired,
  isSaving: PropTypes.bool.isRequired,
  isRefreshing: PropTypes.bool.isRequired,
  isSearching: PropTypes.bool.isRequired,
  isFetching: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  booksError: PropTypes.object,
  bookFilesError: PropTypes.object,
  hasBooks: PropTypes.bool.isRequired,
  hasMonitoredBooks: PropTypes.bool.isRequired,
  hasSeries: PropTypes.bool.isRequired,
  series: PropTypes.arrayOf(PropTypes.object).isRequired,
  hasBookFiles: PropTypes.bool.isRequired,
  previousAuthor: PropTypes.object.isRequired,
  nextAuthor: PropTypes.object.isRequired,
  onMonitorTogglePress: PropTypes.func.isRequired,
  onRefreshPress: PropTypes.func.isRequired,
  onSearchPress: PropTypes.func.isRequired
};

AuthorDetails.defaultProps = {
  statistics: {},
  tags: [],
  isSaving: false
};

export default AuthorDetails;
