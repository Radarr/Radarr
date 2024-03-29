import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Button from 'Components/Link/Button';
import Link from 'Components/Link/Link';
import Menu from 'Components/Menu/Menu';
import MenuContent from 'Components/Menu/MenuContent';
import { sizes } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import AddIndexerPresetMenuItem from './AddIndexerPresetMenuItem';
import styles from './AddIndexerItem.css';

class AddIndexerItem extends Component {

  //
  // Listeners

  onIndexerSelect = () => {
    const {
      implementation,
      implementationName
    } = this.props;

    this.props.onIndexerSelect({ implementation, implementationName });
  };

  //
  // Render

  render() {
    const {
      implementation,
      implementationName,
      infoLink,
      presets,
      onIndexerSelect
    } = this.props;

    const hasPresets = !!presets && !!presets.length;

    return (
      <div
        className={styles.indexer}
      >
        <Link
          className={styles.underlay}
          onPress={this.onIndexerSelect}
        />

        <div className={styles.overlay}>
          <div className={styles.name}>
            {implementationName}
          </div>

          <div className={styles.actions}>
            {
              hasPresets &&
                <span>
                  <Button
                    size={sizes.SMALL}
                    onPress={this.onIndexerSelect}
                  >
                    {translate('Custom')}
                  </Button>

                  <Menu className={styles.presetsMenu}>
                    <Button
                      className={styles.presetsMenuButton}
                      size={sizes.SMALL}
                    >
                      {translate('Presets')}
                    </Button>

                    <MenuContent>
                      {
                        presets.map((preset) => {
                          return (
                            <AddIndexerPresetMenuItem
                              key={preset.name}
                              name={preset.name}
                              implementation={implementation}
                              implementationName={implementationName}
                              onPress={onIndexerSelect}
                            />
                          );
                        })
                      }
                    </MenuContent>
                  </Menu>
                </span>
            }

            <Button
              to={infoLink}
              size={sizes.SMALL}
            >
              {translate('MoreInfo')}
            </Button>
          </div>
        </div>
      </div>
    );
  }
}

AddIndexerItem.propTypes = {
  implementation: PropTypes.string.isRequired,
  implementationName: PropTypes.string.isRequired,
  infoLink: PropTypes.string.isRequired,
  presets: PropTypes.arrayOf(PropTypes.object),
  onIndexerSelect: PropTypes.func.isRequired
};

export default AddIndexerItem;
