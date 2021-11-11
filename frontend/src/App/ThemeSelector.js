import PropTypes from 'prop-types';
import React from 'react';

const theme = window.Radarr.theme;

function ThemeSelector({ children }) {
  return (
    <>
      {
        theme !== 'default' &&
          <>
            <link rel="stylesheet" type="text/css"
              href={'/Content/Theme.Park/radarr-base.css'}
            />
            <link rel="stylesheet" type="text/css"
              href={`/Content/Theme.Park/Themes/${theme}.css`}
            />
          </>
      }
      {children}
    </>
  );
}

ThemeSelector.propTypes = {
  children: PropTypes.object.isRequired
};

export default ThemeSelector;
