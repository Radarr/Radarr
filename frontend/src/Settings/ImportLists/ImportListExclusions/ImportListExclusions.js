import PropTypes from 'prop-types';
import React, { Component } from 'react';
import FieldSet from 'Components/FieldSet';
import Icon from 'Components/Icon';
import Link from 'Components/Link/Link';
import PageSectionContent from 'Components/Page/PageSectionContent';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import { icons } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import selectAll from 'Utilities/Table/selectAll';
import toggleSelected from 'Utilities/Table/toggleSelected';
import EditImportListExclusionModalConnector from './EditImportListExclusionModalConnector';
import ImportListExclusion from './ImportListExclusion';
import styles from './ImportListExclusions.css';

class ImportListExclusions extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      allSelected: false,
      allUnselected: false,
      lastToggled: null,
      selectedState: {},
      isAddImportExclusionModalOpen: false
    };
  }

  //
  // Listeners

  onSelectAllChange = ({ value }) => {
    this.setState(selectAll(this.state.selectedState, value));
  };

  onSelectedChange = ({ id, value, shiftKey = false }) => {
    this.setState((state) => {
      return toggleSelected(state, this.props.items, id, value, shiftKey);
    });
  };

  onAddImportExclusionPress = () => {
    this.setState({ isAddImportExclusionModalOpen: true });
  };

  onModalClose = () => {
    this.setState({ isAddImportExclusionModalOpen: false });
  };

  //
  // Render

  render() {
    const {
      items,
      onConfirmDeleteImportExclusion,
      columns,
      ...otherProps
    } = this.props;

    const {
      allSelected,
      allUnselected,
      selectedState
    } = this.state;

    return (
      <FieldSet legend={translate('ListExclusions')}>
        <PageSectionContent
          errorMessage={translate('UnableToLoadListExclusions')}
          {...otherProps}
        >
          <div>
            <Table
              selectAll={true}
              allSelected={allSelected}
              allUnselected={allUnselected}
              columns={columns}
              {...otherProps}
              onSelectAllChange={this.onSelectAllChange}
            >
              <TableBody>
                {
                  items.map((item, index) => {
                    return (
                      <ImportListExclusion
                        key={item.id}
                        isSelected={selectedState[item.id] || false}
                        {...item}
                        {...otherProps}
                        columns={columns}
                        index={index}
                        onSelectedChange={this.onSelectedChange}
                        onConfirmDeleteImportExclusion={onConfirmDeleteImportExclusion}
                      />
                    );
                  })
                }
              </TableBody>
            </Table>
          </div>

          <div className={styles.addImportExclusion}>
            <Link
              className={styles.addButton}
              onPress={this.onAddImportExclusionPress}
            >
              <Icon name={icons.ADD} />
            </Link>
          </div>

          <EditImportListExclusionModalConnector
            isOpen={this.state.isAddImportExclusionModalOpen}
            onModalClose={this.onModalClose}
          />

        </PageSectionContent>
      </FieldSet>
    );
  }
}

ImportListExclusions.propTypes = {
  isFetching: PropTypes.bool.isRequired,
  error: PropTypes.object,
  columns: PropTypes.arrayOf(PropTypes.object).isRequired,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  onConfirmDeleteImportExclusion: PropTypes.func.isRequired
};

export default ImportListExclusions;
