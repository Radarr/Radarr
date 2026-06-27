import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Card from 'Components/Card';
import Button from 'Components/Link/Button';
import Menu from 'Components/Menu/Menu';
import MenuContent from 'Components/Menu/MenuContent';
import { sizes } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import AddSpecificationPresetMenuItem from './AddSpecificationPresetMenuItem';
import styles from './AddSpecificationItem.css';

class AddSpecificationItem extends Component {

  //
  // Listeners

  onSpecificationSelect = () => {
    const {
      implementation
    } = this.props;

    this.props.onSpecificationSelect({ implementation });
  };

  //
  // Render

  render() {
    const {
      implementation,
      implementationName,
      infoLink,
      presets,
      onSpecificationSelect
    } = this.props;

    const hasPresets = !!presets && !!presets.length;
    const addLabel = translate('AddConditionImplementation', {
      implementationName
    });

    return (
      <Card
        className={styles.specification}
        overlayClassName={styles.overlay}
        overlayContent={true}
        ariaLabel={addLabel}
        title={implementationName}
        onPress={this.onSpecificationSelect}
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
                  onPress={this.onSpecificationSelect}
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
                      presets.map((preset, index) => {
                        return (
                          <AddSpecificationPresetMenuItem
                            key={index}
                            name={preset.name}
                            implementation={implementation}
                            onPress={onSpecificationSelect}
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

AddSpecificationItem.propTypes = {
  implementation: PropTypes.string.isRequired,
  implementationName: PropTypes.string.isRequired,
  infoLink: PropTypes.string.isRequired,
  presets: PropTypes.arrayOf(PropTypes.object),
  onSpecificationSelect: PropTypes.func.isRequired
};

export default AddSpecificationItem;
