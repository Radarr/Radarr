import PropTypes from 'prop-types';
import React, { PureComponent } from 'react';
import classNames from 'classnames';
import formatBytes from 'Utilities/Number/formatBytes';
import { ColorImpairedConsumer } from 'App/ColorImpairedContext';
import DescriptionList from 'Components/DescriptionList/DescriptionList';
import DescriptionListItem from 'Components/DescriptionList/DescriptionListItem';
import styles from './AuthorIndexFooter.css';

class AuthorIndexFooter extends PureComponent {

  //
  // Render

  render() {
    const { author } = this.props;
    const count = author.length;
    let books = 0;
    let bookFiles = 0;
    let ended = 0;
    let continuing = 0;
    let monitored = 0;
    let totalFileSize = 0;

    author.forEach((s) => {
      const { statistics = {} } = s;

      const {
        bookCount = 0,
        bookFileCount = 0,
        sizeOnDisk = 0
      } = statistics;

      books += bookCount;
      bookFiles += bookFileCount;

      if (s.status === 'ended') {
        ended++;
      } else {
        continuing++;
      }

      if (s.monitored) {
        monitored++;
      }

      totalFileSize += sizeOnDisk;
    });

    return (
      <ColorImpairedConsumer>
        {(enableColorImpairedMode) => {
          return (
            <div className={styles.footer}>
              <div>
                <div className={styles.legendItem}>
                  <div
                    className={classNames(
                      styles.continuing,
                      enableColorImpairedMode && 'colorImpaired'
                    )}
                  />
                  <div>Continuing (All books downloaded)</div>
                </div>

                <div className={styles.legendItem}>
                  <div
                    className={classNames(
                      styles.ended,
                      enableColorImpairedMode && 'colorImpaired'
                    )}
                  />
                  <div>Ended (All books downloaded)</div>
                </div>

                <div className={styles.legendItem}>
                  <div
                    className={classNames(
                      styles.missingMonitored,
                      enableColorImpairedMode && 'colorImpaired'
                    )}
                  />
                  <div>Missing Books (Author monitored)</div>
                </div>

                <div className={styles.legendItem}>
                  <div
                    className={classNames(
                      styles.missingUnmonitored,
                      enableColorImpairedMode && 'colorImpaired'
                    )}
                  />
                  <div>Missing Books (Author not monitored)</div>
                </div>
              </div>

              <div className={styles.statistics}>
                <DescriptionList>
                  <DescriptionListItem
                    title="Authors"
                    data={count}
                  />

                  <DescriptionListItem
                    title="Ended"
                    data={ended}
                  />

                  <DescriptionListItem
                    title="Continuing"
                    data={continuing}
                  />
                </DescriptionList>

                <DescriptionList>
                  <DescriptionListItem
                    title="Monitored"
                    data={monitored}
                  />

                  <DescriptionListItem
                    title="Unmonitored"
                    data={count - monitored}
                  />
                </DescriptionList>

                <DescriptionList>
                  <DescriptionListItem
                    title="Books"
                    data={books}
                  />

                  <DescriptionListItem
                    title="Files"
                    data={bookFiles}
                  />
                </DescriptionList>

                <DescriptionList>
                  <DescriptionListItem
                    title="Total File Size"
                    data={formatBytes(totalFileSize)}
                  />
                </DescriptionList>
              </div>
            </div>
          );
        }}
      </ColorImpairedConsumer>
    );
  }
}

AuthorIndexFooter.propTypes = {
  author: PropTypes.arrayOf(PropTypes.object).isRequired
};

export default AuthorIndexFooter;
