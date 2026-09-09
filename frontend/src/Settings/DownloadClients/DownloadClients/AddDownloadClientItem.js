import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Card from 'Components/Card';
import Button from 'Components/Link/Button';
import Menu from 'Components/Menu/Menu';
import MenuContent from 'Components/Menu/MenuContent';
import { sizes } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import AddDownloadClientPresetMenuItem from './AddDownloadClientPresetMenuItem';
import styles from './AddDownloadClientItem.css';

class AddDownloadClientItem extends Component {

  //
  // Listeners

  onDownloadClientSelect = () => {
    const {
      implementation
    } = this.props;

    this.props.onDownloadClientSelect({ implementation });
  };

  //
  // Render

  render() {
    const {
      implementation,
      implementationName,
      infoLink,
      presets,
      onDownloadClientSelect
    } = this.props;

    const hasPresets = !!presets && !!presets.length;
    const addLabel = translate('AddDownloadClientImplementation', {
      implementationName
    });

    return (
      <Card
        className={styles.downloadClient}
        overlayClassName={styles.overlay}
        overlayContent={true}
        ariaLabel={addLabel}
        title={implementationName}
        onPress={this.onDownloadClientSelect}
      >
        <div className={styles.name}>
          {implementationName}
        </div>

        <div className={styles.actions}>
          {
            hasPresets &&
              <span>
                <Button
                  size={sizes.SMALL}
                  onPress={this.onDownloadClientSelect}
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
                          <AddDownloadClientPresetMenuItem
                            key={preset.name}
                            name={preset.name}
                            implementation={implementation}
                            onPress={onDownloadClientSelect}
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
      </Card>
    );
  }
}

AddDownloadClientItem.propTypes = {
  implementation: PropTypes.string.isRequired,
  implementationName: PropTypes.string.isRequired,
  infoLink: PropTypes.string.isRequired,
  presets: PropTypes.arrayOf(PropTypes.object),
  onDownloadClientSelect: PropTypes.func.isRequired
};

export default AddDownloadClientItem;
