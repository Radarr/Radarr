import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Button from 'Components/Link/Button';
import Link from 'Components/Link/Link';
import Menu from 'Components/Menu/Menu';
import MenuContent from 'Components/Menu/MenuContent';
import { sizes } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import AddNetImportPresetMenuItem from './AddNetImportPresetMenuItem';
import styles from './AddNetImportItem.css';

class AddNetImportItem extends Component {

  //
  // Listeners

  onNetImportSelect = () => {
    const {
      implementation
    } = this.props;

    this.props.onNetImportSelect({ implementation });
  }

  //
  // Render

  render() {
    const {
      implementation,
      implementationName,
      infoLink,
      presets,
      onNetImportSelect
    } = this.props;

    const hasPresets = !!presets && !!presets.length;

    return (
      <div
        className={styles.netImport}
      >
        <Link
          className={styles.underlay}
          onPress={this.onNetImportSelect}
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
                    onPress={this.onNetImportSelect}
                  >
                    Custom
                  </Button>

                  <Menu className={styles.presetsMenu}>
                    <Button
                      className={styles.presetsMenuButton}
                      size={sizes.SMALL}
                    >
                      Presets
                    </Button>

                    <MenuContent>
                      {
                        presets.map((preset) => {
                          return (
                            <AddNetImportPresetMenuItem
                              key={preset.name}
                              name={preset.name}
                              implementation={implementation}
                              onPress={onNetImportSelect}
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

AddNetImportItem.propTypes = {
  implementation: PropTypes.string.isRequired,
  implementationName: PropTypes.string.isRequired,
  infoLink: PropTypes.string.isRequired,
  presets: PropTypes.arrayOf(PropTypes.object),
  onNetImportSelect: PropTypes.func.isRequired
};

export default AddNetImportItem;
