import * as filterTypes from './filterTypes';
import isAfter from 'Utilities/Date/isAfter';
import isBefore from 'Utilities/Date/isBefore';

const filterTypePredicates = {
  [filterTypes.CONTAINS]: function(itemValue, filterValue) {
    if (Array.isArray(itemValue)) {
      return itemValue.some((v) => v === filterValue);
    }

    return itemValue.toLowerCase().contains(filterValue.toLowerCase());
  },

  [filterTypes.EQUAL]: function(itemValue, filterValue) {
    return itemValue === filterValue;
  },

  [filterTypes.GREATER_THAN]: function(itemValue, filterValue) {
    return itemValue > filterValue;
  },

  [filterTypes.GREATER_THAN_OR_EQUAL]: function(itemValue, filterValue) {
    return itemValue >= filterValue;
  },

  [filterTypes.LESS_THAN]: function(itemValue, filterValue) {
    return itemValue < filterValue;
  },

  [filterTypes.LESS_THAN_OR_EQUAL]: function(itemValue, filterValue) {
    return itemValue <= filterValue;
  },

  [filterTypes.NOT_CONTAINS]: function(itemValue, filterValue) {
    if (Array.isArray(itemValue)) {
      return !itemValue.some((v) => v === filterValue);
    }

    return !itemValue.toLowerCase().contains(filterValue.toLowerCase());
  },

  [filterTypes.NOT_EQUAL]: function(itemValue, filterValue) {
    return itemValue !== filterValue;
  },

  [filterTypes.IN_LAST]: function(itemValue, filterValue) {
    return (
      isAfter(itemValue, { [filterValue.time]: filterValue.value * -1 }) &&
      isBefore(itemValue)
    );
  },

  [filterTypes.IN_NEXT]: function(itemValue, filterValue) {
    return (
      isAfter(itemValue) &&
      isBefore(itemValue, { [filterValue.time]: filterValue.value })
    );
  }
};

export default filterTypePredicates;
