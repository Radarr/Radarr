
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import isString from 'Utilities/String/isString';
import split from 'Utilities/String/split';
import TagInput from './TagInput';

function createMapStateToProps() {
  return createSelector(
    (state, { value }) => value,
    (tags) => {
      const isArray = !isString(tags);
      const tagsArray = isArray ? tags :split(tags);

      return {
        tags: tagsArray.reduce((result, tag) => {
          if (tag) {
            result.push({
              id: tag,
              name: tag
            });
          }

          return result;
        }, []),
        isArray
      };
    }
  );
}

class TextTagInputConnector extends Component {

  //
  // Listeners

  onTagAdd = (tag) => {
    const {
      name,
      value,
      isArray,
      onChange
    } = this.props;

    const newValue = isArray ? [...value] : split(value);
    newValue.push(tag.name);

    onChange({ name, value: newValue.join(',') });
  }

  onTagDelete = ({ index }) => {
    const {
      name,
      value,
      isArray,
      onChange
    } = this.props;

    const newValue = isArray ? [...value] : split(value);
    newValue.splice(index, 1);

    onChange({
      name,
      value: newValue.join(',')
    });
  }

  //
  // Render

  render() {
    return (
      <TagInput
        tagList={[]}
        onTagAdd={this.onTagAdd}
        onTagDelete={this.onTagDelete}
        {...this.props}
      />
    );
  }
}

TextTagInputConnector.propTypes = {
  name: PropTypes.string.isRequired,
  value: PropTypes.oneOfType([PropTypes.string, PropTypes.arrayOf(PropTypes.string)]),
  isArray: PropTypes.bool.isRequired,
  onChange: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, null)(TextTagInputConnector);
